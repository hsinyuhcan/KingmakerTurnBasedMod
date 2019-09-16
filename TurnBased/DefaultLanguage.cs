using Kingmaker.EntitySystem.Persistence.JsonUtility;
using ModMaker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace TurnBased
{
    [JsonObject(MemberSerialization.OptOut)]
    public class DefaultLanguage : ILanguage
    {
        [JsonProperty]
        public string Language { get; set; } = "English (Default)";

        [JsonProperty]
        public Version Version { get; set; }

        [JsonProperty]
        public string Contributors { get; set; }

        [JsonProperty]
        public string HomePage { get; set; }

        [JsonProperty]
        public Dictionary<string, string> Strings { get; set; } = new Dictionary<string, string>()
        {
            { "Menu_Tab_Gameplay", "Gameplay" },
            { "Menu_Opt_TrunBasedMode", "Turn-Based Mode" },
            { "Menu_Btn_ResetSettings", "Reset Settings" },
            { "Menu_Sub_Rule", "Rule" },
            { "Menu_Opt_DistanceOfFiveFootStep", "Distance Modifier Of 5-Foot Step: {0:f2}x" },
            { "Menu_Cmt_DistanceOfFiveFootStep", " (Larger value will make slower units unable to take a 5-foot step)" },
            { "Menu_Opt_SurpriseRound", "Surprise Round" },
            { "Menu_Cmt_SurpriseRound", " (All units that haven't been seen will get a surprise round before regular rounds)" },
            { "Menu_Opt_PreventUnconsciousUnitLeavingCombat", "Prevent Unconscious Units From Instantly Leaving Combat" },
            { "Menu_Cmt_PreventUnconsciousUnitLeavingCombat", " (Only dead units will instantly leave Combat)" },
            { "Menu_Opt_FlankingCountAllNearbyOpponents", "Flanking Count All Opponents Within Threatened Range" },
            { "Menu_Cmt_FlankingCountAllNearbyOpponents", " (Regardless unit's current command)" },
            { "Menu_Opt_RerollPerceptionDiceAgainstStealthOncePerRound", "Re-roll Perception Dice Against Stealth Once Per Round" },
            { "Menu_Cmt_RerollPerceptionDiceAgainstStealthOncePerRound", " (Instead of rolling once per combat)" },
            { "Menu_Sub_Pathfinding", "Pathfinding" },
            { "Menu_Opt_RadiusOfCollision", "Radius Modifier Of Collision Detection: {0:f2}x" },
            { "Menu_Cmt_RadiusOfCollision", " (A modifier affecting pathfinding for all units, NOT AFFECT REACH)" },
            { "Menu_Opt_MovingThroughFriendlyUnit", "Moving Through Friendly Units" },
            { "Menu_Cmt_MovingThroughFriendlyUnit", " (Units can move through ally)" },
            { "Menu_Opt_MovingThroughNonHostileUnit", "Moving Through Non-Hostile Units" },
            { "Menu_Cmt_MovingThroughNonHostileUnit", " (Units can move through ally and neutral units)" },
            { "Menu_Opt_MovingThroughApplyToPlayer", "... Apply To Player" },
            { "Menu_Opt_MovingThroughApplyToNeutralUnit", "... Apply To Neutral Units" },
            { "Menu_Opt_MovingThroughApplyToEnemy", "... Apply To Enemy" },
            { "Menu_Opt_AvoidOverlapping", "Try To Avoid Overlapping When Moving Through" },
            { "Menu_Cmt_AvoidOverlapping", " (Avoid moving through a unit if they will overlap each other)" },
            { "Menu_Opt_AvoidOverlappingOnCharge", "Try To Avoid Overlapping When Charging" },
            { "Menu_Cmt_AvoidOverlappingOnCharge", " (Apply normal pathfinding rule instead of ignore all obstacles)" },
            { "Menu_Opt_DoNotMovingThroughNonAlly", "DO NOT Moving Through Non-Ally Units" },
            { "Menu_Cmt_DoNotMovingThroughNonAlly", " (Disable the default \"soft obstacle\" effect on non-ally units)" },
            { "Menu_Sub_Automation", "Automation" },
            { "Menu_Opt_AutoTurnOffAIOnTurnStart", "Auto Turn Off Unit's AI On Player's Turn Start" },
            { "Menu_Opt_AutoTurnOnAIOnCombatEnd", "Auto Turn On Party's AI On Turn-Based Combat End" },
            { "Menu_Opt_AutoSelectUnitOnTurnStart", "Auto Select Current Unit On Player's Turn Start" },
            { "Menu_Opt_AutoSelectEntirePartyOnCombatEnd", "Auto Select The Entire Party On Turn-Based Combat End" },
            { "Menu_Opt_AutoCancelActionsOnTurnStart", "Auto Cancel Actions On Player's Turn Start" },
            { "Menu_Opt_AutoCancelActionsOnCombatEnd", "Auto Cancel Actions On Turn-Based Combat End" },
            { "Menu_Opt_AutoCancelActionsOnFiveFootStepFinish", "Auto Cancel Actions On Player's Unit Finished The 5-Foot Step" },
            { "Menu_Opt_AutoCancelActionsOnFirstMoveFinish", "Auto Cancel Actions On Player's Unit Finished The First Move Action Through Moving" },
            { "Menu_Opt_AutoEnableFiveFootStepOnTurnStart", "Auto Enable 5-Foot Step On Player's Turn Start" },
            { "Menu_Opt_AutoEndTurnWhenActionsAreUsedUp", "Auto End Turn If All Actions Are Used Up" },
            { "Menu_Opt_AutoEndTurnExceptSwiftAction", "... Except Swift Action" },
            { "Menu_Opt_AutoEndTurnWhenPlayerIdle", "Auto End Turn If Player's Unit Is Idle" },
            { "Menu_Cmt_AutoEndTurnWhenPlayerIdle", " (Can be used for auto combat)" },
            { "Menu_Tab_Interface", "Interface" },
            { "Menu_Opt_DoNotMarkInvisibleUnit", "DO NOT Mark Invisible Units" },
            { "Menu_Cmt_DoNotMarkInvisibleUnit", " (Disable highlight, camera, indicators... etc)" },
            { "Menu_Opt_DoNotShowInvisibleUnitOnCombatTracker", "DO NOT Show Invisible Units On The Combat Tracker" },
            { "Menu_Cmt_DoNotShowInvisibleUnitOnCombatTracker", " (Display them as \"Unknown\" when they're acting)" },
            { "Menu_Sub_CombatTracker", "Combat Tracker" },
            { "Menu_Opt_CombatTrackerScale", "Size Scale: {0:f2}x" },
            { "Menu_Opt_CombatTrackerWidth", "Width: {0:d3}" },
            { "Menu_Opt_CombatTrackerMaxUnits", "Max Units: {0:d2}" },
            { "Menu_Opt_CameraScrollToUnitOnClickUI", "Camera Scroll To Unit On Click" },
            { "Menu_Opt_SelectUnitOnClickUI", "Select Unit On Click" },
            { "Menu_Opt_InspectOnRightClickUI", "Inspect Unit On Right Click" },
            { "Menu_Opt_ShowIsFlatFootedIconOnUI", "Show An Exclamation Mark Icon If The Unit Lost Dexterity Bonus To AC" },
            { "Menu_Sub_View", "View" },
            { "Menu_Opt_HighlightCurrentUnit", "Highlight Current Unit" },
            { "Menu_Opt_CameraScrollToCurrentUnit", "Camera Scroll To Current Unit On Turn Start" },
            { "Menu_Opt_CameraLockOnCurrentPlayerUnit", "Camera Lock On Current Player Unit" },
            { "Menu_Cmt_CameraLockOnCurrentPlayerUnit", " (Auto unlock on game pause)" },
            { "Menu_Opt_CameraLockOnCurrentNonPlayerUnit", "Camera Lock On Current Non-Player Unit" },
            { "Menu_Sub_AttackIndicator", "Attack Indicator" },
            { "Menu_Opt_ShowAttackIndicatorOfCurrentUnit", "Show Attack Indicator Of Current Unit" },
            { "Menu_Opt_ShowAttackIndicatorForPlayer", "... For Player" },
            { "Menu_Opt_ShowAttackIndicatorForNonPlayer", "... For Non-Player" },
            { "Menu_Opt_ShowAttackIndicatorOnHoverUI", "Show Attack Indicator On Hover" },
            { "Menu_Opt_ShowAutoCastAbilityRange", "Show Ability Range Instead Of Attack Range When Using Auto-Cast" },
            { "Menu_Opt_CheckForObstaclesOnTargeting", "Check If There Is Any Obstacle When Determining Whether The Target Is Within Range" },
            { "Menu_Sub_MovementIndicator", "Movement Indicator" },
            { "Menu_Opt_ShowMovementIndicatorOfCurrentUnit", "Show Movement Indicator Of Current Unit" },
            { "Menu_Opt_ShowMovementIndicatorForPlayer", "... For Player" },
            { "Menu_Opt_ShowMovementIndicatorForNonPlayer", "... For Non-Player" },
            { "Menu_Opt_ShowMovementIndicatorOnHoverUI", "Show Movement Indicator On Hover" },
            { "Menu_Tab_HotkeyAndTime", "Hotkey & Time" },
            { "Menu_Sub_Hotkey", "Hotkey" },
            { "Menu_Btn_Set", "Set" },
            { "Menu_Btn_Clear", "Clear" },
            { "Menu_Txt_Duplicated", "Duplicated!!" },
            { "Menu_Opt_ToggleFiveFootStepOnRightClickGround", "Toggle 5-foot Step When Right Click On The Ground" },
            { "Menu_Sub_Time", "Time" },
            { "Menu_Opt_TimeScaleBetweenTurns", "Time Scale Multiplier Between Turns: {0:f2}x" },
            { "Menu_Opt_TimeScaleInPlayerTurn", "Time Scale Multiplier For Player Units: {0:f2}x" },
            { "Menu_Opt_TimeScaleInNonPlayerTurn", "Time Scale Multiplier For Non-Player Units: {0:f2}x" },
            { "Menu_Opt_TimeScaleInUnknownTurn", "Time Scale Multiplier For Unknown Units: {0:f2}x" },
            { "Menu_Opt_MaxDelayBetweenIterativeAttacks", "Max Delay Between Iterative Attacks: {0:f2}s" },
            { "Menu_Cmt_MaxDelayBetweenIterativeAttacks", " (It's 6 second / attacks count by default)" },
            { "Menu_Opt_CastingTimeOfFullRoundSpell", "Casting Time Multiplier Of Full Round Spell: {0:f2}x" },
            { "Menu_Cmt_CastingTimeOfFullRoundSpell", " (The animation of casting a full round spell is 6 seconds by default)" },
            { "Menu_Opt_TimeToWaitForIdleAI", "Time To Wait For Idle AI: {0:f2}s" },
            { "Menu_Opt_TimeToWaitForEndingTurn", "Time To Wait For Ending Turn: {0:f2}s" },
            { "Menu_Sub_Pause", "Pause" },
            { "Menu_Opt_DoNotPauseOnCombatStart", "DO NOT Auto Pause On Turn-Based Combat Start" },
            { "Menu_Cmt_DoNotPauseOnCombatStart", " (Ignore the game setting)" },
            { "Menu_Opt_PauseOnPlayerTurnStart", "Pause On Player's Turn Start" },
            { "Menu_Opt_PauseOnPlayerTurnEnd", "Pause On Player's Turn End" },
            { "Menu_Opt_PauseOnNonPlayerTurnStart", "Pause On Non-Player's Turn Start" },
            { "Menu_Opt_PauseOnNonPlayerTurnEnd", "Pause On Non-Player's Turn End" },
            { "Menu_Opt_PauseOnPlayerFinishFiveFoot", "Pause On Player's Unit Finished The 5-Foot Step" },
            { "Menu_Opt_PauseOnPlayerFinishFirstMove", "Pause On Player's Unit Finished The First Move Action Through Moving" },
            { "Menu_Tab_Bugfix", "Bugfix" },
            { "Menu_Btn_TB", "TB" },
            { "Menu_Btn_RT", "RT" },
            { "Menu_Opt_FixNeverInCombatWithoutMC", "Fix when the main character is not in your party, the game will never consider that player is in combat" },
            { "Menu_Opt_FixCombatNotEndProperly", "Fix combat will not end properly if it's ended by a cutsense" },
            { "Menu_Opt_FixUnitNotLeaveCombatWhenNotInGame", "Fix units will never leave combat if they become inactive (cause a glitch on Call Forth Kanerah / Kalikke)" },
            { "Menu_Opt_FixActionTypeOfBardicPerformance", "Fix the action type of starting a Bardic Performance and the effect of Singing Steel" },
            { "Menu_Opt_FixActionTypeOfCharge", "Fix the action type of Charge (Standard Action => Full Round Action)" },
            { "Menu_Opt_FixActionTypeOfOverrun", "Fix the action type of Overrun (Standard Action => Full Round Action)" },
            { "Menu_Opt_FixActionTypeOfVitalStrike", "Fix the action type of Vital Strike (Full Round Action => Standard Action)" },
            { "Menu_Opt_FixActionTypeOfAngelicForm", "Fix the action type of Angelic Form (Standard Action => Move Action)" },
            { "Menu_Opt_FixActionTypeOfKineticBlade", "Fix activating Kinetic Blade is regarded as drawing weapon and consumes an additional standard action" },
            { "Menu_Opt_FixKineticistWontStopPriorCommand", "Fix Kineticist will not stop its previous action if you command it to attack with Kinetic Blade before combat" },
            { "Menu_Opt_FixSpellstrikeOnNeutralUnit", "Fix Spellstrike does not take effect when attacking a neutral target" },
            { "Menu_Opt_FixSpellstrikeWithMetamagicReach", "Fix Spellstrike does not take effect when using Metamagic (Reach) on a touch spell" },
            { "Menu_Opt_FixDamageBonusOfBlastRune", "Fix the damage bonus of Blast Rune isn't increased with class level (it's the first ability of Rune Domain)" },
            { "Menu_Opt_FixOnePlusDiv2ToDiv2", "Fix the damage bonus of some abilities (level / 2 + 1 => level / 2) (Blast Rune, Moonfire)" },
            { "Menu_Opt_FixFxOfShadowEvocationSirocco", "Fix the Fx effect of Shadow Evocation (Sirocco) is missing (use normal Sirocco to replace it)" },
            { "Menu_Opt_FixAbilityNotAutoDeactivateIfCombatEnded", "Fix some abilities will not be auto deactivated after combat (Inspire Greatness, Inspire Heroics)" },
            { "Menu_Opt_FixBlindFightDistance", "Fix Blind-Fight needs a extreme close distance to prevent from losing AC instead of melee distance" },
            { "Menu_Opt_FixDweomerLeap", "Fix Dweomer Leap can be triggered by ally and always consumes no action (it should consume a swift action)" },
            { "Menu_Opt_FixConfusedUnitCanAttackDeadUnit", "Fix sometimes a confused unit can act normally because it tried to attack an unattackable dead unit" },
            { "Menu_Opt_FixHasMotionThisTick", "Fix sometimes the game does not regard a unit that is forced to move as a unit that is moving (cause AoO behavior inconsistent)" },
            { "Menu_Opt_FixCanMakeAttackOfOpportunityToUnmovedTarget", "Fix that you can make an AoO to an unmoved unit just as it's leaving the threatened range (when switching from reach weapon)" },
            { "Menu_Opt_FixAbilityCircleRadius", "Fix the visual circle of certain abilities is inconsistent with the real range" },
            { "Menu_Opt_FixAbilityCircleNotAppear", "Fix the ability circle does not appear properly when first time selecting an ability of any unit (using a hotkey)" },
            { "Menu_Opt_FixAbilityCanTargetUntargetableUnit", "Fix you can target untargetable units using abilities" },
            { "Menu_Opt_FixAbilityCanTargetDeadUnit", "Fix you can target dead units using abilities that cannot be cast to dead target" },
            { "Menu_Tab_Language", "Language" },
            { "Menu_Sub_Current", "Current" },
            { "Menu_Txt_Language", "Language: {0}" },
            { "Menu_Txt_Version", "Version: {0}" },
            { "Menu_Txt_Contributors", "Contributors: {0}" },
            { "Menu_Txt_HomePage", "Home Page:" },
            { "Menu_Btn_Export", "Export: {0}" },
            { "Menu_Btn_SortAndExport", "Sort And Export: {0}" },
            { "Menu_Cmt_SortAndExport", " (Warning: it will delete all unused entries, too)" },
            { "Menu_Txt_FaildToExport", "Faild to export: {0}" },
            { "Menu_Sub_Import", "Import" },
            { "Menu_Btn_RefreshFileList", "Refresh File List" },
            { "Menu_Btn_DefaultLanguage", "Default Language" },
            { "Menu_Txt_FaildToImport", "Faild to import: {0}" },
            { "Hotkey_Toggle_TurnBasedMode", "Toggle Turn-BasedMode" },
            { "Hotkey_Toggle_AttackIndicator", "Toggle Attack Indicator" },
            { "Hotkey_Toggle_MovementIndicator", "Toggle Movement Indicator" },
            { "Hotkey_Button_EndTurn", "Button: End Turn" },
            { "Hotkey_Button_Delay", "Button: Delay" },
            { "Hotkey_Button_FiveFootStep", "Button: 5-Foot Step" },
            { "Hotkey_Button_FullAttack", "Button: Full Attack" },
            { "UI_Btn_EndTurn", "End Turn" },
            { "UI_Btn_Delay", "Delay" },
            { "UI_Btn_FiveFootStep", "<size=-1>5-Feet Step</size>" },
            { "UI_Btn_FullAttack", "<size=-4>Full Attack</size>" },
            { "UI_Txt_Unknown", "Unknown" },
            { "UI_Txt_Error", "Error: Turn-Based Combat Mod" },
            { "UI_Txt_TurnBasedMode", "Mode: Turn-Based" },
            { "UI_Txt_RealTimeMode", "Mode: Real Time" },
            { "UI_Log_RoundStarted", "Round {0} started." },
            { "UI_Log_SurpriseRoundStarted", "Surprise round started." }
        };

        public T Deserialize<T>(TextReader reader)
        {
            DefaultJsonSettings.Initialize();
            return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        }

        public void Serialize<T>(TextWriter writer, T obj)
        {
            DefaultJsonSettings.Initialize();
            writer.Write(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }
}
