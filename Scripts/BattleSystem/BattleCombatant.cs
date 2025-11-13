using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.BattleSystem
{
    public class BattleCombatant : MonoBehaviour
    {
        [Header("Combatant Info")]
        public string characterName;
        public bool isPlayer;
        public Sprite characterSprite;
        public GameObject characterModel;

        [Header("Stats")]
        public CharacterStats stats;

        [Header("Combat State")]
        [SerializeField] private CombatantState combatState = CombatantState.Normal;
        [SerializeField] private int breakTurnsRemaining;
        [SerializeField] private ElementType elementalAffinity = ElementType.None;

        [Header("Skills")]
        public List<Skill> availableSkills = new List<Skill>();

        [Header("Active Effects")]
        private List<ActiveStatusEffect> activeStatusEffects = new List<ActiveStatusEffect>();

        // Events
        public event Action<int> OnDamageTaken;
        public event Action<int> OnHealed;
        public event Action OnBreakStateEntered;
        public event Action OnBreakStateExited;
        public event Action OnDeath;
        public event Action<Skill> OnSkillUsed;

        // Properties
        public CombatantState CombatState => combatState;
        public bool IsInBreakState => combatState == CombatantState.Break;
        public bool IsAlive => stats.IsAlive && combatState != CombatantState.Dead;
        public ElementType ElementalAffinity => elementalAffinity;

        private void Awake()
        {
            if (stats == null)
            {
                stats = new CharacterStats();
            }
            stats.Initialize();
        }

        public void InitializeCombatant()
        {
            combatState = CombatantState.Normal;
            breakTurnsRemaining = 0;
            activeStatusEffects.Clear();
        }

        public int TakeDamage(int damage, ElementType element, bool targetsShieldBar)
        {
            int actualDamage = damage;

            // Apply element effectiveness only to shield bar
            if (targetsShieldBar && stats.CurrentShieldBar > 0)
            {
                float effectiveness = ElementEffectiveness.GetEffectiveness(element, elementalAffinity);
                actualDamage = Mathf.RoundToInt(damage * effectiveness);

                int shieldDamage = Mathf.Min(actualDamage, stats.CurrentShieldBar);
                stats.CurrentShieldBar -= shieldDamage;

                if (stats.CurrentShieldBar <= 0)
                {
                    EnterBreakState();
                    // Remaining damage after breaking shield
                    int remainingDamage = actualDamage - shieldDamage;
                    if (remainingDamage > 0)
                    {
                        ApplyHPDamage(remainingDamage);
                    }
                }

                OnDamageTaken?.Invoke(shieldDamage);
                return shieldDamage;
            }
            else
            {
                // HP damage - no element effectiveness, but 10% bonus damage
                if (IsInBreakState)
                {
                    actualDamage = Mathf.RoundToInt(damage * GameConstants.BREAK_STATE_DAMAGE_MULTIPLIER);
                }

                ApplyHPDamage(actualDamage);
                OnDamageTaken?.Invoke(actualDamage);
                return actualDamage;
            }
        }

        private void ApplyHPDamage(int damage)
        {
            stats.CurrentHP -= damage;
            if (stats.CurrentHP <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount, bool healShieldBar)
        {
            if (healShieldBar)
            {
                int oldShieldBar = stats.CurrentShieldBar;
                stats.CurrentShieldBar += amount;
                int actualHeal = stats.CurrentShieldBar - oldShieldBar;
                OnHealed?.Invoke(actualHeal);

                // Exit break state if shield bar is restored
                if (IsInBreakState && stats.CurrentShieldBar > 0)
                {
                    ExitBreakState();
                }
            }
            else
            {
                int oldHP = stats.CurrentHP;
                stats.CurrentHP += amount;
                int actualHeal = stats.CurrentHP - oldHP;
                OnHealed?.Invoke(actualHeal);
            }
        }

        private void EnterBreakState()
        {
            combatState = CombatantState.Break;
            breakTurnsRemaining = GameConstants.BREAK_STATE_DURATION_TURNS;
            OnBreakStateEntered?.Invoke();
        }

        private void ExitBreakState()
        {
            if (combatState == CombatantState.Break)
            {
                combatState = CombatantState.Normal;
                breakTurnsRemaining = 0;
                OnBreakStateExited?.Invoke();
            }
        }

        private void Die()
        {
            combatState = CombatantState.Dead;
            stats.CurrentHP = 0;
            OnDeath?.Invoke();
        }

        public void ProcessTurnStart()
        {
            // Process status effects
            for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeStatusEffects[i];
                effect.duration--;

                // Apply DoT/HoT effects
                if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.Poison))
                {
                    ApplyHPDamage(effect.statusEffect.power);
                }
                else if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.Regen))
                {
                    Heal(effect.statusEffect.power, false);
                }

                if (effect.duration <= 0)
                {
                    activeStatusEffects.RemoveAt(i);
                }
            }

            // Handle break state duration
            if (IsInBreakState)
            {
                breakTurnsRemaining--;
                if (breakTurnsRemaining <= 0)
                {
                    ExitBreakState();
                }
            }
        }

        public void ProcessTurnEnd()
        {
            // Reset defending state
            if (combatState == CombatantState.Defending)
            {
                combatState = CombatantState.Normal;
            }
        }

        public void ApplyStatusEffect(StatusEffect statusEffect)
        {
            activeStatusEffects.Add(new ActiveStatusEffect
            {
                statusEffect = statusEffect,
                duration = statusEffect.duration
            });
        }

        public void Defend()
        {
            combatState = CombatantState.Defending;
        }

        public bool HasStatusEffect(BuffDebuffType effectType)
        {
            return activeStatusEffects.Exists(e => e.statusEffect.effectType.HasFlag(effectType));
        }

        public float GetStatModifier(StatType statType)
        {
            float modifier = 1f;

            foreach (var effect in activeStatusEffects)
            {
                switch (statType)
                {
                    case StatType.Attack:
                        if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.AttackUp))
                            modifier += effect.statusEffect.statModifier;
                        if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.AttackDown))
                            modifier -= effect.statusEffect.statModifier;
                        break;
                    case StatType.Defense:
                        if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.DefenseUp))
                            modifier += effect.statusEffect.statModifier;
                        if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.DefenseDown))
                            modifier -= effect.statusEffect.statModifier;
                        break;
                    case StatType.Speed:
                        if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.SpeedUp))
                            modifier += effect.statusEffect.statModifier;
                        if (effect.statusEffect.effectType.HasFlag(BuffDebuffType.SpeedDown))
                            modifier -= effect.statusEffect.statModifier;
                        break;
                }
            }

            // Defending gives defense bonus
            if (combatState == CombatantState.Defending && statType == StatType.Defense)
            {
                modifier += 0.5f;
            }

            return modifier;
        }

        public bool CanAct()
        {
            if (!IsAlive) return false;
            if (IsInBreakState) return false;
            if (HasStatusEffect(BuffDebuffType.Stun)) return false;
            return true;
        }

        public void UseSkill(Skill skill)
        {
            if (skill.CanUse(stats))
            {
                skill.PayCost(stats);
                OnSkillUsed?.Invoke(skill);
            }
        }

        [Serializable]
        private class ActiveStatusEffect
        {
            public StatusEffect statusEffect;
            public int duration;
        }
    }
}
