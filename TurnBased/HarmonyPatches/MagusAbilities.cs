using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using TurnBased.Utility;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class MagusAbilities
    {
        // fix Magus Spell Combat and Spellstrike
        [HarmonyPatch(typeof(MagusController), nameof(MagusController.HandleUnitCommandDidAct), typeof(UnitCommand))]
        static class MagusController_HandleUnitCommandDidAct_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitCommand command, ref float? __state)
            {
                if (IsInCombat() && command.Executor.IsInCombat)
                {
                    if (command.IsSpellCombat())
                    {
                        command.Executor.CombatState.Cooldown.MoveAction += TIME_MOVE_ACTION;
                    }

                    __state = command.TimeSinceStart;
                    command.SetTimeSinceStart(0f);
                }
            }

            [HarmonyPostfix]
            static void Postfix(UnitCommand command, ref float? __state)
            {
                if (__state.HasValue)
                {
                    command.SetTimeSinceStart(__state.Value);
                }
            }
        }

        // fix Magus Spell Combat (don't check the distance to target when ordering Spell Combat)
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

        // fix Magus Spell Combat (check MoveAction isteand of LastMoveTime)
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