using UnityEngine;
using static TurnBased.Main;

namespace TurnBased.Utility
{
    public static class SettingsWrapper
    {
        #region Constants

        public const float TIME_STANDARD_ACTION = 6f;
        public const float TIME_MOVE_ACTION = 3f;
        public const float TIME_SWIFT_ACTION = 6f;

        public const string HOTKEY_PREFIX = "TurnBasedHotkey";
        public const string HOTKEY_FOR_TOGGLE_MODE = HOTKEY_PREFIX + "ToggleTurnBasedMode";
        public const string HOTKEY_FOR_TOGGLE_MOVEMENT_INDICATOR = HOTKEY_PREFIX + "ToggleMovementIndicator";
        public const string HOTKEY_FOR_TOGGLE_ATTACK_INDICATOR = HOTKEY_PREFIX + "ToggleAttackIndicator";
        public const string HOTKEY_FOR_FIVE_FOOT_STEP = HOTKEY_PREFIX + "5FootStep";
        public const string HOTKEY_FOR_DELAY = HOTKEY_PREFIX + "Delay";
        public const string HOTKEY_FOR_END_TURN = HOTKEY_PREFIX + "EndTurn";

        public const float UNIT_BUTTON_HEIGHT = 32.5f;
        public const float UNIT_BUTTON_SPACE = 2.5f;
        public static readonly Vector2 DEFAULT_BLOCK_SIZE = new Vector2(415f, 730f);
        public static readonly Vector2 DEFAULT_BLOCK_PADDING = new Vector2(80f, 60f);

        #endregion

        #region Gameplay

        public static bool SetChargeAsFullRoundAction {
            get => Mod.Settings.toggleSetChargeAsFullRoundAction;
            set {
                if (Mod.Settings.toggleSetChargeAsFullRoundAction != value)
                {
                    Mod.Settings.toggleSetChargeAsFullRoundAction = value;
                    Mod.Core.Blueprint.UpdateChargeAbility();
                }
            } 
        }

        public static bool SetVitalStrikeAsStandardAction {
            get => Mod.Settings.toggleSetVitalStrikeAsStandardAction;
            set {
                if (Mod.Settings.toggleSetVitalStrikeAsStandardAction != value)
                {
                    Mod.Settings.toggleSetVitalStrikeAsStandardAction = value;
                    Mod.Core.Blueprint.UpdateVitalStrikeAbility();
                }
            }
        }

        public static bool FixTheCostToStartBardicPerformance {
            get => Mod.Settings.toggleFixTheCostToStartBardicPerformance;
            set => Mod.Settings.toggleFixTheCostToStartBardicPerformance = value;
        }

        public static bool FlankingCountAllOpponents {
            get => Mod.Settings.toggleFlankingCountAllOpponentsWithinThreatenRange;
            set => Mod.Settings.toggleFlankingCountAllOpponentsWithinThreatenRange = value;
        }

        public static float DistanceOfFiveFootStep {
            get => Mod.Settings.distanceOfFiveFootStep;
            set => Mod.Settings.distanceOfFiveFootStep = value;
        }

        public static bool MovingThroughFriends {
            get => Mod.Settings.toggleMovingThroughFriends;
            set {
                if (Mod.Settings.toggleMovingThroughFriends != value)
                {
                    Mod.Settings.toggleMovingThroughFriends = value;
                    if (value)
                    {
                        MovingThroughNonEnemies = false;
                    }
                }
            }
        }

        public static bool MovingThroughNonEnemies {
            get => Mod.Settings.toggleMovingThroughNonEnemies;
            set {
                if (Mod.Settings.toggleMovingThroughNonEnemies != value)
                {
                    Mod.Settings.toggleMovingThroughNonEnemies = value;
                    if (value)
                    {
                        MovingThroughFriends = false;
                    }
                }
            }
        }

        public static bool MovingThroughOnlyAffectPlayer {
            get => Mod.Settings.toggleMovingThroughOnlyAffectPlayer;
            set {
                if (Mod.Settings.toggleMovingThroughOnlyAffectPlayer != value)
                {
                    Mod.Settings.toggleMovingThroughOnlyAffectPlayer = value;
                    if (value)
                    {
                        MovingThroughOnlyAffectNonEnemies = false;
                    }
                }
            }
        }

