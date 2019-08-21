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
                // (IgnoreMemory() || unit.Memory.Contains(unit)) || unit.HasLOS(unit)
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
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
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    List<CodeInstruction> patchingCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Call,
                            new Func<bool>(IgnoreMemory).Method),
                        new CodeInstruction(OpCodes.Brtrue, codes.NewLabel(startIndex + 5, il))
                    };
                    return codes.InsertRange(startIndex, patchingCodes, true).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static bool IgnoreMemory()
            {
                return IsEnabled();
            }
        }
    }
}