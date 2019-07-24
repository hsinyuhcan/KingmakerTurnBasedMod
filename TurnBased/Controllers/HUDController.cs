using Kingmaker;
using Kingmaker.PubSubSystem;
using ModMaker;
using ModMaker.Utility;
using System.Reflection;
using TurnBased.HUD;
using UnityEngine;
using static TurnBased.Main;

namespace TurnBased.Controllers
{
    public class HUDController :
        IModEventHandler,
        IPartyCombatHandler,
        ISceneHandler
    {
        public static void Attach()
        {
            if (Core.Mod.CombatTrackerManager.IsNullOrDestroyed())
            {
                Core.Mod.CombatTrackerManager = CombatTrackerManager.CreateObject();
            }

            if (Core.Mod.AttackIndicatorManager.IsNullOrDestroyed())
            {
                Core.Mod.AttackIndicatorManager = AttackIndicatorManager.CreateObject();
            }

            if (Core.Mod.MovementIndicatorManager.IsNullOrDestroyed())
            {
                Core.Mod.MovementIndicatorManager = MovementIndicatorManager.CreateObject();
            }
        }

        public static void Detach()
        {
            CombatTrackerManager combatTracker = Core.Mod.CombatTrackerManager;
            if (!combatTracker.IsNullOrDestroyed())
            {
                combatTracker.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(combatTracker.gameObject);
            }
            Core.Mod.CombatTrackerManager = null;

            AttackIndicatorManager attackIndicator = Core.Mod.AttackIndicatorManager;
            if (!attackIndicator.IsNullOrDestroyed())
            {
                attackIndicator.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(attackIndicator.gameObject);
            }
            Core.Mod.AttackIndicatorManager = null;

            MovementIndicatorManager movementIndicator = Core.Mod.MovementIndicatorManager;
            if (!movementIndicator.IsNullOrDestroyed())
            {
                movementIndicator.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(movementIndicator.gameObject);
            }
            Core.Mod.MovementIndicatorManager = null;
        }

#if DEBUG
        public static void Clear()
        {
            while (true)
            {
                Transform combatTracker = Game.Instance.UI.Common.transform.Find("HUDLayout/TurnBasedCombatTracker");
                if (!combatTracker.IsNullOrDestroyed())
                {
                    combatTracker.SetParent(null, false);
                    UnityEngine.Object.DestroyImmediate(combatTracker.gameObject);
                }
                else
                {
                    break;
                }
            }
            Core.Mod.CombatTrackerManager = null;

            while (true)
            {
                Transform attackIndicator = Game.Instance.UI.Common.transform.Find("AbilityTargetSelect/TurnBasedAttackIndicator");
                if (!attackIndicator.IsNullOrDestroyed())
                {
                    attackIndicator.SetParent(null, false);
                    UnityEngine.Object.DestroyImmediate(attackIndicator.gameObject);
                }
                else
                {
                    break;
                }
            }
            Core.Mod.AttackIndicatorManager = null;

            while (true)
            {
                Transform movementIndicator = Game.Instance.UI.Common.transform.Find("AbilityTargetSelect/TurnBasedMovementIndicator");
                if (!movementIndicator.IsNullOrDestroyed())
                {
                    movementIndicator.SetParent(null, false);
                    UnityEngine.Object.DestroyImmediate(movementIndicator.gameObject);
                }
                else
                {
                    break;
                }
            }
            Core.Mod.MovementIndicatorManager = null;
        }
#endif

        #region Event Handlers

        public void HandleModEnable()
        {
            Core.Debug($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}()");

            Attach();
            EventBus.Subscribe(this);
        }

        public void HandleModDisable()
        {
            Core.Debug($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}()");

            Detach();
            EventBus.Unsubscribe(this);
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            Core.Debug($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}({inCombat})");

            if (inCombat)
            {
                Attach();
            }
        }

        public void OnAreaBeginUnloading()
        {
            Core.Debug($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}()");

            Detach();
        }

        public void OnAreaDidLoad() { }

        #endregion
    }
}
