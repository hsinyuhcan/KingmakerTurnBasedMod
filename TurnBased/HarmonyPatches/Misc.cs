using Harmony12;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.SettingsUI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using UnityEngine;
using static ModMaker.Utility.ReflectionCache;
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
                    CurrentTurn()?.CommandToggleFiveFootStep();
                    return false;
                }
                return true;
            }
        }

        // speed up iterative attacks
        [HarmonyPatch(typeof(UnitAttack), "OnTick")]
        static class UnitAttack_OnTick_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // m_AnimationsDuration / (float)m_AllAttacks.Count
                // ---------------- after  ----------------
                // ModifyDelay(m_AnimationsDuration / (float)m_AllAttacks.Count)
                CodeInstruction[] findingCodes = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<UnitAttack, float>("m_AnimationsDuration")),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<UnitAttack, List<AttackHandInfo>>("m_AllAttacks")),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<List<AttackHandInfo>, int>(nameof(List<AttackHandInfo>.Count)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Conv_R4),
                    new CodeInstruction(OpCodes.Div),
               };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    return codes.Insert(startIndex + findingCodes.Length, new CodeInstruction(OpCodes.Call,
                        new Func<float, float>(ModifyDelay).Method), true).Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod());
                    return codes;
                }
            }

            static float ModifyDelay(float delay)
            {
                return IsInCombat() && delay > MaxDelayBetweenIterativeAttacks ? MaxDelayBetweenIterativeAttacks : delay;
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
                if (IsInCombat() && __instance.Unit.IsInCombat && FlankingCountAllNearbyOpponents)
                {
                    __result = __instance.EngagedBy.Count > 1 && !__instance.Unit.Descriptor.State.Features.CannotBeFlanked;
                    return false;
                }
                return true;
            }
        }

        // forbid AI from trying to use full-round abilities when they don't have enough actions
        [HarmonyPatch(typeof(AiAction), nameof(AiAction.ScoreActor), typeof(DecisionContext))]
        static class AiAction_ScoreActor_Patch
        {
            [HarmonyPostfix]
            static void Postfix(DecisionContext context)
            {
                if (IsInCombat() && context.CurrentScore > 0f && (context.Ability?.RequireFullRoundAction ?? false))
                {
                    UnitEntityData unit = context.Target.Unit ?? context.Unit;
                    if (!unit.IsSurprising() && !unit.HasFullRoundAction())
                    {
                        context.CurrentScore = 0f;
                    }
                }
            }
        }

        // suppress auto pause on combat start
        [HarmonyPatch(typeof(AutoPauseController), nameof(AutoPauseController.HandlePartyCombatStateChanged), typeof(bool))]
        static class AutoPauseController_HandlePartyCombatStateChanged_Patch
        {

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                return codes.Patch(il, PreTranspiler, PostTranspiler);
            }

            static bool? PreTranspiler()
            {
                if (IsEnabled() && DoNotPauseOnCombatStart)
                {
                    bool pauseOnEngagement = SettingsRoot.Instance.PauseOnEngagement.CurrentValue;
                    SettingsRoot.Instance.PauseOnEngagement.CurrentValue = false;
                    return pauseOnEngagement;
                }
                return null;
            }

            static void PostTranspiler(bool? pauseOnEngagement)
            {
                if (pauseOnEngagement.HasValue)
                {
                    SettingsRoot.Instance.PauseOnEngagement.CurrentValue = pauseOnEngagement.Value;
                }
            }
        }
    }
}