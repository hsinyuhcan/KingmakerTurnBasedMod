using ModMaker;
using ModMaker.Utility;
using UnityEngine;
using UnityModManagerNet;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Menus
{
    public class PauseOptions : Menu.IToggleablePage
    {
        GUIStyle _buttonStyle;

        public string Name => "Pause";

        public int Priority => 500;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Core == null || !Core.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            }

            OnGUIPause();
        }

        void OnGUIPause()
        {
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
