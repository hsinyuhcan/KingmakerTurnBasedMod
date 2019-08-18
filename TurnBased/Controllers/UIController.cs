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
            if (!CombatTracker)
            {
                CombatTracker = CombatTrackerManager.CreateObject();
            }

            if (!AttackIndicator)
            {
                AttackIndicator = AttackIndicatorManager.CreateObject();
            }

            if (!MovementIndicator)
            {
                MovementIndicator = MovementIndicatorManager.CreateObject();
            }
        }

        public void Detach()
        {
            CombatTracker.SafeDestroy();
            CombatTracker = null;

            AttackIndicator.SafeDestroy();
            AttackIndicator = null;

            MovementIndicator.SafeDestroy();
            MovementIndicator = null;
        }

#if DEBUG
        public void Clear()
        {
            while (true)
            {
                Transform combatTracker = Game.Instance.UI.Common.transform.Find("HUDLayout/TurnBasedCombatTracker");
                if (combatTracker)
                {
                    UnityEngine.Object.DestroyImmediate(combatTracker.gameObject);
                }
                else
                {
                    break;
                }
            }
            CombatTracker = null;

            while (true)
            {
                Transform attackIndicator = Game.Instance.UI.Common.transform.Find("AbilityTargetSelect/TurnBasedAttackIndicator");
                if (attackIndicator)
                {
                    UnityEngine.Object.DestroyImmediate(attackIndicator.gameObject);
                }
                else
                {
                    break;
                }
            }
            AttackIndicator = null;

            while (true)
            {
                Transform movementIndicator = Game.Instance.UI.Common.transform.Find("AbilityTargetSelect/TurnBasedMovementIndicator");
                if (movementIndicator)
                {
                    UnityEngine.Object.DestroyImmediate(movementIndicator.gameObject);
                }
                else
                {
                    break;
                }
            }
            MovementIndicator = null;
        }
#endif

        #region Event Handlers

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Mod.Core.UI = this;
            Attach();

            EventBus.Subscribe(this);
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Unsubscribe(this);

            Detach();
            Mod.Core.UI = null;
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