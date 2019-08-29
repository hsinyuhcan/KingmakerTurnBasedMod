using Harmony12;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Controllers;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class ActionCooldowns
    {
        // check the cooldown, you can perform one standard action and one move action, or perform two move action in a turn
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.HasCooldownForCommand), typeof(UnitCommand.CommandType))]
        static class UnitCombatState_HasCooldownForCommandType_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, UnitCommand.CommandType commandType, ref bool __result)
            {
                if (IsInCombat() && __instance.Unit.IsInCombat)
                {
                    switch (commandType)
                    {
                        case UnitCommand.CommandType.Free:
                            __result = false;
                            break;
                        case UnitCommand.CommandType.Move:
                            __result = !__instance.Unit.HasMoveAction();
                            break;
                        case UnitCommand.CommandType.Standard:
                            UnitCommand moveCommand = __instance.Unit.Commands.GetCommand(UnitCommand.CommandType.Move);
                            __result = (moveCommand != null && moveCommand.IsRunning) || !__instance.Unit.HasStandardAction();
                            break;
                        case UnitCommand.CommandType.Swift:
                            __result = __instance.Cooldown.SwiftAction > 0f;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return false;
                }
                return true;
            }
        }

        // check the cooldown, fix full round spell restriction
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.HasCooldownForCommand), typeof(UnitCommand))]
        static class UnitCombatState_HasCooldownForCommand_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, UnitCommand command, ref bool __result)
            {
                if (IsInCombat() && __instance.Unit.IsInCombat)
                {
                    __result = !command.IsIgnoreCooldown &&
                        (ShouldRestrictCommand(__instance.Unit, command) || __instance.HasCooldownForCommand(command.Type));
                    return false;
                }
                return true;
            }

            static bool ShouldRestrictCommand(UnitEntityData unit, UnitCommand command)
            {
                return !unit.IsSurprising() && command.IsFullRoundAbility() && !unit.HasFullRoundAction();
            }
        }

        // restrict full attack by checking move action cooldown instead of LastUsageOfMoveActionTime
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.IsFullAttackRestrictedBecauseOfMoveAction), MethodType.Getter)]
        static class UnitCombatState_get_IsFullAttackRestrictedBecauseOfMoveAction_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, ref bool __result)
            {
                if (IsInCombat() && __instance.Unit.IsInCombat)
                {
                    TurnController currentTurn;
                    __result = __instance.Unit.UsedOneMoveAction() ||
                        (__instance.Unit == (currentTurn = Mod.Core.Combat.CurrentTurn)?.Unit && !currentTurn.EnabledFullAttack);
                    return false;
                }
                return true;
            }
        }

        // fix some abilities (e.g. Pounce) could be restricted by move action even if they should ignore action cooldowns
        [HarmonyPatch(typeof(UnitAttack), "InitAttacks")]
        static class UnitAttack_InitAttacks_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // !base.Executor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction
                // ---------------- after  ----------------
                // IsFullAttackRestricted(!base.Executor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction, this)
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        GetPropertyInfo<UnitCommand, UnitEntityData>(nameof(UnitCommand.Executor)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitEntityData, UnitCombatState>(nameof(UnitEntityData.CombatState)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitCombatState, bool>(nameof(UnitCombatState.IsFullAttackRestrictedBecauseOfMoveAction)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Brtrue),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    List<CodeInstruction> patchingCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call,
                            new Func<bool, UnitAttack, bool>(IsFullAttackRestricted).Method)
                    };
                    return codes.InsertRange(startIndex + 4, patchingCodes, true).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static bool IsFullAttackRestricted(bool isFullAttackRestrictedBecauseOfMoveAction, UnitAttack command)
            {
                if (IsInCombat() && command.Executor.IsInCombat)
                {
                    if (command.Executor.IsMoveActionRestricted())
                        return true;
                    else if (command.IsIgnoreCooldown)
                        return false;
                }
                return isFullAttackRestrictedBecauseOfMoveAction;
            }
        }

        // fix action cooldown doesn't advance when fail the concentration check
        [HarmonyPatch(typeof(UnitCommand), "ForceFinish", typeof(UnitCommand.ResultType))]
        static class UnitCommand_ForceFinish_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitCommand __instance)
            {
                if (IsInCombat() && __instance.Executor.IsInCombat && !__instance.IsActed)
                {
                    __instance.SetIsActed(true);
                }
            }
        }

        // fix action cooldown
        [HarmonyPatch(typeof(UnitActionController), nameof(UnitActionController.UpdateCooldowns), typeof(UnitCommand))]
        static class UnitActionController_UpdateCooldowns_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCommand command)
            {
                if (IsInCombat() && command.Executor.IsInCombat)
                {
                    command.Executor.UpdateCooldowns(command);
                    return false;
                }
                return true;
            }
        }

        // stop cooling during units' turn
        [HarmonyPatch(typeof(UnitCombatCooldownsController), "TickOnUnit", typeof(UnitEntityData))]
        static class UnitCombatCooldownsController_TickOnUnit_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitEntityData unit)
            {
                if (IsInCombat())
                {
                    if (unit.IsInCombat)
                    {
                        if (IsPassing())
                        {
                            UnitCombatState combatState = unit.CombatState;
                            UnitCombatState.Cooldowns cooldown = combatState.Cooldown;
                            float gameDeltaTime = Game.Instance.TimeController.GameDeltaTime;

                            if (cooldown.Initiative > 0f)
                            {
                                if (gameDeltaTime >= cooldown.Initiative)
                                {
                                    gameDeltaTime -= cooldown.Initiative;
                                    cooldown.Initiative = 0f;
                                }
                                else
                                {
                                    cooldown.Initiative -= gameDeltaTime;
                                    gameDeltaTime = 0f;
                                }
                            }

                            if (gameDeltaTime > 0f)
                            {
                                cooldown.StandardAction = Math.Max(0f, cooldown.StandardAction - gameDeltaTime);
                                cooldown.MoveAction = Math.Max(0f, cooldown.MoveAction - gameDeltaTime);
                                cooldown.SwiftAction = Math.Max(0f, cooldown.SwiftAction - gameDeltaTime);
                                cooldown.AttackOfOpportunity = Math.Max(0f, cooldown.AttackOfOpportunity - gameDeltaTime);
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return IsPassing();
                    }
                }
                return true;
            }
        }
    }
}