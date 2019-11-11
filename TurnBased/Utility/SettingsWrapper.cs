using Kingmaker.Utility;
using static TurnBased.Main;

namespace TurnBased.Utility
{
    public static class SettingsWrapper
    {
        #region Constants

        // actions
        public const float TIME_STANDARD_ACTION = 6f;
        public const float TIME_MOVE_ACTION = 3f;
        public const float TIME_SWIFT_ACTION = 6f;

        // hotkeys
        public const string HOTKEY_PREFIX = "Hotkey_";
        public const string HOTKEY_FOR_TOGGLE_MODE = HOTKEY_PREFIX + "Toggle_TurnBasedMode";
        public const string HOTKEY_FOR_TOGGLE_ATTACK_INDICATOR = HOTKEY_PREFIX + "Toggle_AttackIndicator";
        public const string HOTKEY_FOR_TOGGLE_MOVEMENT_INDICATOR = HOTKEY_PREFIX + "Toggle_MovementIndicator";
        public const string HOTKEY_FOR_END_TURN = HOTKEY_PREFIX + "Button_EndTurn";
        public const string HOTKEY_FOR_DELAY = HOTKEY_PREFIX + "Button_Delay";
        public const string HOTKEY_FOR_FIVE_FOOT_STEP = HOTKEY_PREFIX + "Button_FiveFootStep";
        public const string HOTKEY_FOR_FULL_ATTACK = HOTKEY_PREFIX + "Button_FullAttack";

        // combat tracker
        public const float UNIT_BUTTON_HEIGHT = 32.5f;
        public const float UNIT_BUTTON_SPACE = 2.5f;
        public const float PADDING_X_PERCENT = 0.18f;
        public const float PADDING_Y_PERCENT = 0.08f;

        public static float MetersOfFiveFootStep => 5f * Feet.FeetToMetersRatio * DistanceOfFiveFootStep;

        #endregion

        #region Gameplay

        public static float DistanceOfFiveFootStep {
            get => Mod.Settings.distanceOfFiveFootStep;
            set => Mod.Settings.distanceOfFiveFootStep = value;
        }

        public static bool SurpriseRound {
            get => Mod.Settings.toggleSurpriseRound;
            set => Mod.Settings.toggleSurpriseRound = value;
        }

        public static bool PreventUnconsciousUnitLeavingCombat {
            get => Mod.Settings.togglePreventUnconsciousUnitLeavingCombat;
            set => Mod.Settings.togglePreventUnconsciousUnitLeavingCombat = value;
        }

        public static bool FlankingCountAllNearbyOpponents {
            get => Mod.Settings.toggleFlankingCountAllOpponentsWithinThreatenedRange;
            set => Mod.Settings.toggleFlankingCountAllOpponentsWithinThreatenedRange = value;
        }

        public static bool RerollPerceptionDiceAgainstStealthOncePerRound {
            get => Mod.Settings.toggleRerollPerceptionDiceAgainstStealthOncePerRound;
            set => Mod.Settings.toggleRerollPerceptionDiceAgainstStealthOncePerRound = value;
        }

        public static float RadiusOfCollision {
            get => Mod.Settings.radiusOfCollision;
            set => Mod.Settings.radiusOfCollision = value;
        }

        public static bool MovingThroughFriendlyUnit {
            get => Mod.Settings.toggleMovingThroughFriendlyUnit;
            set {
                if (Mod.Settings.toggleMovingThroughFriendlyUnit != value)
                {
                    Mod.Settings.toggleMovingThroughFriendlyUnit = value;
                    if (value)
                    {
                        MovingThroughNonHostileUnit = false;
                    }
                }
            }
        }

        public static bool MovingThroughNonHostileUnit {
            get => Mod.Settings.toggleMovingThroughNonHostileUnit;
            set {
                if (Mod.Settings.toggleMovingThroughNonHostileUnit != value)
                {
                    Mod.Settings.toggleMovingThroughNonHostileUnit = value;
                    if (value)
                    {
                        MovingThroughFriendlyUnit = false;
                    }
                }
            }
        }

        public static bool MovingThroughApplyToPlayer {
            get => Mod.Settings.toggleMovingThroughApplyToPlayer;
            set => Mod.Settings.toggleMovingThroughApplyToPlayer = value;
        }

