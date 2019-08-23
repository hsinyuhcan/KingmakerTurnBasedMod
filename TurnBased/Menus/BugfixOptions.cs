using ModMaker;
using ModMaker.Utility;
using System;
using TurnBased.Utility;
using UnityEngine;
using UnityModManagerNet;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Menus
{
    public class BugfixOptions : IMenuSelectablePage
    {
        GUIStyle _buttonStyle;
        GUIStyle _labelStyle;

        public string Name => "Bugfix";

        public int Priority => 600;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Mod == null || !Mod.Enabled)
                return;

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
                _labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, padding = _buttonStyle.padding };
            }

            OnGUIBugfix();
        }

        void OnGUIBugfix()
        {
            BugfixToggle(FixNeverInCombatWithoutMC,
                "Fix when the main character is not in your party, the game will never consider that you are in combat", false, true);

            BugfixToggle(new BugfixOption(true, false),
                "Fix combat will not end properly if it's ended by a cutsense", false, false);

            BugfixToggle(new BugfixOption(true, true),
                "Fix units will never leave combat if they become inactive (cause Call Forth Kanerah / Kalikke glitch)", false, false);

            BugfixToggle(FixActionTypeOfBardicPerformance,
                "Fix the action type of starting a Bardic Performance with / without Singing Steel", true, true);

            BugfixToggle(FixActionTypeOfCharge,
                "Fix the action type of Charge (Standard Action => Full Round Action)", true, true,
                () => Mod.Core.Blueprint.ActionTypeOfCharge.Update());

            BugfixToggle(FixActionTypeOfOverrun,
                "Fix the action type of Overrun (Standard Action => Full Round Action)", true, true,
                () => Mod.Core.Blueprint.ActionTypeOfOverrun.Update());

            BugfixToggle(FixActionTypeOfVitalStrike,
                "Fix the action type of Vital Strike (Full Round Action => Standard Action)", true, true,
                () => Mod.Core.Blueprint.ActionTypeOfVitalStrike.Update());

            BugfixToggle(FixActionTypeOfAngelicForm,
                "Fix the action type of Angelic Form (Standard Action => Move Action)", true, true,
                () => Mod.Core.Blueprint.ActionTypeOfAngelicForm.Update());

            BugfixToggle(FixActionTypeOfKineticBlade,
                "Fix activating Kinetic Blade is regarded as drawing weapon and costs an additional standard action", true, true);

            BugfixToggle(FixKineticistWontStopPriorCommand,
                "Fix Kineticist will not stop its previous action if you command it to attack with Kinetic Blade before combat", true, true);

            BugfixToggle(FixSpellstrikeOnNeutralUnit,
                "Fix Spellstrike does not take effect when attacking a neutral target", true, true);

            BugfixToggle(FixSpellstrikeWithMetamagicReach,
                "Fix Spellstrike does not take effect when using Metamagic (Reach) on a touch spell", true, true);

            BugfixToggle(FixAbilityNotAutoDeactivateIfCombatEnded,
                "Fix some abilities will not be auto deactivated after combat (Inspire Greatness, Inspire Heroics)", true, true,
                () => Mod.Core.Blueprint.AbilityDeactivateIfCombatEnded.Update());
            
            BugfixToggle(FixBlindFightDistance,
                "Fix Blind-Fight needs a extreme close distance to prevent from losing AC instead of melee distance", true, true);

            BugfixToggle(FixConfusedUnitCanAttackDeadUnit,
                "Fix sometimes a confused unit can act normally because it tried to attack a dead unit and failed", true, true);

            BugfixToggle(FixHasMotionThisTick,
                "Fix sometimes the game does not regard a unit that is forced to move as a unit that is moved (cause AoO inconsistent)", true, true);

            BugfixToggle(FixAbilityCircleRadius,
                "Fix the visual circle of certain abilities is inconsistent with the real range", false, true);
            
            BugfixToggle(FixAbilityCircleNotAppear,
                "Fix the ability circle does not appear properly when you first time select any ability of the unit using a hotkey", true, true);

            BugfixToggle(FixAbilityCanTargetDeadUnit,
                "Fix dead units can be targeted even when current ability cannot be cast to dead target", true, true);
        }

        private void BugfixToggle(BugfixOption option, string text, bool canToggleTB, bool canToggleRT, Action onToggle = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (canToggleTB)
                    GUIHelper.ToggleButton(ref option.ForTB, "TB", onToggle, onToggle, _buttonStyle, GUILayout.ExpandWidth(false));
                else
                    GUIHelper.ToggleButton(option.ForTB, "TB", _labelStyle, GUILayout.ExpandWidth(false));

                if (canToggleRT)
                    GUIHelper.ToggleButton(ref option.ForRT, "RT", onToggle, onToggle, _buttonStyle, GUILayout.ExpandWidth(false));
                else
                    GUIHelper.ToggleButton(option.ForRT, "RT", _labelStyle, GUILayout.ExpandWidth(false));

                GUILayout.Label(text, _labelStyle, GUILayout.ExpandWidth(false));
            }
        }
    }
}