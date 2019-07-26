using Harmony12;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
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
                    //__instance.AvoidanceDisabled = __instance.IsCharging;
                    __instance.SetFieldValue("m_ChargeAvoidanceFinishTime", TimeSpan.Zero);
                }
            }
        }

        // fix Charge ability sometimes stops by obstacles on the halfway
        [HarmonyPatch(typeof(ObstacleAnalyzer), nameof(ObstacleAnalyzer.TraceAlongNavmesh), typeof(Vector3), typeof(Vector3))]
        static class ObstacleAnalyzer_TraceAlongNavmesh_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(Vector3 end, ref Vector3 __result)
            {
                if (IsInCombat() && (Mod.Core.Combat.CurrentTurn?.Unit.View.AgentASP.IsCharging?? false))
                {
                    __result = end;
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