        public static bool MovingThroughApplyToNeutralUnit {
            get => Mod.Settings.toggleMovingThroughApplyToNeutralUnit;
            set => Mod.Settings.toggleMovingThroughApplyToNeutralUnit = value;
        }

        public static bool MovingThroughApplyToEnemy {
            get => Mod.Settings.toggleMovingThroughApplyToEnemy;
            set => Mod.Settings.toggleMovingThroughApplyToEnemy = value;
        }

        public static bool AvoidOverlapping {
            get => Mod.Settings.toggleAvoidOverlapping;
            set => Mod.Settings.toggleAvoidOverlapping = value;
        }

        public static bool AvoidOverlappingOnCharge {
            get => Mod.Settings.toggleAvoidOverlappingOnCharge;
            set => Mod.Settings.toggleAvoidOverlappingOnCharge = value;
        }

        public static bool DoNotMovingThroughNonAlly {
            get => Mod.Settings.toggleDoNotMovingThroughNonAlly;
            set => Mod.Settings.toggleDoNotMovingThroughNonAlly = value;
        }

        public static bool AutoTurnOffAIOnTurnStart {
            get => Mod.Settings.toggleAutoTurnOffAIOnTurnStart;
            set => Mod.Settings.toggleAutoTurnOffAIOnTurnStart = value;
        }

        public static bool AutoTurnOnAIOnCombatEnd {
            get => Mod.Settings.toggleAutoTurnOnAIOnCombatEnd;
            set => Mod.Settings.toggleAutoTurnOnAIOnCombatEnd = value;
        }

        public static bool AutoSelectUnitOnTurnStart {
            get => Mod.Settings.toggleAutoSelectUnitOnTurnStart;
            set => Mod.Settings.toggleAutoSelectUnitOnTurnStart = value;
        }

        public static bool AutoSelectEntirePartyOnCombatEnd {
            get => Mod.Settings.toggleAutoSelectEntirePartyOnCombatEnd;
            set => Mod.Settings.toggleAutoSelectEntirePartyOnCombatEnd = value;
        }

        public static bool AutoCancelActionsOnTurnStart {
            get => Mod.Settings.toggleAutoCancelActionsOnTurnStart;
            set => Mod.Settings.toggleAutoCancelActionsOnTurnStart = value;
        }

        public static bool AutoCancelActionsOnCombatEnd {
            get => Mod.Settings.toggleAutoCancelActionsOnCombatEnd;
            set => Mod.Settings.toggleAutoCancelActionsOnCombatEnd = value;
        }

        public static bool AutoCancelActionsOnFiveFootStepFinish {
            get => Mod.Settings.toggleAutoCancelActionsOnFiveFootStepFinish;
            set => Mod.Settings.toggleAutoCancelActionsOnFiveFootStepFinish = value;
        }

        public static bool AutoCancelActionsOnFirstMoveFinish {
            get => Mod.Settings.toggleAutoCancelActionsOnFirstMoveFinish;
            set => Mod.Settings.toggleAutoCancelActionsOnFirstMoveFinish = value;
        }

        public static bool AutoEnableFiveFootStepOnTurnStart {
            get => Mod.Settings.toggleAutoEnableFiveFootStepOnTurnStart;
            set => Mod.Settings.toggleAutoEnableFiveFootStepOnTurnStart = value;
        }

        public static bool AutoEndTurnWhenActionsAreUsedUp {
            get => Mod.Settings.toggleAutoEndTurnWhenActionsAreUsedUp;
            set => Mod.Settings.toggleAutoEndTurnWhenActionsAreUsedUp = value;
        }

        public static bool AutoEndTurnExceptSwiftAction {
            get => Mod.Settings.toggleAutoEndTurnExceptSwiftAction;
            set => Mod.Settings.toggleAutoEndTurnExceptSwiftAction = value;
        }

        public static bool AutoEndTurnWhenPlayerIdle {
            get => Mod.Settings.toggleAutoEndTurnWhenPlayerIdle;
            set => Mod.Settings.toggleAutoEndTurnWhenPlayerIdle = value;
        }

        #endregion

        #region Interface

