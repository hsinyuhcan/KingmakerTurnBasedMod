using Kingmaker.Controllers;
using Kingmaker.PubSubSystem;
using ModMaker;
using ModMaker.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using TurnBased.Controllers;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased
{
    public class Core :
        IModEventHandler,
        ISceneHandler
    {
        internal Dictionary<AbilityExecutionProcess, TimeSpan> LastTickTimeOfAbilityExecutionProcess = new Dictionary<AbilityExecutionProcess, TimeSpan>();

        public BlueprintController Blueprints { get; internal set; }

        public CombatController Combat { get; internal set; }

        public HotkeyController Hotkeys { get; internal set; }

        public int Priority => 200;

        public UIController UI { get; internal set; }

        public bool Enabled {
            get => Mod.Settings.toggleTurnBasedMode;
            set {
                if (Mod.Settings.toggleTurnBasedMode != value)
                {
                    Mod.Debug(MethodBase.GetCurrentMethod(), value);

                    Mod.Settings.toggleTurnBasedMode = value;
                    Blueprints.Update(true);
                    Combat.Reset(value);
                    EventBus.RaiseEvent<IWarningNotificationUIHandler>
                        (h => h.HandleWarning(value ? Local["UI_Txt_TurnBasedMode"] : Local["UI_Txt_RealTimeMode"], false));
                }
            }
        }

        public void ResetSettings()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Mod.ResetSettings();
            Mod.Settings.lastModVersion = Mod.Version.ToString();
            LocalizationFileName = Local.FileName;
            Hotkeys?.Update(true, true);
            Blueprints?.Update(true);
        }

        private void HandleToggleTurnBasedMode()
        {
            Enabled = !Enabled;
        }

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            if (LocalizationFileName != null)
            {
                Local.Import(LocalizationFileName, e => Mod.Error(e));
                LocalizationFileName = Local.FileName;
            }

            if (!Version.TryParse(Mod.Settings.lastModVersion, out Version version) || version < new Version(1, 0, 0))
                ResetSettings();
            else
                Mod.Settings.lastModVersion = Mod.Version.ToString();

            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);
            EventBus.Subscribe(this);
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());
            
            EventBus.Unsubscribe(this);
            HotkeyHelper.Unbind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            LastTickTimeOfAbilityExecutionProcess.Clear();

            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);
        }
    }
}