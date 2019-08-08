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

        public BlueprintController Blueprint { get; private set; }

        public CombatController Combat { get; internal set; }

        public HotkeyController Hotkeys { get; internal set; }

        public UIController UI { get; internal set; }

        private void HandleToggleTurnBasedMode()
        {
            Combat.Enabled = !Combat.Enabled;
        }

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Subscribe(this);
            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);

            Blueprint = new BlueprintController();
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());
            
            EventBus.Unsubscribe(this);
            HotkeyHelper.Unbind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);

            Blueprint.Dispose();
            Blueprint = null;
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_MODE, HandleToggleTurnBasedMode);

            Mod.Core.LastTickTimeOfAbilityExecutionProcess.Clear();
        }
    }
}
