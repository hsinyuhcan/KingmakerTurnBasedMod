using Kingmaker;
using Kingmaker.PubSubSystem;
using ModMaker;
using ModMaker.Utility;
using System.Reflection;
using TurnBased.UI;
using UnityEngine;
using static TurnBased.Main;

namespace TurnBased.Controllers
{
    public class UIController :
        IModEventHandler,
        //IPartyCombatHandler,
        ISceneHandler
    {
        public CombatTrackerManager CombatTracker { get; private set; }

        public AttackIndicatorManager AttackIndicator { get; private set; }

        public MovementIndicatorManager MovementIndicator { get; private set; }

        public void Attach()
        {
            if (CombatTracker.IsNullOrDestroyed())
            {
                CombatTracker = CombatTrackerManager.CreateObject();
            }

            if (AttackIndicator.IsNullOrDestroyed())
            {
                AttackIndicator = AttackIndicatorManager.CreateObject();
            }

            if (MovementIndicator.IsNullOrDestroyed())
            {
                MovementIndicator = MovementIndicatorManager.CreateObject();
            }
        }

        public void Detach()
        {
            if (!CombatTracker.IsNullOrDestroyed())
            {
                CombatTracker.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(CombatTracker.gameObject);
            }
            CombatTracker = null;

            if (!AttackIndicator.IsNullOrDestroyed())
            {
                AttackIndicator.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(AttackIndicator.gameObject);
            }
            AttackIndicator = null;

            if (!MovementIndicator.IsNullOrDestroyed())
            {
                MovementIndicator.transform.SetParent(null, false);
                UnityEngine.Object.DestroyImmediate(MovementIndicator.gameObject);
            }
            MovementIndicator = null;
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
            Mod.Core.UI.CombatTracker = null;

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
            Mod.Core.UI.AttackIndicator = null;

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
            Mod.Core.UI.MovementIndicator = null;
        }
#endif

        #region Event Handlers

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Subscribe(this);

            Mod.Core.UI = this;
            Attach();
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Unsubscribe(this);

            Mod.Core.UI = null;
            Detach();
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Attach();
        }

        #endregion
    }
}
