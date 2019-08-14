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
    public class HotkeyAndTimeScaleOptions : IMenuSelectablePage
    {
        private string _waitingHotkeyName;

        GUIStyle _buttonStyle;
        GUIStyle _downButtonStyle;

        public string Name => "Hotkey & Time Scale";

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
            }

            OnGUIHotkey();

            GUILayout.Space(10f);

            OnGUITimeScale();
        }

        private void OnGUIHotkey()
        {
            GUILayout.Label("Hotkeys:");

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
                    foreach (string name in hotkeys.Keys)
                    {
                        GUILayout.Label(name.Substring(HOTKEY_PREFIX.Length).ToSentence());
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

            GUILayout.Space(10f);

            ToggleFiveFootStepOnRightClickGround =
                GUIHelper.ToggleButton(ToggleFiveFootStepOnRightClickGround,
                "Toggle 5-foot Step When Right Click On The Ground", _buttonStyle, GUILayout.ExpandWidth(false));
        }

        void OnGUITimeScale()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Minimum FPS: {MinimumFPS:f0}", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                MinimumFPS =
                    GUIHelper.RoundedHorizontalSlider(MinimumFPS, 0, 12f, 20f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                GUILayout.Label("(Auto decrease Time Scale to prevent FPS drops below this value)".Color(RGBA.silver), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Time Scale Multiplier Between Turns: {TimeScaleBetweenTurns:f2}x", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                TimeScaleBetweenTurns =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleBetweenTurns, 1, 1f, 10f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Time Scale Multiplier In Player's Turn: {TimeScaleInPlayerTurn:f2}x", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                TimeScaleInPlayerTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInPlayerTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Time Scale Multiplier In Non-Player's Turn: {TimeScaleInNonPlayerTurn:f2}x", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                TimeScaleInNonPlayerTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeScaleInNonPlayerTurn, 1, 1f, 5f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            GUILayout.Space(10f);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Casting Time Multiplier Of Full Round Spell: {CastingTimeOfFullRoundSpell:f2}x", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                CastingTimeOfFullRoundSpell =
                    GUIHelper.RoundedHorizontalSlider(CastingTimeOfFullRoundSpell, 1, 0.5f, 1f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            GUILayout.Space(10f);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Time To Wait For Idle AI: {TimeToWaitForIdleAI:f2}s", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                TimeToWaitForIdleAI =
                    GUIHelper.RoundedHorizontalSlider(TimeToWaitForIdleAI, 1, 0.1f, 3f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Time To Wait For Ending Turn: {TimeToWaitForEndingTurn:f2}s", GUILayout.ExpandWidth(false));
                GUILayout.Space(5f);
                TimeToWaitForEndingTurn =
                    GUIHelper.RoundedHorizontalSlider(TimeToWaitForEndingTurn, 1, 0.1f, 3f, GUILayout.Width(100f), GUILayout.ExpandWidth(false));
            }
        }
    }
}
