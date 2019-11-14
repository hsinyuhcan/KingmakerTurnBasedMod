using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class Magus
    {
        // fix Magus Spell Combat and Spellstrike (Spell Combat consumes one more move action after attack)
        [HarmonyPatch(typeof(MagusController), nameof(MagusController.HandleUnitCommandDidAct), typeof(UnitCommand))]
        static class MagusController_HandleUnitCommandDidAct_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitCommand command)
            {
                if (IsInCombat() && command.Executor.IsInCombat && command.IsSpellCombatAttack())
                {
                    command.Executor.CombatState.Cooldown.MoveAction += TIME_MOVE_ACTION;
                }
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> codes, ILGenerator il)
            {
                // ---------------- before ----------------
                // unitUseAbility.TimeSinceStart
                // ---------------- after  ----------------
                // GetTimeSinceStart(unitUseAbility)
                return codes
                    .ReplaceAll(
                        new CodeInstruction(OpCodes.Callvirt,
                            GetPropertyInfo<UnitCommand, float>(nameof(UnitCommand.TimeSinceStart)).GetGetMethod()),
                        new CodeInstruction(OpCodes.Call,
                            new Func<UnitCommand, float>(GetTimeSinceStart).Method),
                        true)
                    .Complete();
            }

            static float GetTimeSinceStart(UnitCommand command)
            {
                return (IsInCombat() && command.Executor.IsInCombat) ? 0f : command.TimeSinceStart;
            }
        }

        // fix Magus Spell Combat (don't check the distance from the target when commanding)
        [HarmonyPatch(typeof(MagusController), nameof(MagusController.HandleUnitRunCommand), typeof(UnitCommand))]
        static class MagusController_HandleUnitRunCommand_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCommand command)
            {
                if (IsInCombat() && command.Executor.IsInCombat)
                {
                    if (command is UnitAttack unitAttack)
                    {
                        if (command.Executor.PreparedSpellCombat())
                        {
                            UnitPartMagus unitPartMagus = command.Executor.Get<UnitPartMagus>();
                            unitAttack.IgnoreCooldown(unitPartMagus.AttackIgnoringCooldownBeforeTime);
                            unitPartMagus.Owner.AddBuff(
                                BlueprintRoot.Instance.SystemMechanics.MagusSpellCombatBuff,
                                unitPartMagus.Owner.Unit, 1.Rounds().Seconds, null);
                        }
                        else if (command.Executor.PreparedSpellStrike())
                        {
                            UnitPartMagus unitPartMagus = command.Executor.Get<UnitPartMagus>();
                            unitAttack.IgnoreCooldown(unitPartMagus.AttackIgnoringCooldownBeforeTime);
                            unitAttack.IsSingleAttack = true;
                        }
                    }
                    return false;
                }
                return true;
            }
        }

        // fix Magus Spell Combat (check MoveAction instead of LastMoveTime)
        [HarmonyPatch(typeof(UnitPartMagus), nameof(UnitPartMagus.IsSpellCombatThisRoundAllowed), typeof(bool))]
        static class UnitPartMagus_IsSpellCombatThisRoundAllowed_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitPartMagus __instance, bool checkMovement, ref bool __result)
            {
                if (IsInCombat() && __instance.Owner.Unit.IsInCombat)
                {
                    __result = (__instance.EldritchArcher ? 
                        __instance.IsRangedWeapon(__instance.Owner.Unit.GetFirstWeapon()) :
                        __instance.HasOneHandedMeleeWeaponAndFreehand(__instance.Owner)) &&
                        Game.Instance.TimeController.GameTime - __instance.LastSpellCombatOpportunityTime < 1.Rounds().Seconds &&
                        (!checkMovement || __instance.Owner.Unit.HasMoveAction());
                    return false;
                }
                return true;
            }
        }
    }
}