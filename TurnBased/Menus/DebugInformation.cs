#if DEBUG
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using ModMaker;
using ModMaker.Utility;
using System.Collections.Generic;
using System.Linq;
using TurnBased.Controllers;
using TurnBased.Utility;
using UnityEngine;
using UnityModManagerNet;
using static ModMaker.Utility.RichTextExtensions;
using static TurnBased.Main;

namespace TurnBased.Menus
{
    public class DebugInformation : IMenuSelectablePage
    {
        public string Name => "Debug";

        public int Priority => 900;

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (Mod == null || !Mod.Enabled)
                return;

            if (GUILayout.Button("Clear HUD", GUILayout.ExpandWidth(false)))
            {
                Mod.Core.UI.Clear();
            }

            GUILayout.Space(10f);

            OnGUIDebug();

            GUILayout.Space(10f);

            GUILayout.Label($"Ability Execution Process: {Mod.Core?.LastTickTimeOfAbilityExecutionProcess.Count}");
        }

        private void OnGUIDebug()
        {
            GUILayout.Label($"Time Scale: {Time.timeScale:f2}x");
            GUILayout.Label($"Game Time: {Game.Instance.Player.GameTime}");

            CombatController roundController = Mod.Core.Combat;
            if (roundController != null)
            {
                TurnController currentTurn = roundController.CurrentTurn;

                GUILayout.Space(10f);
                GUILayout.Label($"Combat Initialized: {roundController.Initialized}");

                GUILayout.Space(10f);
                if (GUILayout.Button("Reset Current Turn", GUILayout.ExpandWidth(false)) && currentTurn != null)
                {
                    roundController.StartTurn(currentTurn.Unit);
                }

                GUILayout.Space(10f);
                GUILayout.Label($"Turn Status: {currentTurn?.Status}");
                GUILayout.Label($"Time Waited For AI: {currentTurn?.TimeWaitedForIdleAI:f4}");
                GUILayout.Label($"Time Waited To End Turn: {currentTurn?.TimeWaitedToEndTurn:f4}");
                GUILayout.Label($"Time Moved: {currentTurn?.TimeMoved:f4}");
                GUILayout.Label($"Time Moved (5-Foot Step): {currentTurn?.TimeMovedByFiveFootStep:f4}");
                GUILayout.Label($"Feet Moved (5-Foot Step): {currentTurn?.MetersMovedByFiveFootStep / Feet.FeetToMetersRatio:f4}");
                GUILayout.Label($"Has Normal Movement: {currentTurn?.HasNormalMovement()}");
                GUILayout.Label($"Has 5-Foot Step: {currentTurn?.HasFiveFootStep()}");
                GUILayout.Label($"Has Free Touch: {currentTurn?.Unit.HasFreeTouch()}");
                GUILayout.Label($"Prepared Spell Combat: {currentTurn?.Unit.PreparedSpellCombat()}");
                GUILayout.Label($"Prepared Spell Strike: {currentTurn?.Unit.PreparedSpellStrike()}");

                GUILayout.Space(10f);
                GUILayout.Label("Current Unit:");
                GUILayout.Label(currentTurn?.Unit.ToString().Color(RGBA.yellow));
                GUILayout.Label($"Free Action: {currentTurn?.Commands.Raw[0]}");
                GUILayout.Label($"Standard Action: {currentTurn?.Commands.Standard}" +
                    $" (IsFullRoundAction: {currentTurn?.Commands.Raw[1].IsFullRoundAction()}" +
                    $", IsFreeTouch: {currentTurn?.Commands.Raw[1].IsFreeTouch()}" +
                    $", IsSpellCombat: {currentTurn?.Commands.Raw[1].IsSpellCombatAttack()}" +
                    $", IsSpellStrike: {currentTurn?.Commands.Raw[1].IsSpellstrikeAttack()})");
                GUILayout.Label($"Move Action: {currentTurn?.Commands.Raw[3]}");
                GUILayout.Label($"Swift Action: {currentTurn?.Commands.Raw[2]}");

                IEnumerable<UnitEntityData> units = roundController.SortedUnits;

                if (units.Any())
                {
                    GUILayout.Space(10f);
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Unit ID");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label($"{unit}");
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Name");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label($"{unit.CharacterName}");
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Init");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label(unit.CombatState.Initiative.ToString());
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Init_C");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label(HightlightedCooldownText(unit.CombatState.Cooldown.Initiative));
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Std_C");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label(HightlightedCooldownText(unit.CombatState.Cooldown.StandardAction));
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Move_C");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label(HightlightedCooldownText(unit.CombatState.Cooldown.MoveAction));
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("Swift_C");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label(HightlightedCooldownText(unit.CombatState.Cooldown.SwiftAction));
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("AoO_C");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label(HightlightedCooldownText(unit.CombatState.Cooldown.AttackOfOpportunity));
                        }

                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Label("AoO");
                            foreach (UnitEntityData unit in units)
                                GUILayout.Label(unit.CombatState.AttackOfOpportunityCount.ToString());
                        }

                        GUILayout.FlexibleSpace();
                    }
                }
            }
        }

        private string HightlightedCooldownText(float cooldown)
        {
            if (cooldown > 0)
                return $"{cooldown:F4}".Color(RGBA.yellow);
            else
                return $"{cooldown:F4}";
        }
    }
}
#endif