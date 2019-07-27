using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System.Linq;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Controllers
{
    public class BlueprintController
    {
        private readonly string[] _vitalStrikeAssetGuid = new string[] {
            "efc60c91b8e64f244b95c66b270dbd7c",
            "c714cd636700ac24a91ca3df43326b00",
            "11f971b6453f74d4594c538e3c88d499"
        };

        private bool? _backupChargeIsFullRoundAction;
        private bool[] _backupVitalStrikeIsFullRoundAction;

        public LibraryScriptableObject LibraryObject => typeof(ResourcesLibrary).GetFieldValue<LibraryScriptableObject>("s_LibraryObject");

        public void Update()
        {
            UpdateChargeAbility();
            UpdateVitalStrikeAbility();
        }

        public void UpdateChargeAbility()
        {
            LibraryScriptableObject libraryObject = LibraryObject;
            if (libraryObject != null)
            {
                if (!_backupChargeIsFullRoundAction.HasValue)
                    _backupChargeIsFullRoundAction =
                        (libraryObject.BlueprintsByAssetId["c78506dd0e14f7c45a599990e4e65038"] as BlueprintAbility).IsFullRoundAction;

                bool modify = Mod.Core.Combat.CombatInitialized && SetChargeAsFullRoundAction;
                (libraryObject.BlueprintsByAssetId["c78506dd0e14f7c45a599990e4e65038"] as BlueprintAbility)
                    .SetFieldValue("m_IsFullRoundAction", modify ? true : _backupChargeIsFullRoundAction.Value);
            }
        }

        public void UpdateVitalStrikeAbility()
        {
            LibraryScriptableObject libraryObject = LibraryObject;
            if (libraryObject != null)
            {
                if (_backupVitalStrikeIsFullRoundAction == null)
                    _backupVitalStrikeIsFullRoundAction = _vitalStrikeAssetGuid
                        .Select(guid => (libraryObject.BlueprintsByAssetId[guid] as BlueprintAbility).IsFullRoundAction)
                        .ToArray();

                bool modify = Mod.Core.Combat.CombatInitialized && SetVitalStrikeAsStandardAction;
                for (int i = 0; i < _vitalStrikeAssetGuid.Length; i++)
                    (libraryObject.BlueprintsByAssetId[_vitalStrikeAssetGuid[i]] as BlueprintAbility)
                        .SetFieldValue("m_IsFullRoundAction", modify ? false : _backupVitalStrikeIsFullRoundAction[i]);
            }
        }
    }
}
