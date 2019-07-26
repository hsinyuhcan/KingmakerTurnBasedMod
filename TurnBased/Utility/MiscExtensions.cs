using Kingmaker.Controllers.Combat;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace TurnBased.Utility
{
    public static class MiscExtensions
    {
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
    }
}
