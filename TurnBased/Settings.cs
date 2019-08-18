using Kingmaker.UI.SettingsUI;
using ModMaker.Utility;
using TurnBased.Utility;
using UnityModManagerNet;

namespace TurnBased
{
    public class Settings : UnityModManager.ModSettings
    {
        // gameplay
        public bool toggleSurpriseRound = true;
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

        public bool toggleAutoTurnOffAIOnTurnStart = true;
        public bool toggleAutoTurnOnAIOnCombatEnd = true;
        public bool toggleAutoSelectUnitOnTurnStart = true;
        public bool toggleAutoSelectEntirePartyOnCombatEnd = true;
        public bool toggleAutoCancelActionsOnTurnStart = true;
        public bool toggleAutoCancelActionsOnCombatEnd = true;
        public bool toggleAutoEnableFiveFootStepOnTurnStart;
        public bool toggleAutoCancelActionsOnFiveFootStepFinish = true;
        public bool toggleAutoCancelActionsOnFirstMoveFinish = true;

        public bool toggleAutoEndTurn;
        public bool toggleDoNotAutoEndTurnWhenHasSwiftAction = true;

        // interface
        public bool toggleHighlightCurrentUnit = true;
        public bool toggleCameraScrollToCurrentUnit = true;
        public bool toggleCameraLockOnCurrentPlayerUnit;
        public bool toggleCameraLockOnCurrentNonPlayerUnit = true;

        public bool toggleShowAttackIndicatorOfCurrentUnit = true;
        public bool toggleShowAttackIndicatorOfPlayer = true;
        public bool toggleShowAttackIndicatorOfNonPlayer;
        public bool toggleShowAttackIndicatorOnHoverUI = true;
        public bool toggleShowAutoCastAbilityRange = true;
        public bool toggleCheckForObstaclesOnTargeting = true;

        public bool toggleShowMovementIndicatorOfCurrentUnit = true;
        public bool toggleShowMovementIndicatorOfPlayer = true;
        public bool toggleShowMovementIndicatorOfNonPlayer;
        public bool toggleShowMovementIndicatorOnHoverUI = true;

        public bool toggleShowIsFlatFootedIconOnUI = true;
        public bool toggleShowIsFlatFootedIconOnHoverUI;

        public bool toggleSelectUnitOnClickUI;
        public bool toggleCameraScrollToUnitOnClickUI = true;
        public bool toggleShowUnitDescriptionOnRightClickUI = true;

        public bool toggleDoNotMarkInvisibleUnit = true;
        public bool toggleDoNotShowInvisibleUnitOnCombatTracker = true;

        public float combatTrackerScale = 0.9f;
        public float combatTrackerWidth = 350f;
        public int combatTrackerMaxUnits = 15;

        // hotkeys
        public SerializableDictionary<string, BindingKeysData> hotkeys = new SerializableDictionary<string, BindingKeysData>();
        public bool hotkeyToggleFiveFootStepOnRightClickGround;

        // time scale
        public float minimumFPS = 12f;
        public float timeScaleBetweenTurns = 5f;
        public float timeScaleInPlayerTurn = 1f;
        public float timeScaleInNonPlayerTurn = 2f;
        public float castingTimeOfFullRoundSpell = 0.5f;
        public float timeToWaitForIdleAI = 0.5f;
        public float timeToWaitForEndingTurn = 0.1f;

        // bugfix
        public BugfixOption toggleFixNeverInCombatWithoutMC = new BugfixOption(true, false);
        public BugfixOption toggleFixActionTypeOfBardicPerformance = new BugfixOption(true, false);
        public BugfixOption toggleFixActionTypeOfCharge = new BugfixOption(true, false);
        public BugfixOption toggleFixActionTypeOfOverrun = new BugfixOption(true, false);
        public BugfixOption toggleFixActionTypeOfVitalStrike = new BugfixOption(true, false);
        public BugfixOption toggleFixActionTypeOfAngelicForm = new BugfixOption(true, false);
        public BugfixOption toggleFixActionTypeOfKineticBlade = new BugfixOption(true, false);
        public BugfixOption toggleFixKineticistWontStopPriorCommand = new BugfixOption(true, false);
        public BugfixOption toggleFixSpellstrikeOnNeutralUnit = new BugfixOption(true, false);
        public BugfixOption toggleFixSpellstrikeWithMetamagicReach = new BugfixOption(true, false);
        public BugfixOption toggleFixAbilityNotAutoDeactivateIfCombatEnded = new BugfixOption(true, false);
        public BugfixOption toggleFixHasMotionThisTick = new BugfixOption(true, false);
        public BugfixOption toggleFixAbilityCircleRadius = new BugfixOption(true, false);
        public BugfixOption toggleFixAbilityCircleNotAppear = new BugfixOption(true, false);

        // pause
        public bool toggleDoNotPauseOnCombatStart = true;
        public bool togglePauseOnPlayerTurnStart;
        public bool togglePauseOnPlayerTurnEnd;
        public bool togglePauseOnNonPlayerTurnStart;
        public bool togglePauseOnNonPlayerTurnEnd;
        public bool togglePauseOnPlayerFinishFiveFoot;
        public bool togglePauseOnPlayerFinishFirstMove;
    }
}