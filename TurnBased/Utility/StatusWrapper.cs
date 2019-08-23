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
            return Mod.Enabled && Mod.Core.Enabled;
        }

        public static bool IsInCombat()
        {
            CombatController combat;
            GameModeType currentMode;
            return Mod.Enabled && (combat = Mod.Core.Combat).Initialized && combat.HasEnemyInCombat &&
                ((currentMode = Game.Instance.CurrentMode) == GameModeType.Default || currentMode == GameModeType.Pause);
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