        public static bool DoNotMarkInvisibleUnit {
            get => Mod.Settings.toggleDoNotMarkInvisibleUnit;
            set => Mod.Settings.toggleDoNotMarkInvisibleUnit = value;
        }

        public static bool DoNotShowInvisibleUnitOnCombatTracker {
            get => Mod.Settings.toggleDoNotShowInvisibleUnitOnCombatTracker;
            set => Mod.Settings.toggleDoNotShowInvisibleUnitOnCombatTracker = value;
        }

        public static float CombatTrackerScale {
            get => Mod.Settings.combatTrackerScale;
            set => Mod.Settings.combatTrackerScale = value;
        }

        public static float CombatTrackerWidth {
            get => Mod.Settings.combatTrackerWidth;
            set => Mod.Settings.combatTrackerWidth = value;
        }

        public static int CombatTrackerMaxUnits {
            get => Mod.Settings.combatTrackerMaxUnits;
            set => Mod.Settings.combatTrackerMaxUnits = value;
        }

        public static bool CameraScrollToUnitOnClickUI {
            get => Mod.Settings.toggleCameraScrollToUnitOnClickUI;
            set => Mod.Settings.toggleCameraScrollToUnitOnClickUI = value;
        }

        public static bool SelectUnitOnClickUI {
            get => Mod.Settings.toggleSelectUnitOnClickUI;
            set => Mod.Settings.toggleSelectUnitOnClickUI = value;
        }

        public static bool InspectOnRightClickUI {
            get => Mod.Settings.toggleInspectOnRightClickUI;
            set => Mod.Settings.toggleInspectOnRightClickUI = value;
        }

        public static bool ShowIsFlatFootedIconOnUI {
            get => Mod.Settings.toggleShowIsFlatFootedIconOnUI;
            set => Mod.Settings.toggleShowIsFlatFootedIconOnUI = value;
        }

        public static bool HighlightCurrentUnit {
            get => Mod.Settings.toggleHighlightCurrentUnit;
            set => Mod.Settings.toggleHighlightCurrentUnit = value;
        }

        public static bool CameraScrollToCurrentUnit {
            get => Mod.Settings.toggleCameraScrollToCurrentUnit;
            set => Mod.Settings.toggleCameraScrollToCurrentUnit = value;
        }

        public static bool CameraLockOnCurrentPlayerUnit {
            get => Mod.Settings.toggleCameraLockOnCurrentPlayerUnit;
            set => Mod.Settings.toggleCameraLockOnCurrentPlayerUnit = value;
        }

        public static bool CameraLockOnCurrentNonPlayerUnit {
            get => Mod.Settings.toggleCameraLockOnCurrentNonPlayerUnit;
            set => Mod.Settings.toggleCameraLockOnCurrentNonPlayerUnit = value;
        }

        public static bool ShowAttackIndicatorOfCurrentUnit {
            get => Mod.Settings.toggleShowAttackIndicatorOfCurrentUnit;
            set => Mod.Settings.toggleShowAttackIndicatorOfCurrentUnit = value;
        }

        public static bool ShowAttackIndicatorForPlayer {
            get => Mod.Settings.toggleShowAttackIndicatorForPlayer;
            set => Mod.Settings.toggleShowAttackIndicatorForPlayer = value;
        }

        public static bool ShowAttackIndicatorForNonPlayer {
            get => Mod.Settings.toggleShowAttackIndicatorForNonPlayer;
            set => Mod.Settings.toggleShowAttackIndicatorForNonPlayer = value;
        }

        public static bool ShowAttackIndicatorOnHoverUI {
            get => Mod.Settings.toggleShowAttackIndicatorOnHoverUI;
            set => Mod.Settings.toggleShowAttackIndicatorOnHoverUI = value;
        }

        public static bool ShowAutoCastAbilityRange {
            get => Mod.Settings.toggleShowAutoCastAbilityRange;
            set => Mod.Settings.toggleShowAutoCastAbilityRange = value;
        }

        public static bool CheckForObstaclesOnTargeting {
            get => Mod.Settings.toggleCheckForObstaclesOnTargeting;
            set => Mod.Settings.toggleCheckForObstaclesOnTargeting = value;
        }

        public static bool ShowMovementIndicatorOfCurrentUnit {
            get => Mod.Settings.toggleShowMovementIndicatorOfCurrentUnit;
            set => Mod.Settings.toggleShowMovementIndicatorOfCurrentUnit = value;
        }

