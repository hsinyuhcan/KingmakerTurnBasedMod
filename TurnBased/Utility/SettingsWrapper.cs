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
            get => Core.Settings.toggleSetChargeAsFullRoundAction;
            set {
                if (Core.Settings.toggleSetChargeAsFullRoundAction != value)
                {
                    Core.Settings.toggleSetChargeAsFullRoundAction = value;
                    Core.Mod.UpdateChargeAbility();
                }
            } 
        }

        public static bool SetVitalStrikeAsStandardAction {
            get => Core.Settings.toggleSetVitalStrikeAsStandardAction;
            set {
                if (Core.Settings.toggleSetVitalStrikeAsStandardAction != value)
                {
                    Core.Settings.toggleSetVitalStrikeAsStandardAction = value;
                    Core.Mod.UpdateVitalStrikeAbility();
                }
            }
        }

        public static bool FixTheCostToStartBardicPerformance {
            get => Core.Settings.toggleFixTheCostToStartBardicPerformance;
            set => Core.Settings.toggleFixTheCostToStartBardicPerformance = value;
        }

        public static bool FlankingCountAllOpponents {
            get => Core.Settings.toggleFlankingCountAllOpponentsWithinThreatenRange;
            set => Core.Settings.toggleFlankingCountAllOpponentsWithinThreatenRange = value;
        }

        public static float DistanceOfFiveFootStep {
            get => Core.Settings.distanceOfFiveFootStep;
            set => Core.Settings.distanceOfFiveFootStep = value;
        }

        public static bool MovingThroughFriends {
            get => Core.Settings.toggleMovingThroughFriends;
            set {
                if (Core.Settings.toggleMovingThroughFriends != value)
                {
                    Core.Settings.toggleMovingThroughFriends = value;
                    if (value)
                    {
                        MovingThroughNonEnemies = false;
                    }
                }
            }
        }

        public static bool MovingThroughNonEnemies {
            get => Core.Settings.toggleMovingThroughNonEnemies;
            set {
                if (Core.Settings.toggleMovingThroughNonEnemies != value)
                {
                    Core.Settings.toggleMovingThroughNonEnemies = value;
                    if (value)
                    {
                        MovingThroughFriends = false;
                    }
                }
            }
        }

        public static bool MovingThroughOnlyAffectPlayer {
            get => Core.Settings.toggleMovingThroughOnlyAffectPlayer;
            set {
                if (Core.Settings.toggleMovingThroughOnlyAffectPlayer != value)
                {
                    Core.Settings.toggleMovingThroughOnlyAffectPlayer = value;
                    if (value)
                    {
                        MovingThroughOnlyAffectNonEnemies = false;
                    }
                }
            }
        }

        public static bool MovingThroughOnlyAffectNonEnemies {
            get => Core.Settings.toggleMovingThroughOnlyAffectNonEnemies;
            set {
                if (Core.Settings.toggleMovingThroughOnlyAffectNonEnemies != value)
                {
                    Core.Settings.toggleMovingThroughOnlyAffectNonEnemies = value;
                    if (value)
                    {
                        MovingThroughOnlyAffectPlayer = false;
                    }
                }
            }
        }

        public static float RadiusOfCollision {
            get => Core.Settings.radiusOfCollision;
            set => Core.Settings.radiusOfCollision = value;
        }

        public static bool AutoTurnOffAI {
            get => Core.Settings.toggleAutoTurnOffPlayerAI;
            set => Core.Settings.toggleAutoTurnOffPlayerAI = value;
        }

        public static bool AutoTurnOnAI {
            get => Core.Settings.toggleAutoTurnOnPlayerAI;
            set => Core.Settings.toggleAutoTurnOnPlayerAI = value;
        }

        public static bool AutoSelectEntireParty {
            get => Core.Settings.toggleAutoSelectEntireParty;
            set => Core.Settings.toggleAutoSelectEntireParty = value;
        }

        public static bool AutoSelectCurrentUnit {
            get => Core.Settings.toggleAutoSelectCurrentUnit;
            set => Core.Settings.toggleAutoSelectCurrentUnit = value;
        }

        public static bool AutoEnableFiveFootStep {
            get => Core.Settings.toggleAutoEnableFiveFootStep;
            set => Core.Settings.toggleAutoEnableFiveFootStep = value;
        }
        
        public static bool AutoCancelActionsOnPlayerTurnStart {
            get => Core.Settings.toggleAutoCancelActionsOnPlayerTurnStart;
            set => Core.Settings.toggleAutoCancelActionsOnPlayerTurnStart = value;
        }

        public static bool AutoCancelActionsOnPlayerFinishFiveFoot {
            get => Core.Settings.toggleAutoCancelActionsOnPlayerFinishFiveFoot;
            set => Core.Settings.toggleAutoCancelActionsOnPlayerFinishFiveFoot = value;
        }

        public static bool AutoCancelActionsOnPlayerFinishFirstMove {
            get => Core.Settings.toggleAutoCancelActionsOnPlayerFinishFirstMove;
            set => Core.Settings.toggleAutoCancelActionsOnPlayerFinishFirstMove = value;
        }

        public static bool AutoEndTurn {
            get => Core.Settings.toggleAutoEndTurn;
            set => Core.Settings.toggleAutoEndTurn = value;
        }

        public static bool DoNotAutoEndTurnWhenHasSwiftAction {
            get => Core.Settings.toggleDoNotAutoEndTurnWhenHasSwiftAction;
            set => Core.Settings.toggleDoNotAutoEndTurnWhenHasSwiftAction = value;
        }

        public static bool AllowCommandNonPlayerToPerformSpecialActions {
            get => Core.Settings.toggleAllowCommandNonPlayerToPerformSpecialActions;
            set => Core.Settings.toggleAllowCommandNonPlayerToPerformSpecialActions = value;
        }

        #endregion

        #region Interface

        public static bool HighlightCurrentUnit {
            get => Core.Settings.toggleHighlightCurrentUnit;
            set => Core.Settings.toggleHighlightCurrentUnit = value;
        }

        public static bool CameraScrollToCurrentUnit {
            get => Core.Settings.toggleCameraScrollToCurrentUnit;
            set => Core.Settings.toggleCameraScrollToCurrentUnit = value;
        }

        public static bool CameraLockOnCurrentPlayerUnit {
            get => Core.Settings.toggleCameraLockOnCurrentPlayerUnit;
            set => Core.Settings.toggleCameraLockOnCurrentPlayerUnit = value;
        }

        public static bool CameraLockOnCurrentNonPlayerUnit {
            get => Core.Settings.toggleCameraLockOnCurrentNonPlayerUnit;
            set => Core.Settings.toggleCameraLockOnCurrentNonPlayerUnit = value;
        }

        public static bool ShowAttackIndicatorOfCurrentUnit {
            get => Core.Settings.toggleShowAttackIndicatorOfCurrentUnit;
            set => Core.Settings.toggleShowAttackIndicatorOfCurrentUnit = value;
        }

        public static bool ShowAttackIndicatorOfPlayer {
            get => Core.Settings.toggleShowAttackIndicatorOfPlayer;
            set => Core.Settings.toggleShowAttackIndicatorOfPlayer = value;
        }

        public static bool ShowAttackIndicatorOfNonPlayer {
            get => Core.Settings.toggleShowAttackIndicatorOfNonPlayer;
            set => Core.Settings.toggleShowAttackIndicatorOfNonPlayer = value;
        }

        public static bool ShowMovementIndicatorOfCurrentUnit {
            get => Core.Settings.toggleShowMovementIndicatorOfCurrentUnit;
            set => Core.Settings.toggleShowMovementIndicatorOfCurrentUnit = value;
        }

        public static bool ShowMovementIndicatorOfPlayer {
            get => Core.Settings.toggleShowMovementIndicatorOfPlayer;
            set => Core.Settings.toggleShowMovementIndicatorOfPlayer = value;
        }

        public static bool ShowMovementIndicatorOfNonPlayer {
            get => Core.Settings.toggleShowMovementIndicatorOfNonPlayer;
            set => Core.Settings.toggleShowMovementIndicatorOfNonPlayer = value;
        }

        public static bool ShowAttackIndicatorOnHoverUI {
            get => Core.Settings.toggleShowAttackIndicatorOnHoverUI;
            set => Core.Settings.toggleShowAttackIndicatorOnHoverUI = value;
        }

        public static bool ShowMovementIndicatorOnHoverUI {
            get => Core.Settings.toggleShowMovementIndicatorOnHoverUI;
            set => Core.Settings.toggleShowMovementIndicatorOnHoverUI = value;
        }

        public static bool ShowIsFlatFootedIconOnHoverUI {
            get => Core.Settings.toggleShowIsFlatFootedIconOnHoverUI;
            set {
                if (Core.Settings.toggleShowIsFlatFootedIconOnHoverUI != value)
                {
                    Core.Settings.toggleShowIsFlatFootedIconOnHoverUI = value;
                    if (value)
                    {
                        ShowIsFlatFootedIconOnUI = false;
                    }
                }
            }
        }

        public static bool ShowIsFlatFootedIconOnUI {
            get => Core.Settings.toggleShowIsFlatFootedIconOnUI;
            set {
                if (Core.Settings.toggleShowIsFlatFootedIconOnUI != value)
                {
                    Core.Settings.toggleShowIsFlatFootedIconOnUI = value;
                    if (value)
                    {
                        ShowIsFlatFootedIconOnHoverUI = false;
                    }
                }
            }
        }

        public static bool SelectUnitOnClickUI {
            get => Core.Settings.toggleSelectUnitOnClickUI;
            set => Core.Settings.toggleSelectUnitOnClickUI = value;
        }

        public static bool CameraScrollToUnitOnClickUI {
            get => Core.Settings.toggleCameraScrollToUnitOnClickUI;
            set => Core.Settings.toggleCameraScrollToUnitOnClickUI = value;
        }

        public static bool ShowUnitDescriptionOnRightClickUI {
            get => Core.Settings.toggleShowUnitDescriptionOnRightClickUI;
            set => Core.Settings.toggleShowUnitDescriptionOnRightClickUI = value;
        }
        
        public static float HUDWidth {
            get => Core.Settings.hudWidth;
            set => Core.Settings.hudWidth = value;
        }

        public static int HUDMaxUnitsDisplayed {
            get => Core.Settings.hudMaxUnitsDisplayed;
            set => Core.Settings.hudMaxUnitsDisplayed = value;
        }

        #endregion

        #region Time Scale

        public static float TimeScaleBetweenTurns {
            get => Core.Settings.timeScaleBetweenTurns;
            set => Core.Settings.timeScaleBetweenTurns = value;
        }

        public static float TimeScaleInPlayerTurn {
            get => Core.Settings.timeScaleInPlayerTurn;
            set => Core.Settings.timeScaleInPlayerTurn = value;
        }

        public static float TimeScaleInNonPlayerTurn {
            get => Core.Settings.timeScaleInNonPlayerTurn;
            set => Core.Settings.timeScaleInNonPlayerTurn = value;
        }

        public static float CastingTimeOfFullRoundSpell {
            get => Core.Settings.castingTimeOfFullRoundSpell;
            set => Core.Settings.castingTimeOfFullRoundSpell = value;
        }

        public static float TimeToWaitForIdleAI {
            get => Core.Settings.timeToWaitForIdleAI;
            set => Core.Settings.timeToWaitForIdleAI = value;
        }

        public static float TimeToWaitForEndingTurn {
            get => Core.Settings.timeToWaitForEndingTurn;
            set => Core.Settings.timeToWaitForEndingTurn = value;
        }

        #endregion

        #region Pause

        public static bool PauseOnPlayerTurnStart {
            get => Core.Settings.togglePauseOnPlayerTurnStart;
            set => Core.Settings.togglePauseOnPlayerTurnStart = value;
        }

        public static bool PauseOnPlayerTurnEnd {
            get => Core.Settings.togglePauseOnPlayerTurnEnd;
            set => Core.Settings.togglePauseOnPlayerTurnEnd = value;
        }

        public static bool PauseOnNonPlayerTurnStart {
            get => Core.Settings.togglePauseOnNonPlayerTurnStart;
            set => Core.Settings.togglePauseOnNonPlayerTurnStart = value;
        }

        public static bool PauseOnNonPlayerTurnEnd {
            get => Core.Settings.togglePauseOnNonPlayerTurnEnd;
            set => Core.Settings.togglePauseOnNonPlayerTurnEnd = value;
        }

        public static bool PauseOnPlayerFinishFiveFoot {
            get => Core.Settings.togglePauseOnPlayerFinishFiveFoot;
            set => Core.Settings.togglePauseOnPlayerFinishFiveFoot = value;
        }

        public static bool PauseOnPlayerFinishFirstMove {
            get => Core.Settings.togglePauseOnPlayerFinishFirstMove;
            set => Core.Settings.togglePauseOnPlayerFinishFirstMove = value;
        }

        #endregion
    }
}
