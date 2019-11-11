using ModMaker;
using ModMaker.Utility;
using System;
using TurnBased.Controllers;
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

        public string Name => Local["Menu_Tab_Bugfix"];

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
            BlueprintController blueprint = Mod.Core.Blueprints;

            BugfixToggle(FixNeverInCombatWithoutMC,
                Local["Menu_Opt_FixNeverInCombatWithoutMC"], false, true);

            BugfixToggle(new BugfixOption(true, false),
                Local["Menu_Opt_FixCombatNotEndProperly"], false, false);

            BugfixToggle(new BugfixOption(true, true),
                Local["Menu_Opt_FixUnitNotLeaveCombatWhenNotInGame"], false, false);

            BugfixToggle(FixActionTypeOfBardicPerformance,
                Local["Menu_Opt_FixActionTypeOfBardicPerformance"], true, true);

            BugfixToggle(FixActionTypeOfSwappingWeapon,
                Local["Menu_Opt_FixActionTypeOfSwappingWeapon"], true, true);
            
            BugfixToggle(FixActionTypeOfCharge,
                Local["Menu_Opt_FixActionTypeOfCharge"], true, true,
                () => blueprint.ActionTypeOfCharge.Update());

            BugfixToggle(FixActionTypeOfOverrun,
                Local["Menu_Opt_FixActionTypeOfOverrun"], true, true,
                () => blueprint.ActionTypeOfOverrun.Update());

            BugfixToggle(FixActionTypeOfVitalStrike,
                Local["Menu_Opt_FixActionTypeOfVitalStrike"], true, true,
                () => blueprint.ActionTypeOfVitalStrike.Update());

            BugfixToggle(FixActionTypeOfAngelicForm,
                Local["Menu_Opt_FixActionTypeOfAngelicForm"], true, true,
                () => blueprint.ActionTypeOfAngelicForm.Update());

            BugfixToggle(FixActionTypeOfKineticBlade,
                Local["Menu_Opt_FixActionTypeOfKineticBlade"], true, true);

            BugfixToggle(FixKineticistWontStopPriorCommand,
                Local["Menu_Opt_FixKineticistWontStopPriorCommand"], true, true);

            BugfixToggle(FixSpellstrikeOnNeutralUnit,
                Local["Menu_Opt_FixSpellstrikeOnNeutralUnit"], true, true);

            BugfixToggle(FixSpellstrikeWithMetamagicReach,
                Local["Menu_Opt_FixSpellstrikeWithMetamagicReach"], true, true);
            
            BugfixToggle(FixDamageBonusOfBlastRune,
                Local["Menu_Opt_FixDamageBonusOfBlastRune"], true, true,
                () => blueprint.DamageBonusOfBlastRune.Update());

            BugfixToggle(FixOnePlusDiv2ToDiv2,
                Local["Menu_Opt_FixOnePlusDiv2ToDiv2"], true, true,
                () => blueprint.OnePlusDiv2ToDiv2.Update());
            
            BugfixToggle(FixFxOfShadowEvocationSirocco,
                Local["Menu_Opt_FixFxOfShadowEvocationSirocco"], true, true,
                () => blueprint.FxOfShadowEvocationSirocco.Update());

            BugfixToggle(FixAbilityNotAutoDeactivateIfCombatEnded,
                Local["Menu_Opt_FixAbilityNotAutoDeactivateIfCombatEnded"], true, true,
                () => blueprint.AbilityNotDeactivateIfCombatEnded.Update());

            BugfixToggle(FixBlindFightDistance,
                Local["Menu_Opt_FixBlindFightDistance"], true, true);

            BugfixToggle(FixDweomerLeap,
                Local["Menu_Opt_FixDweomerLeap"], true, true);

            BugfixToggle(FixConfusedUnitCanAttackDeadUnit,
                Local["Menu_Opt_FixConfusedUnitCanAttackDeadUnit"], true, true);

            BugfixToggle(FixAcrobaticsMobility,
                Local["Menu_Opt_FixAcrobaticsMobility"], true, true);

            BugfixToggle(FixCanMakeAttackOfOpportunityToUnmovedTarget,
                Local["Menu_Opt_FixCanMakeAttackOfOpportunityToUnmovedTarget"], true, true);

            BugfixToggle(FixHasMotionThisTick,
                Local["Menu_Opt_FixHasMotionThisTick"], true, true);

            BugfixToggle(FixAbilityCircleRadius,
                Local["Menu_Opt_FixAbilityCircleRadius"], false, true);

            BugfixToggle(FixAbilityCircleNotAppear,
                Local["Menu_Opt_FixAbilityCircleNotAppear"], true, true);

            BugfixToggle(FixAbilityCanTargetUntargetableUnit,
                Local["Menu_Opt_FixAbilityCanTargetUntargetableUnit"], true, true);

            BugfixToggle(FixAbilityCanTargetDeadUnit,
                Local["Menu_Opt_FixAbilityCanTargetDeadUnit"], true, true);

            BugfixToggle(FixNeutralUnitCanAttackAlly,
                Local["Menu_Opt_FixNeutralUnitCanAttackAlly"], true, true);

            BugfixToggle(FixInspectingTriggerAuraEffect,
                Local["Menu_Opt_FixInspectingTriggerAuraEffect"], true, true);
        }

        private void BugfixToggle(BugfixOption option, string text, bool canToggleTB, bool canToggleRT, Action onToggle = null)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (canToggleTB)
                    GUIHelper.ToggleButton(ref option.ForTB, Local["Menu_Btn_TB"], onToggle, onToggle, _buttonStyle, GUILayout.ExpandWidth(false));
                else
                    GUIHelper.ToggleButton(option.ForTB, Local["Menu_Btn_TB"], _labelStyle, GUILayout.ExpandWidth(false));

                if (canToggleRT)
                    GUIHelper.ToggleButton(ref option.ForRT, Local["Menu_Btn_RT"], onToggle, onToggle, _buttonStyle, GUILayout.ExpandWidth(false));
                else
                    GUIHelper.ToggleButton(option.ForRT, Local["Menu_Btn_RT"], _labelStyle, GUILayout.ExpandWidth(false));

                GUILayout.Label(text, _labelStyle, GUILayout.ExpandWidth(false));
            }
        }
    }
}