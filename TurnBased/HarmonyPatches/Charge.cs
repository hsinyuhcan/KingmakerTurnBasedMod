using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.View;
using Pathfinding;
using System;
using System.Collections.Generic;
using TurnBased.Utility;
using UnityEngine;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class Charge
    {
        // fix Charge ability disables the obstacle detection for 1 second
        [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.IsCharging), MethodType.Setter)]
        static class UnitMovementAgent_set_IsCharging_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitMovementAgent __instance)
            {
                if (IsEnabled())
                {
                    __instance.SetChargeAvoidanceFinishTime(TimeSpan.Zero);
                }
            }
        }

        // fix Charge ability could be interrupted (because of many reasons)
        [HarmonyPatch(typeof(AbilityCustomCharge), nameof(AbilityCustomCharge.Deliver), typeof(AbilityExecutionContext), typeof(TargetWrapper))]
        static class AbilityCustomCharge_Deliver_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(AbilityExecutionContext context, TargetWrapper targetWrapper, ref IEnumerator<AbilityDeliveryTarget> __result)
            {
                if (IsEnabled())
                {
                    __result = Deliver(context, targetWrapper);
                    return false;
                }
                return true;
            }

            public static IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
            {
                UnitEntityData target = targetWrapper.Unit;
                if (target == null)
                {
                    UberDebug.LogError("Target unit is missing");
                    yield break;
                }

                UnitEntityData caster = context.Caster;
                if (caster.GetThreatHand() == null)
                {
                    UberDebug.LogError("Invalid caster's weapon");
                    yield break;
                }

                UnitMovementAgent agentASP = caster.View.AgentASP;

                caster.View.StopMoving();
                agentASP.IsCharging = true;
                agentASP.ForcePath(new ForcedPath(new List<Vector3> { caster.Position, target.Position }));
                caster.Descriptor.State.IsCharging = true;
                caster.Descriptor.AddBuff(BlueprintRoot.Instance.SystemMechanics.ChargeBuff, context, 1.Rounds().Seconds);
                UnitAttack unitAttack = new UnitAttack(target);
                unitAttack.Init(caster);

                float timeSinceStart = 0f;
                while (unitAttack.ShouldUnitApproach)
                {
                    timeSinceStart += Game.Instance.TimeController.GameDeltaTime;
                    if (timeSinceStart > 6f)
                    {
                        UberDebug.Log("Charge: timeSinceStart > 6f");
                        yield break;
                    }
                    else if (caster.GetThreatHand() == null)
                    {
                        UberDebug.Log("Charge: caster.GetThreatHand() == null");
                        yield break;
                    }
                    else if (!caster.Descriptor.State.CanMove)
                    {
                        UberDebug.Log("Charge: !caster.Descriptor.State.CanMove");
                        yield break;
                    }
                    else if (!(bool)agentASP)
                    {
                        UberDebug.Log("Charge: !(bool)caster.View.AgentASP");
                        yield break;
                    }
                    else if (!agentASP.IsReallyMoving)
                    {
                        agentASP.ForcePath(new ForcedPath(new List<Vector3> { caster.Position, target.Position }));
                        if (!agentASP.IsReallyMoving)
                        {
                            UberDebug.Log("Charge: !caster.View.AgentASP.IsReallyMoving");
                            yield break;
                        }
                    }

                    agentASP.MaxSpeedOverride = Math.Max(agentASP.MaxSpeedOverride ?? 0f, caster.CombatSpeedMps * 2f);
                    yield return null;
                }

                caster.View.StopMoving();
                unitAttack.IgnoreCooldown(null);
                unitAttack.IsCharge = true;
                caster.Commands.AddToQueueFirst(unitAttack);
            }
        }

        // don't ignore obstacles when charging
        [HarmonyPatch(typeof(UnitMovementAgent), "ChargingAvoidance", MethodType.Getter)]
        static class UnitMovementAgent_ChargingAvoidance_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(ref bool __result)
            {
                if (IsInCombat() && AvoidOverlappingOnCharge)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}