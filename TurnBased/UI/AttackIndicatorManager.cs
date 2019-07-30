using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.AbilityTarget;
using Kingmaker.UnitLogic.Abilities;
using ModMaker.Utility;
using System.Reflection;
using TurnBased.Controllers;
using TurnBased.Utility;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.UI
{
    public class AttackIndicatorManager : 
        MonoBehaviour,
        IAbilityTargetHoverUIHandler,
        IAbilityTargetSelectionUIHandler,
        IShowAoEAffectedUIHandler
    {
        private bool _isAbilityHovered;
        private bool _isAbilitySelected;
        private bool _isHandlingAOEMove;
        private RangeIndicatorManager _range;

        public UnitEntityData Unit { get; private set; }

        void Awake()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Subscribe(this);

            HotkeyHelper.Bind(HOTKEY_FOR_TOGGLE_ATTACK_INDICATOR, HandleToggleAttackIndicator);
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe(this);

            HotkeyHelper.Unbind(HOTKEY_FOR_TOGGLE_ATTACK_INDICATOR, HandleToggleAttackIndicator);

            if (!_range.IsNullOrDestroyed())
                Destroy(_range.gameObject);
        }

        void Update()
        {
            if (IsInCombat() && !_isAbilityHovered && !_isAbilitySelected)
            {
                UnitEntityData unit = ShowAttackIndicatorOnHoverUI ? Mod.Core.UI.CombatTracker.HoveringUnit : null;
                float radius = 0f;

                if (unit != null && !unit.IsCurrentUnit())
                {
                    radius = unit.GetAttackRange();
                }
                else
                {
                    TurnController currentTurn = Mod.Core.Combat.CurrentTurn;
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
                if (_isHandlingAOEMove)
                    EventBus.RaiseEvent<IShowAoEAffectedUIHandler>(h => h.HandleAoECancel());
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

        private void HandleToggleAttackIndicator()
        {
            ShowAttackIndicatorOfCurrentUnit = !ShowAttackIndicatorOfCurrentUnit;
        }

        public void HandleAbilityTargetHover(AbilityData ability, bool hover)
        {
            _isAbilityHovered = hover;
        }

        public void HandleAbilityTargetSelectionStart(AbilityData ability)
        {
            _isAbilitySelected = true;
        }

        public void HandleAbilityTargetSelectionEnd(AbilityData ability)
        {
            _isAbilitySelected = false;
        }

        public void HandleAoEMove(Vector3 pos, AbilityData ability)
        {
            _isHandlingAOEMove = ability == null;
        }

        public void HandleAoECancel()
        {
            _isHandlingAOEMove = false;
        }
    }
}
