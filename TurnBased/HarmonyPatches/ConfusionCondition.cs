using Harmony12;
using Kingmaker.UnitLogic.Commands;
using TurnBased.Utility;
using static TurnBased.Main;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class ConfusionCondition
    {
        // ** fix do-nothing action that produced by confusion
        [HarmonyPatch(typeof(UnitDoNothing), "OnTick")]
        static class UnitDoNothing_OnTick_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitDoNothing __instance)
            {
                if (IsInCombat() && __instance.Executor.IsCurrentUnit())
                {
                    __instance.SetTimeSinceStart(6f);
                    Mod.Core.Combat.CurrentTurn.ForceToEnd();
                }
            }
        }

        // ** fix self-harm action that produced by confusion
        [HarmonyPatch(typeof(UnitSelfHarm), "OnAction")]
        static class UnitSelfHarm_OnAction_Patch
        {
            [HarmonyPostfix]
            static void Postfix(UnitSelfHarm __instance)
            {
                if (IsInCombat() && __instance.Executor.IsCurrentUnit())
                {
                    Mod.Core.Combat.CurrentTurn.ForceToEnd();
                }
            }
        }
    }
}