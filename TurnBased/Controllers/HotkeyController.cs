using Kingmaker.PubSubSystem;
using Kingmaker.UI;
using Kingmaker.UI.SettingsUI;
using ModMaker;
using ModMaker.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Controllers
{
    public class HotkeyController : 
        IModEventHandler,
        ISceneHandler
    {
        public IDictionary<string, BindingKeysData> BindingKeys => Mod.Settings.hotkeys;

        private void Initialize()
        {
            Dictionary<string, BindingKeysData> hotkeys = new Dictionary<string, BindingKeysData>()
            {
                {HOTKEY_FOR_TOGGLE_MODE, new BindingKeysData() { IsAltDown = true, Key = KeyCode.T } },
                {HOTKEY_FOR_TOGGLE_ATTACK_INDICATOR, new BindingKeysData() { IsAltDown = true, Key = KeyCode.R }},
                {HOTKEY_FOR_TOGGLE_MOVEMENT_INDICATOR, new BindingKeysData() { IsAltDown = true, Key = KeyCode.R }},
                {HOTKEY_FOR_FIVE_FOOT_STEP, new BindingKeysData() { IsAltDown = true, Key = KeyCode.F } },
                {HOTKEY_FOR_DELAY, new BindingKeysData() { IsAltDown = true, Key = KeyCode.D }},
                {HOTKEY_FOR_END_TURN, new BindingKeysData() { IsAltDown = true, Key = KeyCode.E } },
            };

            foreach (string name in BindingKeys.Keys.ToList())
                if (!hotkeys.ContainsKey(name))
                    BindingKeys.Remove(name);

            foreach (KeyValuePair<string, BindingKeysData> item in hotkeys)
                if (!BindingKeys.ContainsKey(item.Key))
                    BindingKeys.Add(item.Key, item.Value);
        }

        public void Reset()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Initialize();
            RegisterAll();
        }

        public void SetHotkey(string name, BindingKeysData value)
        {
            BindingKeys[name] = value;
            TryRegisterHotkey(name, value);
        }

        private void RegisterAll()
        {
            foreach (KeyValuePair<string, BindingKeysData> item in BindingKeys)
                TryRegisterHotkey(item.Key, item.Value);
        }

        private void UnregisterAll()
        {
            foreach (string name in BindingKeys.Keys)
                TryRegisterHotkey(name, null);
        }

        private void TryRegisterHotkey(string name, BindingKeysData value)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), name, HotkeyHelper.GetKeyText(value));

            if (value != null)
            {
                HotkeyHelper.RegisterKey(name, value, KeyboardAccess.GameModesGroup.World);
            }
            else
            {
                HotkeyHelper.UnregisterKey(name);
            }
        }

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Subscribe(this);

            Mod.Core.Hotkeys = this;
            Initialize();
            RegisterAll();
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Unsubscribe(this);

            Mod.Core.Hotkeys = null;
            UnregisterAll();
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            RegisterAll();
        }
    }
}
