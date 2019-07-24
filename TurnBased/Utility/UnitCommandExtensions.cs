using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using ModMaker.Utility;
using System;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Utility
{
    public static class UnitCommandExtensions
    {
        public static int GetAttackIndex(this UnitAttack command)
        {
            return command.GetFieldValue<UnitAttack, int>("m_AttackIndex");
        }

        public static bool IsFullAttack(this UnitCommand command)
        {
            return command is UnitAttack unitAttack && unitAttack.IsFullAttack;
        }

        public static bool IsFullRoundSpell(this UnitCommand command)
        {
            return command is UnitUseAbility unitUseAbility &&
                unitUseAbility.Spell != null && unitUseAbility.Spell.RequireFullRoundAction;
        }

        public static bool IsFullRoundAction(this UnitCommand command)
        {
            return command.IsFullAttack() || command.IsFullRoundSpell();
        }

        public static bool IsFreeTouch(this UnitCommand command)
        {
            if (command is UnitUseAbility unitUseAbility)
            {
                UnitPartTouch unitPartTouch = command.Executor.Get<UnitPartTouch>();
                if (unitPartTouch != null &&
                    unitPartTouch.IsCastedInThisRound &&
                    unitUseAbility.Spell == unitPartTouch.Ability.Data)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsSpellCombat(this UnitCommand command)
        {
            return command is UnitAttack &&
                command.IsIgnoreCooldown &&
                command.Executor.Descriptor.HasFact(BlueprintRoot.Instance.SystemMechanics.MagusSpellCombatBuff);
        }

        public static bool IsSpellStrike(this UnitCommand command)
        {
            return command is UnitAttack &&
                command.IsIgnoreCooldown &&
                command.Executor.Descriptor.HasFact(BlueprintRoot.Instance.SystemMechanics.MagusSpellStrikeBuff);
        }

        internal static void UpdateCooldowns(this UnitCommand command)
        {
            if (command.Executor.IsCurrentUnit())
                Core.Mod.RoundController.CurrentTurn.NeedStealthCheck = true;

            if (!command.IsIgnoreCooldown)
            {
                UnitCombatState.Cooldowns cooldown = command.Executor.CombatState.Cooldown;
                switch (command.Type)
                {
                    case UnitCommand.CommandType.Free:
                        break;
                    case UnitCommand.CommandType.Move:
                        cooldown.MoveAction += TIME_MOVE_ACTION;
                        break;
                    case UnitCommand.CommandType.Standard:
                        cooldown.StandardAction += TIME_STANDARD_ACTION;
                        if (command.IsFullRoundAction())
                            cooldown.MoveAction += TIME_MOVE_ACTION;
                        break;
                    case UnitCommand.CommandType.Swift:
                        cooldown.SwiftAction += TIME_SWIFT_ACTION;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