        public static bool ShowMovementIndicatorForPlayer {
            get => Mod.Settings.toggleShowMovementIndicatorForPlayer;
            set => Mod.Settings.toggleShowMovementIndicatorForPlayer = value;
        }

        public static bool ShowMovementIndicatorForNonPlayer {
            get => Mod.Settings.toggleShowMovementIndicatorForNonPlayer;
            set => Mod.Settings.toggleShowMovementIndicatorForNonPlayer = value;
        }

        public static bool ShowMovementIndicatorOnHoverUI {
            get => Mod.Settings.toggleShowMovementIndicatorOnHoverUI;
            set => Mod.Settings.toggleShowMovementIndicatorOnHoverUI = value;
        }

        #endregion

        #region Hotkey

        public static bool ToggleFiveFootStepOnRightClickGround {
            get => Mod.Settings.hotkeyToggleFiveFootStepOnRightClickGround;
            set => Mod.Settings.hotkeyToggleFiveFootStepOnRightClickGround = value;
        }

        #endregion

        #region Time Scale

        public static float TimeScaleBetweenTurns {
            get => Mod.Settings.timeScaleBetweenTurns;
            set => Mod.Settings.timeScaleBetweenTurns = value;
        }

        public static float TimeScaleInPlayerTurn {
            get => Mod.Settings.timeScaleInPlayerTurn;
            set => Mod.Settings.timeScaleInPlayerTurn = value;
        }

        public static float TimeScaleInNonPlayerTurn {
            get => Mod.Settings.timeScaleInNonPlayerTurn;
            set => Mod.Settings.timeScaleInNonPlayerTurn = value;
        }

        public static float TimeScaleInUnknownTurn {
            get => Mod.Settings.timeScaleInUnknownTurn;
            set => Mod.Settings.timeScaleInUnknownTurn = value;
        }

        public static float MaxDelayBetweenIterativeAttacks {
            get => Mod.Settings.maxDelayBetweenIterativeAttacks;
            set => Mod.Settings.maxDelayBetweenIterativeAttacks = value;
        }

        public static float CastingTimeOfFullRoundSpell {
            get => Mod.Settings.castingTimeOfFullRoundSpell;
            set => Mod.Settings.castingTimeOfFullRoundSpell = value;
        }

        public static float TimeToWaitForIdleAI {
            get => Mod.Settings.timeToWaitForIdleAI;
            set => Mod.Settings.timeToWaitForIdleAI = value;
        }

        public static float TimeToWaitForEndingTurn {
            get => Mod.Settings.timeToWaitForEndingTurn;
            set => Mod.Settings.timeToWaitForEndingTurn = value;
        }

        #endregion

        #region Pause

        public static bool DoNotPauseOnCombatStart {
            get => Mod.Settings.toggleDoNotPauseOnCombatStart;
            set => Mod.Settings.toggleDoNotPauseOnCombatStart = value;
        }

        public static bool PauseOnPlayerTurnStart {
            get => Mod.Settings.togglePauseOnPlayerTurnStart;
            set => Mod.Settings.togglePauseOnPlayerTurnStart = value;
        }

        public static bool PauseOnPlayerTurnEnd {
            get => Mod.Settings.togglePauseOnPlayerTurnEnd;
            set => Mod.Settings.togglePauseOnPlayerTurnEnd = value;
        }

        public static bool PauseOnNonPlayerTurnStart {
            get => Mod.Settings.togglePauseOnNonPlayerTurnStart;
            set => Mod.Settings.togglePauseOnNonPlayerTurnStart = value;
        }

        public static bool PauseOnNonPlayerTurnEnd {
            get => Mod.Settings.togglePauseOnNonPlayerTurnEnd;
            set => Mod.Settings.togglePauseOnNonPlayerTurnEnd = value;
        }

        public static bool PauseOnPlayerFinishFiveFoot {
            get => Mod.Settings.togglePauseOnPlayerFinishFiveFoot;
            set => Mod.Settings.togglePauseOnPlayerFinishFiveFoot = value;
        }

