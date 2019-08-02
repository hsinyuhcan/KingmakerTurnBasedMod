using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using ModMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TurnBased.Utility;
using UnityEngine;
using static TurnBased.Main;
using static TurnBased.Utility.SettingsWrapper;
using static TurnBased.Utility.StatusWrapper;

namespace TurnBased.Controllers
{
    public class CombatController : 
        IModEventHandler,
        IInGameHandler,
        IPartyCombatHandler,
        ISceneHandler,
        IUnitCombatHandler,
        IUnitCommandActHandler,
        IUnitCommandEndHandler,
        IUnitHandler,
        IUnitInitiativeHandler
    {
        private bool _enabled = true;
        private TimeSpan _combatStartTime;
        private float _combatTimeSinceStart;
        private List<UnitEntityData> _units = new List<UnitEntityData>();
        private HashSet<UnitEntityData> _unitsInSupriseRound = new HashSet<UnitEntityData>();
        private readonly UnitsOrderComaprer _unitsOrderComaprer = new UnitsOrderComaprer();
        private bool _unitsSorted;

        internal readonly HashSet<RayView> TickedRayView = new HashSet<RayView>();

        public bool Enabled {
            get => _enabled;
            set {
                if (_enabled != value)
                {
                    _enabled = value;
                    Reset(value);
                }
            }
        }

        public bool CombatInitialized { get; private set; }

        public TurnController CurrentTurn { get; private set; }

        public bool IsSurpriseRound { get; private set; }

        internal void Tick()
        {
            // fix when the combat end by a cutscenes, HandlePartyCombatStateChanged will not be triggered
            if (_units.Count == 0)
            {
                foreach (UnitEntityData allCharacter in Game.Instance.Player.AllCharacters)
                {
                    allCharacter.Buffs.OnCombatEnded();
                }
                EventBus.RaiseEvent<IPartyCombatHandler>(h => h.HandlePartyCombatStateChanged(false));

                return;
            }

            // advance the turn status
            CurrentTurn?.Tick();

            // reset parameters of the pervious tick
            _unitsSorted = false;
            TickedRayView.Clear();

            // try to start a turn for the next unit
            if (CurrentTurn == null)
            {
                // exiting Surprise Round
                if (IsSurpriseRound && _combatTimeSinceStart >= 6f)
                {
                    IsSurpriseRound = false;
                    _unitsInSupriseRound.Clear();
                }

                // pick the next unit
                UnitEntityData nextUnit = GetSortedUnits().First();
                if (nextUnit.GetTimeToNextTurn() <= 0f && nextUnit.CanPerformAction())
                {
                    InitTurn(nextUnit);
                    _unitsSorted = false;
                }
            }
        }

        internal void TickTime()
        {
            if (CurrentTurn == null)
            {
                // modify time scale
                if (TimeScaleBetweenTurns > 1f)
                {
                    Time.timeScale *= TimeScaleBetweenTurns;
                }

                // trim the delta time, when a turn will start at the end of this tick
                TimeController timeController = Game.Instance.TimeController;
                float timeToNextTurn = GetSortedUnits().First().GetTimeToNextTurn();
                if (timeController.GameDeltaTime > timeToNextTurn && timeToNextTurn != 0f)
                {
                    timeController.SetDeltaTime(timeToNextTurn);
                    timeController.SetGameDeltaTime(timeToNextTurn);
                }

                // advance time
                _combatTimeSinceStart += Game.Instance.TimeController.GameDeltaTime;
            }
            else
            {
                // modify time scale
                bool isDirectlyControllable = CurrentTurn.Unit.IsDirectlyControllable;
                if (isDirectlyControllable && TimeScaleInPlayerTurn > 1f)
                {
                    Time.timeScale *= TimeScaleInPlayerTurn;
                }
                else if (!isDirectlyControllable && TimeScaleInNonPlayerTurn > 1f)
                {
                    Time.timeScale *= TimeScaleInNonPlayerTurn;
                }
            }

            // set game time
            Game.Instance.Player.GameTime = _combatStartTime + TimeSpan.FromSeconds(_combatTimeSinceStart);
        }

        public IEnumerable<UnitEntityData> GetSortedUnits()
        {
            if (!_unitsSorted)
            {
                _units = _units.OrderBy(unit => unit, _unitsOrderComaprer).ToList();    // stable sort
                _unitsSorted = true;
            }
            return _units;
        }

        public bool IsSurprising(UnitEntityData unit)
        {
            return IsSurpriseRound && _unitsInSupriseRound.Contains(unit);
        }

        public void InitTurn(UnitEntityData unit)
        {
            CurrentTurn = new TurnController(unit);
            CurrentTurn.OnDelay += HandleDelayTurn;
            CurrentTurn.OnEnd += HandleEndTurn;
        }

        private void AddUnit(UnitEntityData unit)
        {
            if (unit.IsInCombat && !_units.Contains(unit))
            {
                _units.Add(unit);
                _unitsSorted = false;
            }
        }

        private void RemoveUnit(UnitEntityData unit)
        {
            if (CurrentTurn != null && CurrentTurn.Unit == unit)
            {
                CurrentTurn = null;
            }

            if (_units.Remove(unit))
            {
                _unitsInSupriseRound.Remove(unit);
                _unitsSorted = false;
            }
        }

        private void Reset(bool tryToInitialize, bool isPartyCombatStateChanged = false)
        {
            _combatStartTime = Game.Instance.Player.GameTime;
            _combatTimeSinceStart = 0f;
            _units.Clear();
            _unitsInSupriseRound.Clear();
            _unitsSorted = false;
            TickedRayView.Clear();
            CurrentTurn = null;
            IsSurpriseRound = false;

            // QoLs - on ending the turn-based combat
            if (CombatInitialized && !tryToInitialize)
            {
                if (AutoTurnOnAI)
                    foreach (UnitEntityData unit in UIUtility.GetGroup(false, true))
                        unit.IsAIEnabled = true;

                if (AutoSelectEntireParty)
                    Game.Instance.UI.SelectionManager?.SelectAll();
            }

            // initializing
            if (tryToInitialize && Enabled && Game.Instance.Player.IsInCombat)
            {
                _units.AddRange(Game.Instance.State.Units.Where(unit => unit.IsInCombat));

                if (isPartyCombatStateChanged)
                {
                    foreach (UnitEntityData unit in _units)
                    {
                        if (unit.IsPlayersEnemy ?
                            unit.HasCombatCommand(command => command.TargetUnit.IsPlayerFaction) :
                            unit.HasCombatCommand() &&
                            !Game.Instance.UnitGroups.Any(group => group.IsEnemy(unit) && group.Memory.ContainsVisible(unit)))
                        {
                            _unitsInSupriseRound.Add(unit);
                        }
                    }

                    if (_unitsInSupriseRound.Count > 0)
                    {
                        if (_unitsInSupriseRound.Count < _units.Count)
                            IsSurpriseRound = true;
                        else
                            _unitsInSupriseRound.Clear();
                    }
                }

                CombatInitialized = true;
            }
            else
            {
                CombatInitialized = false;
            }

            // update ability modifications
            Mod.Core.Blueprint.Update();
        }

        #region Event Handlers

        private void HandleDelayTurn(UnitEntityData unit, UnitEntityData targetUnit)
        {
            if (unit != targetUnit)
            {
                RemoveUnit(unit);
                _units.Insert(_units.IndexOf(targetUnit) + 1, unit);
                if (IsSurpriseRound && _combatTimeSinceStart + unit.GetTimeToNextTurn() < 6f)
                    _unitsInSupriseRound.Add(unit);
            }
        }

        private void HandleEndTurn(UnitEntityData unit)
        {
            RemoveUnit(unit);
            _units.Add(unit);
        }

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Subscribe(this);

            Mod.Core.Combat = this;
            Reset(true);
        }

