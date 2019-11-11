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
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class CombatTrigger
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
                CodeInstruction[] findingCodes = new CodeInstruction[]
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
                    CodeInstruction[] patchingCodes = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Dup),
                        new CodeInstruction(OpCodes.Brtrue, codes.NewLabel(startIndex + findingCodes.Length - 1, il)),
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call,
                            new Func<UnitEntityData, bool>(IsAboutToAttackOrBeAttacked).Method)
                    };
                    return codes.InsertRange(startIndex + findingCodes.Length - 1, patchingCodes, true).Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod());
                    return codes;
                }
            }

            static bool IsAboutToAttackOrBeAttacked(UnitEntityData unit)
            {
                if (IsEnabled())
                {
                    // the unit is about to attack
                    if (unit.HasOffensiveCommand(command =>
                    {
                        UnitEntityData target = command.TargetUnit;
                        UnitState state;
                        return target.IsInGame && !(state = target.Descriptor.State).IsDead && !state.IsIgnoredByCombat;
                    }))
                        return true;

                    // the unit is about to be attacked &&
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
                                unit.Group.UpdateAttackFactionsCache();
                            }

                            if (unit.IsEnemy(attacker))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        // don't engage an enemy which is not "awake" (in game and not in fog of war)
        [HarmonyPatch(typeof(UnitCombatJoinController), "ShouldEngageEnemy", typeof(UnitEntityData), typeof(UnitEntityData))]
        static class UnitCombatJoinController_ShouldEngageEnemy_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitEntityData enemy, ref bool __result)
            {
                if (IsEnabled() && __result && !enemy.IsAwake)
                {
                    __result = false;
                }
            }
        }

        // stop time advanced during units' turn if any player's enemies are in combat
        [HarmonyPatch(typeof(UnitCombatLeaveController), "Tick")]
        static class UnitCombatLeaveController_Tick_Patch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                if (IsInCombat())
                {
                    if (!Mod.Core.Combat.HasEnemyInCombat)
                    {
                        Game.Instance.Player.Group.LeaveCombatTimer = 3f;
                    }
                    else if (!IsPassing())
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        // prevent unconscious units from instantly leaving combat
        // units remembered by any enemy cannot leave combat (regardless LOS)
        [HarmonyPatch(typeof(UnitCombatLeaveController), "ShouldLeaveCombat", typeof(UnitGroup))]
        static class UnitCombatLeaveController_ShouldLeaveCombat_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before 1 ----------------
                // if (group.All(unit => !unit.Descriptor.State.IsConscious))
                // ---------------- after 1  ----------------
                // if (group.All(unit => IsInCombat() ? unit.Descriptor.State.IsDead : !unit.Descriptor.State.IsConscious))
                // ---------------- before 2 ----------------
                // groupMember.HasLOS(enemy);
                // ---------------- after 2  ----------------
                // IsInCombat() ? true : groupMember.HasLOS(enemy)
                CodeInstruction[] findingCodes_1 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldsfld),
                    new CodeInstruction(OpCodes.Brtrue_S),
                    new CodeInstruction(OpCodes.Ldnull),
                    new CodeInstruction(OpCodes.Ldftn),
                    new CodeInstruction(OpCodes.Newobj)
                };
                CodeInstruction[] findingCodes_2 = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldloc_S),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<UnitEntityData, Func<UnitEntityData, UnitEntityData, bool>>(nameof(UnitEntityData.HasLOS)))
                };
                int startIndex_1 = codes.FindCodes(findingCodes_1);
                int startIndex_2 = codes.FindCodes(findingCodes_2);
                if (startIndex_1 >= 0 && startIndex_2 >= 0)
                {
                    return codes
                        .Replace(startIndex_2 + 2, new CodeInstruction(OpCodes.Call,
                            new Func<UnitEntityData, UnitEntityData, bool>(HasLOS).Method), true)
                        .Replace(startIndex_1 + 4, new CodeInstruction(OpCodes.Ldftn,
                            new Func<UnitEntityData, bool>(ShouldLeaveCombat).Method), true)
                        .Replace(startIndex_1 + 2, new CodeInstruction(OpCodes.Pop))
                        .Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod());
                    return codes;
                }
            }

            static bool ShouldLeaveCombat(UnitEntityData unit)
            {
                return IsInCombat() && PreventUnconsciousUnitLeavingCombat ? 
                    unit.Descriptor.State.IsDead : !unit.Descriptor.State.IsConscious;
            }

            static bool HasLOS(UnitEntityData groupMember, UnitEntityData enemy)
            {
                return IsInCombat() ? true : groupMember.HasLOS(enemy);
            }
        }

        // prevent unconscious units from instantly leaving combat
        [HarmonyPatch(typeof(UnitLifeController), "SetLifeState", typeof(UnitEntityData), typeof(UnitLifeState))]
        static class UnitLifeController_SetLifeState_Patch
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // unit.LeaveCombat();
                // ---------------- after  ----------------
                // if (!IsInCombat() || unit.Descriptor.State.LifeState == UnitLifeState.Dead)
                //     unit.LeaveCombat();
                CodeInstruction[] findingCodes = new CodeInstruction[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Callvirt,
                        GetMethodInfo<UnitEntityData, Action<UnitEntityData>>(nameof(UnitEntityData.LeaveCombat)))
                };
                int startIndex = codes.FindCodes(findingCodes);
                if (startIndex >= 0)
                {
                    return codes.Replace(startIndex + 1, new CodeInstruction(OpCodes.Call,
                        new Action<UnitEntityData>(LeaveCombat).Method), true).Complete();
                }
                else
                {
                    Core.FailedToPatch(MethodBase.GetCurrentMethod());
                    return codes;
                }
            }

            static void LeaveCombat(UnitEntityData unit)
            {
                if (!IsInCombat() || !PreventUnconsciousUnitLeavingCombat || unit.Descriptor.State.LifeState == UnitLifeState.Dead)
                    unit.LeaveCombat();
            }
        }
    }
}