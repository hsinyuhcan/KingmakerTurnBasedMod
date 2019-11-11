using Harmony12;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class Stealth
    {
        // allow units remembered by enemies to enter stealth in combat
        [HarmonyPatch(typeof(UnitStealthController), nameof(UnitStealthController.ShouldBeInStealth), typeof(UnitEntityData))]
        static class UnitStealthController_ShouldBeInStealth_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // unit.Memory.Contains(unit) || unit.HasLOS(unit)
                // ---------------- after  ----------------
                // (IsEnabled() || unit.Memory.Contains(unit)) || unit.HasLOS(unit)
                CodeInstruction[] findingCodes = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_S),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitEntityData, UnitGroupMemory>(nameof(UnitEntityData.Memory)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<UnitGroupMemory, Func<UnitGroupMemory, UnitEntityData, bool>>(nameof(UnitGroupMemory.Contains))),
                    new CodeInstruction(OpCodes.Brtrue),
                    new CodeInstruction(OpCodes.Ldloc_S),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<UnitEntityData, Func<UnitEntityData, UnitEntityData, bool>>(nameof(UnitEntityData.HasLOS))),
                    new CodeInstruction(OpCodes.Brfalse)
                };
                int startIndex = codes.FindLastCodes(findingCodes);
                if (startIndex >= 0)
                {
                    CodeInstruction[] patchingCodes = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Call,
                            new Func<bool>(IsEnabled).Method),
                        new CodeInstruction(OpCodes.Brtrue, codes.NewLabel(startIndex + 5, il))
                    };
                    return codes.InsertRange(startIndex, patchingCodes, true).Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod().DeclaringType);
                    return codes;
                }
            }
        }

        // units in stealth won't be spotted by neutral units
        [HarmonyPatch(typeof(UnitStealthController), nameof(UnitStealthController.TickUnit), typeof(UnitEntityData))]
        static class UnitStealthController_TickUnit_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // !anotherUnit.IsEnemy(unit) && !unit.Stealth.InAmbush
                // ---------------- after  ----------------
                // (IsEnabled() ? !unit.CanAttack(anotherUnit) : !anotherUnit.IsEnemy(unit)) && !unit.Stealth.InAmbush
                CodeInstruction[] findingCodes = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_S),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<UnitEntityData, Func<UnitEntityData, UnitEntityData, bool>>(nameof(UnitEntityData.IsEnemy))),
                    new CodeInstruction(OpCodes.Brtrue),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<UnitEntityData, UnitStealth>(nameof(UnitEntityData.Stealth))),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitStealth, bool>(nameof(UnitStealth.InAmbush)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Brtrue)
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    CodeInstruction[] patchingCodes = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Call,
                            new Func<bool>(IsEnabled).Method),
                        new CodeInstruction(OpCodes.Brfalse, codes.NewLabel(startIndex, il)),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Ldloc_S, codes.Item(startIndex).operand),
                        new CodeInstruction(OpCodes.Callvirt,
                            GetMethodInfo<UnitEntityData, Func<UnitEntityData, UnitEntityData, bool>>(nameof(UnitEntityData.CanAttack))),
                        new CodeInstruction(OpCodes.Brtrue, codes.Item(startIndex + 3).operand),
                        new CodeInstruction(OpCodes.Br, codes.NewLabel(startIndex + 4, il))
                    };
                    return codes.InsertRange(startIndex, patchingCodes, true).Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod().DeclaringType);
                    return codes;
                }
            }
        }
    }
}