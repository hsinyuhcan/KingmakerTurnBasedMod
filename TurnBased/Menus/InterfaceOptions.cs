using ModMaker;
using ModMaker.Utility;
using UnityEngine;
using UnityModManagerNet;
using static ModMaker.Utility.RichTextExtensions;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Menus
{
    public class InterfaceOptions : IMenuSelectablePage
    {
        GUIStyle _buttonStyle;

        public string Name => "Interface";

        public int Priority => 300;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Mod == null || !Mod.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            }

            OnGUICamera();

            GUILayout.Space(10f);

            OnGUIUI();

            GUILayout.Space(10f);

            DoNotMarkInvisibleUnit =
                GUIHelper.ToggleButton(DoNotMarkInvisibleUnit,
                "Do Not Mark Invisible Units" +
                " (Disable highlight, camera, indicators... etc)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            DoNotShowInvisibleUnitOnCombatTracker =
                GUIHelper.ToggleButton(DoNotShowInvisibleUnitOnCombatTracker,
                "Do Not Show Invisible Units On The Combat Tracker", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            OnGUICombatTracker();
        }

        private void OnGUICamera()
        {
            HighlightCurrentUnit =
                GUIHelper.ToggleButton(HighlightCurrentUnit,
                "Highlight Current Unit", _buttonStyle, GUILayout.ExpandWidth(false));

            CameraScrollToCurrentUnit =
                GUIHelper.ToggleButton(CameraScrollToCurrentUnit,
                "Camera Scroll To Current Unit On Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            CameraLockOnCurrentPlayerUnit =
                GUIHelper.ToggleButton(CameraLockOnCurrentPlayerUnit,
                "Camera Lock On Current Player Unit", _buttonStyle, GUILayout.ExpandWidth(false));

            CameraLockOnCurrentNonPlayerUnit =
                GUIHelper.ToggleButton(CameraLockOnCurrentNonPlayerUnit,
                "Camera Lock On Current Non-Player Unit", _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIUI()
        {
            ShowAttackIndicatorOfCurrentUnit =
               GUIHelper.ToggleButton(ShowAttackIndicatorOfCurrentUnit,
               "Show Attack Indicator Of Current Unit", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowAttackIndicatorOfPlayer =
                GUIHelper.ToggleButton(ShowAttackIndicatorOfPlayer,
                "Show Attack Indicator ... Of Player", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowAttackIndicatorOfNonPlayer =
                GUIHelper.ToggleButton(ShowAttackIndicatorOfNonPlayer,
                "Show Attack Indicator ... Of Non-Player", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowAttackIndicatorOnHoverUI =
                GUIHelper.ToggleButton(ShowAttackIndicatorOnHoverUI,
                "Show Attack Indicator When Mouse Hover The UI Element", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowAutoCastAbilityRange =
                GUIHelper.ToggleButton(ShowAutoCastAbilityRange,
                "Show Ability Range Instead Of Attack Range When Using Auto Cast", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            ShowMovementIndicatorOfCurrentUnit =
                GUIHelper.ToggleButton(ShowMovementIndicatorOfCurrentUnit,
                "Show Movement Indicator Of Current Unit", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowMovementIndicatorOfPlayer =
                GUIHelper.ToggleButton(ShowMovementIndicatorOfPlayer,
                "Show Movement Indicator ... Of Player", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowMovementIndicatorOfNonPlayer =
                GUIHelper.ToggleButton(ShowMovementIndicatorOfNonPlayer,
                "Show Movement Indicator ... Of Non-Player", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowMovementIndicatorOnHoverUI =
                GUIHelper.ToggleButton(ShowMovementIndicatorOnHoverUI,
                "Show Movement Indicator When Mouse Hover The UI Element", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            ShowIsFlatFootedIconOnUI =
                GUIHelper.ToggleButton(ShowIsFlatFootedIconOnUI,
                "Show An Icon To Indicate If The Unit Lost Dexterity Bonus To AC", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowIsFlatFootedIconOnHoverUI =
                GUIHelper.ToggleButton(ShowIsFlatFootedIconOnHoverUI,
                "Show An Icon To Indicate If The Unit Lost Dexterity Bonus To AC When Mouse Hover The UI Element", _buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.Space(10f);

            SelectUnitOnClickUI =
                GUIHelper.ToggleButton(SelectUnitOnClickUI,
                "Select Unit When Click The UI Element", _buttonStyle, GUILayout.ExpandWidth(false));

            CameraScrollToUnitOnClickUI =
                GUIHelper.ToggleButton(CameraScrollToUnitOnClickUI,
                "Camera Scroll To Unit When Click The UI Element", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowUnitDescriptionOnRightClickUI =
                GUIHelper.ToggleButton(ShowUnitDescriptionOnRightClickUI,
                "Show Unit Description When Right Click The UI Element", _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUICombatTracker()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Combat Tracker Width: {(int)CombatTrackerWidth:d3}", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                CombatTrackerWidth =
                    GUIHelper.RoundedHorizontalSlider(CombatTrackerWidth, 0, 250f, 500f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Combat Tracker Max Units: {CombatTrackerMaxUnits:d2}", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                CombatTrackerMaxUnits =
                    (int)GUIHelper.RoundedHorizontalSlider(CombatTrackerMaxUnits, 0, 5f, 25f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }
        }
    }
}