        public static bool MovingThroughOnlyAffectNonEnemies {
            get => Mod.Settings.toggleMovingThroughOnlyAffectNonEnemies;
            set {
                if (Mod.Settings.toggleMovingThroughOnlyAffectNonEnemies != value)
                {
                    Mod.Settings.toggleMovingThroughOnlyAffectNonEnemies = value;
                    if (value)
                    {
                        MovingThroughOnlyAffectPlayer = false;
                    }
                }
            }
        }

        public static bool AvoidOverlapping {
            get => Mod.Settings.toggleAvoidOverlapping;
            set => Mod.Settings.toggleAvoidOverlapping = value;
        }

        public static bool AvoidOverlappingOnCharge {
            get => Mod.Settings.toggleAvoidOverlappingOnCharge;
            set => Mod.Settings.toggleAvoidOverlappingOnCharge = value;
        }

        public static bool DoNotMovingThroughNonAllies {
            get => Mod.Settings.toggleDoNotMovingThroughNonAllies;
            set => Mod.Settings.toggleDoNotMovingThroughNonAllies = value;
        }

        public static float RadiusOfCollision {
            get => Mod.Settings.radiusOfCollision;
            set => Mod.Settings.radiusOfCollision = value;
        }

        public static bool AutoTurnOffAI {
            get => Mod.Settings.toggleAutoTurnOffPlayerAI;
            set => Mod.Settings.toggleAutoTurnOffPlayerAI = value;
        }

        public static bool AutoTurnOnAI {
            get => Mod.Settings.toggleAutoTurnOnPlayerAI;
            set => Mod.Settings.toggleAutoTurnOnPlayerAI = value;
        }

        public static bool AutoSelectCurrentUnit {
            get => Mod.Settings.toggleAutoSelectCurrentUnit;
            set => Mod.Settings.toggleAutoSelectCurrentUnit = value;
        }

        public static bool AutoSelectEntireParty {
            get => Mod.Settings.toggleAutoSelectEntireParty;
            set => Mod.Settings.toggleAutoSelectEntireParty = value;
        }

        public static bool AutoCancelActionsOnPlayerTurnStart {
            get => Mod.Settings.toggleAutoCancelActionsOnPlayerTurnStart;
            set => Mod.Settings.toggleAutoCancelActionsOnPlayerTurnStart = value;
        }

        public static bool AutoCancelActionsOnCombatEnd {
            get => Mod.Settings.toggleAutoCancelActionsOnCombatEnd;
            set => Mod.Settings.toggleAutoCancelActionsOnCombatEnd = value;
        }

        public static bool AutoEnableFiveFootStep {
            get => Mod.Settings.toggleAutoEnableFiveFootStep;
            set => Mod.Settings.toggleAutoEnableFiveFootStep = value;
        }
        
        public static bool AutoCancelActionsOnPlayerFinishFiveFoot {
            get => Mod.Settings.toggleAutoCancelActionsOnPlayerFinishFiveFoot;
            set => Mod.Settings.toggleAutoCancelActionsOnPlayerFinishFiveFoot = value;
        }

        public static bool AutoCancelActionsOnPlayerFinishFirstMove {
            get => Mod.Settings.toggleAutoCancelActionsOnPlayerFinishFirstMove;
            set => Mod.Settings.toggleAutoCancelActionsOnPlayerFinishFirstMove = value;
        }

        public static bool AutoEndTurn {
            get => Mod.Settings.toggleAutoEndTurn;
            set => Mod.Settings.toggleAutoEndTurn = value;
        }

        public static bool DoNotAutoEndTurnWhenHasSwiftAction {
            get => Mod.Settings.toggleDoNotAutoEndTurnWhenHasSwiftAction;
            set => Mod.Settings.toggleDoNotAutoEndTurnWhenHasSwiftAction = value;
        }

        #endregion

        #region Interface

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

        public static bool ShowAttackIndicatorOfPlayer {
            get => Mod.Settings.toggleShowAttackIndicatorOfPlayer;
            set => Mod.Settings.toggleShowAttackIndicatorOfPlayer = value;
        }

