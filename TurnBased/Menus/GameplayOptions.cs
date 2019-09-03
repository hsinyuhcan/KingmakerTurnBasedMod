using ModMaker;
using ModMaker.Utility;
using UnityEngine;
using UnityModManagerNet;
using static ModMaker.Utility.RichTextExtensions;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Menus
{
    public class RestrictionsOptions : IMenuSelectablePage
    {
        GUIStyle _buttonStyle;
        GUIStyle _labelStyle;

        public string Name => Local["Menu_Tab_Gameplay"];

        public int Priority => 0;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Mod == null || !Mod.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
                _labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, padding = _buttonStyle.padding };
            }

            using (new GUISubScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    Mod.Core.Enabled =
                        GUIHelper.ToggleButton(Mod.Core.Enabled,
                        Local["Menu_Opt_TrunBasedMode"], _buttonStyle, GUILayout.ExpandWidth(false));

                    if (GUILayout.Button(Local["Menu_Btn_ResetSettings"], _buttonStyle, GUILayout.ExpandWidth(false)))
                    {
                        Mod.Core.ResetSettings();
                    }
                }
            }

            using (new GUISubScope(Local["Menu_Sub_Rule"]))
                OnGUIRule();

            using (new GUISubScope(Local["Menu_Sub_Pathfinding"]))
                OnGUIPathfinding();

            using (new GUISubScope(Local["Menu_Sub_Automation"]))
                OnGUIAutomation();
        }

        private void OnGUIRule()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(DistanceOfFiveFootStep != 1f,
                    string.Format(Local["Menu_Opt_DistanceOfFiveFootStep"], DistanceOfFiveFootStep), _labelStyle, GUILayout.ExpandWidth(false));
                DistanceOfFiveFootStep =
                   GUIHelper.RoundedHorizontalSlider(DistanceOfFiveFootStep, 1, 1f, 2f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Label(Local["Menu_Cmt_DistanceOfFiveFootStep"].Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            SurpriseRound =
                GUIHelper.ToggleButton(SurpriseRound,
                Local["Menu_Opt_SurpriseRound"] +
                Local["Menu_Cmt_SurpriseRound"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            PreventUnconsciousUnitLeavingCombat =
                GUIHelper.ToggleButton(PreventUnconsciousUnitLeavingCombat,
                Local["Menu_Opt_PreventUnconsciousUnitLeavingCombat"] +
                Local["Menu_Cmt_PreventUnconsciousUnitLeavingCombat"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            FlankingCountAllNearbyOpponents =
                GUIHelper.ToggleButton(FlankingCountAllNearbyOpponents,
                Local["Menu_Opt_FlankingCountAllNearbyOpponents"] +
                Local["Menu_Cmt_FlankingCountAllNearbyOpponents"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            RerollPerceptionDiceAgainstStealthOncePerRound =
                GUIHelper.ToggleButton(RerollPerceptionDiceAgainstStealthOncePerRound,
                Local["Menu_Opt_RerollPerceptionDiceAgainstStealthOncePerRound"] +
                Local["Menu_Cmt_RerollPerceptionDiceAgainstStealthOncePerRound"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIPathfinding()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(RadiusOfCollision != 1f, 
                    string.Format(Local["Menu_Opt_RadiusOfCollision"], RadiusOfCollision), _labelStyle, GUILayout.ExpandWidth(false));
                RadiusOfCollision =
                    GUIHelper.RoundedHorizontalSlider(RadiusOfCollision, 1, 0.5f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Label(Local["Menu_Cmt_RadiusOfCollision"].Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            MovingThroughFriendlyUnit =
                GUIHelper.ToggleButton(MovingThroughFriendlyUnit,
                Local["Menu_Opt_MovingThroughFriendlyUnit"] +
                Local["Menu_Cmt_MovingThroughFriendlyUnit"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            MovingThroughNonHostileUnit =
                GUIHelper.ToggleButton(MovingThroughNonHostileUnit,
                Local["Menu_Opt_MovingThroughNonHostileUnit"] +
                Local["Menu_Cmt_MovingThroughNonHostileUnit"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            using (new GUILayout.HorizontalScope())
            {
                MovingThroughApplyToPlayer =
                    GUIHelper.ToggleButton(MovingThroughApplyToPlayer,
                    Local["Menu_Opt_MovingThroughApplyToPlayer"], _buttonStyle, GUILayout.ExpandWidth(false));

                MovingThroughApplyToNeutralUnit =
                    GUIHelper.ToggleButton(MovingThroughApplyToNeutralUnit,
                    Local["Menu_Opt_MovingThroughApplyToNeutralUnit"], _buttonStyle, GUILayout.ExpandWidth(false));

                MovingThroughApplyToEnemy =
                    GUIHelper.ToggleButton(MovingThroughApplyToEnemy,
                    Local["Menu_Opt_MovingThroughApplyToEnemy"], _buttonStyle, GUILayout.ExpandWidth(false));
            }

            AvoidOverlapping =
                GUIHelper.ToggleButton(AvoidOverlapping,
                Local["Menu_Opt_AvoidOverlapping"] +
                Local["Menu_Cmt_AvoidOverlapping"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            AvoidOverlappingOnCharge =
                GUIHelper.ToggleButton(AvoidOverlappingOnCharge,
                Local["Menu_Opt_AvoidOverlappingOnCharge"]  +
                Local["Menu_Cmt_AvoidOverlappingOnCharge"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            DoNotMovingThroughNonAlly =
                GUIHelper.ToggleButton(DoNotMovingThroughNonAlly,
                Local["Menu_Opt_DoNotMovingThroughNonAlly"] +
                Local["Menu_Cmt_DoNotMovingThroughNonAlly"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIAutomation()
        {
            AutoTurnOffAIOnTurnStart =
                GUIHelper.ToggleButton(AutoTurnOffAIOnTurnStart,
                Local["Menu_Opt_AutoTurnOffAIOnTurnStart"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoTurnOnAIOnCombatEnd =
                GUIHelper.ToggleButton(AutoTurnOnAIOnCombatEnd,
                Local["Menu_Opt_AutoTurnOnAIOnCombatEnd"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoSelectUnitOnTurnStart =
                GUIHelper.ToggleButton(AutoSelectUnitOnTurnStart,
                Local["Menu_Opt_AutoSelectUnitOnTurnStart"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoSelectEntirePartyOnCombatEnd =
                GUIHelper.ToggleButton(AutoSelectEntirePartyOnCombatEnd,
                Local["Menu_Opt_AutoSelectEntirePartyOnCombatEnd"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnTurnStart =
                GUIHelper.ToggleButton(AutoCancelActionsOnTurnStart,
                Local["Menu_Opt_AutoCancelActionsOnTurnStart"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnCombatEnd =
                GUIHelper.ToggleButton(AutoCancelActionsOnCombatEnd,
                Local["Menu_Opt_AutoCancelActionsOnCombatEnd"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnFiveFootStepFinish =
                GUIHelper.ToggleButton(AutoCancelActionsOnFiveFootStepFinish,
                Local["Menu_Opt_AutoCancelActionsOnFiveFootStepFinish"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnFirstMoveFinish =
                GUIHelper.ToggleButton(AutoCancelActionsOnFirstMoveFinish,
                Local["Menu_Opt_AutoCancelActionsOnFirstMoveFinish"], _buttonStyle, GUILayout.ExpandWidth(false));

            AutoEnableFiveFootStepOnTurnStart =
                GUIHelper.ToggleButton(AutoEnableFiveFootStepOnTurnStart,
                Local["Menu_Opt_AutoEnableFiveFootStepOnTurnStart"], _buttonStyle, GUILayout.ExpandWidth(false));

            using (new GUILayout.HorizontalScope())
            {
                AutoEndTurnWhenActionsAreUsedUp =
                    GUIHelper.ToggleButton(AutoEndTurnWhenActionsAreUsedUp,
                    Local["Menu_Opt_AutoEndTurnWhenActionsAreUsedUp"], _buttonStyle, GUILayout.ExpandWidth(false));

                AutoEndTurnExceptSwiftAction =
                    GUIHelper.ToggleButton(AutoEndTurnExceptSwiftAction,
                    Local["Menu_Opt_AutoEndTurnExceptSwiftAction"], _buttonStyle, GUILayout.ExpandWidth(false));
            }

            AutoEndTurnWhenPlayerIdle =
                GUIHelper.ToggleButton(AutoEndTurnWhenPlayerIdle,
                Local["Menu_Opt_AutoEndTurnWhenPlayerIdle"] +
                Local["Menu_Cmt_AutoEndTurnWhenPlayerIdle"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));
        }
    }
}
