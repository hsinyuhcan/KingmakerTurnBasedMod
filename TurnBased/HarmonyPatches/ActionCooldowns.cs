using Harmony12;
using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View.Equipment;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Utility.SettingsWrapper;
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
        static class UnitCombatState_HasCooldownForUnitCommand_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, UnitCommand command, ref bool __result)
            {
                if (IsInCombat() && __instance.Unit.IsInCombat)
                {
                    __result = !command.IsIgnoreCooldown &&
                        ((command.IsFullRoundSpell() && !__instance.Unit.HasFullRoundAction()) ||
                        __instance.HasCooldownForCommand(command.Type));
                    return false;
                }
                return true;
            }
        }

        // restrict full round action by move action cooldown
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.IsFullAttackRestrictedBecauseOfMoveAction), MethodType.Getter)]
        static class UnitCombatState_get_IsFullAttackRestrictedBecauseOfMoveAction_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, ref bool __result)
            {
                if (IsInCombat() && __instance.Unit.IsInCombat)
                {
                    __result = __instance.Unit.UsedOneMoveAction();
                    return false;
                }
                return true;
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

        // fix the cooldown of action to start Bardic Performance
        [HarmonyPatch(typeof(UnitActivateAbility), "GetCommandType", typeof(ActivatableAbility))]
        static class UnitActivateAbility_GetCommandType_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(ActivatableAbility ability, ref UnitCommand.CommandType __result)
            {
                if (IsEnabled() && FixTheCostToStartBardicPerformance)
                {
                    if (ability.Blueprint.Group == ActivatableAbilityGroup.BardicPerformance)
                    {
                        if (ability.Owner.State.Features.QuickenPerformance2)
                        {
                            __result = ability.Owner.State.Features.SingingSteel ?
                                UnitCommand.CommandType.Free : UnitCommand.CommandType.Swift;
                        }
                        else if (ability.Owner.State.Features.QuickenPerformance1)
                        {
                            __result = ability.Owner.State.Features.SingingSteel ?
                                UnitCommand.CommandType.Swift : UnitCommand.CommandType.Move;
                        }
                        else
                        {
                            __result = ability.Owner.State.Features.SingingSteel ?
                                UnitCommand.CommandType.Move : UnitCommand.CommandType.Standard;
                        }
                    }
                    else
                    {
                        __result = ability.Blueprint.ActivateWithUnitCommandType;
                    }
                    return false;
                }
                return true;
            }
        }

        // fix the cooldown of action to activate the Kinetic Blade
        [HarmonyPatch(typeof(UnitViewHandsEquipment), nameof(UnitViewHandsEquipment.HandleEquipmentSlotUpdated), typeof(HandSlot), typeof(ItemEntity))]
        static class UnitViewHandsEquipment_HandleEquipmentSlotUpdated_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // ... InCombat ...
                // ---------------- after  ----------------
                // !DontCostAction(slot, previousItem) && InCombat
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        GetPropertyInfo<UnitViewHandsEquipment, bool>(nameof(UnitViewHandsEquipment.InCombat)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Brfalse),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    List<CodeInstruction> patchingCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Call,
                            new Func<HandSlot, ItemEntity, bool>(DontCostAction).Method),
                        new CodeInstruction(OpCodes.Brtrue, codes.Item(startIndex + 2).operand),
                    };
                    return codes.InsertRange(startIndex, patchingCodes, true).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static bool DontCostAction(HandSlot slot, ItemEntity previousItem)
            {
                return IsInCombat() &&
                    ((slot.MaybeItem.IsKineticBlast() && previousItem == null) ||
                    (slot.MaybeItem == null && previousItem.IsKineticBlast()));
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
                        if (IsPassing() && unit.CanPerformAction())
                        {
                            UnitCombatState combatState = unit.CombatState;
                            UnitCombatState.Cooldowns cooldown = combatState.Cooldown;
                            float gameDeltaTime = Game.Instance.TimeController.GameDeltaTime;

                            if (combatState.IsWaitingInitiative)
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