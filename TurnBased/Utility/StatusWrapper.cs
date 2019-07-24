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
            return Core.Enabled && Core.Mod.RoundController.Enabled;
        }

        public static bool IsInCombat()
        {
            return Core.Enabled && Core.Mod.RoundController.CombatInitialized && 
                Game.Instance.CurrentMode != GameModeType.Cutscene;
        }

        public static bool IsPreparing()
        {
            return Core.Mod.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Preparing;
        }

        public static bool IsActing()
        {
            return Core.Mod.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Acting;
        }

        public static bool IsDelaying()
        {
            return Core.Mod.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Delayed;
        }

        public static bool IsEnding()
        {
            return Core.Mod.RoundController.CurrentTurn?.Status == TurnController.TurnStatus.Ending;
        }

        public static bool IsPassing()
        {
            return Core.Mod.RoundController.CurrentTurn == null;
        }
    }
}
