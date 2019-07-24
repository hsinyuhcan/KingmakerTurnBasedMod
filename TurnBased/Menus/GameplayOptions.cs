using ModMaker;
using ModMaker.Utility;
using UnityEngine;
using UnityModManagerNet;
using static ModMaker.Utility.RichTextExtensions;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Menus
{
    public class RestrictionsOptions : Menu.IToggleablePage
    {
        GUIStyle _buttonStyle;

        public string Name => "Gameplay";

        public int Priority => 0;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Core == null || !Core.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            }

            Core.Mod.RoundController.Enabled =
                GUIHelper.ToggleButton(Core.Mod.RoundController.Enabled,
                "Turn-Based Mode", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            OnGUIMechanic();

            GUILayout.Space(10f);

            OnGUIGameplay();
        }

        void OnGUIMechanic()
        {
            SetChargeAsFullRoundAction =
                GUIHelper.ToggleButton(SetChargeAsFullRoundAction,
                "Set Charge As Full Round Action", _buttonStyle, GUILayout.ExpandWidth(false));

            SetVitalStrikeAsStandardAction =
                GUIHelper.ToggleButton(SetVitalStrikeAsStandardAction,
                "Set Vital Strike As Standard Action", _buttonStyle, GUILayout.ExpandWidth(false));

            FixTheCostToStartBardicPerformance =
                GUIHelper.ToggleButton(FixTheCostToStartBardicPerformance,
                "Fix The Cost Of Action To Start Bardic Performance", _buttonStyle, GUILayout.ExpandWidth(false));
            
            FlankingCountAllOpponents =
                GUIHelper.ToggleButton(FlankingCountAllOpponents,
                "Flanking Count All Opponents Within Threaten Range" +
                " (Regardless opponents' current command)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Distance Modifier Of 5-Foot Step: {DistanceOfFiveFootStep:f2}x", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                DistanceOfFiveFootStep =
                    GUIHelper.RoundedHorizontalSlider(DistanceOfFiveFootStep, 1, 1f, 2f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                GUILayout.Label("(Larger value will make slower units unable to take a 5-foot step)".Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            GUILayout.Space(10f);

            MovingThroughFriends =
                GUIHelper.ToggleButton(MovingThroughFriends,
                "Moving Through Friend﻿s" +
                " (Units can move through allies)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            MovingThroughNonEnemies =
                GUIHelper.ToggleButton(MovingThroughNonEnemies,
                "Moving Through Non-Enemies" +
                " (Units can move through allies and neutral units)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            MovingThroughOnlyAffectPlayer =
                GUIHelper.ToggleButton(MovingThroughOnlyAffectPlayer,
                "Moving Through ... Only Affect Player", _buttonStyle, GUILayout.ExpandWidth(false));

            MovingThroughOnlyAffectNonEnemies =
                GUIHelper.ToggleButton(MovingThroughOnlyAffectNonEnemies,
                "Moving Through ... Only Affect Non-Enemies", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Radius Modifier Of Collision Detection: {RadiusOfCollision:f2}x", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                RadiusOfCollision = 
                    GUIHelper.RoundedHorizontalSlider(RadiusOfCollision, 1, 0.5f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                GUILayout.Label($"(A modifier affects all units)".Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }
        }

        void OnGUIGameplay()
        {
            AutoTurnOffAI =
                GUIHelper.ToggleButton(AutoTurnOffAI,
                "Auto Turn Off Unit's AI On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoTurnOnAI =
                GUIHelper.ToggleButton(AutoTurnOnAI,
                "Auto Turn On Unit's AI When Ending The Turn-Based Combat", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoSelectEntireParty =
                GUIHelper.ToggleButton(AutoSelectEntireParty,
                "Auto Select The Entire Party When Ending The Turn-Based Combat", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoSelectCurrentUnit =
                GUIHelper.ToggleButton(AutoSelectCurrentUnit,
                "Auto Select Current Unit On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoEnableFiveFootStep =
                GUIHelper.ToggleButton(AutoEnableFiveFootStep,
                "Auto Enable 5-Foot Step On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));
            
            AutoCancelActionsOnPlayerTurnStart =
                GUIHelper.ToggleButton(AutoCancelActionsOnPlayerTurnStart,
                "Auto Cancel Actions On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnPlayerFinishFiveFoot =
                GUIHelper.ToggleButton(AutoCancelActionsOnPlayerFinishFiveFoot,
                "Auto Cancel Actions On Player's Unit Finished The 5-Foot Step", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnPlayerFinishFirstMove =
                GUIHelper.ToggleButton(AutoCancelActionsOnPlayerFinishFirstMove,
                "Auto Cancel Actions On Player's Unit Finished The First Move Action Through Move", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            AutoEndTurn =
                GUIHelper.ToggleButton(AutoEndTurn,
                "Auto End Turn If Player's Unit Is Idle" +
                " (For Auto Combat)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            DoNotAutoEndTurnWhenHasSwiftAction =
                GUIHelper.ToggleButton(DoNotAutoEndTurnWhenHasSwiftAction,
                "DO NOT Auto End Turn When Current Unit Still Has The Swift Action", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            AllowCommandNonPlayerToPerformSpecialActions =
                GUIHelper.ToggleButton(AllowCommandNonPlayerToPerformSpecialActions,
                "Allow Command Non-Player Units To Perform Special Actions" +
                " (5-foot step, Delay and End Turn)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));
        }
    }
}
