using Kingmaker;
using Kingmaker.GameModes;
using TurnBased.Controllers;
using static TurnBased.Main;

namespace TurnBased.Utility
{
    public static class StatusWrapper
    {
        public static bool IsEnabled()
        {
            return Mod.Enabled && Mod.Core.Combat.Enabled;
        }

        public static bool IsInCombat()
        {
            return Mod.Enabled && Mod.Core.Combat.CombatInitialized &&
                Game.Instance.CurrentMode != GameModeType.Cutscene;
        }

        public static bool IsPreparing()
        {
            return Mod.Core.Combat.CurrentTurn?.Status == TurnController.TurnStatus.Preparing;
        }

        public static bool IsActing()
        {
            return Mod.Core.Combat.CurrentTurn?.Status == TurnController.TurnStatus.Acting;
        }

        public static bool IsDelaying()
        {
            return Mod.Core.Combat.CurrentTurn?.Status == TurnController.TurnStatus.Delayed;
        }

        public static bool IsEnding()
        {
            return Mod.Core.Combat.CurrentTurn?.Status == TurnController.TurnStatus.Ending;
        }

        public static bool IsPassing()
        {
            return Mod.Core.Combat.CurrentTurn == null;
        }
    }
}
