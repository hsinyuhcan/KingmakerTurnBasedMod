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
        public BlueprintModifier<bool> ActionTypeOfCharge = new BlueprintModifier<bool>(
            () => FixActionTypeOfCharge,
            new string[] { "c78506dd0e14f7c45a599990e4e65038", "d4b4757660cb66e4fbf376a43f1ffb13" },
            true,
            (lib, guid) => lib.Get<BlueprintAbility>(guid).IsFullRoundAction,
            (lib, guid, value) => lib.Get<BlueprintAbility>(guid).SetIsFullRoundAction(value));

        // OverrunAbility
        // ChargeAbilityLanternKingStar
        // FlyTrampleTest
        public BlueprintModifier<bool> ActionTypeOfOverrun = new BlueprintModifier<bool>(
            () => FixActionTypeOfOverrun,
            new string[] { "1a3b471ecea51f7439a946b23577fd70", "49b8bf9a35ecbd24482ee416cd7557b8", "f0b622ab2d18ef7439feb8aa5680d6e5" },
            true,
            (lib, guid) => lib.Get<BlueprintAbility>(guid).IsFullRoundAction,
            (lib, guid, value) => lib.Get<BlueprintAbility>(guid).SetIsFullRoundAction(value));

        // VitalStrikeAbility
        // VitalStrikeAbilityImproved
        // VitalStrikeAbilityGreater
        public BlueprintModifier<bool> ActionTypeOfVitalStrike = new BlueprintModifier<bool>(
            () => FixActionTypeOfVitalStrike,
            new string[] { "efc60c91b8e64f244b95c66b270dbd7c", "c714cd636700ac24a91ca3df43326b00", "11f971b6453f74d4594c538e3c88d499" },
            false,
            (lib, guid) => lib.Get<BlueprintAbility>(guid).IsFullRoundAction,
            (lib, guid, value) => lib.Get<BlueprintAbility>(guid).SetIsFullRoundAction(value));

        // TristianAngelAbility
        public BlueprintModifier<UnitCommand.CommandType> ActionTypeOfAngelicForm = new BlueprintModifier<UnitCommand.CommandType>(
            () => FixActionTypeOfAngelicForm,
            new string[] { "83e91b42102fdf04a98e86a0d515cd60" },
            UnitCommand.CommandType.Move,
            (lib, guid) => lib.Get<BlueprintActivatableAbility>(guid).ActivateWithUnitCommandType,
            (lib, guid, value) => lib.Get<BlueprintActivatableAbility>(guid).SetActivateWithUnitCommand(value));

        // InspireGreatnessToggleAbility
        // InspireHeroicsToggleAbility
        public BlueprintModifier<bool> AbilityDeactivateIfCombatEnded = new BlueprintModifier<bool>(
            () => FixAbilityNotAutoDeactivateIfCombatEnded,
            new string[] { "be36959e44ac33641ba9e0204f3d227b", "a4ce06371f09f504fa86fcf6d0e021e4" },
            true,
            (lib, guid) => lib.Get<BlueprintActivatableAbility>(guid).DeactivateIfCombatEnded,
            (lib, guid, value) => lib.Get<BlueprintActivatableAbility>(guid).DeactivateIfCombatEnded = value);

        public void Update(bool modify = true)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), modify);

            ActionTypeOfCharge.Update(modify);
            ActionTypeOfOverrun.Update(modify);
            ActionTypeOfVitalStrike.Update(modify);
            ActionTypeOfAngelicForm.Update(modify);
            AbilityDeactivateIfCombatEnded.Update(modify);
        }

        public class BlueprintModifier<TValue>
        {
            private readonly Func<bool> _option;
            private readonly string[] _assetGuid;
            private readonly TValue _value;
            private readonly Func<LibraryScriptableObject, string, TValue> _getter;
            private readonly Action<LibraryScriptableObject, string, TValue> _setter;
            private TValue[] _backup;

            public BlueprintModifier(Func<bool> option, string[] assetGuid, TValue value, 
                Func<LibraryScriptableObject, string, TValue> getter, Action<LibraryScriptableObject, string, TValue> setter)
            {
                _option = option;
                _assetGuid = assetGuid;
                _value = value;
                _getter = getter;
                _setter = setter;
            }

            public void Update(bool modify = true)
            {
                LibraryScriptableObject library = typeof(ResourcesLibrary).GetFieldValue<LibraryScriptableObject>("s_LibraryObject");
                if (library != null && library.GetInitialized())
                {
                    if (_backup == null)
                        _backup = _assetGuid.Select(guid => _getter(library, guid)).ToArray();

                    for (int i = 0; i < _assetGuid.Length; i++)
                        _setter(library, _assetGuid[i], (modify && _option()) ? _value : _backup[i]);
                } 
            }
        }
    }
}