        public void HandleModDisable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            EventBus.Unsubscribe(this);

            Reset(false);
            Mod.Core.Combat = null;
        }

        public void OnAreaBeginUnloading() { }

        public void OnAreaDidLoad()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Reset(false);
        }

        public void HandlePartyCombatStateChanged(bool inCombat)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), inCombat);

            Reset(inCombat, true);
        }

        public void HandleUnitJoinCombat(UnitEntityData unit)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), unit);

            if (CombatInitialized)
            {
                AddUnit(unit);
            }
        }

        public void HandleUnitSpawned(UnitEntityData entityData)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), entityData);

            if (CombatInitialized)
            {
                AddUnit(entityData);
            }
        }

        public void HandleUnitRollsInitiative(RuleInitiativeRoll rule)
        {
            UnitEntityData unit = rule.Initiator;
            UnitCombatState.Cooldowns cooldown = unit.CombatState.Cooldown;
            if (_combatTimeSinceStart == 0f)
            {
                if (unit.Descriptor.GetFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) is Buff summonedUnitAppearBuff)
                {
                    // units that summoned before the combat (surprise) will act in the first round right after the caster 
                    unit.Descriptor.AddBuff
                        (BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff, summonedUnitAppearBuff.Context,
                        (6f - Game.Instance.TimeController.GameDeltaTime + 
                        summonedUnitAppearBuff.Context.MaybeCaster?.CombatState.Cooldown.StandardAction ?? 0f).Seconds());
                    cooldown.Initiative = 0f;
                }
                else
                {
                    // the surprised units will be flat-footed in the surprise round
                    // if there is no surprise round or the unit is surprising, the unit won't be flat-footed in the 0th round
                    cooldown.Initiative += !IsSurpriseRound || IsSurprising(unit) ? 0f : 6f;
                }
            }
            else
            {
                // if a unit joins the combat in the middle of the combat, it has to wait for exact one round (6s) to act
                // summoned units has a buff forcing them to wait for 6s, so they don't need the action cooldown
                    cooldown.Initiative =
                        unit.Descriptor.HasFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff) ? 0f : 6f;
            }
        }

        public void HandleUnitLeaveCombat(UnitEntityData unit)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), unit);

            if (CombatInitialized)
            {
                 RemoveUnit(unit);
            }
        }

        public void HandleUnitDeath(UnitEntityData entityData)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), entityData);

            if (CombatInitialized)
            {
                RemoveUnit(entityData);
            }
        }

        public void HandleUnitDestroyed(UnitEntityData entityData)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), entityData);

            if (CombatInitialized)
            {
                RemoveUnit(entityData);
            }
        }

        // fix units stays in-combat state while their in-game state changes (caused Call Forth Kanerah/Kalikke glitch)
        public void HandleObjectInGameChaged(EntityDataBase entityData)
        {
            if (entityData is UnitEntityData unit)
            {
                Mod.Debug(MethodBase.GetCurrentMethod(), entityData);

                if (!unit.IsInGame && unit.IsInCombat)
                {
                    unit.LeaveCombat();
                }
            }
        }

        // ** fix touch spell (disallow touch more than once in the same round)
        public void HandleUnitCommandDidAct(UnitCommand command)
        {
            if (IsInCombat() && command.Executor.IsCurrentUnit() && (command.IsFreeTouch() || command.IsSpellstrikeAttack()))
            {
                UnitPartTouch unitPartTouch = command.Executor.Get<UnitPartTouch>();
                unitPartTouch.SetAppearTime(unitPartTouch.AppearTime - TimeSpan.FromSeconds(6d));
            }
        }

        // ** return the move action if current unit attacked only once during a full attack
        public void HandleUnitCommandDidEnd(UnitCommand command)
        {
            if (IsInCombat() && command.Executor.IsCurrentUnit() && 
                command.IsActed && !command.IsIgnoreCooldown &&
                command is UnitAttack unitAttack && unitAttack.IsFullAttack && unitAttack.GetAttackIndex() == 1)
            {
                CurrentTurn.Cooldown.MoveAction -= TIME_MOVE_ACTION;
            }
        }

        #endregion

        public class UnitsOrderComaprer : IComparer<UnitEntityData>
        {
            public int Compare(UnitEntityData x, UnitEntityData y)
            {
                if (x.IsCurrentUnit())
                    return -1;
                else if (y.IsCurrentUnit())
                    return 1;

                bool xCanAct = x.CanPerformAction();
                bool yCanAct = y.CanPerformAction();
                if (xCanAct ^ yCanAct)
                {
                    if (xCanAct)
                        return -1;
                    else
                        return 1;
                }

                //bool xIsSurprising = x.IsSurprising();
                //bool yIsSurprising = y.IsSurprising();
                //if (xIsSurprising ^ yIsSurprising)
                //{
                //    if (xIsSurprising)
                //        return -1;
                //    else
                //        return 1;
                //}

                float xTime = x.GetTimeToNextTurn();
                float yTime = y.GetTimeToNextTurn();

                if (xTime.Approximately(yTime))
                    return 0;
                else if (xTime < yTime)
                    return -1;
                else
                    return 1;

                //int result = x.GetTimeToNextTurn().CompareTo(y.GetTimeToNextTurn());
                //if (result == 0)
                //    return y.CombatState.Initiative.CompareTo(x.CombatState.Initiative);
                //else
                //return result;
            }
        }
    }
}
