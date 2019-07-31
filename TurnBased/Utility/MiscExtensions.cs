using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
using System.Linq;

namespace TurnBased.Utility
{
    public static class MiscExtensions
    {
        public static T Get<T>(this LibraryScriptableObject library, string assetId) where T : BlueprintScriptableObject
        {
            return library.BlueprintsByAssetId[assetId] as T;
        }

        public static void Clear(this UnitCombatState.Cooldowns cooldown)
        {
            cooldown.Initiative = 0f;
            cooldown.StandardAction = 0f;
            cooldown.MoveAction = 0f;
            cooldown.SwiftAction = 0f;
            cooldown.AttackOfOpportunity = 0f;
        }

        public static bool IsKineticBlast(this ItemEntity item)
        {
            return (item as ItemEntityWeapon)?.Blueprint.Category == WeaponCategory.KineticBlast;
        }

        public static bool Approximately(this float x, float y)
        {
            return y - 0.0001f < x && x < y + 0.0001f;
        }

        public static bool IsRunning(this UnitCommands commands)
        {
            return commands.Raw.Any(command => command != null && command.IsRunning);
        }
    }
}
