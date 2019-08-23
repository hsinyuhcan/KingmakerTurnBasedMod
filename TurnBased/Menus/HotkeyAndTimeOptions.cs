using Kingmaker.UI.SettingsUI;
using ModMaker;
using ModMaker.Utility;
using System.Collections.Generic;
using TurnBased.Utility;
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

        public string Name => "Hotkey & Time";

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

            using (new GUISubScope("Hotkey"))
                OnGUIHotkey();

            using (new GUISubScope("Time"))
                OnGUITime();

            using (new GUISubScope("Pause"))
                OnGUIPause();
        }

        private void OnGUIHotkey()
        {
            if (!string.IsNullOrEmpty(_waitingHotkeyName) && HotkeyHelper.ReadKey(out BindingKeysData newKey))
            {
                Mod.Core.Hotkeys.SetHotkey(_waitingHotkeyName, newKey);
                _waitingHotkeyName = null;
            }

            IDictionary<string, BindingKeysData> hotkeys = Mod.Core.Hotkeys.BindingKeys;

            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    foreach (KeyValuePair<string, BindingKeysData> item in hotkeys)
                    {
                        GUIHelper.ToggleButton(item.Value != null,
                            item.Key.Substring(HOTKEY_PREFIX.Length).ToSentence(), _labelStyle, GUILayout.ExpandWidth(false));
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
                        if (GUILayout.Button("Set", waitingThisHotkey ? _downButtonStyle : _buttonStyle))
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
                        if (GUILayout.Button($"Clear", _buttonStyle))
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
                            GUILayout.Label($"Duplicated!!".Color(RGBA.yellow));
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
                "Toggle 5-foot Step When Right Click On The Ground", _buttonStyle, GUILayout.ExpandWidth(false));
        }

        private void OnGUITime()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true,
                   $"Minimum FPS: {MinimumFPS:f0}", _labelStyle, GUILayout.ExpandWidth(false));
                MinimumFPS =
                    GUIHelper.RoundedHorizontalSlider(MinimumFPS, 0, 12f, 20f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                GUILayout.Label("(Auto decrease Time Scale to prevent FPS from dropping below this value)".Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleBetweenTurns != 1f,
                   $"Time Scale Multiplier Between Turns: {TimeScaleBetweenTurns:f2}x", _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleBetweenTurns =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleBetweenTurns, 1, 1f, 10f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleInPlayerTurn != 1f,
                  $"Time Scale Multiplier For Player Units: {TimeScaleInPlayerTurn:f2}x", _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleInPlayerTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInPlayerTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleInNonPlayerTurn != 1f,
                    $"Time Scale Multiplier For Non-Player Units: {TimeScaleInNonPlayerTurn:f2}x", _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleInNonPlayerTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInNonPlayerTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(TimeScaleInUnknownTurn != 1f,
                    $"Time Scale Multiplier For Unknown Units: {TimeScaleInUnknownTurn:f2}x", _labelStyle, GUILayout.ExpandWidth(false));
                TimeScaleInUnknownTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInUnknownTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }
            
            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(CastingTimeOfFullRoundSpell != 1f,
                    $"Casting Time Multiplier Of Full Round Spell: {CastingTimeOfFullRoundSpell:f2}x", _labelStyle, GUILayout.ExpandWidth(false));
                CastingTimeOfFullRoundSpell =
                    GUIHelper.RoundedHorizontalSlider(CastingTimeOfFullRoundSpell, 1, 0.5f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                GUILayout.Label("(The animation of casting a full round spell is 6 seconds by default)".Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true,
                    $"Time To Wait For Idle AI: {TimeToWaitForIdleAI:f2}s", _labelStyle, GUILayout.ExpandWidth(false));
                TimeToWaitForIdleAI =
                    GUIHelper.RoundedHorizontalSlider(TimeToWaitForIdleAI, 1, 0.1f, 3f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUIHelper.ToggleButton(true,
                    $"Time To Wait For Ending Turn: {TimeToWaitForEndingTurn:f2}s", _labelStyle, GUILayout.ExpandWidth(false));
                TimeToWaitForEndingTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeToWaitForEndingTurn, 1, 0.1f, 3f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }
        }

        void OnGUIPause()
        {
            DoNotPauseOnCombatStart =
                GUIHelper.ToggleButton(DoNotPauseOnCombatStart,
                "DO NOT Auto Pause On Combat Start" +
                " (Ignore the game setting)".Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerTurnStart =
                GUIHelper.ToggleButton(PauseOnPlayerTurnStart,
                "Pause On Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerTurnEnd =
                GUIHelper.ToggleButton(PauseOnPlayerTurnEnd,
                "Pause On Player's Turn End", _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnNonPlayerTurnStart =
                GUIHelper.ToggleButton(PauseOnNonPlayerTurnStart,
                "Pause On Non-Player's Turn Start", _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnNonPlayerTurnEnd =
                GUIHelper.ToggleButton(PauseOnNonPlayerTurnEnd,
                "Pause On Non-Player's Turn End", _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerFinishFiveFoot =
                GUIHelper.ToggleButton(PauseOnPlayerFinishFiveFoot,
                "Pause On Player's Unit Finished The 5-Foot Step", _buttonStyle, GUILayout.ExpandWidth(false));

            PauseOnPlayerFinishFirstMove =
                GUIHelper.ToggleButton(PauseOnPlayerFinishFirstMove,
                "Pause On Player's Unit Finished The First Move Action Through Move", _buttonStyle, GUILayout.ExpandWidth(false));
        }
    }
}
