using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
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

namespace TurnBased.Controllers
{
    public class CombatController : 
        IModEventHandler,
        IInGameHandler,
        IPartyCombatHandler,
        ISceneHandler,
        IUnitCombatHandler,
        IUnitHandler,
        IUnitInitiativeHandler
    {
        #region Fields

        private TurnController _currentTurn;
        private bool _hasSurpriseRound;
        private TimeSpan _startTime;
        private float _timeSinceStart;
        private readonly TimeScaleRegulator _timeScale = new TimeScaleRegulator();
        private List<UnitEntityData> _units = new List<UnitEntityData>();
        HashSet<UnitEntityData> _unitsToSurprise = new HashSet<UnitEntityData>();
        private readonly UnitsOrderComaprer _unitsOrderComaprer = new UnitsOrderComaprer();
        private bool _unitsSorted;

        #endregion

        #region Properties

        public TurnController CurrentTurn {
            get => _currentTurn;
            private set {
                if (_currentTurn != value)
                {
                    _currentTurn?.Dispose();
                    _currentTurn = value;
                }
            }
        }

        public bool Initialized { get; private set; }

        internal HashSet<RayView> TickedRayView { get; } = new HashSet<RayView>();

        #endregion

        #region Tick

        internal void Tick()
        {
            // fix if combat is ended by a cutscene, HandlePartyCombatStateChanged will not be triggered
            if (_units.Count == 0 || !Game.Instance.Player.IsInCombat)
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

            // try to start a turn for the next unit
            if (CurrentTurn == null)
            {
                UnitEntityData nextUnit = GetSortedUnits(true).First();
                if (nextUnit.GetTimeToNextTurn() <= 0f)
                {
                    StartTurn(nextUnit);
                }
            }

            // reset parameters of the pervious tick
            TickedRayView.Clear();
        }

        internal void TickTime()
        {
            if (CurrentTurn == null)
            {
                // modify time scale
                _timeScale.Modify(TimeScaleBetweenTurns);

                // trim the delta time, when a turn will start at the end of this tick
                TimeController timeController = Game.Instance.TimeController;
                float timeToNextTurn = GetSortedUnits().First().GetTimeToNextTurn();
                if (timeController.GameDeltaTime > timeToNextTurn && timeToNextTurn != 0f)
                {
                    timeController.SetDeltaTime(timeToNextTurn);
                    timeController.SetGameDeltaTime(timeToNextTurn);
                }

                // advance time
                _timeSinceStart += Game.Instance.TimeController.GameDeltaTime;
            }
            else
            {
                // modify time scale
                _timeScale.Modify(CurrentTurn.Unit.IsDirectlyControllable ? TimeScaleInPlayerTurn : TimeScaleInNonPlayerTurn);
            }

            // set game time
            Game.Instance.Player.GameTime = _startTime + _timeSinceStart.Seconds();
        }

        #endregion

        #region Methods

        public IEnumerable<UnitEntityData> GetSortedUnits(bool forceSort = false)
        {
            if (!_unitsSorted || forceSort)
            {
                _units = _units.OrderBy(unit => unit, _unitsOrderComaprer).ToList();    // stable sort
                _unitsSorted = true;
            }
            return _units;
        }

        public bool IsSurprising(UnitEntityData unit)
        {
            return _hasSurpriseRound && _timeSinceStart < 6f && 
                (unit == CurrentTurn?.Unit ? true : _timeSinceStart + unit.GetTimeToNextTurn() < 6f);
        }

        public void StartTurn(UnitEntityData unit)
        {
            if (unit.IsInCombat && _units.Contains(unit))
            {
                CurrentTurn = new TurnController(unit);
                CurrentTurn.OnDelay += HandleDelayTurn;
                CurrentTurn.OnEnd += HandleEndTurn;
                _unitsSorted = false;
            }
        }

        public void Reset(bool tryToInitialize, bool isPartyCombatStateChanged = false)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), tryToInitialize, isPartyCombatStateChanged);

            // try to initialize
            if (tryToInitialize && Mod.Core.Enabled && 
                Game.Instance.Player.IsInCombat && Game.Instance.Player.Group.HasEnemyInCombat())
                HandleCombatStart(isPartyCombatStateChanged);
            else if(Initialized)
                HandleCombatEnd();
        }

        private void Clear()
        {
            // reset fields and properties
            _hasSurpriseRound = false;
            _startTime = Game.Instance.Player.GameTime;
            _timeSinceStart = 0f;
            _timeScale.Reset();
            _units.Clear();
            _unitsToSurprise.Clear();
            _unitsSorted = false;
            CurrentTurn = null;
            Initialized = false;
            TickedRayView.Clear();
        }

        private void HandleCombatStart(bool isPartyCombatStateChanged)
        {
            Clear();

            _units.AddRange(Game.Instance.State.Units.Where(unit => unit.IsInCombat));

            // surprise round
            if (isPartyCombatStateChanged && SurpriseRound)
            {
                HashSet<UnitEntityData> playerUnits = new HashSet<UnitEntityData>(Game.Instance.Player.ControllableCharacters);
                int notAppearUnitsCount = 0;
                bool isInitiatedByPlayer = _units.Any(unit => playerUnits.Contains(unit) && unit.HasOffensiveCommand());

                // try to join units to the surprise round
                foreach (UnitEntityData unit in _units)
                {
                    if (unit.Descriptor.HasFact(BlueprintRoot.Instance.SystemMechanics.SummonedUnitAppearBuff))
                        // this unit is just summoned by a full round spell and technically it does not exist on combat start
                        notAppearUnitsCount++;
                    else if (unit.IsSummoned(out UnitEntityData caster) && _unitsToSurprise.Contains(caster))
                        // this summoned unit will act after its caster's turn
                        _unitsToSurprise.Add(unit);
                    else if (
                        // player
                        playerUnits.Contains(unit) ?
                        isInitiatedByPlayer && unit.IsUnseen() :
                        // enemy
                        unit.Group.IsEnemy(Game.Instance.Player.Group) ?
                        unit.HasOffensiveCommand(command => playerUnits.Contains(command.TargetUnit)) ||
                        (unit.IsUnseen() && !unit.IsVisibleForPlayer) :
                        // neutral
                        unit.IsUnseen())
                        // this unit will act on its initiative
                        _unitsToSurprise.Add(unit);
                }

                // determine whether the surprise round occurs 
                if (_unitsToSurprise.Count > 0)
                {
                    if (_unitsToSurprise.Count < _units.Count - notAppearUnitsCount)
                        _hasSurpriseRound = true;
                    else
                        _unitsToSurprise.Clear();
                }
            }

            Initialized = true;
        }

        private void HandleCombatEnd()
        {
            Clear();

            // QoLs - on turn-based combat end
            if (AutoTurnOnAIOnCombatEnd)
                foreach (UnitEntityData unit in Game.Instance.Player.ControllableCharacters)
                    unit.IsAIEnabled = true;

            if (AutoSelectEntirePartyOnCombatEnd)
                Game.Instance.UI.SelectionManager?.SelectAll();

            if (AutoCancelActionsOnCombatEnd)
                foreach (UnitEntityData unit in Game.Instance.Player.ControllableCharacters)
                    unit.TryCancelCommands();
        }

        private void AddUnit(UnitEntityData unit)
        {
            if (unit.IsInCombat && !_units.Contains(unit))
            {
                _units.Add(unit);
                _unitsSorted = false;
            }
        }

        private void InsertUnit(UnitEntityData unit, UnitEntityData targetUnit)
        {
            if (unit.IsInCombat && !_units.Contains(unit))
            {
                _units.Insert(_units.IndexOf(targetUnit) + 1, unit);
                _unitsSorted = false;
            }
        }

        private void RemoveUnit(UnitEntityData unit)
        {
            if (_units.Remove(unit))
            {
                if (CurrentTurn?.Unit == unit)
                {
                    CurrentTurn = null;
                }
                _unitsSorted = false;
            }
        }

        #endregion

        #region Event Handlers

        private void HandleDelayTurn(UnitEntityData unit, UnitEntityData targetUnit)
        {
            if (unit != targetUnit)
            {
                RemoveUnit(unit);
                InsertUnit(unit, targetUnit);
            }
        }

        private void HandleEndTurn(UnitEntityData unit)
        {
            RemoveUnit(unit);
            AddUnit(unit);
        }

        public void HandleModEnable()
        {
            Mod.Debug(MethodBase.GetCurrentMethod());

            Mod.Core.Combat = this;
            Reset(true);

            EventBus.Subscribe(this);
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

        public void HandleUnitRollsInitiative(RuleInitiativeRoll rule)
        {
            UnitEntityData unit = rule.Initiator;
            UnitCombatState.Cooldowns cooldown = unit.CombatState.Cooldown;
            if (_timeSinceStart == 0f)
            {
                // it's the beginning of combat
                if (unit.IsSummoned(out UnitEntityData caster) && _units.Contains(caster))
                {
                    // this unit is summoned before the combat, it will act right after its caster
                    cooldown.Initiative = caster.CombatState.Cooldown.Initiative;
                }
                else if (_hasSurpriseRound && !_unitsToSurprise.Contains(unit))
                {
                    // this unit is surprised, it will be flat-footed for one more round
                    cooldown.Initiative += 6f;
                }

                _unitsToSurprise.Remove(unit);
            }
            else
            {
                // it's the middle of combat
                if (unit.IsSummoned(out UnitEntityData caster) && _units.Contains(caster))
                {
                    // summoned units can act instantly, it's delay is controlled by its buff
                    cooldown.Initiative = 0f;

                    // ensure its order is right after its caster
                    RemoveUnit(unit);
                    InsertUnit(unit, caster);
                }
                else
                {
                    if (_hasSurpriseRound && _timeSinceStart < 6f)
                    {
                        // units that join during surprise round will be regard as surprised
                        cooldown.Initiative = 6f;
                    }
                    else
                    {
                        // units that join during regular round have to wait for one round
                        cooldown.Initiative = 0f;
                        cooldown.StandardAction = 6f;
                    }
                }
            }
        }

        public void HandleUnitJoinCombat(UnitEntityData unit)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), unit);

            if (Initialized)
            {
                AddUnit(unit);
            }
        }

        public void HandleUnitSpawned(UnitEntityData entityData)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), entityData);

            if (Initialized)
            {
                AddUnit(entityData);
            }
        }

        public void HandleUnitLeaveCombat(UnitEntityData unit)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), unit);

            if (Initialized)
            {
                 RemoveUnit(unit);
            }
        }

        public void HandleUnitDeath(UnitEntityData entityData)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), entityData);

            if (Initialized)
            {
                RemoveUnit(entityData);
            }
        }

        public void HandleUnitDestroyed(UnitEntityData entityData)
        {
            Mod.Debug(MethodBase.GetCurrentMethod(), entityData);

            if (Initialized)
            {
                RemoveUnit(entityData);
            }
        }

        // fix units will never leave combat if they become inactive (cause Call Forth Kanerah / Kalikke glitch)
        public void HandleObjectInGameChaged(EntityDataBase entityData)
        {
            if (entityData is UnitEntityData unit)
            {
                Mod.Debug(MethodBase.GetCurrentMethod(), unit);

                if (!unit.IsInGame && unit.IsInCombat)
                {
                    unit.LeaveCombat();
                }
            }
        }

        #endregion

        public class TimeScaleRegulator
        {
            private float _appliedModifier;
            private float _previousModifier; 

            public void Modify(float modifier)
            {
                if (modifier <= 1f)
                {
                    Reset();
                    return;
                }

                if (Time.deltaTime > 0f)
                {
                    float fps = 1f / Time.unscaledDeltaTime;
                    if (modifier == _previousModifier && (fps < MinimumFPS || _appliedModifier < _previousModifier))
                    {
                        // if fps < MinimumFPS, make fps closer to MinimumFPS
                        // if fps >= MinimumFPS, make _appliedModifier closer to modifier
                        _appliedModifier = Math.Min(modifier, _appliedModifier * fps / MinimumFPS);
                    }
                    else
                    {
                        _appliedModifier = modifier;
                    }
                    _previousModifier = modifier;
                }

                Time.timeScale *= _appliedModifier = Math.Max(1f, _appliedModifier);
            }

            public void Reset()
            {
                _appliedModifier = 1f;
                _previousModifier = 1f;
            }
        }

        public class UnitsOrderComaprer : IComparer<UnitEntityData>
        {
            public int Compare(UnitEntityData x, UnitEntityData y)
            {
                if (x.IsCurrentUnit())
                    return -1;
                else if (y.IsCurrentUnit())
                    return 1;

                float xTime = x.GetTimeToNextTurn();
                float yTime = y.GetTimeToNextTurn();

                if (xTime.Approximately(yTime, 0.0001f))
                    return 0;
                else if (xTime < yTime)
                    return -1;
                else
                    return 1;
            }
        }
    }
}