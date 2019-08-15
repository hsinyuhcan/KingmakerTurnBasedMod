using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Inspect;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.FogOfWar;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Utility
{
    internal static class UnitEntityDataExtensions
    {
        public static void AddBuffDuration(this UnitEntityData unit, BlueprintBuff blueprint, float duration)
        {
            if (unit.Descriptor.GetFact(blueprint) is Buff buff)
            {
                buff.EndTime += TimeSpan.FromSeconds(duration);
                unit.Descriptor.Buffs.UpdateNextEvent();
            }
        }

        public static void SetBuffDuration(this UnitEntityData unit, BlueprintBuff blueprint, float duration)
        {
            if (unit.Descriptor.GetFact(blueprint) is Buff buff)
            {
                buff.EndTime = Game.Instance.TimeController.GameTime + TimeSpan.FromSeconds(duration);
                unit.Descriptor.Buffs.UpdateNextEvent();
            }
        }

        public static void TryCancelCommands(this UnitEntityData unit)
        {
            if (!unit.Commands.IsRunning())
            {
                unit.HoldState = false;
                unit.Commands.InterruptAll();
                unit.CombatState.LastTarget = null;
                unit.CombatState.ManualTarget = null;
                unit.View.AgentASP?.Stop();
            }
        }

        internal static void UpdateCooldowns(this UnitEntityData unit, UnitCommand command)
        {
            if (unit.IsCurrentUnit())
                Mod.Core.Combat.CurrentTurn.NeedStealthCheck = true;

            if (!command.IsIgnoreCooldown)
            {
                UnitCombatState.Cooldowns cooldown = unit.CombatState.Cooldown;
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

        public static bool CanMoveThrough(this UnitEntityData unit, UnitEntityData target)
        {
            return unit != null && target != null && unit != target &&
                (!MovingThroughOnlyAffectPlayer || Game.Instance.Player.ControllableCharacters.Contains(unit)) &&
                (!MovingThroughOnlyAffectNonEnemies || !unit.Group.IsEnemy(Game.Instance.Player.Group)) &&
                ((MovingThroughNonEnemies && !unit.IsEnemy(target)) || (MovingThroughFriends && unit.IsAlly(target))) &&
                (!AvoidOverlapping || !unit.JustOverlapping(target));
        }

        private static bool JustOverlapping(this UnitEntityData unit, UnitEntityData target)
        {
            UnitMovementAgent agentASP = unit.View.AgentASP;
            Vector3? destination = agentASP.GetDestination();

            if (!destination.HasValue)
                return false;

            float minDistance = agentASP.Corpulence + target.View.AgentASP.Corpulence;

            // the destination is not where the unit is intended to stop at, so we have to step back
            destination = destination.Value - (destination.Value - unit.Position).normalized *
                (agentASP.IsCharging ? unit.GetAttackApproachRadius(target) : agentASP.ApproachRadius);

            // if the destination is going to overlap with target, forbid this behavior
            if (target.DistanceTo(destination.Value) < minDistance)
                return true;

            // if the unit doesn't have enough movement to go through the target, forbid it from going through
            if (unit.IsCurrentUnit() && !agentASP.GetIsInForceMode())
                return Mod.Core.Combat.CurrentTurn.GetRemainingMovementRange(true) <
                    Math.Min(unit.DistanceTo(target) + minDistance, unit.DistanceTo(destination.Value));

            return false;
        }

        public static bool CanPerformAction(this UnitEntityData unit)
        {
            UnitState state = unit.Descriptor.State;
            UnitAnimationManager animationManager = unit.View?.AnimationManager;
            bool isProne = state.Prone.Active;
            int exclusiveState = 0;

            state.Prone.Active = false;
            if (animationManager && ((exclusiveState = animationManager.GetExclusiveState()) == 1 || exclusiveState == 2))
            {
                animationManager.SetExclusiveState(0);
            }

            bool result = state.CanAct || state.CanMove;

            state.Prone.Active = isProne;
            if (exclusiveState == 1 || exclusiveState == 2)
            {
                animationManager.SetExclusiveState(exclusiveState);
            }

            return result;
        }

        public static bool CanTarget(this UnitEntityData unit, UnitEntityData target, float radius,
            bool canTargetEnemies, bool canTargetFriends)
        {
            radius += target.View.Corpulence;
            return target != null && radius != 0f && unit.DistanceTo(target) < radius &&
                (unit.CanAttack(target) ? canTargetEnemies : canTargetFriends) &&
                (!CheckForObstaclesOnTargeting || !LineOfSightGeometry.Instance.HasObstacle(unit.EyePosition, target.Position, 0));
        }

        public static float GetAttackApproachRadius(this UnitEntityData unit, UnitEntityData target)
        {
            float radius = unit.GetAttackRadius();
            return radius != 0f ? radius + target.View.Corpulence : 0f;
        }

        public static float GetAttackRadius(this UnitEntityData unit)
        {
            ItemEntityWeapon weapon = unit.GetFirstWeapon();
            return weapon != null ? unit.View.Corpulence + weapon.AttackRange.Meters : 0f;
        }

        public static float GetTimeToNextTurn(this UnitEntityData unit)
        {
            UnitCombatState.Cooldowns cooldown = unit.CombatState.Cooldown;
            return cooldown.Initiative + Math.Max(cooldown.StandardAction, cooldown.MoveAction);
        }

        public static bool HasFreeTouch(this UnitEntityData unit)
        {
            return unit.Get<UnitPartTouch>() is UnitPartTouch unitPartTouch && unitPartTouch.IsCastedInThisRound;
        }

        public static bool HasFullRoundAction(this UnitEntityData unit)
        {
            return !unit.UsedStandardAction() && !unit.UsedOneMoveAction();
        }

        public static bool HasStandardAction(this UnitEntityData unit)
        {
            return !unit.UsedStandardAction() && !unit.UsedTwoMoveAction();
        }

        public static bool HasMoveAction(this UnitEntityData unit)
        {
            return !unit.UsedStandardAction() ? !unit.UsedTwoMoveAction() : !unit.UsedOneMoveAction();
        }

        public static bool UsedStandardAction(this UnitEntityData unit)
        {
            return unit.CombatState.Cooldown.StandardAction > 0f || unit.Descriptor.State.HasCondition(UnitCondition.Nauseated);
        }

        public static bool UsedOneMoveAction(this UnitEntityData unit)
        {
            return unit.CombatState.Cooldown.MoveAction > 0f || unit.IsMoveActionRestricted();
        }

        public static bool UsedTwoMoveAction(this UnitEntityData unit)
        {
            return unit.CombatState.Cooldown.MoveAction > (unit.IsMoveActionRestricted() ? 0f : TIME_MOVE_ACTION);
        }

        public static bool IsMoveActionRestricted(this UnitEntityData unit)
        {
            return unit.IsSurprising() || unit.Descriptor.State.HasCondition(UnitCondition.Staggered);
        }

        public static bool PreparedSpellCombat(this UnitEntityData unit)
        {
            return unit.Get<UnitPartMagus>() is UnitPartMagus unitPartMagus &&
                unitPartMagus.IsCastMagusSpellInThisRound &&
                unitPartMagus.LastCastedMagusSpellTime > unitPartMagus.LastAttackTime &&
                unitPartMagus.CanUseSpellCombatInThisRound;
        }

        public static bool PreparedSpellStrike(this UnitEntityData unit)
        {
            return unit.Get<UnitPartMagus>() is UnitPartMagus unitPartMagus &&
                unitPartMagus.IsCastMagusSpellInThisRound &&
                unitPartMagus.LastCastedMagusSpellTime > unitPartMagus.LastAttackTime &&
                unitPartMagus.Spellstrike.Active &&
                (unitPartMagus.EldritchArcherSpell != null ||
                (unit.Get<UnitPartTouch>()?.Ability.Data is AbilityData abilityData &&
                unitPartMagus.IsSpellFromMagusSpellList(abilityData)));
        }

        public static IEnumerable<UnitCommand> GetAllCommands(this UnitEntityData unit)
        {
            return unit.Commands.Raw.Concat(unit.Commands.Queue);
        }

        public static bool HasOffensiveCommand(this UnitEntityData unit, Predicate<UnitCommand> pred = null)
        {
            return unit.GetAllCommands().Any(command => command.IsOffensiveCommand() && (pred == null || pred(command)));
        }

        public static bool IsCurrentUnit(this UnitEntityData unit)
        {
            return unit != null && unit == Mod.Core.Combat.CurrentTurn?.Unit;
        }

        public static bool IsSurprising(this UnitEntityData unit)
        {
            return Mod.Core.Combat.IsSurprising(unit);
        }

        public static bool IsUnseen(this UnitEntityData unit)
        {
            return !Game.Instance.UnitGroups.Any(group => group.IsEnemy(unit) && group.Memory.ContainsVisible(unit));
        }

        public static bool IsMoving(this UnitEntityData unit)
        {
            UnitEntityView view = unit?.View;
            return view != null &&
                (view.AnimationManager == null || !view.AnimationManager.IsPreventingMovement) &&
                !view.IsCommandsPreventMovement &&
                view.AgentASP.IsReallyMoving;
        }

        #region Carama And View Functions

        public static void Inspect(this UnitEntityData unit)
        {
            if (unit?.View != null && InspectUnitsHelper.IsInspectAllow(unit))
            {
                PlayerUISettings uiSettings = Game.Instance.Player.UISettings;
                bool showInspect = uiSettings.ShowInspect;
                uiSettings.ShowInspect = true;
                EventBus.RaiseEvent<IUnitClickUIHandler>(h => h.HandleUnitRightClick(unit.View));
                uiSettings.ShowInspect = showInspect;
            }
        }

        public static void SetHighlight(this UnitEntityData unit, bool highlight)
        {
            UnitEntityView view = unit?.View;
            if (view == null || view.IsHighlighted)
                return;

            if (highlight && DoNotMarkInvisibleUnit && !unit.IsVisibleForPlayer)
                return;

            UnitMultiHighlight highlighter = view.GetHighlighter();
            if (highlighter != null)
            {
                UIRoot uiRoot = UIRoot.Instance;
                Player player = Game.Instance.Player;
                highlighter.BaseColor =
                    !highlight ?
                    Color.clear :
                    unit.Descriptor.State.IsDead ?
                    (unit.LootViewed ? uiRoot.VisitedLootColor : uiRoot.StandartUnitLootColor) :
                    player.ControllableCharacters.Contains(unit) ?
                    uiRoot.AllyHighlightColor :
                    unit.Group.IsEnemy(player.Group) ?
                    uiRoot.EnemyHighlightColor :
                    uiRoot.NeutralHighlightColor;
            }
        }

        public static void Select(this UnitEntityData unit)
        {
            if (unit != null && unit.IsDirectlyControllable)
            {
                Game.Instance.UI.SelectionManager.SelectUnit(unit.View);
            }
        }

        public static void ScrollTo(this UnitEntityData unit)
        {
            if (unit != null && !unit.IsHiddenBecauseDead && unit.IsViewActive &&
                (!DoNotMarkInvisibleUnit || unit.IsVisibleForPlayer))
            {
                Game.Instance.UI.GetCameraRig().ScrollTo(unit.Position);
            }
        }

        #endregion
    }
}
