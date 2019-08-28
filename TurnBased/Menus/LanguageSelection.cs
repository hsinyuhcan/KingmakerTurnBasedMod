using ModMaker;
using ModMaker.Utility;
using System.IO;
using TurnBased.Utility;
using UnityEngine;
using UnityModManagerNet;
using static ModMaker.Utility.RichTextExtensions;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Menus
{
    public class LanguageSelection : IMenuSelectablePage
    {
        GUIStyle _buttonStyle;
        GUIStyle _labelStyle;
        GUIStyle _linkStyle;

        string[] _files;
        string _exportMessage;
        string _importMessage;

        public string Name => Local["Menu_Tab_Language"];

        public int Priority => 800;

        public LanguageSelection()
        {
            RefreshFiles();
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Mod == null || !Mod.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
                _labelStyle = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft, padding = new RectOffset(
                    _buttonStyle.padding.left, GUI.skin.label.padding.right, _buttonStyle.padding.top, _buttonStyle.padding.bottom)};
                _linkStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, wordWrap = false };
            }

            using (new GUISubScope(Local["Menu_Sub_Current"]))
                OnGUICurrent();

            using (new GUISubScope(Local["Menu_Sub_Import"]))
                OnGUIImport();
        }

        private void OnGUICurrent()
        {
            GUILayout.Label(string.Format(Local["Menu_Txt_Language"], Local.Language), _labelStyle, GUILayout.ExpandWidth(false));
            GUILayout.Label(string.Format(Local["Menu_Txt_Version"], Local.Version), _labelStyle, GUILayout.ExpandWidth(false));
            GUILayout.Label(string.Format(Local["Menu_Txt_Contributors"], Local.Contributors), _labelStyle, GUILayout.ExpandWidth(false));
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(Local["Menu_Txt_HomePage"], _labelStyle, GUILayout.ExpandWidth(false));
                if (!string.IsNullOrEmpty(Local.HomePage))
                {
                    GUIHelper.Hyperlink(Local.HomePage, Color.white, Color.cyan, _linkStyle);
                }
            }

            GUILayout.Space(5f);

            string fileName = LocalizationFileName ?? "Default.json";

            if (GUILayout.Button(string.Format(Local["Menu_Btn_Export"], fileName), _buttonStyle, GUILayout.ExpandWidth(false)))
            {
                if (Local.Export(fileName))
                    _exportMessage = null;
                else
                    _exportMessage = string.Format(Local["Menu_Txt_FaildToExport"], fileName);
            }

            if (GUILayout.Button(string.Format(Local["Menu_Btn_SortAndExport"], fileName) +
                Local["Menu_Cmt_SortAndExport"].Color(RGBA.silver), _buttonStyle, GUILayout.ExpandWidth(false)))
            {
                Local.Sort();
                if (Local.Export(fileName))
                    _exportMessage = null;
                else
                    _exportMessage = string.Format(Local["Menu_Txt_FaildToExport"], fileName);
            }

            if (!string.IsNullOrEmpty(_exportMessage))
            {
                GUILayout.Label(_exportMessage.Color(RGBA.yellow), _labelStyle, GUILayout.ExpandWidth(false));
            }
        }

        private void OnGUIImport()
        {
            if (GUILayout.Button(Local["Menu_Btn_RefreshFileList"], _buttonStyle, GUILayout.ExpandWidth(false)))
            {
                RefreshFiles();
            }

            if (GUILayout.Button(Local["Menu_Btn_DefaultLanguage"], _buttonStyle, GUILayout.ExpandWidth(false)))
            {
                Local.Reset();
                LocalizationFileName = null;
                _importMessage = null;
            }

            foreach (string fileName in _files)
            {
                if (GUILayout.Button(Path.GetFileNameWithoutExtension(fileName), _buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    if (Local.Import(fileName))
                    {
                        LocalizationFileName = fileName;
                        _importMessage = null;
                    }
                    else
                    {
                        _importMessage = string.Format(Local["Menu_Txt_FaildToImport"], fileName);
                    }
                }
            }

            if (!string.IsNullOrEmpty(_importMessage))
            {
                GUILayout.Label(_importMessage.Color(RGBA.yellow), _labelStyle, GUILayout.ExpandWidth(false));
            }
        }

        private void RefreshFiles()
        {
            _files = Local.GetFileNames("*.json");
        }
    }
}
