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

        public string Name => "Gameplay";

        public int Priority => 0;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Mod == null || !Mod.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            }

            using (new GUILayout.HorizontalScope())
            {
                Mod.Core.Enabled =
                GUIHelper.ToggleButton(Mod.Core.Enabled,
                "Turn-Based Mode", _buttonStyle, GUILayout.ExpandWidth(false));

                if (GUILayout.Button($"Reset Settings", _buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    Mod.ResetSettings();
                    Mod.Core.Blueprint.Update();
                    Mod.Core.Hotkeys.Update();
                }
            }

            GUILayout.Space(10f);

            OnGUIMechanic();

            GUILayout.Space(10f);

            OnGUIGameplay();
        }

        void OnGUIMechanic()
        {
            SurpriseRound =
                GUIHelper.ToggleButton(SurpriseRound,
                "Surprise Round" +
                " (All unseen units get a surprise round before regular rounds)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            FlankingCountAllOpponents =
                GUIHelper.ToggleButton(FlankingCountAllOpponents,
                "Flanking Count All Opponents Within Threaten Range" +
                " (Regardless opponents' current command)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

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
                "Moving Through Friends" +
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

            AvoidOverlapping =
                GUIHelper.ToggleButton(AvoidOverlapping,
                "Try To Avoid Overlapping When Moving Through Friends" +
                " (Forbid moving through a unit if they will overlap each other)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            AvoidOverlappingOnCharge =
                GUIHelper.ToggleButton(AvoidOverlappingOnCharge,
                "Try To Avoid Overlapping When Charging" +
                " (Try to avoid obstacles and be blocked while no valid path)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            DoNotMovingThroughNonAllies =
                GUIHelper.ToggleButton(DoNotMovingThroughNonAllies,
                "DO NOT Moving Through Non-Allies" +
                " (Disable the default \"soft obstacle\" effect on non-ally units)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Radius Modifier Of Collision Detection: {RadiusOfCollision:f2}x", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                RadiusOfCollision = 
                    GUIHelper.RoundedHorizontalSlider(RadiusOfCollision, 1, 0.5f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                GUILayout.Label($"(A modifier affects all units' pathfinding, NOT AFFECT REACH)".Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }
        }

        void OnGUIGameplay()
        {
            AutoTurnOffAIOnTurnStart =
                GUIHelper.ToggleButton(AutoTurnOffAIOnTurnStart,
                "Auto Turn Off Unit's AI On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoTurnOnAIOnCombatEnd =
                GUIHelper.ToggleButton(AutoTurnOnAIOnCombatEnd,
                "Auto Turn On Unit's AI On Turn-Based Combat End", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoSelectUnitOnTurnStart =
                GUIHelper.ToggleButton(AutoSelectUnitOnTurnStart,
                "Auto Select Current Unit On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoSelectEntirePartyOnCombatEnd =
                GUIHelper.ToggleButton(AutoSelectEntirePartyOnCombatEnd,
                "Auto Select The Entire Party On Turn-Based Combat End", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnTurnStart =
                GUIHelper.ToggleButton(AutoCancelActionsOnTurnStart,
                "Auto Cancel Actions On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnCombatEnd =
                GUIHelper.ToggleButton(AutoCancelActionsOnCombatEnd,
                "Auto Cancel Actions On Turn-Based Combat End", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoEnableFiveFootStepOnTurnStart =
                GUIHelper.ToggleButton(AutoEnableFiveFootStepOnTurnStart,
                "Auto Enable 5-Foot Step On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));
            
            AutoCancelActionsOnFiveFootStepFinish =
                GUIHelper.ToggleButton(AutoCancelActionsOnFiveFootStepFinish,
                "Auto Cancel Actions On Player's Unit Finished The 5-Foot Step", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoCancelActionsOnFirstMoveFinish =
                GUIHelper.ToggleButton(AutoCancelActionsOnFirstMoveFinish,
                "Auto Cancel Actions On Player's Unit Finished The First Move Action Through Move", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            AutoEndTurnWhenActionsAreUsedUp =
                GUIHelper.ToggleButton(AutoEndTurnWhenActionsAreUsedUp,
                "Auto End Turn If All Actions Are Used Up", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoEndTurnIgnoreSwiftAction =
                GUIHelper.ToggleButton(AutoEndTurnIgnoreSwiftAction,
                "Auto End Turn If All Actions ... Except Swift Action", _buttonStyle, GUILayout.ExpandWidth(false));

            AutoEndTurnWhenPlayerIdle =
                GUIHelper.ToggleButton(AutoEndTurnWhenPlayerIdle,
                "Auto End Turn If Player's Unit Is Idle" +
                " (Can be used for auto combat)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));
        }
    }
}
