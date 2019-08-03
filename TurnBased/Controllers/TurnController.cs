using Kingmaker;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using System;
using System.Linq;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Controllers
{
    public class TurnController
    {
        private UnitEntityData _delayTarget;

        public readonly UnitEntityData Unit;
        public readonly UnitCommands Commands;
        public readonly UnitCombatState CombatState;
        public readonly UnitCombatState.Cooldowns Cooldown;

        public event Action<UnitEntityData, UnitEntityData> OnDelay;
        public event Action<UnitEntityData> OnEnd;

        #region Properties

        public float MetersOfFiveFootStep => 5f * Feet.FeetToMetersRatio * DistanceOfFiveFootStep;

        public TurnStatus Status { get; private set; }

        public float TimeWaitedForIdleAI { get; private set; }

        public float TimeWaitedToEndTurn { get; private set; }

        public float TimeMoved { get; private set; }

        public float MetersMovedByFiveFootStep { get; private set; }

        public bool ImmuneAttackOfOpportunityOnDisengage { get; private set; }

        public bool EnabledFiveFootStep { get; private set; }

        public bool NeedStealthCheck { get; internal set; }

        public bool WantEnterStealth { get; internal set; }

        #endregion

        public TurnController(UnitEntityData unit)
        {
            Unit = unit;
            Commands = unit.Commands;
            CombatState = unit.CombatState;
            Cooldown = CombatState.Cooldown;
            WantEnterStealth = unit.Stealth.WantEnterStealth;

            Start();
        }

        #region Tick

        internal void Tick()
        {
            if (Status == TurnStatus.Preparing && IsActed())
            {
                Status = TurnStatus.Acting;
            }

            if ((Status == TurnStatus.Preparing || Status == TurnStatus.Acting) && !ContinueActing())
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
            if (Unit.IsMoving())
            {
                if (isInForceMode)
                {
                    TimeMoved += deltaTime;
                    EnabledFiveFootStep = false;
                }
                else
                {
                    float remainingMovementTime = GetRemainingMovementTime();
                    if (deltaTime >= remainingMovementTime)
                    {
                        deltaTime = remainingMovementTime;

                        if (deltaTime > 0f)
                            OnMovementFinished();
                    }

                    if (deltaTime > 0f)
                    {
                        NeedStealthCheck = true;

                        //CheckIfGoingToFinishMove(deltaTime);

                        if (EnabledFiveFootStep)
                        {
                            TimeMoved += deltaTime;
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
            }
        }

        #endregion

        #region Process Control

        private bool ContinueActing()
        {
            ImmuneAttackOfOpportunityOnDisengage = false;

            if (EnabledFiveFootStep)
            {
                if (!HasFiveFootStep())
                    EnabledFiveFootStep = false;
            }
            else
            {
                // auto enabled 5-foot step if possible when having no normal movement left
                if (!HasNormalMovement() && HasFiveFootStep())
                    EnabledFiveFootStep = true;
            }

            bool hasCommandRunning = Commands.IsRunning();

            // check if the current unit can't do anything more in current turn
            if (!Unit.IsInCombat || !Unit.CanPerformAction() ||
                (!hasCommandRunning && GetRemainingTime() <= 0 && !HasExtraAction()))
            {
                return false;
            }
            // check if AI is idle and timeout
            else if (!Unit.IsDirectlyControllable || AutoEndTurn)
            {
                if (!hasCommandRunning && !Unit.HasMotionThisTick)
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
            if (!Commands.IsRunning())
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

        private void Start()
        {
            // ensure the cooldowns are cleared
            Cooldown.Clear();

            // update cooldowns from pre-combat actions
            foreach (UnitCommand command in Commands.Raw.Where(command => command != null && command.IsActing()))
            {
                command.Executor.UpdateCooldowns(command);
            }

            // UnitCombatCooldownsController.TickOnUnit()
            CombatState.OnNewRound();
            EventBus.RaiseEvent<IUnitNewCombatRoundHandler>(handler => handler.HandleNewCombatRound(Unit));

            // UnitTicksController.TickNextRound()
            CombatState.AIData.TickRound();
            Unit.Logic.CallFactComponents<ITickEachRound>(logic => logic.OnNewRound());

            // UnitConfusionController.TickOnUnit() - set the effect of confution
            GetMethod<UnitConfusionController, Action<UnitConfusionController, UnitEntityData>>
                ("TickOnUnit")(new UnitConfusionController(), Unit);

            // reset the counter of AOO
            if (CombatState.AttackOfOpportunityPerRound > 0 &&
                CombatState.AttackOfOpportunityCount <= CombatState.AttackOfOpportunityPerRound)
            {
                CombatState.AttackOfOpportunityCount = CombatState.AttackOfOpportunityPerRound;
                CombatState.DisengageAttackTargets.Clear();
            }

            // QoLs
            bool isDirectlyControllable = Unit.IsDirectlyControllable;

            if (isDirectlyControllable)
            {
                if (AutoTurnOffAI)
                    Unit.IsAIEnabled = false;

                if (AutoSelectCurrentUnit)
                    Unit.Select();

                if (AutoEnableFiveFootStep && HasFiveFootStep())
                    EnabledFiveFootStep = true;

                if (AutoCancelActionsOnPlayerTurnStart)
                    Unit.TryCancelCommands();

                if (PauseOnPlayerTurnStart)
                    Game.Instance.IsPaused = true;
            }
            else
            {
                if (PauseOnNonPlayerTurnStart)
                    Game.Instance.IsPaused = true;
            }

            if (CameraScrollToCurrentUnit)
                Unit.ScrollTo();

            // set turn status
            if (isDirectlyControllable)
                Status = TurnStatus.Preparing;
            else
                Status = TurnStatus.Acting;
        }

        private void ToDealy(UnitEntityData targetUnit)
        {
            Status = TurnStatus.Delaying;
            _delayTarget = targetUnit;
        }

        private void Delay()
        {
            Cooldown.StandardAction = _delayTarget.GetTimeToNextTurn();
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

        #endregion

        #region Special Actions

        public bool CanToggleFiveFootStep()
        {
            return (EnabledFiveFootStep ? HasNormalMovement() : HasFiveFootStep()) &&
                (Status == TurnStatus.Preparing || Status == TurnStatus.Acting) &&
                (AllowCommandNonPlayerToPerformSpecialActions || Unit.IsDirectlyControllable);
        }

        public bool CanDelay()
        {
            return Status == TurnStatus.Preparing &&
                (AllowCommandNonPlayerToPerformSpecialActions || Unit.IsDirectlyControllable);
        }

        public bool CanEndTurn()
        {
            return (Status == TurnStatus.Preparing || Status == TurnStatus.Acting) &&
                (AllowCommandNonPlayerToPerformSpecialActions || Unit.IsDirectlyControllable);
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
            if (CanDelay() && targetUnit != Unit && targetUnit.CanPerformAction())
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
            return TimeMoved > 0f ||
                Cooldown.StandardAction > 0f ||
                Cooldown.MoveAction > 0f ||
                Cooldown.SwiftAction > 0f ||
                !Commands.Empty;
        }

        public bool ShouldRestrictFiveFootStep()
        {
            return TimeMoved > 0f || Unit.CurrentSpeedMps * TIME_MOVE_ACTION <= MetersOfFiveFootStep;
        }

        public bool ShouldRestrictNormalMovement()
        {
            return MetersMovedByFiveFootStep > 0f;
        }

        public bool HasFiveFootStep()
        {
            return (EnabledFiveFootStep || !ShouldRestrictFiveFootStep()) && (MetersMovedByFiveFootStep < MetersOfFiveFootStep);
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
            return (DoNotAutoEndTurnWhenHasSwiftAction && Cooldown.SwiftAction == 0f) || 
                HasFiveFootStep() || Unit.HasFreeTouch() || Unit.PreparedSpellCombat() || Unit.PreparedSpellStrike();
        }

        public float GetRemainingTime()
        {
            return Math.Max(0f, Math.Min(6f, 
                TIME_MOVE_ACTION * 
                (2f - (Unit.UsedStandardAction() ? 1f : 0f) - (Unit.IsMoveActionRestricted() ? 1f : 0f)) - 
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

        #region Misc

        private void OnMovementFinished()
        {
            if (Unit.IsDirectlyControllable && Unit.HasMoveAction())
            {
                if (EnabledFiveFootStep)
                {
                    if (PauseOnPlayerFinishFiveFoot)
                        Game.Instance.IsPaused = true;

                    if (AutoCancelActionsOnPlayerFinishFiveFoot)
                        Unit.TryCancelCommands();
                }
                else
                {
                    if (PauseOnPlayerFinishFirstMove)
                        Game.Instance.IsPaused = true;

                    if (AutoCancelActionsOnPlayerFinishFirstMove)
                        Unit.TryCancelCommands();
                }
            }
        }

        #endregion

        public enum TurnStatus
        {
            None, // shouldn't be
            Preparing,
            Acting,
            Delaying,
            Delayed,
            Ending,
            Ended
        }
    }
}
