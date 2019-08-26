using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;
using System.Linq;
using System.Reflection;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Controllers
{
    public class BlueprintController
    {
        // ChargeAbility
        // SwiftBlowImprovedChargeAbility
        public ValueModifier<BlueprintAbility, bool> ActionTypeOfCharge = new ValueModifier<BlueprintAbility, bool>(
            () => FixActionTypeOfCharge,
            new string[] { "c78506dd0e14f7c45a599990e4e65038", "d4b4757660cb66e4fbf376a43f1ffb13" },
            (blueprint) => blueprint.IsFullRoundAction,
            (blueprint, value) => blueprint.SetIsFullRoundAction(value),
            true);

        // OverrunAbility
        // ChargeAbilityLanternKingStar
        // FlyTrampleTest
        public ValueModifier<BlueprintAbility, bool> ActionTypeOfOverrun = new ValueModifier<BlueprintAbility, bool>(
            () => FixActionTypeOfOverrun,
            new string[] { "1a3b471ecea51f7439a946b23577fd70", "49b8bf9a35ecbd24482ee416cd7557b8", "f0b622ab2d18ef7439feb8aa5680d6e5" },
            (blueprint) => blueprint.IsFullRoundAction,
            (blueprint, value) => blueprint.SetIsFullRoundAction(value),
            true);

        // VitalStrikeAbility
        // VitalStrikeAbilityImproved
        // VitalStrikeAbilityGreater
        public ValueModifier<BlueprintAbility, bool> ActionTypeOfVitalStrike = new ValueModifier<BlueprintAbility, bool>(
            () => FixActionTypeOfVitalStrike,
            new string[] { "efc60c91b8e64f244b95c66b270dbd7c", "c714cd636700ac24a91ca3df43326b00", "11f971b6453f74d4594c538e3c88d499" },
            (blueprint) => blueprint.IsFullRoundAction,
            (blueprint, value) => blueprint.SetIsFullRoundAction(value),
            false);

        // TristianAngelAbility
        public ValueModifier<BlueprintActivatableAbility, UnitCommand.CommandType> ActionTypeOfAngelicForm
            = new ValueModifier<BlueprintActivatableAbility, UnitCommand.CommandType>(
                () => FixActionTypeOfAngelicForm,
                new string[] { "83e91b42102fdf04a98e86a0d515cd60" },
                (blueprint) => blueprint.ActivateWithUnitCommandType,
                (blueprint, value) => blueprint.SetActivateWithUnitCommand(value),
                UnitCommand.CommandType.Move);

        // RuneDomainBaseAbilityAcidArea
        // RuneDomainBaseAbilityColdArea
        // RuneDomainBaseAbilityElectricityArea
        // RuneDomainBaseAbilityFireArea
        public ComponentModifier<BlueprintAbilityAreaEffect> DamageBonusOfBlastRune
            = new ComponentModifier<BlueprintAbilityAreaEffect>(
                () => FixDamageBonusOfBlastRune,
                new string[] { "98c3a36f2a3636c49a3f77c001a25f29", "8b8e98e8e0000f643ad97c744f3f850b",
                    "db868c576c69d0e4a8462645267c6cdc", "9b786945d2ec1884184235a488e5cb9e" },
                (lib, coms) => coms.AddToArray(
                    lib.Get<BlueprintAbility>("92c821ecc8d73564bad15a8a07ed40f2")   // RuneDomainBaseAbilityAcid
                    .GetComponents<ContextRankConfig>().ToArray()));

        // ShadowEvocationGreaterSiroccoArea
        public ValueModifier<BlueprintAbilityAreaEffect, string> FxOfShadowEvocationSirocco
            = new ValueModifier<BlueprintAbilityAreaEffect, string>(
                () => FixFxOfShadowEvocationSirocco,
                new string[] { "bb87c7513a16b9a44b4948a4e932a81b" },
                (blueprint) => blueprint.Fx.AssetId,
                (blueprint, value) => blueprint.Fx.AssetId = value,
                "9f9ebe136ce5a9345b5b016f011c5aa6");    // BlueprintAbilityAreaEffect : SiroccoArea : Sirocco00_Cycle_Aoe

        // InspireGreatnessToggleAbility
        // InspireHeroicsToggleAbility
        public ValueModifier<BlueprintActivatableAbility, bool> AbilityDeactivateIfCombatEnded
            = new ValueModifier<BlueprintActivatableAbility, bool>(
                () => FixAbilityNotAutoDeactivateIfCombatEnded,
                new string[] { "be36959e44ac33641ba9e0204f3d227b", "a4ce06371f09f504fa86fcf6d0e021e4" },
                (blueprint) => blueprint.DeactivateIfCombatEnded,
                (blueprint, value) => blueprint.DeactivateIfCombatEnded = value,
                true);

        public void Update(bool modify = true)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), modify);

            ActionTypeOfCharge.Update(modify);
            ActionTypeOfOverrun.Update(modify);
            ActionTypeOfVitalStrike.Update(modify);
            ActionTypeOfAngelicForm.Update(modify);
            DamageBonusOfBlastRune.Update(modify);
            FxOfShadowEvocationSirocco.Update(modify);
            AbilityDeactivateIfCombatEnded.Update(modify);
        }

        public class BlueprintModifier<TBlueprint, TValue> where TBlueprint : BlueprintScriptableObject
        {
            private Func<bool> _option;
            private string[] _assetGuid;
            private Func<TBlueprint, TValue> _getter;
            private Action<TBlueprint, TValue> _setter;
            private Func<LibraryScriptableObject, TValue, TValue> _modifier;
            private TBlueprint[] _blueprints;
            private TValue[] _backup;
            private TValue[] _value;

            public BlueprintModifier(Func<bool> option, string[] assetGuid, 
                Func<TBlueprint, TValue> getter, Action<TBlueprint, TValue> setter, 
                Func<LibraryScriptableObject, TValue, TValue> modifier)
            {
                _option = option;
                _assetGuid = assetGuid;
                _getter = getter;
                _setter = setter;
                _modifier = modifier;
            }

            private bool TryInitialize()
            {
                if (_value != null)
                    return true;

                if (_assetGuid == null)
                    return false;

                LibraryScriptableObject library = 
                    typeof(ResourcesLibrary).GetFieldValue<LibraryScriptableObject>("s_LibraryObject");
                if (library != null && library.GetInitialized())
                {
                    try
                    {
                        _blueprints = _assetGuid.Select(guid => library.Get<TBlueprint>(guid)).ToArray();
                        _backup = _blueprints.Select(blueprint => _getter(blueprint)).ToArray();
                        int length = _assetGuid.Length;
                        _value = new TValue[length];
                        for (int i = 0; i < length; i++)
                            _value[i] = _modifier(library, _backup[i]);
                        _assetGuid = null;
                        _getter = null;
                        _modifier = null;
                        return true;
                    }
                    catch (Exception e)
                    {
                        Mod.Error(e);
                    }
                }
                return false;
            }

            public void Update(bool modify = true)
            {
                if (TryInitialize())
                    for (int i = 0; i < _blueprints.Length; i++)
                        _setter(_blueprints[i], (modify && _option()) ? _value[i] : _backup[i]);
            }
        }

        public class ComponentModifier<TBlueprint> : BlueprintModifier<TBlueprint, BlueprintComponent[]> 
            where TBlueprint : BlueprintScriptableObject
        {
            public ComponentModifier(Func<bool> option, string[] assetGuid, 
                Func<LibraryScriptableObject, BlueprintComponent[], BlueprintComponent[]> modifier)
                : base(option, assetGuid, 
                      (blueprint) => blueprint.ComponentsArray,
                      (blueprint, value) => blueprint.ComponentsArray = value, 
                      modifier) { }
        }

        public class ValueModifier<TBlueprint, TValue> : BlueprintModifier<TBlueprint, TValue>
            where TBlueprint : BlueprintScriptableObject
        {
            public ValueModifier(Func<bool> option, string[] assetGuid, 
                Func<TBlueprint, TValue> getter, Action<TBlueprint, TValue> setter, TValue value)
                : base(option, assetGuid, getter, setter, (lib, val) => value) { }
        }
    }
}