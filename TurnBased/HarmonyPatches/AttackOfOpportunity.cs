using Harmony12;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using TurnBased.Utility;
using static TurnBased.Main;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class AttackOfOpportunity
    {
        // fix sometimes a unit can't make a AoO when the unit is threatening multiple targets
        // do not provoke attack of opportunity when moving using 5-foot step
        [HarmonyPatch(typeof(UnitCombatState), "ShouldAttackOnDisengage", typeof(UnitEntityData))]
        static class UnitCombatState_ShouldAttackOnDisengage_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitCombatState __instance, UnitEntityData target, ref bool __result, ref UnitReference? __state)
            {
                if (IsInCombat())
                {
                    __state = __instance.LastTarget;
                    __instance.LastTarget = target;

                    if (target.IsCurrentUnit() && Mod.Core.Combat.CurrentTurn.ImmuneAttackOfOpportunityOnDisengage)
                    {
                        __result = false;
                        return false;
                    }
                }
                return true;
            }

            [HarmonyPostfix]
            static void Postfix(UnitCombatState __instance, ref UnitReference? __state)
            {
                if (__state.HasValue)
                {
                    __instance.LastTarget = __state.Value;
                }
            }
        }
    }
}