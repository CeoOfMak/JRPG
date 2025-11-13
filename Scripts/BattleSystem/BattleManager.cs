using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using JRPGGame.Core;
using JRPGGame.Party;
using JRPGGame.Inventory;

namespace JRPGGame.BattleSystem
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Battle Setup")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private bool isBossBattle;

        [Header("Battle State")]
        [SerializeField] private BattleState currentState;
        [SerializeField] private List<BattleCombatant> playerCombatants = new List<BattleCombatant>();
        [SerializeField] private List<BattleCombatant> enemyCombatants = new List<BattleCombatant>();

        [Header("Systems")]
        private TurnOrderSystem turnOrderSystem;
        private BattleRewards battleRewards;

        [Header("Events")]
        public UnityEvent OnBattleStart;
        public UnityEvent OnBattleVictory;
        public UnityEvent OnBattleDefeat;
        public UnityEvent<BattleCombatant> OnTurnStart;
        public UnityEvent<BattleAction> OnActionExecuted;

        // Properties
        public BattleState CurrentState => currentState;
        public List<BattleCombatant> PlayerCombatants => playerCombatants;
        public List<BattleCombatant> EnemyCombatants => enemyCombatants;
        public TurnOrderSystem TurnOrder => turnOrderSystem;
        public bool IsBossBattle => isBossBattle;

        private void Awake()
        {
            turnOrderSystem = new TurnOrderSystem();
        }

        public void StartBattle(List<BattleCombatant> players, List<BattleCombatant> enemies, BattleRewards rewards = null)
        {
            playerCombatants = players;
            enemyCombatants = enemies;
            battleRewards = rewards;

            InitializeBattle();
        }

        private void InitializeBattle()
        {
            currentState = BattleState.Start;

            // Initialize all combatants
            foreach (var combatant in playerCombatants)
            {
                combatant.InitializeCombatant();
                SetupCombatantEvents(combatant);
            }

            foreach (var combatant in enemyCombatants)
            {
                combatant.InitializeCombatant();
                SetupCombatantEvents(combatant);
            }

            // Check if any enemy is a boss
            isBossBattle = enemyCombatants.Any(e => e is BossCombatant);

            // Initialize turn order
            var allCombatants = new List<BattleCombatant>();
            allCombatants.AddRange(playerCombatants);
            allCombatants.AddRange(enemyCombatants);
            turnOrderSystem.InitializeTurnOrder(allCombatants);

            OnBattleStart?.Invoke();

            // Start first turn
            StartCoroutine(BattleFlow());
        }

        private void SetupCombatantEvents(BattleCombatant combatant)
        {
            combatant.OnDeath += () => OnCombatantDeath(combatant);
        }

        private IEnumerator BattleFlow()
        {
            while (currentState != BattleState.Victory && currentState != BattleState.Defeat)
            {
                currentState = BattleState.TurnStart;
                var currentCombatant = turnOrderSystem.CurrentCombatant;

                if (currentCombatant != null && currentCombatant.CanAct())
                {
                    OnTurnStart?.Invoke(currentCombatant);

                    yield return StartCoroutine(ExecuteTurn(currentCombatant));

                    currentCombatant.ProcessTurnEnd();
                }

                // Check win/lose conditions
                if (CheckVictoryCondition())
                {
                    currentState = BattleState.Victory;
                    yield return StartCoroutine(HandleVictory());
                    break;
                }

                if (CheckDefeatCondition())
                {
                    currentState = BattleState.Defeat;
                    yield return StartCoroutine(HandleDefeat());
                    break;
                }

                currentState = BattleState.TurnEnd;
                turnOrderSystem.NextTurn();

                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator ExecuteTurn(BattleCombatant combatant)
        {
            currentState = BattleState.SelectingAction;

            BattleAction action = null;

            if (combatant.isPlayer)
            {
                // Wait for player input (handled by UI)
                yield return StartCoroutine(WaitForPlayerAction(combatant, result => action = result));
            }
            else
            {
                // AI decision
                var enemyAI = GetEnemyAI(combatant);
                if (enemyAI != null)
                {
                    action = enemyAI.DecideAction(combatant, playerCombatants, enemyCombatants);
                }
                yield return new WaitForSeconds(0.5f);
            }

            if (action != null)
            {
                currentState = BattleState.ExecutingAction;
                yield return StartCoroutine(ExecuteAction(action));
                OnActionExecuted?.Invoke(action);
            }
        }

        private IEnumerator WaitForPlayerAction(BattleCombatant combatant, Action<BattleAction> callback)
        {
            // This would be connected to your UI system
            // For now, placeholder
            BattleAction action = new BattleAction();
            action.actor = combatant;
            action.actionType = BattleActionType.Defend;

            yield return new WaitForSeconds(1f);

            callback?.Invoke(action);
        }

        private IEnumerator ExecuteAction(BattleAction action)
        {
            switch (action.actionType)
            {
                case BattleActionType.Skill:
                    yield return StartCoroutine(ExecuteSkill(action));
                    break;

                case BattleActionType.Item:
                    yield return StartCoroutine(ExecuteItem(action));
                    break;

                case BattleActionType.Defend:
                    action.actor.Defend();
                    Debug.Log($"{action.actor.characterName} is defending!");
                    break;

                case BattleActionType.Flee:
                    // Flee logic (only for non-boss battles)
                    if (!isBossBattle)
                    {
                        if (Random.value < 0.5f)
                        {
                            Debug.Log("Successfully fled from battle!");
                            EndBattle(false, false);
                        }
                        else
                        {
                            Debug.Log("Failed to flee!");
                        }
                    }
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator ExecuteSkill(BattleAction action)
        {
            var skill = action.selectedSkill;
            var actor = action.actor;

            if (!skill.CanUse(actor.stats))
            {
                Debug.Log($"{actor.characterName} doesn't have enough resources to use {skill.skillName}!");
                yield break;
            }

            actor.UseSkill(skill);

            Debug.Log($"{actor.characterName} uses {skill.skillName}!");

            // Play animation
            if (!string.IsNullOrEmpty(skill.animationTrigger) && actor.GetComponent<Animator>() != null)
            {
                actor.GetComponent<Animator>().SetTrigger(skill.animationTrigger);
            }

            yield return new WaitForSeconds(0.5f);

            // Execute skill on targets
            foreach (var target in action.targets)
            {
                if (!target.IsAlive) continue;

                switch (skill.skillType)
                {
                    case SkillType.Attack:
                        var damageResult = DamageCalculator.CalculateDamage(actor, target, skill);

                        if (damageResult.missed)
                        {
                            Debug.Log($"{skill.skillName} missed!");
                        }
                        else
                        {
                            // Apply shield damage first
                            if (damageResult.shieldDamage > 0 && target.stats.CurrentShieldBar > 0)
                            {
                                target.TakeDamage(damageResult.shieldDamage, skill.elementType, true);
                            }

                            // Apply main damage
                            int totalDamage = target.TakeDamage(damageResult.totalDamage, skill.elementType, damageResult.targetsShieldBar);

                            string critText = damageResult.isCritical ? "Critical Hit! " : "";
                            Debug.Log($"{critText}{target.characterName} took {totalDamage} damage!");

                            // Show element effectiveness
                            if (skill.elementType != ElementType.None && damageResult.targetsShieldBar)
                            {
                                float effectiveness = ElementEffectiveness.GetEffectiveness(skill.elementType, target.ElementalAffinity);
                                Debug.Log(ElementEffectiveness.GetEffectivenessText(effectiveness));
                            }
                        }
                        break;

                    case SkillType.Heal:
                        int healAmount = DamageCalculator.CalculateHealing(actor, skill);
                        target.Heal(healAmount, skill.healsShieldBar);
                        Debug.Log($"{target.characterName} recovered {healAmount} {(skill.healsShieldBar ? "Shield" : "HP")}!");
                        break;

                    case SkillType.Buff:
                    case SkillType.Debuff:
                        foreach (var statusEffect in skill.statusEffects)
                        {
                            target.ApplyStatusEffect(statusEffect);
                            Debug.Log($"{target.characterName} is affected by {statusEffect.effectType}!");
                        }
                        break;

                    case SkillType.Special:
                        // Custom special effects
                        break;
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        private IEnumerator ExecuteItem(BattleAction action)
        {
            // Item usage would be implemented here
            Debug.Log($"{action.actor.characterName} uses an item!");
            yield return new WaitForSeconds(0.5f);
        }

        private void OnCombatantDeath(BattleCombatant combatant)
        {
            Debug.Log($"{combatant.characterName} has been defeated!");
        }

        private bool CheckVictoryCondition()
        {
            return enemyCombatants.All(e => !e.IsAlive);
        }

        private bool CheckDefeatCondition()
        {
            return playerCombatants.All(p => !p.IsAlive);
        }

        private IEnumerator HandleVictory()
        {
            Debug.Log("Victory!");
            OnBattleVictory?.Invoke();

            if (battleRewards != null)
            {
                DistributeRewards();
            }

            yield return new WaitForSeconds(2f);

            EndBattle(true, false);
        }

        private IEnumerator HandleDefeat()
        {
            Debug.Log("Defeat...");
            OnBattleDefeat?.Invoke();

            yield return new WaitForSeconds(2f);

            EndBattle(false, true);
        }

        private void DistributeRewards()
        {
            if (battleRewards == null) return;

            // Distribute experience
            foreach (var player in playerCombatants)
            {
                if (player.IsAlive)
                {
                    player.stats.AddExperience(battleRewards.experiencePoints);
                    Debug.Log($"{player.characterName} gained {battleRewards.experiencePoints} EXP!");
                }
            }

            // Add gold
            if (PartyManager.Instance != null)
            {
                PartyManager.Instance.AddGold(battleRewards.goldReward);
                Debug.Log($"Received {battleRewards.goldReward} gold!");
            }

            // Add items
            if (InventoryManager.Instance != null)
            {
                foreach (var itemDrop in battleRewards.itemDrops)
                {
                    if (Random.value <= itemDrop.dropChance)
                    {
                        InventoryManager.Instance.AddItem(itemDrop.item, itemDrop.quantity);
                        Debug.Log($"Received {itemDrop.item.itemName} x{itemDrop.quantity}!");
                    }
                }
            }
        }

        private void EndBattle(bool victory, bool defeat)
        {
            // Reset HP after battle (as per requirements)
            foreach (var player in playerCombatants)
            {
                player.stats.ResetAfterBattle();
            }

            // Transition back to overworld or show results screen
            // This would be handled by your scene management
        }

        private EnemyAI GetEnemyAI(BattleCombatant enemy)
        {
            // This would typically be assigned per enemy
            // For now, return a default AI behavior
            return null;
        }

        // Public methods for UI/external control
        public void PlayerSelectSkill(BattleCombatant actor, Skill skill, List<BattleCombatant> targets)
        {
            // This would be called by your battle UI
        }

        public void PlayerSelectItem(BattleCombatant actor, Item item, List<BattleCombatant> targets)
        {
            // This would be called by your battle UI
        }

        public void PlayerDefend(BattleCombatant actor)
        {
            // This would be called by your battle UI
        }

        public void PlayerFlee()
        {
            // This would be called by your battle UI
        }
    }

    [Serializable]
    public class BattleRewards
    {
        public int experiencePoints;
        public int goldReward;
        public List<ItemDrop> itemDrops = new List<ItemDrop>();
    }

    [Serializable]
    public class ItemDrop
    {
        public Item item;
        public int quantity = 1;
        [Range(0f, 1f)]
        public float dropChance = 0.5f;
    }
}
