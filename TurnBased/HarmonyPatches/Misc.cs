using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using TurnBased.Controllers;
using TurnBased.Utility;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class Misc
    {
        // delay one round for summoned units after casting a summon spell
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
                    if ((__instance.Context.SourceAbility?.IsFullRoundAction ?? false) == false ||
                        __instance.Initiator.Faction?.AssetGuid == "d75c5993785785d468211d9a1a3c87a6")
                    {
                        summonedUnit.Descriptor.RemoveFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff);
                    }
                    // add a round of freezing time to the units that summoned using a full-round spell
                    else
                    {
                        summonedUnit.AddBuffDuration(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff, 6f);
                        summonedUnit.SetBuffDuration(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff, 6f);
                    }
                }
            }
        }

        // speed up casting
        [HarmonyPatch(typeof(UnitUseAbility), nameof(UnitUseAbility.Init), typeof(UnitEntityData))]
        static class UnitUseAbility_Init_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitUseAbility __instance)
            {
                if (IsInCombat() && __instance.Executor.IsInCombat && CastingTimeOfFullRoundSpell != 1f)
                {
                    float castTime = __instance.GetCastTime();
                    if (castTime >= 6f)
                    {
                        __instance.SetCastTime(castTime * CastingTimeOfFullRoundSpell);
                    }
                }
            }
        }

        // make flanking don't consider opponents' command anymore
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.IsFlanked), MethodType.Getter)]
        static class UnitCombatState_get_IsFlanked_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, ref bool __result)
            {
                if (IsInCombat() && __instance.Unit.IsInCombat && FlankingCountAllOpponents)
                {
                    __result = __instance.EngagedBy.Count > 1 && !__instance.Unit.Descriptor.State.Features.CannotBeFlanked;
                    return false;
                }
                return true;
            }
        }

        // suppress auto pause on combat start
        [HarmonyPatch(typeof(AutoPauseController), nameof(AutoPauseController.HandlePartyCombatStateChanged), typeof(bool))]
        static class AutoPauseController_HandlePartyCombatStateChanged_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitCombatState __instance, bool inCombat, ref bool? __state)
            {
                if (IsEnabled() && DoNotPauseOnCombatStart && inCombat)
                {
                    __state = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
                    SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                }
            }

            [HarmonyPostfix]
            static void Postfix(UnitCombatState __instance, ref bool? __state)
            {
                if (__state.HasValue)
                {
                    SettingsRoot.Instance.PauseOnEngagement.CurrentValue = __state.Value;
                }
            }
        }

        // ** fix stealth check
        [HarmonyPatch(typeof(UnitStealthController), "TickUnit", typeof(UnitEntityData))]
        static class UnitStealthController_TickUnit_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitEntityData unit, ref float? __state)
            {
                if (IsInCombat() && !IsPassing())
                {
                    __state = Game.Instance.TimeController.GameDeltaTime;
                    Game.Instance.TimeController.SetGameDeltaTime(0f);

                    TurnController currentTurn = Mod.Core.Combat.CurrentTurn;
                    if (unit.IsCurrentUnit() &&
                        (currentTurn.WantEnterStealth != unit.Stealth.WantEnterStealth || currentTurn.NeedStealthCheck))
                    {
                        currentTurn.WantEnterStealth = unit.Stealth.WantEnterStealth;
                        currentTurn.NeedStealthCheck = false;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }

            [HarmonyPostfix]
            static void Postfix(ref float? __state)
            {
                if (__state.HasValue)
                {
                    Game.Instance.TimeController.SetGameDeltaTime(__state.Value);
                }
            }
        }
    }
}