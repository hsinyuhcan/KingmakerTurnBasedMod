using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.ActionBar;
using Kingmaker.UI.Inspect;
using Kingmaker.UI.Selection;
using Kingmaker.UI.Tooltip;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Controllers;
using TurnBased.Utility;
using UnityEngine;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    class HUD
    {
        // show a small circle under a unit if the unit is within attack range
        [HarmonyPatch(typeof(UIDecal), nameof(UIDecal.HandleAoEMove), typeof(Vector3), typeof(AbilityData))]
        static class UIDecal_HandleAoEMove_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UIDecal __instance, AbilityData abilityData)
            {
                if (IsInCombat() && abilityData == null)
                {
                    UnitEntityData unit = Core.Mod.AttackIndicatorManager.Unit;
                    TurnController currentTurn = Core.Mod.RoundController.CurrentTurn;
                    if (currentTurn != null && currentTurn.Unit == unit && currentTurn.EnabledFiveFootStep)
                    {
                        GetMethodDel<UIDecal, Action<UIDecal, bool>>("SetHoverVisibility")
                            (__instance, unit.CanAttackWithWeapon(__instance.Unit, currentTurn.GetRemainingMovementRange()));
                    }
                    else
                    {
                        GetMethodDel<UIDecal, Action<UIDecal, bool>>("SetHoverVisibility")
                            (__instance, unit.CanAttackWithWeapon(__instance.Unit, 0f));
                    }

                    return false;
                }
                return true;
            }
        }

        // disable the attack indicator when the mouse hovering on an ability icon 
        [HarmonyPatch(typeof(ActionBarSlot), nameof(ActionBarSlot.Hover), typeof(bool))]
        static class ActionBarSlot_Hover_Patch
        {
            [HarmonyPostfix]
            static void Postfix(bool state)
            {
                if (IsInCombat())
                {
                    Core.Mod.AttackIndicatorManager.Disabled = state;
                }
            }
        }

        // disable the attack indicator when the mouse hovering on an ability icon 
        [HarmonyPatch(typeof(ActionBarIndexSlot), nameof(ActionBarSlot.Hover), typeof(bool))]
        static class ActionBarIndexSlot_Hover_Patch
        {
            [HarmonyPostfix]
            static void Postfix(bool state)
            {
                if (IsInCombat())
                {
                    Core.Mod.AttackIndicatorManager.Disabled = state;
                }
            }
        }

        // make Unit.Inspect() extension work
        [HarmonyPatch(typeof(InspectController), nameof(InspectController.HandleUnitRightClick), typeof(UnitEntityView))]
        static class UnitViewHandsEquipment_HandleEquipmentSlotUpdated_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // m_TooltipTrigger.SetObject(entityData);
                // ---------------- after  ----------------
                // m_TooltipTrigger.SetObject(entityData);
                // m_TooltipTrigger.ShowTooltipManual(true);
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<InspectController, TooltipTrigger>("m_TooltipTrigger")),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<TooltipTrigger, Action<TooltipTrigger, object>>(nameof(TooltipTrigger.SetObject))),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    List<CodeInstruction> patchingCodes = new List<CodeInstruction>()
                    {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<InspectController, TooltipTrigger>("m_TooltipTrigger")),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<TooltipTrigger, Action<TooltipTrigger, bool>>(nameof(TooltipTrigger.ShowTooltipManual))),
                    };
                    return codes.InsertRange(startIndex + findingCodes.Count, patchingCodes, true).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }
        }
    }
}
