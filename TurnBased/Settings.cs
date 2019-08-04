using Kingmaker.UI.SettingsUI;
using ModMaker.Utility;
using UnityModManagerNet;

namespace TurnBased
{
    public class Settings : UnityModManager.ModSettings
    {
        // gameplay
        public bool toggleSetChargeAsFullRoundAction = true;
        public bool toggleSetVitalStrikeAsStandardAction = true;
        public bool toggleFixTheCostToStartBardicPerformance = true;
        public bool toggleFlankingCountAllOpponentsWithinThreatenRange = true;
        public float distanceOfFiveFootStep = 1.5f;

        public bool toggleMovingThroughFriends = true;
        public bool toggleMovingThroughNonEnemies;
        public bool toggleMovingThroughOnlyAffectPlayer;
        public bool toggleMovingThroughOnlyAffectNonEnemies;
        public bool toggleAvoidOverlapping = true;
        public bool toggleAvoidOverlappingOnCharge = true;
        public bool toggleDoNotMovingThroughNonAllies = true;
        public float radiusOfCollision = 0.9f;

        public bool toggleAutoTurnOffPlayerAI = true;
        public bool toggleAutoTurnOnPlayerAI = true;
        public bool toggleAutoSelectCurrentUnit = true;
        public bool toggleAutoSelectEntireParty = true;
        public bool toggleAutoCancelActionsOnPlayerTurnStart = true;
        public bool toggleAutoCancelActionsOnCombatEnd = true;
        public bool toggleAutoEnableFiveFootStep;
        public bool toggleAutoCancelActionsOnPlayerFinishFiveFoot = true;
        public bool toggleAutoCancelActionsOnPlayerFinishFirstMove = true;

        public bool toggleAutoEndTurn;
        public bool toggleDoNotAutoEndTurnWhenHasSwiftAction = true;

        public bool toggleAllowCommandNonPlayerToPerformSpecialActions;

        // interface
        public bool toggleHighlightCurrentUnit = true;
        public bool toggleCameraScrollToCurrentUnit = true;
        public bool toggleCameraLockOnCurrentPlayerUnit;
        public bool toggleCameraLockOnCurrentNonPlayerUnit = true;

        public bool toggleShowAttackIndicatorOfCurrentUnit = true;
        public bool toggleShowAttackIndicatorOfPlayer = true;
        public bool toggleShowAttackIndicatorOfNonPlayer;
        public bool toggleShowMovementIndicatorOfCurrentUnit = true;
        public bool toggleShowMovementIndicatorOfPlayer = true;
        public bool toggleShowMovementIndicatorOfNonPlayer;

        public bool toggleShowAttackIndicatorOnHoverUI = true;
        public bool toggleShowMovementIndicatorOnHoverUI = true;
        public bool toggleShowIsFlatFootedIconOnHoverUI;
        public bool toggleShowIsFlatFootedIconOnUI = true;
        public bool toggleSelectUnitOnClickUI;
        public bool toggleCameraScrollToUnitOnClickUI = true;
        public bool toggleShowUnitDescriptionOnRightClickUI = true;

        public bool toggleDoNotMarkInvisibleUnit = true;
        public bool toggleDoNotShowInvisibleUnitOnCombatTracker = true;

        public int CombatTrackerMaxUnits = 15;
        public float combatTrackerWidth = 375f;

        // hotkeys
        public SerializableDictionary<string, BindingKeysData> hotkeys = new SerializableDictionary<string, BindingKeysData>();

        // time scale
        public float timeScaleBetweenTurns = 5f;
        public float timeScaleInPlayerTurn = 1f;
        public float timeScaleInNonPlayerTurn = 2f;
        public float castingTimeOfFullRoundSpell = 0.5f;
        public float timeToWaitForIdleAI = 0.5f;
        public float timeToWaitForEndingTurn = 0.1f;

        // pause
        public bool togglePauseOnPlayerTurnStart;
        public bool togglePauseOnPlayerTurnEnd;
        public bool togglePauseOnNonPlayerTurnStart;
        public bool togglePauseOnNonPlayerTurnEnd;
        public bool togglePauseOnPlayerFinishFiveFoot;
        public bool togglePauseOnPlayerFinishFirstMove;
    }
}
