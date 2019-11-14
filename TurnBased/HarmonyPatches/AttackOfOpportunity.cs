using Harmony12;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class AttackOfOpportunity
    {
        // do not provoke attack of opportunity when moving using 5-foot step
        // fix sometimes a unit can't make an AoO if it's threatening multiple targets
        [HarmonyPatch(typeof(UnitCombatState), "ShouldAttackOnDisengage", typeof(UnitEntityData))]
        static class UnitCombatState_ShouldAttackOnDisengage_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitEntityData target, ref bool __result)
            {
                if (IsInCombat())
                {
                    if (target.IsCurrentUnit() && CurrentTurn().ImmuneAttackOfOpportunityOnDisengage)
                    {
                        __result = false;
                        return false;
                    }
                }
                return true;
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // if (m_EngagedUnits.Count <= 1)
                // ---------------- after  ----------------
                // if (IsInCombat())
                //     return true;
                // if (m_EngagedUnits.Count <= 1)
                CodeInstruction[] findingCodes = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<UnitCombatState, Dictionary<UnitEntityData, TimeSpan>>("m_EngagedUnits")),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<Dictionary<UnitEntityData, TimeSpan>, int>(nameof(Dictionary<UnitEntityData, TimeSpan>.Count)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Bgt),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    CodeInstruction[] patchingCodes = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Call, new Func<bool>(IsInCombat).Method),
                        new CodeInstruction(OpCodes.Brfalse, codes.NewLabel(startIndex, il)),
                        new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Ret),
                    };
                    return codes.InsertRange(startIndex, patchingCodes, true).Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod());
                    return codes;
                }
            }
        }
    }
}