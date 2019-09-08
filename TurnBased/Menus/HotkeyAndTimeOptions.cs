using Kingmaker.UI.SettingsUI;
using ModMaker;
using ModMaker.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;
using static ModMaker.Utility.RichTextExtensions;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Menus
{
    public class HotkeyAndTimeOptions : IMenuSelectablePage
    {
        private string _waitingHotkeyName;

        GUIStyle _buttonStyle;
        GUIStyle _downButtonStyle;
        GUIStyle _labelStyle;

        public string Name => Local["Menu_Tab_HotkeyAndTime"];

        public int Priority => 400;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Mod == null || !Mod.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
                _downButtonStyle = new GUIStyle(_buttonStyle)
                {
                    focused = _buttonStyle.active,
                    normal = _buttonStyle.active,
                    hover = _buttonStyle.active
                };
                _downButtonStyle.active.textColor = Color.gray;
                _labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, padding = _buttonStyle.padding };
            }

            using (new GUISubScope(Local["Menu_Sub_Hotkey"]))
                OnGUIHotkey();

            using (new GUISubScope(Local["Menu_Sub_Time"]))
                OnGUITime();

            using (new GUISubScope(Local["Menu_Sub_Pause"]))
                OnGUIPause();
        }

        private void OnGUIHotkey()
        {
            if (!string.IsNullOrEmpty(_waitingHotkeyName) && HotkeyHelper.ReadKey(out BindingKeysData newKey))
            {
                Mod.Core.Hotkeys.SetHotkey(_waitingHotkeyName, newKey);
                _waitingHotkeyName = null;
            }

            IDictionary<string, BindingKeysData> hotkeys = Mod.Core.Hotkeys.Hotkeys;

            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    foreach (KeyValuePair<string, BindingKeysData> item in hotkeys)
                    {
                        GUIHelper.ToggleButton(item.Value != null, Local[item.Key], _labelStyle, GUILayout.ExpandWidth(false));
                    }
                }

                GUILayout.Space(10f);

                using (new GUILayout.VerticalScope())
                {
                    foreach (BindingKeysData key in hotkeys.Values)
                    {
                        GUILayout.Label(HotkeyHelper.GetKeyText(key));
                    }
                }

                GUILayout.Space(10f);

                using (new GUILayout.VerticalScope())
                {
                    foreach (string name in hotkeys.Keys)
                    {
                        bool waitingThisHotkey = _waitingHotkeyName == name;
                        if (GUILayout.Button(Local["Menu_Btn_Set"], waitingThisHotkey ? _downButtonStyle : _buttonStyle))
                        {
                            if (waitingThisHotkey)
                                _waitingHotkeyName = null;
                            else
                                _waitingHotkeyName = name;
                        }
                    }
                }

                using (new GUILayout.VerticalScope())
                {
                    string hotkeyToClear = default;
                    foreach (string name in hotkeys.Keys)
                    {
                        if (GUILayout.Button(Local["Menu_Btn_Clear"], _buttonStyle))
                        {
                            hotkeyToClear = name;

                            if (_waitingHotkeyName == name)
                                _waitingHotkeyName = null;
                        }
                    }
                    if (!string.IsNullOrEmpty(hotkeyToClear))
                        Mod.Core.Hotkeys.SetHotkey(hotkeyToClear, null);
                }

                using (new GUILayout.VerticalScope())
                {
                    foreach (KeyValuePair<string, BindingKeysData> item in hotkeys)
                    {
                        if (item.Value != null && !HotkeyHelper.CanBeRegistered(item.Key, item.Value))
                        {
                            GUILayout.Label(Local["Menu_Txt_Duplicated"].Color(RGBA.yellow));
                        }
                        else
                        {
                            GUILayout.Label(string.Empty);
                        }
                    }
                }

                GUILayout.FlexibleSpace();
            }

            ToggleFiveFootStepOnRightClickGround =
                GUIHelper.ToggleButton(ToggleFiveFootStepOnRightClickGround,
                Local["Menu_Opt_ToggleFiveFootStepOnRightClickGround"], _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUITime()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleBetweenTurns != 1f,
                    string.Format(Local["Menu_Opt_TimeScaleBetweenTurns"], TimeScaleBetweenTurns), _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleBetweenTurns =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleBetweenTurns, 1, 1f, 10f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleInPlayerTurn != 1f,
                    string.Format(Local["Menu_Opt_TimeScaleInPlayerTurn"], TimeScaleInPlayerTurn), _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleInPlayerTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInPlayerTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleInNonPlayerTurn != 1f,
                    string.Format(Local["Menu_Opt_TimeScaleInNonPlayerTurn"], TimeScaleInNonPlayerTurn), _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleInNonPlayerTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInNonPlayerTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleInUnknownTurn != 1f,
                    string.Format(Local["Menu_Opt_TimeScaleInUnknownTurn"], TimeScaleInUnknownTurn), _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleInUnknownTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInUnknownTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(MaxDelayBetweenIterativeAttacks != 3f,
                    string.Format(Local["Menu_Opt_MaxDelayBetweenIterativeAttacks"], MaxDelayBetweenIterativeAttacks), _labelStyle, GUILayout.ExpandWidth(false));
                MaxDelayBetweenIterativeAttacks =
                    GUIHelper.RoundedHorizontalSlider(MaxDelayBetweenIterativeAttacks, 1, 0.5f, 3f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Label(Local["Menu_Cmt_MaxDelayBetweenIterativeAttacks"].Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(CastingTimeOfFullRoundSpell != 1f,
                    string.Format(Local["Menu_Opt_CastingTimeOfFullRoundSpell"], CastingTimeOfFullRoundSpell), _labelStyle, GUILayout.ExpandWidth(false));
                CastingTimeOfFullRoundSpell =
                    GUIHelper.RoundedHorizontalSlider(CastingTimeOfFullRoundSpell, 1, 0.5f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Label(Local["Menu_Cmt_CastingTimeOfFullRoundSpell"].Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true,
                    string.Format(Local["Menu_Opt_TimeToWaitForIdleAI"], TimeToWaitForIdleAI), _labelStyle, GUILayout.ExpandWidth(false));
                TimeToWaitForIdleAI =
                    GUIHelper.RoundedHorizontalSlider(TimeToWaitForIdleAI, 1, 0.1f, 3f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true,
                    string.Format(Local["Menu_Opt_TimeToWaitForEndingTurn"], TimeToWaitForEndingTurn), _labelStyle, GUILayout.ExpandWidth(false));
                TimeToWaitForEndingTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeToWaitForEndingTurn, 1, 0.1f, 3f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }
        }

        private void OnGUIPause()
        {
            DoNotPauseOnCombatStart =
                GUIHelper.ToggleButton(DoNotPauseOnCombatStart,
                Local["Menu_Opt_DoNotPauseOnCombatStart"] +
                Local["Menu_Cmt_DoNotPauseOnCombatStart"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerTurnStart =
                GUIHelper.ToggleButton(PauseOnPlayerTurnStart,
                Local["Menu_Opt_PauseOnPlayerTurnStart"], _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerTurnEnd =
                GUIHelper.ToggleButton(PauseOnPlayerTurnEnd,
                Local["Menu_Opt_PauseOnPlayerTurnEnd"], _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnNonPlayerTurnStart =
                GUIHelper.ToggleButton(PauseOnNonPlayerTurnStart,
                Local["Menu_Opt_PauseOnNonPlayerTurnStart"], _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnNonPlayerTurnEnd =
                GUIHelper.ToggleButton(PauseOnNonPlayerTurnEnd,
                Local["Menu_Opt_PauseOnNonPlayerTurnEnd"], _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerFinishFiveFoot =
                GUIHelper.ToggleButton(PauseOnPlayerFinishFiveFoot,
                Local["Menu_Opt_PauseOnPlayerFinishFiveFoot"], _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerFinishFirstMove =
                GUIHelper.ToggleButton(PauseOnPlayerFinishFirstMove,
                Local["Menu_Opt_PauseOnPlayerFinishFirstMove"], _buttonStyle, GUILayout.ExpandWidth(false));
        }
    }
}
