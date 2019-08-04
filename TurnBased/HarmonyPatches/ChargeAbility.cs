using Harmony12;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using ModMaker.Utility;
using Pathfinding;
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
    static class ChargeAbility
    {
        // fix Charge ability disables the obstacle detection for 1 second
        [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.IsCharging), MethodType.Setter)]
        static class UnitMovementAgent_set_IsCharging_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitMovementAgent __instance)
            {
                if (IsInCombat())
                {
                    __instance.SetChargeAvoidanceFinishTime(TimeSpan.Zero);
                }
            }
        }

        // fix Charge ability could be interrupted due to an unexpected obstacle
        [HarmonyPatch(typeof(ObstacleAnalyzer), nameof(ObstacleAnalyzer.TraceAlongNavmesh), typeof(Vector3), typeof(Vector3))]
        static class ObstacleAnalyzer_TraceAlongNavmesh_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(Vector3 end, ref Vector3 __result)
            {
                if (IsInCombat() && (Mod.Core.Combat.CurrentTurn?.Unit.View.AgentASP?.IsCharging ?? false))
                {
                    __result = end;
                    return false;
                }
                return true;
            }
        }

        // fix Charge ability could be interrupted (due to many reasons)
        [HarmonyPatch(typeof(UnitAttack), nameof(UnitAttack.Init), typeof(UnitEntityData))]
        static class UnitAttack_Init_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitAttack __instance, UnitEntityData executor)
            {
                if (IsInCombat() && (executor.View.AgentASP?.IsCharging ?? false))
                {
                    __instance.IgnoreCooldown(null);
                    __instance.IsCharge = true;
                }
            }
        }

        // fix Charge ability could be interrupted (due to many reasons)
        [HarmonyPatch(typeof(UnitEntityView), nameof(UnitEntityView.MoveTo),
            typeof(UnitCommand), typeof(Vector3), typeof(float), typeof(float), typeof(UnitEntityView))]
        static class UnitEntityView_MoveTo_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitEntityView __instance, UnitCommand command)
            {
                if (IsInCombat() && command is UnitAttack unitAttack && unitAttack.IsCharge)
                {
                    UnitMovementAgent agentASP = __instance.AgentASP;
                    if (agentASP)
                    {
                        bool isCharging = agentASP.IsCharging;
                        agentASP.IsCharging = true;
                        agentASP.ForcePath(new ForcedPath(new List<Vector3> { unitAttack.Executor.Position, unitAttack.TargetUnit.Position }));
                        agentASP.IsCharging = isCharging;

                        if (agentASP.IsReallyMoving)
                        {
                            agentASP.MaxSpeedOverride =
                                Math.Max(agentASP.MaxSpeedOverride ?? 0f, unitAttack.Executor.CombatSpeedMps * 2f);
                        }
                    }
                    return false;
                }
                return true;
            }
        }

        // fix Pounce doesn't take effect if Charge took a full round action
        [HarmonyPatch(typeof(UnitAttack), "InitAttacks")]
        static class UnitAttack_InitAttacks_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // !base.Executor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction
                // ---------------- after  ----------------
                // IgnoreMoveActionCheck(this) || !base.Executor.CombatState.IsFullAttackRestrictedBecauseOfMoveAction
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call,
                        GetPropertyInfo<UnitCommand, UnitEntityData>(nameof(UnitCommand.Executor)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitEntityData, UnitCombatState>(nameof(UnitEntityData.CombatState)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitCombatState, bool>(nameof(UnitCombatState.IsFullAttackRestrictedBecauseOfMoveAction)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Brtrue),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    List<CodeInstruction> patchingCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call,
                            new Func<UnitAttack, bool>(IgnoreMoveActionCheck).Method),
                        new CodeInstruction(OpCodes.Brtrue, codes.NewLabel(startIndex + findingCodes.Count, il)),
                    };
                    return codes.InsertRange(startIndex, patchingCodes, true).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static bool IgnoreMoveActionCheck(UnitAttack command)
            {
                return IsInCombat() && command.IsCharge && !command.Executor.IsMoveActionRestricted();
            }
        }
    }
}