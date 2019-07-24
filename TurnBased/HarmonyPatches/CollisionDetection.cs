using Harmony12;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Pathfinding;
using TurnBased.Utility;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    internal static class CollisionDetection
    {

        // moving through friend feature
        [HarmonyPatch(typeof(UnitMovementAgent), "UpdateUnitAvoidance")]
        static class UnitMovementAgent_UpdateUnitAvoidance_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitMovementAgent __instance, UnitEntityData unit)
            {
                return !IsInCombat() || !(__instance.Unit?.EntityData).CanMoveThrough(unit);
            }
        }

        // moving through friend feature
        [HarmonyPatch(typeof(UnitMovementAgent), "OnPathComplete", typeof(Path))]
        static class UnitMovementAgent_OnPathComplete_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitMovementAgent __instance)
            {
                if (IsInCombat())
                {
                    Core.Mod.PathfindingUnit = __instance.Unit?.EntityData;
                }
            }

            [HarmonyPostfix]
            static void Postfix()
            {
                if (IsInCombat())
                {
                    Core.Mod.PathfindingUnit = null;
                }
            }
        }

        // moving through friend feature
        [HarmonyPatch(typeof(ObstaclePathfinder), "IntersectObstacle")]
        static class ObstaclePathfinder_IntersectObstacle_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitMovementAgent unit)
            {
                return !IsInCombat() || !Core.Mod.PathfindingUnit.CanMoveThrough(unit.Unit?.EntityData);
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