        public static bool PauseOnPlayerFinishFirstMove {
            get => Mod.Settings.togglePauseOnPlayerFinishFirstMove;
            set => Mod.Settings.togglePauseOnPlayerFinishFirstMove = value;
        }

        #endregion

        #region Bugfix

        public static BugfixOption FixNeverInCombatWithoutMC => Mod.Settings.toggleFixNeverInCombatWithoutMC;

        public static BugfixOption FixActionTypeOfBardicPerformance => Mod.Settings.toggleFixActionTypeOfBardicPerformance;

        public static BugfixOption FixActionTypeOfSwappingWeapon => Mod.Settings.toggleFixActionTypeOfSwappingWeapon;

        public static BugfixOption FixActionTypeOfCharge => Mod.Settings.toggleFixActionTypeOfCharge;

        public static BugfixOption FixActionTypeOfOverrun => Mod.Settings.toggleFixActionTypeOfOverrun;

        public static BugfixOption FixActionTypeOfVitalStrike => Mod.Settings.toggleFixActionTypeOfVitalStrike;

        public static BugfixOption FixActionTypeOfAngelicForm => Mod.Settings.toggleFixActionTypeOfAngelicForm;

        public static BugfixOption FixActionTypeOfKineticBlade => Mod.Settings.toggleFixActionTypeOfKineticBlade;

        public static BugfixOption FixKineticistWontStopPriorCommand => Mod.Settings.toggleFixKineticistWontStopPriorCommand;

        public static BugfixOption FixSpellstrikeOnNeutralUnit => Mod.Settings.toggleFixSpellstrikeOnNeutralUnit;

        public static BugfixOption FixSpellstrikeWithMetamagicReach => Mod.Settings.toggleFixSpellstrikeWithMetamagicReach;

        public static BugfixOption FixSpellstrikeWithNaturalWeapon => Mod.Settings.toggleFixSpellstrikeWithNaturalWeapon;

        public static BugfixOption FixDamageBonusOfBlastRune => Mod.Settings.toggleFixDamageBonusOfBlastRune;

        public static BugfixOption FixOnePlusDiv2ToDiv2 => Mod.Settings.toggleFixOnePlusDiv2ToDiv2;

        public static BugfixOption FixFxOfShadowEvocationSirocco => Mod.Settings.toggleFixFxOfShadowEvocationSirocco;

        public static BugfixOption FixAbilityNotAutoDeactivateIfCombatEnded => Mod.Settings.toggleFixAbilityNotAutoDeactivateIfCombatEnded;

        public static BugfixOption FixBlindFightDistance => Mod.Settings.toggleFixBlindFightDistance;

        public static BugfixOption FixDweomerLeap => Mod.Settings.toggleFixDweomerLeap;

        public static BugfixOption FixConfusedUnitCanAttackDeadUnit => Mod.Settings.toggleFixConfusedUnitCanAttackDeadUnit;

        public static BugfixOption FixAcrobaticsMobility => Mod.Settings.toggleFixAcrobaticsMobility;

        public static BugfixOption FixCanMakeAttackOfOpportunityToUnmovedTarget => Mod.Settings.toggleFixCanMakeAttackOfOpportunityToUnmovedTarget;

        public static BugfixOption FixHasMotionThisTick => Mod.Settings.toggleFixHasMotionThisTick;

        public static BugfixOption FixAbilityCircleRadius => Mod.Settings.toggleFixAbilityCircleRadius;

        public static BugfixOption FixAbilityCircleNotAppear => Mod.Settings.toggleFixAbilityCircleNotAppear;

        public static BugfixOption FixAbilityCanTargetUntargetableUnit => Mod.Settings.toggleFixAbilityCanTargetUntargetableUnit;

        public static BugfixOption FixAbilityCanTargetDeadUnit => Mod.Settings.toggleFixAbilityCanTargetDeadUnit;

        public static BugfixOption FixNeutralUnitCanAttackAlly => Mod.Settings.toggleFixNeutralUnitCanAttackAlly;

        public static BugfixOption FixInspectingTriggerAuraEffect => Mod.Settings.toggleFixInspectingTriggerAuraEffect;

        #endregion

        #region Localization

        public static string LocalizationFileName {
            get => Mod.Settings.localizationFileName;
            set => Mod.Settings.localizationFileName = value;
        }

        #endregion
    }
}