        public static bool ShowAttackIndicatorOfNonPlayer {
            get => Mod.Settings.toggleShowAttackIndicatorOfNonPlayer;
            set => Mod.Settings.toggleShowAttackIndicatorOfNonPlayer = value;
        }

        public static bool ShowAttackIndicatorOnHoverUI {
            get => Mod.Settings.toggleShowAttackIndicatorOnHoverUI;
            set => Mod.Settings.toggleShowAttackIndicatorOnHoverUI = value;
        }

        public static bool ShowAutoCastAbilityRange {
            get => Mod.Settings.toggleShowAutoCastAbilityRange;
            set => Mod.Settings.toggleShowAutoCastAbilityRange = value;
        }

        public static bool ShowMovementIndicatorOfCurrentUnit {
            get => Mod.Settings.toggleShowMovementIndicatorOfCurrentUnit;
            set => Mod.Settings.toggleShowMovementIndicatorOfCurrentUnit = value;
        }

        public static bool ShowMovementIndicatorOfPlayer {
            get => Mod.Settings.toggleShowMovementIndicatorOfPlayer;
            set => Mod.Settings.toggleShowMovementIndicatorOfPlayer = value;
        }

        public static bool ShowMovementIndicatorOfNonPlayer {
            get => Mod.Settings.toggleShowMovementIndicatorOfNonPlayer;
            set => Mod.Settings.toggleShowMovementIndicatorOfNonPlayer = value;
        }

        public static bool ShowMovementIndicatorOnHoverUI {
            get => Mod.Settings.toggleShowMovementIndicatorOnHoverUI;
            set => Mod.Settings.toggleShowMovementIndicatorOnHoverUI = value;
        }

        public static bool ShowIsFlatFootedIconOnUI {
            get => Mod.Settings.toggleShowIsFlatFootedIconOnUI;
            set {
                if (Mod.Settings.toggleShowIsFlatFootedIconOnUI != value)
                {
                    Mod.Settings.toggleShowIsFlatFootedIconOnUI = value;
                    if (value)
                    {
                        ShowIsFlatFootedIconOnHoverUI = false;
                    }
                }
            }
        }

        public static bool ShowIsFlatFootedIconOnHoverUI {
            get => Mod.Settings.toggleShowIsFlatFootedIconOnHoverUI;
            set {
                if (Mod.Settings.toggleShowIsFlatFootedIconOnHoverUI != value)
                {
                    Mod.Settings.toggleShowIsFlatFootedIconOnHoverUI = value;
                    if (value)
                    {
                        ShowIsFlatFootedIconOnUI = false;
                    }
                }
            }
        }

        public static bool SelectUnitOnClickUI {
            get => Mod.Settings.toggleSelectUnitOnClickUI;
            set => Mod.Settings.toggleSelectUnitOnClickUI = value;
        }

        public static bool CameraScrollToUnitOnClickUI {
            get => Mod.Settings.toggleCameraScrollToUnitOnClickUI;
            set => Mod.Settings.toggleCameraScrollToUnitOnClickUI = value;
        }

        public static bool ShowUnitDescriptionOnRightClickUI {
            get => Mod.Settings.toggleShowUnitDescriptionOnRightClickUI;
            set => Mod.Settings.toggleShowUnitDescriptionOnRightClickUI = value;
        }

        public static bool DoNotMarkInvisibleUnit {
            get => Mod.Settings.toggleDoNotMarkInvisibleUnit;
            set => Mod.Settings.toggleDoNotMarkInvisibleUnit = value;
        }

        public static bool DoNotShowInvisibleUnitOnCombatTracker {
            get => Mod.Settings.toggleDoNotShowInvisibleUnitOnCombatTracker;
            set => Mod.Settings.toggleDoNotShowInvisibleUnitOnCombatTracker = value;
        }
        
        public static float CombatTrackerWidth {
            get => Mod.Settings.combatTrackerWidth;
            set => Mod.Settings.combatTrackerWidth = value;
        }

        public static int CombatTrackerMaxUnits {
            get => Mod.Settings.CombatTrackerMaxUnits;
            set => Mod.Settings.CombatTrackerMaxUnits = value;
        }

        #endregion

        #region Time Scale

        public static float MinimumFPS {
            get => Mod.Settings.minimumFPS;
            set => Mod.Settings.minimumFPS = value;
        }

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
    }
}
