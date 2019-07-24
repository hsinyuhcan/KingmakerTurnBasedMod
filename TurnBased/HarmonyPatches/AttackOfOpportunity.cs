using Harmony12;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using UnityEngine;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    internal static class AttackOfOpportunity
    {
        // fix sometimes the game won't consider a unit is moved even if it's moved, caused AOO inconsistent
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
                return IsInCombat() ? awakeUnit.View?.transform.position ?? awakeUnit.Position : awakeUnit.Position;
            }
        }

        // fix sometimes a unit can't make the AOO when the unit is threatening multiple targets
        // do not provoke attack of opportunity when moving using 5-foot step
        [HarmonyPatch(typeof(UnitCombatState), "ShouldAttackOnDisengage", typeof(UnitEntityData))]
        static class UnitCombatState_ShouldAttackOnDisengage_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, UnitEntityData target, ref bool __result, ref UnitEntityData __state)
            {
                if (IsInCombat())
                {
                    __state = __instance.LastTarget;
                    __instance.LastTarget = target;

                    if (target.IsCurrentUnit() && Core.Mod.RoundController.CurrentTurn.ImmuneAttackOfOpportunityOnDisengage)
                    {
                        __result = false;
                        return false;
                    }
                }
                return true;
            }

            [HarmonyPostfix]
            static void Postfix(UnitCombatState __instance, ref UnitEntityData __state)
            {
                if (IsInCombat())
                {
                    __instance.LastTarget = __state;
                }
            }
        }
    }
}
