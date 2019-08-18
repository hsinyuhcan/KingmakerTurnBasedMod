using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.Visual;
using Kingmaker.Visual.Animation.Kingmaker;
using System;
using System.Reflection;
using UnityEngine;
using static ModMaker.Utility.ReflectionCache;

namespace TurnBased.Utility
{
    public static class NonPublicAccessExtensions
    {
        public static void SetIsFullRoundAction(this BlueprintAbility blueprintAbility, bool value)
        {
            blueprintAbility.SetFieldValue("m_IsFullRoundAction", value);
        }

        public static void SetActivateWithUnitCommand(this BlueprintActivatableAbility blueprintAbility, UnitCommand.CommandType value)
        {
            blueprintAbility.SetFieldValue("m_ActivateWithUnitCommand", value);
        }

        public static bool GetInitialized(this LibraryScriptableObject library)
        {
            return library.GetFieldValue<LibraryScriptableObject, bool>("m_Initialized");
        }

        public static void SetPrevTickTime(this RayView rayView, TimeSpan value)
        {
            rayView.SetFieldValue("m_PrevTickTime", value);
        }

        public static void SetDeltaTime(this TimeController timeController, float value)
        {
            timeController.SetPropertyValue(nameof(TimeController.DeltaTime), value);
        }

        public static void SetGameDeltaTime(this TimeController timeController, float value)
        {
            timeController.SetPropertyValue(nameof(TimeController.GameDeltaTime), value);
        }

        public static void SetHoverVisibility(this UIDecalBase uiDecalBase, bool state)
        {
            GetMethod<UIDecalBase, Action<UIDecalBase, bool>>("SetHoverVisibility")
                (uiDecalBase, state);
        }

        public static int GetExclusiveState(this UnitAnimationManager unitAnimationManager)
        {
            return (int)typeof(UnitAnimationManager)
                .GetField("m_ExclusiveState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(unitAnimationManager);
        }

        public static void SetExclusiveState(this UnitAnimationManager unitAnimationManager, int value)
        {
            typeof(UnitAnimationManager)
                .GetField("m_ExclusiveState", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(unitAnimationManager, value);
        }

        public static void TickOnUnit(this UnitConfusionController unitConfusionController, UnitEntityData unit)
        {
            GetMethod<UnitConfusionController, Action<UnitConfusionController, UnitEntityData>>
                ("TickOnUnit")(unitConfusionController, unit);
        }

        public static UnitCommand GetPaired(this UnitCommands unitCommands, UnitCommand cmd)
        {
            return GetMethod<UnitCommands, Func<UnitCommands, UnitCommand, UnitCommand>>("GetPaired")
                (unitCommands, cmd);
        }

        public static void InterruptAndRemoveCommand(this UnitCommands unitCommands, UnitCommand.CommandType type)
        {
            GetMethod<UnitCommands, Action<UnitCommands, UnitCommand.CommandType>>
                ("InterruptAndRemoveCommand")(unitCommands, type);
        }

        public static void UpdateCombatTarget(this UnitCommands unitCommands, UnitCommand cmd)
        {
            GetMethod<UnitCommands, Action<UnitCommands, UnitCommand>>("UpdateCombatTarget")(unitCommands, cmd);
        }

        public static UnitMultiHighlight GetHighlighter(this UnitEntityView unitEntityView)
        {
            return unitEntityView.GetFieldValue<UnitEntityView, UnitMultiHighlight>("m_Highlighter");
        }

        public static Vector3? GetDestination(this UnitMovementAgent unitMovementAgent)
        {
            return unitMovementAgent.GetFieldValue<UnitMovementAgent, Vector3?>("m_Destination");
        }

        public static bool GetIsInForceMode(this UnitMovementAgent unitMovementAgent)
        {
            return unitMovementAgent.GetFieldValue<UnitMovementAgent, bool>("m_IsInForceMode");
        }

        public static float GetMinSpeed(this UnitMovementAgent unitMovementAgent)
        {
            return unitMovementAgent.GetFieldValue<UnitMovementAgent, float>("m_MinSpeed");
        }

        public static void SetChargeAvoidanceFinishTime(this UnitMovementAgent unitMovementAgent, TimeSpan value)
        {
            unitMovementAgent.SetFieldValue("m_ChargeAvoidanceFinishTime", value);
        }

        public static void SetMinSpeed(this UnitMovementAgent unitMovementAgent, float value)
        {
            unitMovementAgent.SetFieldValue("m_MinSpeed", value);
        }

        public static void SetSlowDownTime(this UnitMovementAgent unitMovementAgent, float value)
        {
            unitMovementAgent.SetFieldValue("m_SlowDownTime", value);
        }

        public static void SetWarmupTime(this UnitMovementAgent unitMovementAgent, float value)
        {
            unitMovementAgent.SetFieldValue("m_WarmupTime", value);
        }

        public static bool HasOneHandedMeleeWeaponAndFreehand(this UnitPartMagus unitPartMagus, UnitDescriptor unit)
        {
            return GetMethod<UnitPartMagus, Func<UnitPartMagus, UnitDescriptor, bool>>
                ("HasOneHandedMeleeWeaponAndFreehand")(unitPartMagus, unit);
        }

        public static bool IsRangedWeapon(this UnitPartMagus unitPartMagus, ItemEntityWeapon weapon)
        {
            return GetMethod<UnitPartMagus, Func<UnitPartMagus, ItemEntityWeapon, bool>>
                ("IsRangedWeapon")(unitPartMagus, weapon);
        }

        public static void SetAppearTime(this UnitPartTouch unitPartTouch, TimeSpan value)
        {
            unitPartTouch.SetPropertyValue(nameof(UnitPartTouch.AppearTime), value);
        }

        public static float GetCastTime(this UnitUseAbility unitUseAbility)
        {
            return unitUseAbility.GetFieldValue<UnitUseAbility, float>("m_CastTime");
        }

        public static void SetCastTime(this UnitUseAbility unitUseAbility, float value)
        {
            unitUseAbility.SetFieldValue("m_CastTime", value);
        }
    }
}