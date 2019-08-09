using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.AbilityTarget;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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
                UnitEntityData unit;
                float radius = 0f;
                bool canTargetEnemies = true;
                bool canTargetFriends = false;

                if ((unit = ShowAttackIndicatorOnHoverUI ? Mod.Core.UI.CombatTracker.HoveringUnit : null) != null && 
                    !unit.IsCurrentUnit())
                {
                    GetRadius();
                }
                else
                {
                    TurnController currentTurn = Mod.Core.Combat.CurrentTurn;
                    if ((unit = currentTurn?.Unit) != null && ShowAttackIndicatorOfCurrentUnit &&
                        (unit.IsDirectlyControllable ? ShowAttackIndicatorOfPlayer : ShowAttackIndicatorOfNonPlayer))
                    {
                        GetRadius();

                        if (radius > 0f && currentTurn.EnabledFiveFootStep)
                        {
                            radius += currentTurn.GetRemainingMovementRange();
                        }
                    }
                }

                if (unit != null && radius > 0 && (!DoNotMarkInvisibleUnit || unit.IsVisibleForPlayer))
                {
                    _range.SetPosition(unit);
                    _range.SetRadius(radius);
                    _range.SetVisible(true);

                    Unit = unit;
                    EventBus.RaiseEvent<IShowAoEAffectedUIHandler>
                        (h => h.HandleAoEMove(new Vector3(radius, canTargetEnemies ? 1f : 0f, canTargetFriends ? 1f : 0f), null));

                    return;
                }

                void GetRadius()
                {
                    AbilityData ability = ShowAutoCastAbilityRange ? unit.GetAvailableAutoUseAbility() : null;
                    if (ability != null)
                    {
                        radius = ability.GetAbilityRadius();
                        canTargetEnemies = ability.Blueprint.CanTargetEnemies;
                        canTargetFriends = ability.Blueprint.CanTargetFriends;
                        _range.VisibleColor = ability.TargetAnchor == AbilityTargetAnchor.Owner ? Color.green : Color.yellow;

                    }
                    else
                    {
                        radius = unit.GetAttackRadius();
                        _range.VisibleColor = Color.red;
                    }
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
