using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using System;
using System.Linq;
using System.Reflection;
using TurnBased.Utility;
using static ModMaker.Utility.ReflectionCache;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;

namespace TurnBased.Controllers
{
    public class BlueprintController : IDisposable
    {
        public LibraryScriptableObject LibraryObject 
            => typeof(ResourcesLibrary).GetFieldValue<LibraryScriptableObject>("s_LibraryObject");

        public void Dispose()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Update(false);
        }

        public void Update(bool modify = true)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), modify);

            UpdateChargeAbility(modify);
            UpdateVitalStrikeAbility(modify);
        }

        #region Charge

        private bool? _backupChargeIsFullRoundAction;

        public void UpdateChargeAbility(bool modify = true)
        {
            LibraryScriptableObject library = LibraryObject;
            if (library != null && library.GetInitialized())
            {
                Mod.Debug(MethodBase.GetCurrentMethod(), modify && FixActionTypeOfCharge);

                if (!_backupChargeIsFullRoundAction.HasValue)
                    _backupChargeIsFullRoundAction =
                        library.Get<BlueprintAbility>("c78506dd0e14f7c45a599990e4e65038").IsFullRoundAction;

                library.Get<BlueprintAbility>("c78506dd0e14f7c45a599990e4e65038").SetIsFullRoundAction
                    ((modify && FixActionTypeOfCharge) ? true : _backupChargeIsFullRoundAction.Value);
            }
        }

        #endregion

        #region Vital Strike

        private readonly string[] _vitalStrikeAssetGuid = new string[] {
            "efc60c91b8e64f244b95c66b270dbd7c", "c714cd636700ac24a91ca3df43326b00", "11f971b6453f74d4594c538e3c88d499"
        };

        private bool[] _backupVitalStrikeIsFullRoundAction;

        public void UpdateVitalStrikeAbility(bool modify = true)
        {
            LibraryScriptableObject library = LibraryObject;
            if (library != null && library.GetInitialized())
            {
                Mod.Debug(MethodBase.GetCurrentMethod(), modify && FixActionTypeOfVitalStrike);

                if (_backupVitalStrikeIsFullRoundAction == null)
                    _backupVitalStrikeIsFullRoundAction = _vitalStrikeAssetGuid
                        .Select(guid => library.Get<BlueprintAbility>(guid).IsFullRoundAction)
                        .ToArray();

                for (int i = 0; i < _vitalStrikeAssetGuid.Length; i++)
                    library.Get<BlueprintAbility>(_vitalStrikeAssetGuid[i]).SetIsFullRoundAction
                        ((modify && FixActionTypeOfVitalStrike) ? false : _backupVitalStrikeIsFullRoundAction[i]);
            }
        }

        #endregion
    }
}
