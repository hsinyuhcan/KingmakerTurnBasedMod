using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.AbilityTarget;
using ModMaker.Utility;
using System.Reflection;
using TurnBased.Controllers;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.UI
{
    public class MovementIndicatorManager : MonoBehaviour
    {
        private RangeIndicatorManager _rangeInner;
        private RangeIndicatorManager _rangeOuter;

        public static MovementIndicatorManager CreateObject()
        {
            GameObject abilityTargetSelect = Game.Instance.UI.Common?.transform.Find("AbilityTargetSelect")?.gameObject;
            GameObject aoeRange = abilityTargetSelect?.GetComponent<AbilityAoERange>().Range;

            if (!aoeRange)
                return null;

            GameObject movementIndicator = new GameObject("TurnBasedMovementIndicator");
            movementIndicator.transform.SetParent(abilityTargetSelect.transform, true);

            MovementIndicatorManager movementIndicatorManager = movementIndicator.AddComponent<MovementIndicatorManager>();

            movementIndicatorManager._rangeInner = RangeIndicatorManager.CreateObject(aoeRange, "MovementRangeInner");
            movementIndicatorManager._rangeInner.VisibleColor = Color.white;
            DontDestroyOnLoad(movementIndicatorManager._rangeInner.gameObject);

            movementIndicatorManager._rangeOuter = RangeIndicatorManager.CreateObject(aoeRange, "MovementRangeOuter");
            movementIndicatorManager._rangeOuter.VisibleColor = Color.white;
            DontDestroyOnLoad(movementIndicatorManager._rangeOuter.gameObject);

            return movementIndicatorManager;
        }

        void OnEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_MOVEMENT_INDICATOR, HandleToggleMovementIndicator);
        }

        void OnDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            HotkeyHelper.Unbind(HOTKEY_FOR_TOGGLE_MOVEMENT_INDICATOR, HandleToggleMovementIndicator);
        }

        void OnDestroy()
        {
            _rangeInner.SafeDestroy();
            _rangeOuter.SafeDestroy();
        }

        void Update()
        {
            if (IsInCombat() && IsHUDShown())
            {
                UnitEntityData unit = null;
                float radiusInner = 0f;
                float radiusOuter = 0f;

                if (ShowMovementIndicatorOnHoverUI && (unit = Mod.Core.UI.CombatTracker.HoveringUnit) != null)
                {
                    radiusInner = unit.CurrentSpeedMps * TIME_MOVE_ACTION;
                    radiusOuter = radiusInner * 2f;
                }
                else
                {
                    if (ShowMovementIndicatorOfCurrentUnit && (unit = CurrentUnit(out TurnController currentTurn)) != null &&
                        (unit.IsDirectlyControllable ? ShowMovementIndicatorForPlayer : ShowMovementIndicatorForNonPlayer))
                    {
                        radiusInner = currentTurn.GetRemainingMovementRange();
                        radiusOuter = currentTurn.GetRemainingMovementRange(true);
                    }
                }

                if (unit != null && radiusOuter > 0 && (!DoNotMarkInvisibleUnit || unit.IsVisibleForPlayer))
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

        private void HandleToggleMovementIndicator()
        {
            ShowMovementIndicatorOfCurrentUnit = !ShowMovementIndicatorOfCurrentUnit;
        }
    }
}