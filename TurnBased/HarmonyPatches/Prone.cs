using Harmony12;
using Kingmaker;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.View;
using System;
using TurnBased.Utility;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HarmonyPatches
{
    static class Prone
    {

        // fix prone (units can only stand up in their turn)
        [HarmonyPatch(typeof(UnitProneController), nameof(UnitProneController.Tick), typeof(UnitEntityData))]
        static class UnitProneController_Tick_Patch
        {
            [HarmonyPrefix]
            static void Prefix(UnitEntityData unit)
            {
                if (IsInCombat())
                {
                    ProneState proneState = unit.Descriptor.State.Prone;
                    if (proneState.Active)
                    {
                        if (IsPassing())
                        {
                            if (unit.IsInCombat)
                            {
                                proneState.Duration = 3f.Seconds() - unit.GetTimeToNextTurn().Seconds() - new TimeSpan(1L);
                                if (proneState.Duration < TimeSpan.Zero)
                                {
                                    proneState.Duration = TimeSpan.Zero;
                                }
                                proneState.Duration -= Game.Instance.TimeController.DeltaTime.Seconds();
                            }
                        }
                        else
                        {
                            if (unit.IsCurrentUnit())
                            {
                                if (IsActing() && unit.HasMoveAction())
                                {
                                    proneState.Duration = 3f.Seconds();
                                }
                                else
                                {
                                    proneState.Duration = TimeSpan.Zero;
                                }
                            }
                            proneState.Duration -= Game.Instance.TimeController.DeltaTime.Seconds();
                        }
                    }
                }
            }
        }

        // fix prone (remove the delay after standing up)
        [HarmonyPatch(typeof(UnitEntityView), nameof(UnitEntityView.IsGetUp), MethodType.Getter)]
        static class UnitEntityView_get_IsGetUp_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(UnitEntityView __instance, ref bool __result)
            {
                if (IsInCombat())
                {
                    __result = __instance.AnimationManager?.IsStandUp ?? false;
                    return false;
                }
                return true;
            }
        }
    }
}
