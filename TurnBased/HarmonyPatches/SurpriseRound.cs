using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class SurpriseRound
    {
        // don't avoid joining the combat because of standard actions
        [HarmonyPatch(typeof(UnitEntityData), nameof(UnitEntityData.JoinCombat))]
        static class UnitEntityData_JoinCombat_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitEntityData __instance, ref UnitCommand __state)
            {
                if (IsEnabled())
                {
                    __state = __instance.Commands.Standard;
                    __instance.Commands.Raw[(int)UnitCommand.CommandType.Standard] = null;
                }
            }

            [HarmonyPostfix]
            static void Postfix(UnitEntityData __instance, ref UnitCommand __state)
            {
                if (__state != null)
                {
                    __instance.Commands.Raw[(int)UnitCommand.CommandType.Standard] = __state;
                }
            }
        }

        // restrict full attack out of combat
        [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.IsFullAttackRestrictedBecauseOfMoveAction), MethodType.Getter)]
        static class UnitCombatState_get_IsFullAttackRestrictedBecauseOfMoveAction_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, ref bool __result)
            {
                if (IsEnabled() && !Mod.Core.Combat.CombatInitialized)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        // allow units to join the combat when they are about to attack or be attacked
        [HarmonyPatch(typeof(UnitCombatJoinController), "TickUnit", typeof(UnitEntityData))]
        static class UnitCombatJoinController_TickUnit_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // unit.Group.IsInCombat
                // ---------------- after  ----------------
                // unit.Group.IsInCombat || IsAboutToAttackOrBeAttacked(unit)
                List<CodeInstruction> findingCodes = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetPropertyInfo<UnitEntityData, UnitGroup>(nameof(UnitEntityData.Group)).GetGetMethod(true)),
                    new CodeInstruction(OpCodes.Ldfld,
                        GetFieldInfo<UnitGroup, CountingGuard>(nameof(UnitGroup.IsInCombat))),
                    new CodeInstruction(OpCodes.Call),
                    new CodeInstruction(OpCodes.Stloc_0),
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    List<CodeInstruction> patchingCodes = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Brtrue, codes.NewLabel(startIndex + findingCodes.Count - 1, il)),
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call,
                            new Func<UnitEntityData, bool>(IsAboutToAttackOrBeAttacked).Method)
                    };
                    return codes.InsertRange(startIndex + findingCodes.Count - 1, patchingCodes, true).Complete();
                }
                else
                {
                    throw new Exception($"Failed to patch '{MethodBase.GetCurrentMethod().DeclaringType}'");
                }
            }

            static bool IsAboutToAttackOrBeAttacked(UnitEntityData unit)
            {
                if (IsEnabled())
                {
                    // the unit is about to attack
                    if (unit.HasOffensiveCommand(command =>
                    {
                        UnitState state = command.TargetUnit.Descriptor.State;
                        return !state.IsDead && !state.IsIgnoredByCombat;
                    }))
                        return true;

                    // the unit is about to be attacked
                    foreach (UnitCommand command in Game.Instance.State.AwakeUnits.SelectMany(enemy => enemy.GetAllCommands()))
                    {
                        if (command.IsOffensiveCommand() && command.TargetUnit == unit)
                        {
                            UnitEntityData attacker = command.Executor;

                            UnitMemoryController memory = Game.Instance.UnitMemoryController;
                            if (!unit.Descriptor.State.HasCondition(UnitCondition.Invisible) || 
                                attacker.Descriptor.IsSeeInvisibility ||
                                (attacker.Get<UnitPartBlindsense>()?.Reach(unit) ?? false))
                            {
                                memory.AddToMemory(attacker, unit);
                            }
                            memory.AddToMemory(unit, attacker);

                            if (unit.Descriptor.Faction.Neutral &&
                                (unit.Blueprint.GetComponent<UnitAggroFilter>()?.ShouldAggro(unit, attacker) ?? true))
                            {
                                unit.AttackFactions.Add(attacker.Descriptor.Faction);
                            }
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}