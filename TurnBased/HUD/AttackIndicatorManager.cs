using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.AbilityTarget;
using ModMaker.Utility;
using System.Reflection;
using TurnBased.Controllers;
using TurnBased.Utility;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.HUD
{
    public class AttackIndicatorManager : MonoBehaviour
    {
        private RangeIndicatorManager _range;

        public bool Disabled;

        public UnitEntityData Unit { get; private set; }

        void Awake()
        {
            Core.Debug($"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}()");
        }

        void OnDestroy()
        {
            if (!_range.IsNullOrDestroyed())
                Destroy(_range.gameObject);
        }

        void Update()
        {
            bool isInCombat = IsInCombat();
            if (isInCombat && !Disabled && Game.Instance.SelectedAbilityHandler?.Ability == null)
            {
                UnitEntityData unit = ShowAttackIndicatorOnHoverUI ? Core.Mod.CombatTrackerManager.HoveringUnit : null;
                float radius = 0f;

                if (unit != null && !unit.IsCurrentUnit())
                {
                    radius = unit.GetAttackRange();
                }
                else
                {
                    TurnController currentTurn = Core.Mod.RoundController.CurrentTurn;
                    if (currentTurn != null)
                    {
                        unit = currentTurn.Unit;
                        if (ShowAttackIndicatorOfCurrentUnit &&
                            (unit.IsDirectlyControllable ? ShowAttackIndicatorOfPlayer : ShowAttackIndicatorOfNonPlayer))
                        {
                            radius = currentTurn.EnabledFiveFootStep ?
                                unit.GetAttackRange() + currentTurn.GetRemainingMovementRange() : unit.GetAttackRange();
                        }
                    }
                }

                if (unit != null && radius > 0)
                {
                    _range.SetPosition(unit);
                    _range.SetRadius(radius);
                    _range.SetVisible(true);

                    Unit = unit;
                    EventBus.RaiseEvent<IShowAoEAffectedUIHandler>(h => h.HandleAoEMove(new Vector3(), null));

                    return;
                }
            }

            if (Unit != null)
            {
                _range.SetVisible(false);

                Unit = null;
                EventBus.RaiseEvent<IShowAoEAffectedUIHandler>(h => h.HandleAoECancel());
            }

            if (!isInCombat && Disabled)
            {
                Disabled = false;
            }
        }

        public static AttackIndicatorManager CreateObject()
        {
            GameObject abilityTargetSelect = Game.Instance.UI.Common?.transform.Find("AbilityTargetSelect")?.gameObject;
            GameObject aoeRange = abilityTargetSelect?.GetComponent<AbilityAoERange>().Range;

            if (aoeRange.IsNullOrDestroyed())
                return null;

            GameObject tbAttackIndicator = new GameObject("TurnBasedAttackIndicator");
            tbAttackIndicator.transform.SetParent(abilityTargetSelect.transform, true);

            AttackIndicatorManager tbAttackIndicatorManager = tbAttackIndicator.AddComponent<AttackIndicatorManager>();

            tbAttackIndicatorManager._range = RangeIndicatorManager.CreateObject(aoeRange, "AttackRange", false);
            tbAttackIndicatorManager._range.VisibleColor = Color.red;
            DontDestroyOnLoad(tbAttackIndicatorManager._range.gameObject);

            return tbAttackIndicatorManager;
        }
    }
}
