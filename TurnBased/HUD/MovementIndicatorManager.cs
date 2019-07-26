using Kingmaker;
using Kingmaker.EntitySystem.Entities;
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
    public class MovementIndicatorManager : MonoBehaviour
    {
        private RangeIndicatorManager _rangeInner;
        private RangeIndicatorManager _rangeOuter;

        void Awake()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());
        }

        void OnDestroy()
        {
            if (!_rangeInner.IsNullOrDestroyed())
                Destroy(_rangeInner.gameObject);

            if (!_rangeOuter.IsNullOrDestroyed())
                Destroy(_rangeOuter.gameObject);
        }

        void Update()
        {
            if (IsInCombat())
            {
                UnitEntityData unit = ShowMovementIndicatorOnHoverUI ? Mod.Core.CombatTrackerManager.HoveringUnit : null;
                float radiusInner = 0f;
                float radiusOuter = 0f;

                if (unit != null && !unit.IsCurrentUnit())
                {
                    radiusInner = unit.CurrentSpeedMps * TIME_MOVE_ACTION;
                    radiusOuter = radiusInner * 2f;
                }
                else
                {
                    TurnController currentTurn = Mod.Core.RoundController.CurrentTurn;
                    if (currentTurn != null)
                    {
                        unit = currentTurn.Unit;
                        if (ShowMovementIndicatorOfCurrentUnit &&
                            (unit.IsDirectlyControllable ? ShowMovementIndicatorOfPlayer : ShowMovementIndicatorOfNonPlayer))
                        {
                            radiusInner = currentTurn.GetRemainingMovementRange();
                            radiusOuter = currentTurn.GetRemainingMovementRange(true);
                        }
                    }
                }

                if (unit != null && radiusOuter > 0)
                {
                    _rangeOuter.SetPosition(unit);
                    _rangeOuter.SetRadius(radiusOuter);
                    _rangeOuter.SetVisible(true);

                    if (radiusOuter != radiusInner)
                    {
                        _rangeInner.SetPosition(unit);
                        _rangeInner.SetRadius(radiusInner);
                        _rangeInner.SetVisible(true);
                    }
                    else
                    {
                        _rangeInner.SetVisible(false);
                    }

                    return;
                }
            }

            _rangeInner.SetVisible(false);
            _rangeOuter.SetVisible(false);
        }

        public static MovementIndicatorManager CreateObject()
        {
            GameObject abilityTargetSelect = Game.Instance.UI.Common?.transform.Find("AbilityTargetSelect")?.gameObject;
            GameObject aoeRange = abilityTargetSelect?.GetComponent<AbilityAoERange>().Range;

            if (aoeRange.IsNullOrDestroyed())
                return null;

            GameObject tbMovementIndicator = new GameObject("TurnBasedMovementIndicator");
            tbMovementIndicator.transform.SetParent(abilityTargetSelect.transform, true);

            MovementIndicatorManager tbMovementIndicatorManager = tbMovementIndicator.AddComponent<MovementIndicatorManager>();

            tbMovementIndicatorManager._rangeInner = RangeIndicatorManager.CreateObject(aoeRange, "MovementRangeInner");
            tbMovementIndicatorManager._rangeInner.VisibleColor = Color.white;
            DontDestroyOnLoad(tbMovementIndicatorManager._rangeInner.gameObject);

            tbMovementIndicatorManager._rangeOuter = RangeIndicatorManager.CreateObject(aoeRange, "MovementRangeOuter");
            tbMovementIndicatorManager._rangeOuter.VisibleColor = Color.white;
            DontDestroyOnLoad(tbMovementIndicatorManager._rangeOuter.gameObject);

            return tbMovementIndicatorManager;
        }
    }
}
