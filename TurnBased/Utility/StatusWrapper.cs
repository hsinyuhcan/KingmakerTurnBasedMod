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
            return Mod.Enabled && Mod.Core.RoundController.Enabled;
        }

        public static bool IsInCombat()
        {
            return Mod.Enabled && Mod.Core.RoundController.CombatInitialized &&
                Game.Instance.CurrentMode != GameModeType.Cutscene;
        }

        public static bool IsPreparing()
        {
            return Mod.Core.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Preparing;
        }

        public static bool IsActing()
        {
            return Mod.Core.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Acting;
        }

        public static bool IsDelaying()
        {
            return Mod.Core.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Delayed;
        }

        public static bool IsEnding()
        {
            return Mod.Core.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Ending;
        }

        public static bool IsPassing()
        {
            return Mod.Core.RoundController.CurrentTurn == null;
        }
    }
}
