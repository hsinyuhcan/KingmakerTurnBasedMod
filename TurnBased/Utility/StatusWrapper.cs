using Kingmaker;
using Kingmaker.EntitySystem.Entities;
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
            GameModeType currentMode;
            return Mod.Enabled && Mod.Core.Combat.Initialized &&
                ((currentMode = Game.Instance.CurrentMode) == GameModeType.Default || currentMode == GameModeType.Pause);
        }

        public static bool IsPreparing()
        {
            return CurrentTurn()?.Status == TurnController.TurnStatus.Preparing;
        }

        public static bool IsActing()
        {
            return CurrentTurn()?.Status == TurnController.TurnStatus.Acting;
        }

        public static bool IsDelaying()
        {
            return CurrentTurn()?.Status == TurnController.TurnStatus.Delayed;
        }

        public static bool IsEnding()
        {
            return CurrentTurn()?.Status == TurnController.TurnStatus.Ending;
        }

        public static bool IsPassing()
        {
            return CurrentTurn() == null;
        }

        public static TurnController CurrentTurn()
        {
            return Mod.Core.Combat.CurrentTurn;
        }

        public static UnitEntityData CurrentUnit()
        {
            return CurrentTurn()?.Unit;
        }

        public static UnitEntityData CurrentUnit(out TurnController currentTurn)
        {
            return (currentTurn = CurrentTurn())?.Unit;
        }
    }
}