using DG.Tweening;
using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Class.Kineticist;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Decals;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using UnityEngine;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.HarmonyPatches
{
    static class Bugfixes
    {
        // fix when main character is not in the party, the game will never consider the player is in combat
        [HarmonyPatch(typeof(Player), nameof(Player.IsInCombat), MethodType.Getter)]
        static class Player_get_IsInCombat_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(Player __instance, ref bool __result)
            {
                if (Mod.Enabled && FixNeverInCombatWithoutMC)
                {
                    __result = __instance.PartyCharacters.FirstOrDefault().Value?.Group.IsInCombat ?? false;
                    return false;
                }
                return true;
            }
        }

        // fix the action type for starting a Bardic Performance with/without Singing Steel
        [HarmonyPatch(typeof(UnitActivateAbility), "GetCommandType", typeof(ActivatableAbility))]
        static class UnitActivateAbility_GetCommandType_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(ActivatableAbility ability, ref UnitCommand.CommandType __result)
            {
                if (Mod.Enabled && FixActionTypeOfBardicPerformance)
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

        // fix activating Kinetic Blade is regarded as drawing weapon and costs an additional standard action
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
                return Mod.Enabled && FixActionTypeOfKineticBlade &&
                    ((slot.MaybeItem.IsKineticBlast() && previousItem == null) ||
                    (slot.MaybeItem == null && previousItem.IsKineticBlast()));
            }
        }

        // fix Kineticist will not stop its previous action if you command it to attack with Kinetic Blade before combat
        [HarmonyPatch(typeof(KineticistController), "TryRunKineticBladeActivationAction")]
        static class KineticistController_TryRunKineticBladeActivationAction_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitCommand cmd, ref UnitCommands.CustomHandlerData? customHandler)
            {
                if (Mod.Enabled && FixKineticistWontStopPriorCommand && 
                    customHandler.HasValue && (customHandler.Value.ExecuteBefore ?? cmd) != cmd)
                {
                    UnitCommands commands = cmd.Executor.Commands;

                    // remove conflicting command
                    UnitCommand prior = commands.Raw[(int)cmd.Type] ?? commands.GetPaired(cmd);
                    if (Game.Instance.IsPaused && commands.PreviousCommand == null && prior != null && prior.IsRunning)
                    {
                        commands.PreviousCommand = prior;
                        commands.PreviousCommand.SuppressAnimation();
                        commands.Raw[(int)commands.PreviousCommand.Type] = null;
                    }
                    else
                    {
                        commands.InterruptAndRemoveCommand(cmd.Type);
                    }

                    // update target
                    if (cmd.Type == UnitCommand.CommandType.Standard || commands.Standard == null)
                    {
                        commands.UpdateCombatTarget(cmd);
                    }
                }
            }
        }

        // fix Spellstrike does not take effect when attacking a neutral target
        [HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.CreateCastCommand))]
        static class UnitUseAbility_CreateCastCommand_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // unit.IsEnemy(target.Unit)
                // ---------------- after  ----------------
                // IsTarget(unit, target.Unit)
                return codes.ReplaceAll(
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<UnitEntityData, Func<UnitEntityData, UnitEntityData, bool>>(nameof(UnitEntityData.IsEnemy))),
                    new CodeInstruction(OpCodes.Callvirt,
                        new Func<UnitEntityData, UnitEntityData, bool>(IsTarget).Method),
                    true);
            }

            static bool IsTarget(UnitEntityData unit, UnitEntityData target)
            {
                return (Mod.Enabled && FixSpellstrikeOnNeutralUnit) ? unit.CanAttack(target) : unit.IsEnemy(target);
            }
        }

        // fix Spellstrike does not take effect when using Metamagic (Reach) on a touch spell
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.IsRay), MethodType.Getter)]
        static class AbilityData_get_IsRay_Patch
        {
            [HarmonyPostfix]
            static void Postfix(AbilityData __instance, ref bool __result)
            {
                if (Mod.Enabled && FixSpellstrikeWithMetamagicReach && !__result)
                {
                    if (__instance.Blueprint.GetComponent<AbilityDeliverTouch>() != null &&
                        __instance.HasMetamagic(Metamagic.Reach))
                    {
                        __result = true;
                    }
                }
            }
        }

        // fix Spellstrike does not take effect when using Metamagic (Reach) on a touch spell
        [HarmonyPatch(typeof(UnitPartMagus), nameof(UnitPartMagus.IsSpellFromMagusSpellList), typeof(AbilityData))]
        static class UnitPartMagus_IsSpellFromMagusSpellList_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitPartMagus __instance, AbilityData spell, ref bool __result)
            {
                if (Mod.Enabled && FixSpellstrikeWithMetamagicReach && !__result)
                {
                    if (__instance.Spellbook.Blueprint.SpellList.SpellsByLevel.Any(list =>
                        list.Spells.Any(item => item.StickyTouch?.TouchDeliveryAbility == spell.Blueprint)))
                    {
                        __result = true;
                    }
                }
            }
        }

        // fix sometimes the game does not regard a unit that is forced to move as a unit that is moved (cause AoO inconsistent)
        [HarmonyPatch(typeof(UnitMoveController), nameof(UnitMoveController.Tick))]
        static class UnitMoveController_Tick_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // awakeUnit.PreviousPosition = awakeUnit.Position;
                // ---------------- after  ----------------
                // awakeUnit.PreviousPosition = GetPreviousPosition(awakeUnit);
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitEntityData, Vector3>(nameof(UnitEntityData.Position)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitEntityData, Vector3>(nameof(UnitEntityData.PreviousPosition)).GetSetMethod(true))
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    return codes.Replace(startIndex + 2, new CodeInstruction(OpCodes.Call,
                        new Func<UnitEntityData, Vector3>(GetPreviousPosition).Method), false).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static Vector3 GetPreviousPosition(UnitEntityData awakeUnit)
            {
                return (Mod.Enabled && FixHasMotionThisTick) ? 
                    awakeUnit.View?.transform.position ?? awakeUnit.Position : awakeUnit.Position;
            }
        }

        // fix the visual circle of certain abilities is inconsistent with the real range
        [HarmonyPatch(typeof(AbilityData), nameof(AbilityData.GetVisualDistance))]
        static class AbilityData_GetVisualDistance_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // return Blueprint.GetRange(flag).Meters + corpulence + 0.5f;
                // ---------------- after  ----------------
                // return Blueprint.GetRange(flag).Meters + corpulence + GetValue();
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldc_R4, 0.5f),
                    new CodeInstruction(OpCodes.Add),
                };
                int startIndex = codes.FindLastCodes(findingCodes);
                if (startIndex >= 0)
                {
                    return codes.Replace(startIndex, new CodeInstruction(OpCodes.Call,
                            new Func<float>(GetValue).Method), false).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static float GetValue()
            {
                return Mod.Enabled && FixAbilityCircleRadius ? 0f : 0.5f;
            }
        }

        // fix the ability circle does not appear properly when you first time select any ability of the unit using the hotkey  
        [HarmonyPatch(typeof(GUIDecal), "InitAnimator")]
        static class GUIDecal_InitAnimator_Patch
        {
            [HarmonyPostfix]
            static void Postfix(Tweener ___m_AppearAnimation, Tweener ___m_DisappearAnimation)
            {
                if (Mod.Enabled && FixAbilityCircleNotAppear)
                {
                    ___m_AppearAnimation.Pause();
                    ___m_DisappearAnimation.Pause();
                }
            }
        }
    }
}