using Harmony12;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Components;
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
    static class Summon
    {
        // freeze the summoned unit for one round when it's summoned by a full-round spell
        [HarmonyPatch(typeof(RuleSummonUnit), nameof(RuleSummonUnit.OnTrigger), typeof(RulebookEventContext))]
        static class RuleSummonUnit_OnTrigger_Patch
        {
            [HarmonyPostfix]
            static void Postfix(RuleSummonUnit __instance)
            {
                if (IsEnabled() && __instance.SummonedUnit is UnitEntityData summonedUnit)
                {
                    // don't change RangedLegerdemainUnit
                    if (summonedUnit.Blueprint.AssetGuid == "661093277286dd5459cd825e0205f908")
                    {
                        return;
                    }
                    
                    // remove the freezing time when it's not summoned by a full round spell or it's summoned by a trap
                    if (!(__instance.Context?.SourceAbilityContext?.Ability.RequireFullRoundAction ?? false) ||
                        __instance.Initiator.Faction?.AssetGuid == "d75c5993785785d468211d9a1a3c87a6")
                    {
                        summonedUnit.Descriptor.RemoveFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff);
                    }
                    // add a round of duration and freezing time to the units that summoned using a full-round spell
                    else
                    {
                        summonedUnit.AddBuffDuration(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff, 6f);
                        summonedUnit.SetBuffDuration(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff, 6f);
                    }
                }
            }
        }

        // remove the delay from dismembering summoned units
        [HarmonyPatch(typeof(SummonedUnitBuff), nameof(SummonedUnitBuff.OnRemoved))]
        static class SummonedUnitBuff_OnRemoved_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // TimeSpan delay = (dismemberUnitFX = null) ? 6f.Seconds() : dismemberUnitFX.Delay.Seconds();
                // ---------------- after  ----------------
                // TimeSpan delay = (dismemberUnitFX = null) ? 6f.Seconds() : dismemberUnitFX.Delay.Seconds();
                // delay = GetDelay(delay);
                CodeInstruction[] findingCodes = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldc_R4, 6f),
                    new CodeInstruction(OpCodes.Call,
                        GetMethodInfo<Func<float, TimeSpan>>(typeof(TimeSpanExtension), nameof(TimeSpanExtension.Seconds))),
                    new CodeInstruction(OpCodes.Stloc_2),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    CodeInstruction[] patchingCodes = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Ldloc_2),
                        new CodeInstruction(OpCodes.Call,
                            new Func<TimeSpan, TimeSpan>(GetDelay).Method),
                        new CodeInstruction(OpCodes.Stloc_2),
                    };
                    return codes.InsertRange(startIndex + findingCodes.Length, patchingCodes, true).Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod());
                    return codes;
                }
            }

            static TimeSpan GetDelay(TimeSpan delay)
            {
                return IsEnabled() ? 0f.Seconds() : delay;
            }
        }
    }
}