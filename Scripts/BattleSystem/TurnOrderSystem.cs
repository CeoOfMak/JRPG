using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.BattleSystem
{
    public class TurnOrderSystem
    {
        private List<BattleCombatant> turnOrder;
        private int currentTurnIndex;

        public BattleCombatant CurrentCombatant =>
            turnOrder != null && currentTurnIndex < turnOrder.Count ? turnOrder[currentTurnIndex] : null;

        public List<BattleCombatant> TurnOrder => turnOrder;
        public int CurrentTurnIndex => currentTurnIndex;

        public void InitializeTurnOrder(List<BattleCombatant> allCombatants)
        {
            turnOrder = new List<BattleCombatant>(allCombatants);
            CalculateTurnOrder();
            currentTurnIndex = 0;
        }

        private void CalculateTurnOrder()
        {
            // Sort by speed (with speed modifiers applied)
            turnOrder = turnOrder.OrderByDescending(c =>
            {
                if (!c.IsAlive) return -1; // Dead combatants go last

                float speed = c.stats.Speed * c.GetStatModifier(StatType.Speed);
                // Add small random variance to prevent ties
                speed += Random.Range(0f, 0.1f);
                return speed;
            }).ToList();
        }

        public void NextTurn()
        {
            currentTurnIndex++;

            // If we've gone through all combatants, recalculate turn order for new round
            if (currentTurnIndex >= turnOrder.Count)
            {
                currentTurnIndex = 0;
                CalculateTurnOrder();
                ProcessNewRound();
            }

            // Skip dead or incapacitated combatants
            while (CurrentCombatant != null && !CurrentCombatant.CanAct())
            {
                CurrentCombatant.ProcessTurnStart();
                CurrentCombatant.ProcessTurnEnd();
                currentTurnIndex++;

                if (currentTurnIndex >= turnOrder.Count)
                {
                    currentTurnIndex = 0;
                    CalculateTurnOrder();
                    ProcessNewRound();
                }
            }

            if (CurrentCombatant != null)
            {
                CurrentCombatant.ProcessTurnStart();
            }
        }

        private void ProcessNewRound()
        {
            Debug.Log("New combat round started");
        }

        public void RemoveCombatant(BattleCombatant combatant)
        {
            int index = turnOrder.IndexOf(combatant);
            if (index >= 0)
            {
                turnOrder.RemoveAt(index);
                if (index < currentTurnIndex)
                {
                    currentTurnIndex--;
                }
            }
        }

        public void RecalculateTurnOrder()
        {
            CalculateTurnOrder();
        }

        public int GetTurnPosition(BattleCombatant combatant)
        {
            return turnOrder.IndexOf(combatant);
        }

        public List<BattleCombatant> GetUpcomingTurns(int count)
        {
            List<BattleCombatant> upcoming = new List<BattleCombatant>();
            int index = currentTurnIndex + 1;

            for (int i = 0; i < count && upcoming.Count < count; i++)
            {
                if (index >= turnOrder.Count)
                    index = 0;

                if (turnOrder[index].CanAct())
                {
                    upcoming.Add(turnOrder[index]);
                }

                index++;
            }

            return upcoming;
        }
    }
}
