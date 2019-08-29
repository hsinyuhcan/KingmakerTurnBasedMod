using Kingmaker;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using System;
using System.Linq;
using TurnBased.Utility;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Controllers
{
    public class TurnController :
        IDisposable,
        IUnitCommandActHandler,
        IUnitCommandEndHandler,
        IUnitGetUpHandler
    {

        #region Fields & Events

        private bool _aiUsedFiveFootStep;
        private bool _enabledFiveFootStep;
        private UnitEntityData _delayTarget;

        public readonly UnitEntityData Unit;
        public readonly UnitCommands Commands;
        public readonly UnitCombatState CombatState;
        public readonly UnitCombatState.Cooldowns Cooldown;

        public event Action<UnitEntityData, UnitEntityData> OnDelay;
        public event Action<UnitEntityData> OnEnd;
        
        #endregion
        
        #region Properties

        public float MetersOfFiveFootStep => 5f * Feet.FeetToMetersRatio * DistanceOfFiveFootStep;

        public TurnStatus Status { get; private set; }

        public float TimeWaitedForIdleAI { get; private set; }

        public float TimeWaitedToEndTurn { get; private set; }

        public float TimeMoved { get; private set; }

        public float TimeMovedByFiveFootStep { get; private set; }

        public float MetersMovedByFiveFootStep { get; private set; }

        public bool EnabledFiveFootStep {
            get => _enabledFiveFootStep;
            private set {
                if (_enabledFiveFootStep != value)
                {
                    _enabledFiveFootStep = value;

                    if (!value && _aiUsedFiveFootStep && 
                        !Commands.Standard.IsStarted && Commands.Standard.ShouldUnitApproach && HasNormalMovement())
                    {
                        // don't cheat if the unit doesn't need it 
                        Cooldown.MoveAction += TimeMovedByFiveFootStep;
                        TimeMovedByFiveFootStep = 0f;
                    }
                }
            }
        }

        public bool EnabledFullAttack { get; private set; } = true;

        public bool ImmuneAttackOfOpportunityOnDisengage { get; private set; }

        #endregion

        public TurnController(UnitEntityData unit)
        {
            Unit = unit;
            Commands = unit.Commands;
            CombatState = unit.CombatState;
            Cooldown = CombatState.Cooldown;

            EventBus.Subscribe(this);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe(this);
        }

        #region Tick

        internal void Tick()
        {
            ImmuneAttackOfOpportunityOnDisengage = false;

            if (Status == TurnStatus.Preparing && (IsActed() || !Commands.Empty || !Unit.IsAbleToAct()))
            {
                Status = TurnStatus.Acting;
            }

            if (Status == TurnStatus.Acting && !ContinueActing())
            {
                ToEnd();
            }

            if (Status == TurnStatus.Delaying)
            {
                Delay();
            }
            else if (Status == TurnStatus.Ending && !ContinueWaiting())
            {
                End();
            }
        }

        internal void TickMovement(ref float deltaTime, bool isInForceMode)
        {
            if (isInForceMode)
            {
                // Charge, Overrun... etc
                TimeMoved += deltaTime;
                EnabledFiveFootStep = false;
            }
            else
            {
                // check remaining movement
                float remainingMovementTime = GetRemainingMovementTime();
                if (deltaTime >= remainingMovementTime)
                {
                    deltaTime = remainingMovementTime;
                    if (deltaTime > 0f)
                    {
                        // is going to finish a movement
                        OnMovementFinished();
                    }
                    else
                    {
                        // has no remaining movement
                        return;
                    }
                }

                // consume movement
                if (EnabledFiveFootStep)
                {
                    TimeMoved += deltaTime;
                    TimeMovedByFiveFootStep += deltaTime;
                    MetersMovedByFiveFootStep += deltaTime * Unit.CurrentSpeedMps;
                    EnabledFiveFootStep = HasFiveFootStep();
                    ImmuneAttackOfOpportunityOnDisengage = true;
                }
                else
                {
                    TimeMoved += deltaTime;
                    Cooldown.MoveAction += deltaTime;
                }
            }
        }

        private void OnMovementFinished()
        {
            if (Unit.IsDirectlyControllable && Unit.HasMoveAction())
            {
                if (EnabledFiveFootStep)
                {
                    if (PauseOnPlayerFinishFiveFoot)
                        Game.Instance.IsPaused = true;

                    if (AutoCancelActionsOnFiveFootStepFinish)
                        Unit.TryCancelCommands();
                }
                else
                {
                    if (PauseOnPlayerFinishFirstMove)
                        Game.Instance.IsPaused = true;

                    if (AutoCancelActionsOnFirstMoveFinish)
                        Unit.TryCancelCommands();
                }
            }
        }

        #endregion

        #region Process Control

        public void Start()
        {
            // ensure the cooldowns are cleared
            Cooldown.Clear();

            // update cooldowns of acting actions
            foreach (UnitCommand command in Commands.Raw.Where(command => command != null && command.IsActing()))
            {
                command.Executor.UpdateCooldowns(command);
            }

            // reset the counter of AOO - UnitCombatCooldownsController.TickOnUnit()
            if (CombatState.AttackOfOpportunityPerRound > 0 &&
                CombatState.AttackOfOpportunityCount <= CombatState.AttackOfOpportunityPerRound)
            {
                CombatState.AttackOfOpportunityCount = CombatState.AttackOfOpportunityPerRound;
                CombatState.DisengageAttackTargets.Clear();
            }

            // reset AI data - UnitCombatCooldownsController.TickOnUnit()
            CombatState.OnNewRound();
            EventBus.RaiseEvent<IUnitNewCombatRoundHandler>(handler => handler.HandleNewCombatRound(Unit));

            // reset AI data and trigger certain per-round buffs - UnitTicksController.TickNextRound()
            CombatState.AIData.TickRound();
            Unit.Logic.CallFactComponents<ITickEachRound>(logic => logic.OnNewRound());

            // update confusion effects
            new UnitConfusionController().Tick();

            // QoLs
            if (Unit.IsDirectlyControllable)
            {
                if (AutoTurnOffAIOnTurnStart)
                    Unit.IsAIEnabled = false;

                if (AutoSelectUnitOnTurnStart)
                    Unit.Select();

                if (AutoEnableFiveFootStepOnTurnStart && HasFiveFootStep())
                    EnabledFiveFootStep = true;

                if (AutoCancelActionsOnTurnStart)
                    Unit.TryCancelCommands();

                if (PauseOnPlayerTurnStart)
                    Game.Instance.IsPaused = true;
            }
            else
            {
                if (PauseOnNonPlayerTurnStart)
                    Game.Instance.IsPaused = true;
            }

            if (RerollPerceptionDiceAgainstStealthOncePerRound)
                Unit.CachedPerceptionRoll = 0;

            if (CameraScrollToCurrentUnit)
                Unit.ScrollTo();

            // toggle full attack
            TryAutoToggleFullAttack();

            // set turn status
            Status = Unit.IsDirectlyControllable ? TurnStatus.Preparing : TurnStatus.Acting;
        }

        private bool ContinueActing()
        {
            TryAutoToggleFullAttack();
            TryAutoToggleFiveFootStep();

            bool hasRunningAction = Commands.IsRunning() || Unit.View.IsGetUp;

            // check if the current unit can't do anything more in current turn
            if (!Unit.IsInCombat || !Unit.IsAbleToAct() ||
                (AutoEndTurnWhenActionsAreUsedUp && !hasRunningAction && GetRemainingTime() <= 0 && !HasExtraAction()))
            {
                return false;
            }
            // check if AI is idle and timeout
            else if (!Unit.IsDirectlyControllable || AutoEndTurnWhenPlayerIdle)
            {
                if (!hasRunningAction && !Unit.HasMotionThisTick)
                {
                    // delay after timeout
                    TimeWaitedForIdleAI += Game.Instance.TimeController.GameDeltaTime;
                    if (TimeWaitedForIdleAI > Math.Max(TimeToWaitForIdleAI, Game.Instance.TimeController.DeltaTime))
                    {
                        return false;
                    }
                }
                else
                {
                    TimeWaitedForIdleAI = 0f;
                }
            }

            return true;
        }

        private bool ContinueWaiting()
        {
            // wait for the current action finish
            if (!Commands.IsRunning() || Unit.View.IsGetUp)
            {
                // delay after finish
                TimeWaitedToEndTurn += Game.Instance.TimeController.GameDeltaTime;
                if (TimeWaitedToEndTurn > Math.Max(TimeToWaitForEndingTurn, Game.Instance.TimeController.DeltaTime))
                {
                    return false;
                }
            }
            else
            {
                TimeWaitedToEndTurn = 0f;
            }

            return true;
        }

        private void TryAutoToggleFullAttack()
        {
            if (EnabledFullAttack && !Unit.HasFullRoundAction() && !Unit.PreparedSpellCombat())
            {
                EnabledFullAttack = false;
            }
        }

        private void TryAutoToggleFiveFootStep()
        {
            if (EnabledFiveFootStep)
            {
                EnabledFiveFootStep = HasFiveFootStep();
            }
            else if (HasFiveFootStep())
            {
                // the unit can use 5-foot step
                if (!HasNormalMovement())
                {
                    // the unit has no remaining normal movement
                    EnabledFiveFootStep = true;
                }
                else if (!Unit.IsDirectlyControllable)
                {
                    UnitCommand command = Commands.Standard;
                    if (command != null && !command.IsStarted && command.Target != null &&
                        Unit.DistanceTo(command.Target.Point) < command.ApproachRadius + MetersOfFiveFootStep)
                    {
                        // the AI unit can approach target with 5-foot step
                        EnabledFiveFootStep = true;
                        _aiUsedFiveFootStep = true;
                    }
                }
            }
        }

        private void ToDealy(UnitEntityData targetUnit)
        {
            Status = TurnStatus.Delaying;
            _delayTarget = targetUnit;
        }

        private void Delay()
        {
            Cooldown.StandardAction = _delayTarget.GetTimeToNextTurn();
            Cooldown.MoveAction = 0f;
            Status = TurnStatus.Delayed;
            OnDelay(Unit, _delayTarget);
        }

        private void ToEnd()
        {
            Status = TurnStatus.Ending;
        }

        private void End()
        {
            if (Unit.IsDirectlyControllable ? PauseOnPlayerTurnEnd : PauseOnNonPlayerTurnEnd)
                Game.Instance.IsPaused = true;

            Cooldown.StandardAction = 6f;
            Cooldown.MoveAction = Math.Min(6f, Cooldown.MoveAction);
            Status = TurnStatus.Ended;
            OnEnd(Unit);
        }

        public void ForceToEnd()
        {
            // eat all possible actions
            MetersMovedByFiveFootStep = MetersOfFiveFootStep;
            Cooldown.StandardAction = TIME_STANDARD_ACTION;
            Cooldown.MoveAction = TIME_MOVE_ACTION;
            Cooldown.SwiftAction = TIME_SWIFT_ACTION;

            ToEnd();
        }

        public void ForceTickActivatableAbilities()
        {
            TurnStatus status = Status;
            Status = TurnStatus.Acting;
            new UnitActivatableAbilitiesController().TickOnUnit(Unit);
            Status = status;
        }

        #endregion

        #region Special Actions

        public bool CanToggleFullAttack()
        {
            return Unit.IsDirectlyControllable && (Status == TurnStatus.Preparing || Status == TurnStatus.Acting) &&
                Unit.HasFullRoundAction();
        }

        public bool CanToggleFiveFootStep()
        {
            return Unit.IsDirectlyControllable && (Status == TurnStatus.Preparing || Status == TurnStatus.Acting) &&
                (EnabledFiveFootStep ? HasNormalMovement() : HasFiveFootStep());
        }

        public bool CanDelay()
        {
            return Unit.IsDirectlyControllable && !IsActed() && !Commands.IsRunning();
        }

        public bool CanEndTurn()
        {
            return Unit.IsDirectlyControllable && (Status == TurnStatus.Preparing || Status == TurnStatus.Acting);
        }

        public void CommandToggleFullAttack()
        {
            if (CanToggleFullAttack())
            {
                EnabledFullAttack = !EnabledFullAttack;
            }
        }

        public void CommandToggleFiveFootStep()
        {
            if (CanToggleFiveFootStep())
            {
                EnabledFiveFootStep = !EnabledFiveFootStep;
            }
        }

        public void CommandDelay(UnitEntityData targetUnit)
        {
            if (CanDelay() && targetUnit != Unit)
            {
                ToDealy(targetUnit);
            }
        }

        public void CommandEndTurn()
        {
            if (CanEndTurn())
            {
                ToEnd();
            }
        }

        #endregion

        #region State

        public bool IsActed()
        {
            return TimeMoved > 0f || Cooldown.StandardAction > 0f || Cooldown.MoveAction > 0f || Cooldown.SwiftAction > 0f;
        }

        private bool ShouldRestrictFiveFootStep()
        {
            return !EnabledFiveFootStep && (TimeMoved > 0f || Unit.CurrentSpeedMps * TIME_MOVE_ACTION <= MetersOfFiveFootStep);
        }

        private bool ShouldRestrictNormalMovement()
        {
            return !_aiUsedFiveFootStep && MetersMovedByFiveFootStep > 0f;
        }

        public bool HasFiveFootStep()
        {
            return !ShouldRestrictFiveFootStep() && (MetersMovedByFiveFootStep < MetersOfFiveFootStep);
        }

        public bool HasNormalMovement()
        {
            return !ShouldRestrictNormalMovement() && GetRemainingTime() > 0f;
        }

        public bool HasMovement()
        {
            return HasFiveFootStep() || HasNormalMovement();
        }

        public bool HasExtraAction()
        {
            return (!AutoEndTurnExceptSwiftAction && Cooldown.SwiftAction == 0f) || 
                HasFiveFootStep() || Unit.HasFreeTouch() || Unit.PreparedSpellCombat() || Unit.PreparedSpellStrike();
        }

        public float GetRemainingTime()
        {
            return Math.Max(0f, Math.Min(6f, 
                TIME_MOVE_ACTION * ((!Unit.IsMoveActionRestricted() ? 2f : 1f) - (Unit.UsedStandardAction() ? 1f : 0f)) - 
                Cooldown.MoveAction));
        }

        public float GetRemainingMovementTime(bool total = false)
        {
            if (EnabledFiveFootStep)
            {
                return (MetersOfFiveFootStep - MetersMovedByFiveFootStep) / Unit.CurrentSpeedMps;
            }
            else if (!ShouldRestrictNormalMovement())
            {
                float time = GetRemainingTime();
                if (!total && time > TIME_MOVE_ACTION)
                    return time - TIME_MOVE_ACTION;
                else if (time > 0f)
                    return time;
            }
            return 0f;
        }

        public float GetRemainingMovementRange(bool total = false)
        {
            if (EnabledFiveFootStep)
            {
                return MetersOfFiveFootStep - MetersMovedByFiveFootStep;
            }
            else if (!ShouldRestrictNormalMovement())
            {
                float time = GetRemainingTime();
                if (!total && time > TIME_MOVE_ACTION)
                    return Unit.CurrentSpeedMps * (time - TIME_MOVE_ACTION);
                else if (time > 0f)
                    return Unit.CurrentSpeedMps * time;
            }
            return 0f;
        }

        #endregion

        #region Event Handlers

        // fix touch spell (disallow touch more than once in the same round)
        public void HandleUnitCommandDidAct(UnitCommand command)
        {
            if (command.Executor == Unit && (command.IsFreeTouch() || command.IsSpellstrikeAttack()))
            {
                UnitPartTouch unitPartTouch = command.Executor.Get<UnitPartTouch>();
                unitPartTouch?.SetAppearTime(unitPartTouch.AppearTime - 6f.Seconds());
            }
        }

        // return the move action if current unit attacked only once during a full attack
        public void HandleUnitCommandDidEnd(UnitCommand command)
        {
            if (command.Executor == Unit && command is UnitAttack unitAttack)
            {
                if (command.IsActed && !command.IsIgnoreCooldown && unitAttack.IsFullAttack && unitAttack.GetAttackIndex() == 1)
                {
                    Cooldown.MoveAction -= TIME_MOVE_ACTION;
                    if (!_aiUsedFiveFootStep && HasNormalMovement())
                    {
                        EnabledFiveFootStep = false;
                    }
                }
            }
        }

        // cost a move action when stand up
        public void HandleUnitWillGetUp(UnitEntityData unit)
        {
            if (unit == Unit)
            {
                Cooldown.MoveAction += TIME_MOVE_ACTION;
                Unit.TryCancelCommands();
            }
        }

        #endregion

        public enum TurnStatus
        {
            None, // shouldn't be here
            Preparing,
            Acting,
            Delaying,
            Delayed,
            Ending,
            Ended
        }
    }
}