using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Commands.Base;
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
        public BlueprintModifier<BlueprintAbility, bool> ActionTypeOfCharge = new BlueprintModifier<BlueprintAbility, bool>(
            () => FixActionTypeOfCharge,
            new string[] { "c78506dd0e14f7c45a599990e4e65038", "d4b4757660cb66e4fbf376a43f1ffb13" },
            (blueprint) => blueprint.IsFullRoundAction,
            (blueprint, value) => blueprint.SetIsFullRoundAction(value),
            true);

        // OverrunAbility
        // ChargeAbilityLanternKingStar
        // FlyTrampleTest
        public BlueprintModifier<BlueprintAbility, bool> ActionTypeOfOverrun = new BlueprintModifier<BlueprintAbility, bool>(
            () => FixActionTypeOfOverrun,
            new string[] { "1a3b471ecea51f7439a946b23577fd70", "49b8bf9a35ecbd24482ee416cd7557b8", "f0b622ab2d18ef7439feb8aa5680d6e5" },
            (blueprint) => blueprint.IsFullRoundAction,
            (blueprint, value) => blueprint.SetIsFullRoundAction(value),
            true);

        // VitalStrikeAbility
        // VitalStrikeAbilityImproved
        // VitalStrikeAbilityGreater
        public BlueprintModifier<BlueprintAbility, bool> ActionTypeOfVitalStrike = new BlueprintModifier<BlueprintAbility, bool>(
            () => FixActionTypeOfVitalStrike,
            new string[] { "efc60c91b8e64f244b95c66b270dbd7c", "c714cd636700ac24a91ca3df43326b00", "11f971b6453f74d4594c538e3c88d499" },
            (blueprint) => blueprint.IsFullRoundAction,
            (blueprint, value) => blueprint.SetIsFullRoundAction(value),
            false);

        // TristianAngelAbility
        public BlueprintModifier<BlueprintActivatableAbility, UnitCommand.CommandType> ActionTypeOfAngelicForm
            = new BlueprintModifier<BlueprintActivatableAbility, UnitCommand.CommandType>(
                () => FixActionTypeOfAngelicForm,
                new string[] { "83e91b42102fdf04a98e86a0d515cd60" },
                (blueprint) => blueprint.ActivateWithUnitCommandType,
                (blueprint, value) => blueprint.SetActivateWithUnitCommand(value),
                UnitCommand.CommandType.Move);

        // InspireGreatnessToggleAbility
        // InspireHeroicsToggleAbility
        public BlueprintModifier<BlueprintActivatableAbility, bool> AbilityDeactivateIfCombatEnded 
            = new BlueprintModifier<BlueprintActivatableAbility, bool>(
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
            AbilityDeactivateIfCombatEnded.Update(modify);
        }

        public class BlueprintModifier<TBlueprint, TValue> where TBlueprint : BlueprintScriptableObject
        {
            private readonly Func<bool> _option;
            private string[] _assetGuid;
            private readonly Func<TBlueprint, TValue> _getter;
            private readonly Action<TBlueprint, TValue> _setter;
            private readonly TValue _value;
            private TBlueprint[] _blueprints;
            private TValue[] _backup;

            public BlueprintModifier(Func<bool> option, string[] assetGuid, 
                Func<TBlueprint, TValue> getter, Action<TBlueprint, TValue> setter, TValue value)
            {
                _option = option;
                _assetGuid = assetGuid;
                _getter = getter;
                _setter = setter;
                _value = value;
            }

            private void TryGetBlueprints()
            {
                if (_assetGuid == null)
                    return;

                LibraryScriptableObject library = typeof(ResourcesLibrary).GetFieldValue<LibraryScriptableObject>("s_LibraryObject");
                if (library != null && library.GetInitialized())
                {
                    try
                    {
                        _blueprints = _assetGuid.Select(guid => library.Get<TBlueprint>(guid)).ToArray();
                        _backup = _blueprints.Select(blueprint => _getter(blueprint)).ToArray();
                        _assetGuid = null;
                    }
                    catch (Exception e)
                    {
                        Mod.Error(e);
                    }
                }
            }

            public void Update(bool modify = true)
            {
                TryGetBlueprints();

                if (_blueprints != null && _backup != null)
                    for (int i = 0; i < _blueprints.Length; i++)
                        _setter(_blueprints[i], (modify && _option()) ? _value : _backup[i]);
            }
        }
    }
}