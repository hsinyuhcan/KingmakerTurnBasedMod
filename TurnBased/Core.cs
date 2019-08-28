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
        private bool _enabled = true;

        internal Dictionary<AbilityExecutionProcess, TimeSpan> LastTickTimeOfAbilityExecutionProcess = new Dictionary<AbilityExecutionProcess, TimeSpan>();

        public BlueprintController Blueprint { get; } = new BlueprintController();

        public CombatController Combat { get; internal set; }

        public HotkeyController Hotkeys { get; internal set; }

        public UIController UI { get; internal set; }

        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled != value)
                {
                    Mod.Debug(MethodBase.GetCurrentMethod(), value);

                    _enabled = value;
                    Combat.Reset(value);

                    EventBus.RaiseEvent<IWarningNotificationUIHandler>(h =>
                        h.HandleWarning(value ? Local["UI_Txt_TurnBasedMode"] : Local["UI_Txt_RealTimeMode"], false));
                }
            }
        }

        private void HandleToggleTurnBasedMode()
        {
            Enabled = !Enabled;
            Mod.Core.Blueprint.Update();
        }

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            if (LocalizationFileName != null)
                if (!Local.Import(LocalizationFileName))
                    LocalizationFileName = null;

            Mod.Core.Blueprint.Update();

            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);
            EventBus.Subscribe(this);
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());
            
            EventBus.Unsubscribe(this);
            HotkeyHelper.Unbind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);

            Mod.Core.Blueprint.Update(false);
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);

            Mod.Core.Blueprint.Update();
            Mod.Core.LastTickTimeOfAbilityExecutionProcess.Clear();
        }
    }
}