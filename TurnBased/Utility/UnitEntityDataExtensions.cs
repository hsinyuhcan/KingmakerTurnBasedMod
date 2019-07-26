using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Inspect;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.Visual;
using ModMaker.Utility;
using System;
using System.Linq;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Utility
{
    internal static class UnitEntityDataExtensions
    {
        public static void CancelCommands(this UnitEntityData unit)
        {
            unit.HoldState = false;
            unit.Commands.InterruptAll(command => !command.IsStarted);
            unit.CombatState.LastTarget = null;
            unit.CombatState.ManualTarget = null;
        }

        public static bool CanMoveThrough(this UnitEntityData unit, UnitEntityData target)
        {
            return unit != null && target != null &&
                (!MovingThroughOnlyAffectPlayer || unit.IsPlayerFaction) &&
                (!MovingThroughOnlyAffectNonEnemies || !unit.IsPlayersEnemy) &&
                ((MovingThroughNonEnemies && !unit.IsEnemy(target)) || (MovingThroughFriends && unit.IsAlly(target)));
        }

        public static bool CanPerformAction(this UnitEntityData unit)
        {
            UnitState state = unit.Descriptor.State;
            return (state.CanAct || state.CanMove) && unit.View != null && !unit.View.IsGetUp;
        }

        public static bool CanAttackWithWeapon(this UnitEntityData unit, UnitEntityData target, float movement)
        {
            WeaponSlot hand = unit?.GetFirstWeaponSlot();
            return hand != null && target != null && unit.IsEnemy(target) &&
                unit.DistanceTo(target) < unit.View.Corpulence + target.View.Corpulence + hand.Weapon.AttackRange.Meters + movement;
        }

        public static float GetAttackRange(this UnitEntityData unit)
        {
            WeaponSlot hand = unit.GetFirstWeaponSlot();
            if (hand != null)
            {
                float meters = hand.Weapon.AttackRange.Meters;
                return unit.View.Corpulence + meters;
            }
            else
            {
                return 0f;
            }
        }

        public static WeaponSlot GetFirstWeaponSlot(this UnitEntityData unit)
        {
            if (unit.Body.PrimaryHand.MaybeWeapon != null)
            {
                return unit.Body.PrimaryHand;
            }
            if (unit.Body.SecondaryHand.MaybeWeapon != null)
            {
                return unit.Body.SecondaryHand;
            }
            return unit.Body.AdditionalLimbs.FirstOrDefault(limb => limb.MaybeWeapon != null);
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

        public static bool IsCurrentUnit(this UnitEntityData unit)
        {
            return unit != null && unit == Mod.Core.Combat.CurrentTurn?.Unit;
        }

        public static bool IsSurprising(this UnitEntityData unit)
        {
            return Mod.Core.Combat.IsSurprising(unit);
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

            UnitMultiHighlight highlighter = view.GetFieldValue<UnitEntityView, UnitMultiHighlight>("m_Highlighter");
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
                    uiRoot.NaturalHighlightColor;
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
            if (unit != null && !unit.IsHiddenBecauseDead && unit.IsViewActive)
            {
                Game.Instance.UI.GetCameraRig().ScrollTo(unit.Position);
                //Game.Instance.CameraController?.Follower?.Follow(_unit);
            }
        }

        #endregion
    }
}
