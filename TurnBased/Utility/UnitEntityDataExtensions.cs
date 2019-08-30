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
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.Utility
{
    public static class UnitEntityDataExtensions
    {
        #region Buff

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

        #endregion

        #region Command & Action

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

        public static IEnumerable<UnitCommand> GetAllCommands(this UnitEntityData unit)
        {
            return unit.Commands.Raw.Concat(unit.Commands.Queue);
        }

        public static bool HasOffensiveCommand(this UnitEntityData unit, Predicate<UnitCommand> pred = null)
        {
            return unit.GetAllCommands().Any(command => command.IsOffensiveCommand() && (pred == null || pred(command)));
        }

        public static bool IsAbleToAct(this UnitEntityData unit)
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

            bool result = (state.CanAct || state.CanMove) && !state.HasCondition(UnitCondition.Prone);

            state.Prone.Active = isProne;
            if (exclusiveState == 1 || exclusiveState == 2)
            {
                animationManager.SetExclusiveState(exclusiveState);
            }

            return result;
        }

        public static bool HasFreeTouch(this UnitEntityData unit)
        {
            return unit.Get<UnitPartTouch>()?.IsCastedInThisRound ?? false;
        }

        public static bool PreparedSpellCombat(this UnitEntityData unit)
        {
            UnitPartMagus unitPartMagus = unit.Get<UnitPartMagus>();
            return unitPartMagus != null &&
                unitPartMagus.IsCastMagusSpellInThisRound &&
                unitPartMagus.LastCastedMagusSpellTime > unitPartMagus.LastAttackTime &&
                unitPartMagus.CanUseSpellCombatInThisRound;
        }

        public static bool PreparedSpellStrike(this UnitEntityData unit)
        {
            UnitPartMagus unitPartMagus = unit.Get<UnitPartMagus>();
            return unitPartMagus != null &&
                unitPartMagus.IsCastMagusSpellInThisRound &&
                unitPartMagus.LastCastedMagusSpellTime > unitPartMagus.LastAttackTime &&
                unitPartMagus.Spellstrike.Active &&
                (unitPartMagus.EldritchArcherSpell != null ||
                (unit.Get<UnitPartTouch>()?.Ability.Data is AbilityData abilityData &&
                unitPartMagus.IsSpellFromMagusSpellList(abilityData)));
        }

        #endregion

        #region Cooldown

        public static void UpdateCooldowns(this UnitEntityData unit, UnitCommand command)
        {
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

        public static float GetTimeToNextTurn(this UnitEntityData unit)
        {
            UnitCombatState.Cooldowns cooldown = unit.CombatState.Cooldown;
            return cooldown.Initiative + Math.Max(cooldown.StandardAction, cooldown.MoveAction);
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

        #endregion

        #region Pathfinding

        public static bool CanMoveThrough(this UnitEntityData unit, UnitEntityData target)
        {
            return unit != null && target != null && unit != target &&
                unit.StatusSwitch(MovingThroughApplyToPlayer, MovingThroughApplyToNeutralUnit, MovingThroughApplyToEnemy) &&
                ((MovingThroughFriendlyUnit && unit.IsAlly(target)) || (MovingThroughNonHostileUnit && !unit.IsEnemy(target))) &&
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
                return CurrentTurn().GetRemainingMovementRange(true) <
                    Math.Min(unit.DistanceTo(target) + minDistance, unit.DistanceTo(destination.Value));

            return false;
        }

        #endregion

        #region State

        public static bool IsCurrentUnit(this UnitEntityData unit)
        {
            return unit != null && unit == CurrentUnit();
        }

        public static bool IsSurprising(this UnitEntityData unit)
        {
            return Mod.Core.Combat.IsSurprising(unit);
        }

        public static bool IsSummoned(this UnitEntityData unit, out UnitEntityData caster)
        {
            caster = (unit.Descriptor.GetFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) as Buff)?.Context.MaybeCaster;
            return caster != null && caster != unit;
        }

        public static bool IsUnseen(this UnitEntityData unit)
        {
            return !Game.Instance.UnitGroups.Any(group => group.IsEnemy(unit) && group.Memory.ContainsVisible(unit));
        }

        public static T StatusSwitch<T>(this UnitEntityData unit, T player, T neutral, T enemy)
        {
            Player p = Game.Instance.Player;
            return p.ControllableCharacters.Contains(unit) ? player : unit.Group.IsEnemy(p.Group) ? enemy : neutral;
        }

        #endregion

        #region Targeting

        public static bool CanTarget(this UnitEntityData unit, UnitEntityData target, float radius,
            bool canTargetEnemies, bool canTargetFriends)
        {
            radius += target.View.Corpulence;
            return target != null && radius != 0f && unit.DistanceTo(target) < radius &&
                (unit.CanAttack(target) ? canTargetEnemies : canTargetFriends) &&
                (!CheckForObstaclesOnTargeting || !LineOfSightGeometry.Instance.HasObstacle(unit.EyePosition, target.Position, 0));
        }

        public static float GetAttackRadius(this UnitEntityData unit)
        {
            ItemEntityWeapon weapon = unit.GetFirstWeapon();
            return weapon != null ? unit.View.Corpulence + weapon.AttackRange.Meters : 0f;
        }

        public static float GetAttackApproachRadius(this UnitEntityData unit, UnitEntityData target)
        {
            float radius = unit.GetAttackRadius();
            return radius != 0f ? radius + target.View.Corpulence : 0f;
        }

        #endregion

        #region UI

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
            if (view == null || view.IsHighlighted || (highlight && DoNotMarkInvisibleUnit && !unit.IsVisibleForPlayer))
                return;

            UnitMultiHighlight highlighter = view.GetHighlighter();
            if (highlighter)
            {
                UIRoot uiRoot = UIRoot.Instance;
                Player player = Game.Instance.Player;
                highlighter.BaseColor =
                    // none
                    !highlight ? Color.clear :
                    // loot
                    unit.Descriptor.State.IsDead ? (unit.LootViewed ? uiRoot.VisitedLootColor : uiRoot.StandartUnitLootColor) :
                    // player, neutral, enemy
                    unit.StatusSwitch(uiRoot.AllyHighlightColor, uiRoot.NeutralHighlightColor, uiRoot.EnemyHighlightColor);
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