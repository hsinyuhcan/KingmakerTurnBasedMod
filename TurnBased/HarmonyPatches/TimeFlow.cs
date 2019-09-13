using Harmony12;
using Kingmaker;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class TimeFlow
    {
        // control combat process
        [HarmonyPatch(typeof(Game), "Tick")]
        static class Game_Tick_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (IsInCombat() && !Game.Instance.IsPaused)
                {
                    try
                    {
                        Mod.Core.Combat.Tick();
                    }
                    catch (Exception e)
                    {
                        Mod.Error(e);
                        Game.Instance.IsPaused = true;
                        EventBus.RaiseEvent<IWarningNotificationUIHandler>(h => h.HandleWarning(Local["UI_Txt_Error"], false));
                    }
                }
            }
        }

        // freeze game time during a unit's turn, and set the time scale
        [HarmonyPatch(typeof(TimeController), "Tick")]
        static class TimeController_Tick_Patch
        {
            [HarmonyPrefix]
            static void Prefix(ref float? __state)
            {
                if (IsInCombat() && !Game.Instance.IsPaused && !Game.Instance.InvertPauseButtonPressed)
                {
                    __state = Game.Instance.TimeController.PlayerTimeScale;
                    UnitEntityData unit = CurrentUnit();
                    Game.Instance.TimeController.PlayerTimeScale = __state.Value *
                        (unit == null ? TimeScaleBetweenTurns :
                        (!DoNotShowInvisibleUnitOnCombatTracker || unit.IsVisibleForPlayer) ?
                        (unit.IsDirectlyControllable ? TimeScaleInPlayerTurn : TimeScaleInNonPlayerTurn) : TimeScaleInUnknownTurn);
                }
            }

            [HarmonyPostfix]
            static void Postfix(ref float? __state)
            {
                if (__state.HasValue)
                {
                    Game.Instance.TimeController.PlayerTimeScale = __state.Value;
                }

                if (IsInCombat() && !Game.Instance.IsPaused)
                {
                    try
                    {
                        Mod.Core.Combat.TickTime();
                    }
                    catch (Exception e)
                    {
                        Mod.Error(e);
                        Game.Instance.IsPaused = true;
                        EventBus.RaiseEvent<IWarningNotificationUIHandler>(h => h.HandleWarning(Local["UI_Txt_Error"], false));
                    }
                }
            }
        }

        // block commands (e.g. stop non-'attack of opportunity' actions of non-current units), fix Magus spell combat
        [HarmonyPatch(typeof(UnitActionController), "TickCommand", typeof(UnitCommand), typeof(bool))]
        static class UnitActionController_TickCommand_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCommand command)
            {
                if (IsInCombat())
                {
                    //                          passing     acting      ending      delaying
                    // not in combat            o           x           x           x
                    // in combat - not current  x           x           x           x
                    // in combat - current      x           o           o           x

                    bool canTick = default;

                    if (command is UnitAttackOfOpportunity)
                    {
                        canTick = true;
                    }
                    else if(IsPassing())
                    {
                        canTick = !command.Executor.IsInCombat;
                    }
                    else
                    {
                        canTick = command.Executor.IsCurrentUnit() && (IsActing() || IsEnding());

                        if (canTick && !command.IsStarted)
                        {
                            if (command.IsSpellCombatAttack() && !command.Executor.HasMoveAction())
                            {
                                command.Executor.Descriptor.RemoveFact(BlueprintRoot.Instance.SystemMechanics.MagusSpellCombatBuff);
                                command.Interrupt();
                                canTick = false;
                            }

                            if (command is UnitUseAbility unitUseAbility && !unitUseAbility.Spell.IsAvailableForCast)
                            {
                                command.Interrupt();
                                canTick = false;
                            }
                        }
                    }

                    if (!canTick)
                    {
                        return false;
                    }

                    if (command.Executor.IsInCombat)
                    {
                        command.NextApproachTime = Game.Instance.TimeController.GameTime;
                    }
                }
                return true;
            }
        }

        // block movement / ** make moving increase move action cooldown
        [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.TickMovement), typeof(float))]
        static class UnitMovementAgent_TickMovement_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitMovementAgent __instance, ref float deltaTime, ref float? __state)
            {
                if (IsInCombat())
                {
                    //                          passing     acting      ending      delaying
                    // not in combat            o           x           x           x
                    // in combat - not current  x           x           x           x
                    // in combat - current      x           o           x           x

                    bool canMove = default;
                    bool isInForceMode = __instance.GetIsInForceMode();
                    UnitEntityView view = __instance.Unit;

                    if (IsPassing())
                    {
                        canMove = !(view?.EntityData?.IsInCombat ?? false);
                    }
                    else
                    {
                        canMove = (view?.EntityData).IsCurrentUnit() && IsActing() &&
                            !(view.AnimationManager?.IsPreventingMovement ?? false) &&
                            !view.IsCommandsPreventMovement && __instance.IsReallyMoving;

                        if (canMove)
                        {
                            CurrentTurn().TickMovement(ref deltaTime, isInForceMode);

                            if (deltaTime <= 0f)
                            {
                                canMove = false;
                            }
                            else if (!isInForceMode)
                            {
                                // disable acceleration effect
                                __state = __instance.GetMinSpeed();
                                __instance.SetMinSpeed(1f);
                                __instance.SetWarmupTime(0f);
                                __instance.SetSlowDownTime(0f);
                            }
                        }
                    }

                    if (!canMove)
                    {
                        if (!isInForceMode)
                        {
                            __instance.Stop();
                        }
                        return false;
                    }
                }
                return true;
            }

            [HarmonyPostfix]
            static void Postfix(UnitMovementAgent __instance, ref float? __state)
            {
                if (__state.HasValue)
                {
                    __instance.SetMinSpeed(__state.Value);
                }
            }
        }

        // fix the exact range of movement is slightly shorter than the indicator range
        [HarmonyPatch(typeof(UnitMovementAgent), "SlowDown")]
        static class UnitMovementAgent_SlowDown_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitMovementAgent __instance)
            {
                if (IsInCombat() && (__instance.Unit?.EntityData?.IsInCombat ?? false))
                {
                    return false;
                }
                return true;
            }
        }

        // fix toggleable abilities
        [HarmonyPatch(typeof(UnitActivatableAbilitiesController), "TickOnUnit", typeof(UnitEntityData))]
        static class UnitActivatableAbilitiesController_TickOnUnit_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // activatableAbility.TimeToNextRound -= Game.Instance.TimeController.GameDeltaTime;
                // if (activatableAbility.TimeToNextRound <= 0f)
                // ---------------- after  ----------------
                // activatableAbility.TimeToNextRound = GetTimeToNextRound(unit);
                // if (activatableAbility.TimeToNextRound <= 0f && CanTickNewRound(unit))
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Dup),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<ActivatableAbility, float>(nameof(ActivatableAbility.TimeToNextRound)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Call,
                        GetPropertyInfo<Game, Game>(nameof(Game.Instance)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<Game, TimeController>(nameof(Game.TimeController))),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<TimeController, float>(nameof(TimeController.GameDeltaTime)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Sub),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<ActivatableAbility, float>(nameof(ActivatableAbility.TimeToNextRound)).GetSetMethod(true)),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<ActivatableAbility, float>(nameof(ActivatableAbility.TimeToNextRound)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Ldc_R4),
                    new CodeInstruction(OpCodes.Bgt_Un),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    List<CodeInstruction> patchingCodes_1 = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call,
                            new Func<float, UnitEntityData, float>(GetTimeToNextRound).Method)
                    };
                    List<CodeInstruction> patchingCodes_2 = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call,
                            new Func<UnitEntityData, bool>(CanTickNewRound).Method),
                        new CodeInstruction(OpCodes.Brfalse, codes.Item(startIndex + findingCodes.Count - 1).operand)
                    };
                    return codes
                        .InsertRange(startIndex + findingCodes.Count, patchingCodes_2, true)
                        .ReplaceRange(startIndex + 3, 4, patchingCodes_1, false).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static float GetTimeToNextRound(float timeToNextRound, UnitEntityData unit)
            {
                if (IsInCombat())
                {
                    if (!IsPassing())
                    {
                        return timeToNextRound;
                    }
                    else if (unit.IsInCombat)
                    {
                        return unit.GetTimeToNextTurn();
                    }
                }
                return timeToNextRound -= Game.Instance.TimeController.GameDeltaTime;
            }

            static bool CanTickNewRound(UnitEntityData unit)
            {
                return !IsInCombat() || !unit.IsInCombat || (unit.IsCurrentUnit() && (IsActing() || IsEnding()));
            }
        }

        // tick ray effects even while time is frozen (e.g. Lightning Bolt)
        [HarmonyPatch(typeof(RayView), nameof(RayView.Update))]
        static class RayView_Update_Patch
        {
            [HarmonyPrefix]
            static void Prefix(RayView __instance)
            {
                if (IsInCombat() && !IsPassing())
                {
                    if (Mod.Core.Combat.TickedRayView.Add(__instance))
                    {
                        __instance.SetPrevTickTime(TimeSpan.Zero);
                    }
                }
            }
        }

        // tick AbilityDeliverEffect even while time is frozen (e.g. Scorching Ray)
        [HarmonyPatch(typeof(AbilityExecutionProcess), nameof(AbilityExecutionProcess.Tick))]
        static class AbilityExecutionProcess_Tick_Patch
        {
            [HarmonyPrefix]
            static void Prefix(AbilityExecutionProcess __instance, ref TimeSpan? __state)
            {
                if ((IsInCombat() && __instance.Context.AbilityBlueprint.GetComponent<AbilityDeliverEffect>() != null) ||
                    (Mod.Enabled && Mod.Core.LastTickTimeOfAbilityExecutionProcess.ContainsKey(__instance)))
                {
                    if (Mod.Core.LastTickTimeOfAbilityExecutionProcess.TryGetValue(__instance, out TimeSpan gameTime))
                    {
                        gameTime += Game.Instance.TimeController.GameDeltaTime.Seconds();
                    }
                    else
                    {
                        gameTime = Game.Instance.Player.GameTime;
                    }

                    Mod.Core.LastTickTimeOfAbilityExecutionProcess[__instance] = gameTime;

                    __state = Game.Instance.Player.GameTime;
                    Game.Instance.Player.GameTime = gameTime;
                }
            }

            [HarmonyPostfix]
            static void Postfix(AbilityExecutionProcess __instance, ref TimeSpan? __state)
            {
                if (__state.HasValue)
                {
                    if (__instance.IsEnded)
                        Mod.Core.LastTickTimeOfAbilityExecutionProcess.Remove(__instance);

                    Game.Instance.Player.GameTime = __state.Value;
                }
            }
        }

        // stop ticking AbilityAreaEffectMovement when time is frozen (e.g. Cloudkill)
        [HarmonyPatch(typeof(AbilityAreaEffectMovement), "OnTick", typeof(MechanicsContext), typeof(AreaEffectEntityData))]
        static class AbilityAreaEffectMovement_OnTick_Patch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                return !IsInCombat() || IsPassing();
            }
        }

        // stop ticking cutscene commands when time is frozen (to fix some scripted event animations being skipped during TB combat)
        [HarmonyPatch(typeof(CutscenePlayerData.TrackData), nameof(CutscenePlayerData.TrackData.Tick), typeof(CutscenePlayerData))]
        static class TrackData_Tick_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(CutscenePlayerData.TrackData __instance)
            {
                return !IsInCombat() || IsPassing() || !__instance.IsPlaying;
            }
        }

        // ** moved to TurnController
        [HarmonyPatch(typeof(UnitTicksController), "TickOnUnit", typeof(UnitEntityData))]
        static class UnitTicksController_TickOnUnit_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitEntityData unit)
            {
                return !IsInCombat() || !unit.IsInCombat;
            }
        }

        // stop time advanced during units' turn
        [HarmonyPatch]
        static class BaseUnitController_TickDeltaTime_Patch
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> TargetMethods(HarmonyInstance instance)
            {
                yield return GetTargetMethod(typeof(UnitInPitController), "TickOnUnit");
                //yield return GetTargetMethod(typeof(UnitProneController));
                yield return GetTargetMethod(typeof(UnitsProximityController), "TickOnUnit");
            }


            [HarmonyPrefix]
            static void Prefix(ref float? __state)
            {
                if (IsInCombat() && !IsPassing())
                {
                    __state = Game.Instance.TimeController.DeltaTime;
                    Game.Instance.TimeController.SetDeltaTime(0f);
                }
            }

            [HarmonyPostfix]
            static void Postfix(ref float? __state)
            {
                if (__state.HasValue)
                {
                    Game.Instance.TimeController.SetDeltaTime(__state.Value);
                }
            }

            static MethodBase GetTargetMethod(Type type, string name)
            {
                return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        // stop time advanced during units' turn
        [HarmonyPatch]
        static class BaseUnitController_TickGameDeltaTime_Patch
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> TargetMethods(HarmonyInstance instance)
            {
                yield return GetTargetMethod(typeof(UnitFearController), "TickOnUnit");
                yield return GetTargetMethod(typeof(UnitStealthController), "Tick");
                yield return GetTargetMethod(typeof(UnitSwallowWholeController), "TickOnUnit");
                yield return GetTargetMethod(typeof(AreaEffectEntityData), "Tick");
            }

            [HarmonyPrefix]
            static void Prefix(ref float? __state)
            {
                if (IsInCombat() && !IsPassing())
                {
                    __state = Game.Instance.TimeController.GameDeltaTime;
                    Game.Instance.TimeController.SetGameDeltaTime(0f);
                }
            }

            [HarmonyPostfix]
            static void Postfix(ref float? __state)
            {
                if (__state.HasValue)
                {
                    Game.Instance.TimeController.SetGameDeltaTime(__state.Value);
                }
            }

            static MethodBase GetTargetMethod(Type type, string name)
            {
                return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        // stop time advanced during units' turn
        // ok - it's good
        // oo - no matter
        // ss - in special patch
        // ?? - not sure
        // xx - don't touch
        // UnitCombatCooldownsController        // ss - GameDeltaTime
        // UnitActivatableAbilitiesController   // ss - GameDeltaTime
        // SummonedUnitsController              // xx
        // UnitAnimationController              // xx
        // UnitBuffsController                  // oo - GameTime
        // UnitConfusionController              // ss - GameTime
        // UnitForceMoveController              // xx
        // UnitGrappleController                // oo - GameTime
        // UnitGuardController                  // xx
        // UnitLifeController                   // xx
        // UnitMimicController                  // xx
        // UnitRoamingController                // ?? - GameTime
        // UnitsProximityController             // ?? - DeltaTime
        // UnitTicksController                  // ss - GameDeltaTime
        // UnitStealthController                // ss - GameTime + GameDeltaTime
    }
}
