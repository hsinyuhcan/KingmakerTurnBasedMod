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
            if (Mod.Core.CombatTrackerManager.IsNullOrDestroyed())
            {
                Mod.Core.CombatTrackerManager = CombatTrackerManager.CreateObject();
            }

            if (Mod.Core.AttackIndicatorManager.IsNullOrDestroyed())
            {
                Mod.Core.AttackIndicatorManager = AttackIndicatorManager.CreateObject();
            }

            if (Mod.Core.MovementIndicatorManager.IsNullOrDestroyed())
            {
                Mod.Core.MovementIndicatorManager = MovementIndicatorManager.CreateObject();
            }
        }

        public static void Detach()
        {
            CombatTrackerManager combatTracker = Mod.Core.CombatTrackerManager;
            if (!combatTracker.IsNullOrDestroyed())
            {
                combatTracker.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(combatTracker.gameObject);
            }
            Mod.Core.CombatTrackerManager = null;

            AttackIndicatorManager attackIndicator = Mod.Core.AttackIndicatorManager;
            if (!attackIndicator.IsNullOrDestroyed())
            {
                attackIndicator.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(attackIndicator.gameObject);
            }
            Mod.Core.AttackIndicatorManager = null;

            MovementIndicatorManager movementIndicator = Mod.Core.MovementIndicatorManager;
            if (!movementIndicator.IsNullOrDestroyed())
            {
                movementIndicator.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(movementIndicator.gameObject);
            }
            Mod.Core.MovementIndicatorManager = null;
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
            Mod.Core.CombatTrackerManager = null;

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
            Mod.Core.AttackIndicatorManager = null;

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
            Mod.Core.MovementIndicatorManager = null;
        }
#endif

        #region Event Handlers

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Attach();
            EventBus.Subscribe(this);
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Detach();
            EventBus.Unsubscribe(this);
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), inCombat);

            if (inCombat)
            {
                Attach();
            }
        }

        public void OnAreaBeginUnloading()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Detach();
        }

        public void OnAreaDidLoad() { }

        #endregion
    }
}
