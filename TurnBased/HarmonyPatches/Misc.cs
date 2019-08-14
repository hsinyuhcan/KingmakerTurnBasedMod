using Harmony12;
using Kingmaker;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic.Commands;
using TurnBased.Controllers;
using TurnBased.Utility;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class Misc
    {
        // toggle 5-foot step when right click on the ground
        [HarmonyPatch(typeof(ClickGroundHandler), nameof(ClickGroundHandler.OnClick), typeof(GameObject), typeof(Vector3), typeof(int))]
        static class ClickGroundHandler_OnClick_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(int button)
            {
                if (IsInCombat() && ToggleFiveFootStepOnRightClickGround && button == 1)
                {
                    Mod.Core.Combat.CurrentTurn?.CommandToggleFiveFootStep();
                    return false;
                }
                return true;
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
                if (IsEnabled() && __instance.Unit.IsInCombat && FlankingCountAllOpponents)
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