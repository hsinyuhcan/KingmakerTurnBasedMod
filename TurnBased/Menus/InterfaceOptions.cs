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

        public string Name => Local["Menu_Tab_Interface"];

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
                    Local["Menu_Opt_DoNotMarkInvisibleUnit"] +
                    Local["Menu_Cmt_DoNotMarkInvisibleUnit"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

                DoNotShowInvisibleUnitOnCombatTracker =
                    GUIHelper.ToggleButton(DoNotShowInvisibleUnitOnCombatTracker,
                    Local["Menu_Opt_DoNotShowInvisibleUnitOnCombatTracker"] +
                    Local["Menu_Cmt_DoNotShowInvisibleUnitOnCombatTracker"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));
            }

            using (new GUISubScope(Local["Menu_Sub_CombatTracker"]))
                OnGUICombatTracker();

            using (new GUISubScope(Local["Menu_Sub_View"]))
                OnGUIView();

            using (new GUISubScope(Local["Menu_Sub_AttackIndicator"]))
                OnGUIAttackIndicator();

            using (new GUISubScope(Local["Menu_Sub_MovementIndicator"]))
                OnGUIMovementIndicator();
        }

        private void OnGUICombatTracker()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(CombatTrackerScale != 1f,
                    string.Format(Local["Menu_Opt_CombatTrackerScale"], CombatTrackerScale), _labelStyle, GUILayout.ExpandWidth(false));
                CombatTrackerScale =
                    GUIHelper.RoundedHorizontalSlider(CombatTrackerScale, 2, 0.8f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true,
                    string.Format(Local["Menu_Opt_CombatTrackerWidth"], (int)CombatTrackerWidth), _labelStyle, GUILayout.ExpandWidth(false));
                CombatTrackerWidth =
                    GUIHelper.RoundedHorizontalSlider(CombatTrackerWidth, -1, 300f, 500f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true,
                    string.Format(Local["Menu_Opt_CombatTrackerMaxUnits"], CombatTrackerMaxUnits), _labelStyle, GUILayout.ExpandWidth(false));
                CombatTrackerMaxUnits =
                    (int)GUIHelper.RoundedHorizontalSlider(CombatTrackerMaxUnits, 0, 5f, 25f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            CameraScrollToUnitOnClickUI =
                GUIHelper.ToggleButton(CameraScrollToUnitOnClickUI,
                Local["Menu_Opt_CameraScrollToUnitOnClickUI"], _buttonStyle, GUILayout.ExpandWidth(false));

            SelectUnitOnClickUI =
                GUIHelper.ToggleButton(SelectUnitOnClickUI,
                Local["Menu_Opt_SelectUnitOnClickUI"], _buttonStyle, GUILayout.ExpandWidth(false));

            InspectOnRightClickUI =
                GUIHelper.ToggleButton(InspectOnRightClickUI,
                Local["Menu_Opt_InspectOnRightClickUI"], _buttonStyle, GUILayout.ExpandWidth(false));

            ShowIsFlatFootedIconOnUI =
                GUIHelper.ToggleButton(ShowIsFlatFootedIconOnUI,
                Local["Menu_Opt_ShowIsFlatFootedIconOnUI"], _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIView()
        {
            HighlightCurrentUnit =
                GUIHelper.ToggleButton(HighlightCurrentUnit,
                Local["Menu_Opt_HighlightCurrentUnit"], _buttonStyle, GUILayout.ExpandWidth(false));

            CameraScrollToCurrentUnit =
                GUIHelper.ToggleButton(CameraScrollToCurrentUnit,
                Local["Menu_Opt_CameraScrollToCurrentUnit"], _buttonStyle, GUILayout.ExpandWidth(false));

            CameraLockOnCurrentPlayerUnit =
                GUIHelper.ToggleButton(CameraLockOnCurrentPlayerUnit,
                Local["Menu_Opt_CameraLockOnCurrentPlayerUnit"] +
                Local["Menu_Cmt_CameraLockOnCurrentPlayerUnit"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            CameraLockOnCurrentNonPlayerUnit =
                GUIHelper.ToggleButton(CameraLockOnCurrentNonPlayerUnit,
                Local["Menu_Opt_CameraLockOnCurrentNonPlayerUnit"], _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIAttackIndicator()
        {
            using (new GUILayout.HorizontalScope())
            {
                ShowAttackIndicatorOfCurrentUnit =
                    GUIHelper.ToggleButton(ShowAttackIndicatorOfCurrentUnit,
                    Local["Menu_Opt_ShowAttackIndicatorOfCurrentUnit"], _buttonStyle, GUILayout.ExpandWidth(false));

                ShowAttackIndicatorForPlayer =
                    GUIHelper.ToggleButton(ShowAttackIndicatorForPlayer,
                    Local["Menu_Opt_ShowAttackIndicatorForPlayer"], _buttonStyle, GUILayout.ExpandWidth(false));

                ShowAttackIndicatorForNonPlayer =
                    GUIHelper.ToggleButton(ShowAttackIndicatorForNonPlayer,
                    Local["Menu_Opt_ShowAttackIndicatorForNonPlayer"], _buttonStyle, GUILayout.ExpandWidth(false));
            }

            ShowAttackIndicatorOnHoverUI =
                GUIHelper.ToggleButton(ShowAttackIndicatorOnHoverUI,
                Local["Menu_Opt_ShowAttackIndicatorOnHoverUI"], _buttonStyle, GUILayout.ExpandWidth(false));

            ShowAutoCastAbilityRange =
                GUIHelper.ToggleButton(ShowAutoCastAbilityRange,
                Local["Menu_Opt_ShowAutoCastAbilityRange"], _buttonStyle, GUILayout.ExpandWidth(false));

            CheckForObstaclesOnTargeting =
                GUIHelper.ToggleButton(CheckForObstaclesOnTargeting,
                Local["Menu_Opt_CheckForObstaclesOnTargeting"], _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUIMovementIndicator()
        {
            using (new GUILayout.HorizontalScope())
            {
                ShowMovementIndicatorOfCurrentUnit =
                    GUIHelper.ToggleButton(ShowMovementIndicatorOfCurrentUnit,
                    Local["Menu_Opt_ShowMovementIndicatorOfCurrentUnit"], _buttonStyle, GUILayout.ExpandWidth(false));

                ShowMovementIndicatorForPlayer =
                    GUIHelper.ToggleButton(ShowMovementIndicatorForPlayer,
                    Local["Menu_Opt_ShowMovementIndicatorForPlayer"], _buttonStyle, GUILayout.ExpandWidth(false));

                ShowMovementIndicatorForNonPlayer =
                    GUIHelper.ToggleButton(ShowMovementIndicatorForNonPlayer,
                    Local["Menu_Opt_ShowMovementIndicatorForNonPlayer"], _buttonStyle, GUILayout.ExpandWidth(false));
            }

            ShowMovementIndicatorOnHoverUI =
                GUIHelper.ToggleButton(ShowMovementIndicatorOnHoverUI,
                Local["Menu_Opt_ShowMovementIndicatorOnHoverUI"], _buttonStyle, GUILayout.ExpandWidth(false));
        }
    }
}
