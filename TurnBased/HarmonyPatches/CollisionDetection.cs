using Harmony12;
using Kingmaker.View;
using TurnBased.Utility;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class CollisionDetection
    {
        // moving through friend feature
        [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.AvoidanceDisabled), MethodType.Getter)]
        static class UnitMovementAgent_AvoidanceDisabled_Patch
        {
            [HarmonyPrefix]
            static void Postfix(UnitMovementAgent __instance, ref bool __result)
            {
                if (IsInCombat() && !__result)
                {
                    __result = (Mod.Core.Combat.CurrentTurn?.Unit).CanMoveThrough(__instance.Unit?.EntityData);
                }
            }
        }

        // modify collision radius
        [HarmonyPatch(typeof(UnitMovementAgent), nameof(UnitMovementAgent.Corpulence), MethodType.Getter)]
        static class UnitMovementAgent_get_Corpulence_Patch
        {
            [HarmonyPostfix]
            static void Postfix(ref float __result)
            {
                if (IsInCombat())
                {
                    __result *= RadiusOfCollision;
                }
            }
        }
    }
}
