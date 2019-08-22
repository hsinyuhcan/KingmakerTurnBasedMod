using ModMaker;
using ModMaker.Utility;
using TurnBased.Utility;
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
        GUIStyle _labelStyle;

        public string Name => "Interface";

        public int Priority => 200;

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
                DoNotMarkInvisibleUnit =
                    GUIHelper.ToggleButton(DoNotMarkInvisibleUnit,
                    "DO NOT Mark Invisible Units" +
                    " (Disable highlight, camera, indicators... etc)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

                DoNotShowInvisibleUnitOnCombatTracker =
                    GUIHelper.ToggleButton(DoNotShowInvisibleUnitOnCombatTracker,
                    "DO NOT Show Invisible Units On The Combat Tracker", _buttonStyle, GUILayout.ExpandWidth(false));
            }

            using (new GUISubScope("Combat Tracker"))
                OnGUICombatTracker();

            using (new GUISubScope("View"))
                OnGUIView();

            using (new GUISubScope("Attack Indicator"))
                OnGUIAttackIndicator();

            using (new GUISubScope("Movement Indicator"))
                OnGUIMovementIndicator();
        }

        private void OnGUICombatTracker()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(CombatTrackerScale != 1f,
                  $"Size Scale: {CombatTrackerScale:f2}", _labelStyle, GUILayout.ExpandWidth(false));
                CombatTrackerScale =
                    GUIHelper.RoundedHorizontalSlider(CombatTrackerScale, 2, 0.8f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true, 
                    $"Width: {(int)CombatTrackerWidth:d3}", _labelStyle, GUILayout.ExpandWidth(false));
                CombatTrackerWidth =
                    GUIHelper.RoundedHorizontalSlider(CombatTrackerWidth, 0, 250f, 500f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true, 
                    $"Units: {CombatTrackerMaxUnits:d2}", _labelStyle, GUILayout.ExpandWidth(false));
                CombatTrackerMaxUnits =
                    (int)GUIHelper.RoundedHorizontalSlider(CombatTrackerMaxUnits, 0, 5f, 25f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            SelectUnitOnClickUI =
                GUIHelper.ToggleButton(SelectUnitOnClickUI,
                "Select Unit On Click", _buttonStyle, GUILayout.ExpandWidth(false));

            CameraScrollToUnitOnClickUI =
                GUIHelper.ToggleButton(CameraScrollToUnitOnClickUI,
                "Camera Scroll To Unit On Click", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowUnitDescriptionOnRightClickUI =
                GUIHelper.ToggleButton(ShowUnitDescriptionOnRightClickUI,
                "Show Unit Description On Right Click", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowIsFlatFootedIconOnUI =
                GUIHelper.ToggleButton(ShowIsFlatFootedIconOnUI,
                "Show An Icon To Indicate If The Unit Lost Dexterity Bonus To AC", _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIView()
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

        private void OnGUIAttackIndicator()
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
                "Show Attack Indicator On Hover", _buttonStyle, GUILayout.ExpandWidth(false));

            ShowAutoCastAbilityRange =
                GUIHelper.ToggleButton(ShowAutoCastAbilityRange,
                "Show Ability Range Instead Of Attack Range When Using Auto Cast", _buttonStyle, GUILayout.ExpandWidth(false));

            CheckForObstaclesOnTargeting =
                GUIHelper.ToggleButton(CheckForObstaclesOnTargeting,
                "Check For Obstacles When Determining Whether The Enemy Is Within Range", _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIMovementIndicator()
        {
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
                "Show Movement Indicator On Hover", _buttonStyle, GUILayout.ExpandWidth(false));
        }
    }
}
