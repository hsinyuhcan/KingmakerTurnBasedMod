using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using System.Linq;

namespace TurnBased.Utility
{
    public static class MiscExtensions
    {
        public static float GetAbilityRadius(this AbilityData ability)
        {
            if (ability.Blueprint.Range != AbilityRange.Unlimited)
            {
                float meters;

                if (ability.TargetAnchor == AbilityTargetAnchor.Owner)
                {
                    if (ability.Blueprint.AoERadius == 0.Feet())
                    {
                        return 0f;
                    }
                    meters = ability.Blueprint.AoERadius.Meters;
                }
                else
                {
                    meters = ability.GetVisualDistance();
                }

                if (ability.IsPierceOrCone)
                {
                    meters += ability.Caster.Unit.Corpulence;
                }

                return meters;
            }
            return 0f;
        }

        public static void Clear(this UnitCombatState.Cooldowns cooldown)
        {
            cooldown.Initiative = 0f;
            cooldown.StandardAction = 0f;
            cooldown.MoveAction = 0f;
            cooldown.SwiftAction = 0f;
            cooldown.AttackOfOpportunity = 0f;
        }

        public static bool Approximately(this float x, float y, float deviation)
        {
            return y - deviation < x && x < y + deviation;
        }

        public static bool IsKineticBlast(this ItemEntity item)
        {
            return (item as ItemEntityWeapon)?.Blueprint.Category == WeaponCategory.KineticBlast;
        }

        public static T Get<T>(this LibraryScriptableObject library, string assetId) where T : BlueprintScriptableObject
        {
            return library.BlueprintsByAssetId[assetId] as T;
        }

        public static bool IsRunning(this UnitCommands commands)
        {
            return commands.Raw.Any(command => command != null && command.IsRunning);
        }

        public static bool HasEnemy(this UnitGroup group)
        {
            return Game.Instance.UnitGroups.Any(other => other != group && other.IsInCombat && other.IsEnemy(group));
        }
    }
}