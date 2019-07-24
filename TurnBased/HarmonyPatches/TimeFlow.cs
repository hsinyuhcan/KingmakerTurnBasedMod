using Harmony12;
using Kingmaker;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    internal static class TimeFlow
    {
        // control combat process
        [HarmonyPatch(typeof(Game), "Tick")]
        static class Game_Tick_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!Game.Instance.IsPaused)
                {
                    if (IsInCombat())
                    {
                        try
                        {
                            Core.Mod.RoundController.Tick();
                        }
                        catch (Exception e)
                        {
                            Core.Error($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}()");
                            Core.Error($"{e.Message}\n{e.StackTrace}");
                            Game.Instance.IsPaused = true;
                        }
                    }
                }
            }
        }

        // freeze game time during a unit's turn, and set the time scale
        [HarmonyPatch(typeof(TimeController), "Tick")]
        static class TimeController_Tick_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!Game.Instance.IsPaused)
                {
                    if (IsInCombat())
                    {
                        try
                        {
                            Core.Mod.RoundController.TickTime();
                        }
                        catch (Exception e)
                        {
                            Core.Error($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}()");
                            Core.Error($"{e.Message}\n{e.StackTrace}");
                            Game.Instance.IsPaused = true;
                        }
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
                        canTick = command.Executor.IsCurrentUnit() && !IsDelaying();

                        if (canTick && !command.IsStarted &&
                            command.IsSpellCombat() && !command.Executor.HasMoveAction())
                        {
                            command.Executor.Descriptor.RemoveFact(BlueprintRoot.Instance.SystemMechanics.MagusSpellCombatBuff);
                            command.Interrupt();
                            canTick = false;
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

                    bool isInForceMode = __instance.GetFieldValue<UnitMovementAgent, bool>("m_IsInForceMode");

                    if ((__instance.Unit?.EntityData).IsCurrentUnit() && !IsDelaying() && !IsEnding() && 
                        (isInForceMode || Core.Mod.RoundController.CurrentTurn.HasMovement()))
                    {
                        if (!isInForceMode)
                        {
                            // disable acceleration effect
                            __state = __instance.GetFieldValue<UnitMovementAgent, float>("m_MinSpeed");
                            __instance.SetFieldValue("m_MinSpeed", 1f);
                            __instance.SetFieldValue("m_WarmupTime", 0f);
                            __instance.SetFieldValue("m_SlowDownTime", 0f);
                        }

                        Core.Mod.RoundController.CurrentTurn?.TickMovement(ref deltaTime, isInForceMode);
                    }
                    else if (!IsPassing() || (__instance.Unit != null && __instance.Unit.EntityData.IsInCombat))
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
                    __instance.SetFieldValue("m_MinSpeed", __state.Value);
                }
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
                    if (Core.Mod.RoundController.TickedRayView.Add(__instance))
                    {
                        __instance.SetFieldValue("m_PrevTickTime", TimeSpan.Zero);
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
                    (Core.Enabled && Core.Mod.LastTickTimeOfAbilityExecutionProcess.ContainsKey(__instance)))
                {
                    if (Core.Mod.LastTickTimeOfAbilityExecutionProcess.TryGetValue(__instance, out TimeSpan gameTime))
                    {
                        gameTime += TimeSpan.FromSeconds(Game.Instance.TimeController.GameDeltaTime);
                    }
                    else
                    {
                        gameTime = Game.Instance.Player.GameTime;
                    }

                    Core.Mod.LastTickTimeOfAbilityExecutionProcess[__instance] = gameTime;

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
                        Core.Mod.LastTickTimeOfAbilityExecutionProcess.Remove(__instance);

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
        [HarmonyPatch(typeof(UnitConfusionController), "TickOnUnit", typeof(UnitEntityData))]
        static class UnitConfusionController_TickOnUnit_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitEntityData unit)
            {
                return !IsInCombat() || !unit.IsInCombat || unit.IsCurrentUnit();
            }

            [HarmonyPostfix]
            static void Postfix(UnitEntityData unit)
            {
                if (IsInCombat() && unit.IsCurrentUnit())
                {
                    UnitPartConfusion unitPartConfusion = unit.Get<UnitPartConfusion>();
                    if (unitPartConfusion != null &&
                        unitPartConfusion.RoundStartTime == Game.Instance.TimeController.GameTime)
                    {
                        unitPartConfusion.RoundStartTime -= TimeSpan.FromSeconds(0.5d);
                    }
                }
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
                return !IsInCombat() || !unit.IsInCombat || (unit.IsCurrentUnit() && !IsPreparing() && !IsDelaying());
            }
        }

        // stop ticking during units' turn (do not leave combat instantly) if there are still enemies
        [HarmonyPatch(typeof(UnitCombatLeaveController), "Tick")]
        static class UnitCombatLeaveController_Tick_Patch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                if (IsInCombat() && !IsPassing())
                {
                    UnitGroup playerGroup = Game.Instance.Player.Group;
                    return !Game.Instance.UnitGroups
                        .Any(group => group != playerGroup && group.IsInCombat && group.IsEnemy(playerGroup));
                }
                return true;
            }
        }

        // stop ticking during units' turn
        [HarmonyPatch]
        static class BaseUnitController_TickOnUnit_Patch
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> TargetMethods(HarmonyInstance instance)
            {
                // ok - it's good
                // oo - no matter
                // ss - in another patch
                // ?? - not sure
                // xx - don't touch
                //yield return GetTargetMethod(typeof(UnitCombatCooldownsController));        // ss - GameDeltaTime
                //yield return GetTargetMethod(typeof(UnitActivatableAbilitiesController));   // ss - GameDeltaTime
                //yield return GetTargetMethod(typeof(SummonedUnitsController));              // xx
                //yield return GetTargetMethod(typeof(UnitAnimationController));              // xx
                //yield return GetTargetMethod(typeof(UnitBuffsController));                  // oo - GameTime
                //yield return GetTargetMethod(typeof(UnitConfusionController));              // ss - GameTime
                //yield return GetTargetMethod(typeof(UnitForceMoveController));              // xx
                //yield return GetTargetMethod(typeof(UnitGrappleController));                // oo - GameTime
                //yield return GetTargetMethod(typeof(UnitGuardController));                  // xx
                //yield return GetTargetMethod(typeof(UnitLifeController));                   // xx
                //yield return GetTargetMethod(typeof(UnitMimicController));                  // xx
                yield return GetTargetMethod(typeof(UnitRoamingController));                // ?? - GameTime
                yield return GetTargetMethod(typeof(UnitsProximityController));             // ?? - DeltaTime
                //yield return GetTargetMethod(typeof(UnitTicksController));                  // ss - GameDeltaTime
                //yield return GetTargetMethod(typeof(UnitStealthController), "TickUnit");    // ss - GameTime + GameDeltaTime
            }

            [HarmonyPrefix]
            static bool Prefix()
            {
                return !IsInCombat() || IsPassing();
            }

            static MethodBase GetTargetMethod(Type type)
            {
                return type.GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new Type[] { typeof(UnitEntityData) }, null);
            }
        }

        // stop time advanced during units' turn
        [HarmonyPatch]
        static class BaseUnitController_TickDeltaTime_Patch
        {
            [HarmonyTargetMethods]
            static IEnumerable<MethodBase> TargetMethods(HarmonyInstance instance)
            {
                yield return GetTargetMethod(typeof(UnitInPitController));
                yield return GetTargetMethod(typeof(UnitProneController));
            }


            [HarmonyPrefix]
            static void Prefix(ref float? __state)
            {
                if (IsInCombat() && !IsPassing())
                {
                    __state = Game.Instance.TimeController.DeltaTime;
                    Game.Instance.TimeController.SetPropertyValue(nameof(TimeController.DeltaTime), 0f);
                }
            }

            [HarmonyPostfix]
            static void Postfix(ref float? __state)
            {
                if (__state.HasValue)
                {
                    Game.Instance.TimeController.SetPropertyValue(nameof(TimeController.DeltaTime), __state.Value);
                }
            }

            static MethodBase GetTargetMethod(Type type)
            {
                return type.GetMethod("TickOnUnit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new Type[] { typeof(UnitEntityData) }, null);
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
                yield return GetTargetMethod(typeof(UnitSwallowWholeController), "TickOnUnit");
                yield return GetTargetMethod(typeof(AreaEffectEntityData), "Tick");

            }

            [HarmonyPrefix]
            static void Prefix(ref float? __state)
            {
                if (IsInCombat() && !IsPassing())
                {
                    __state = Game.Instance.TimeController.GameDeltaTime;
                    Game.Instance.TimeController.SetPropertyValue(nameof(TimeController.GameDeltaTime), 0f);
                }
            }

            [HarmonyPostfix]
            static void Postfix(ref float? __state)
            {
                if (__state.HasValue)
                {
                    Game.Instance.TimeController.SetPropertyValue(nameof(TimeController.GameDeltaTime), __state.Value);
                }
            }

            static MethodBase GetTargetMethod(Type type, string name)
            {
                return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }
    }
}
