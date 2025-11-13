using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.BattleSystem
{
    public class BossCombatant : BattleCombatant
    {
        [Header("Boss Specific")]
        [SerializeField] private List<BossPhase> phases = new List<BossPhase>();
        [SerializeField] private int currentPhaseIndex = 0;
        [SerializeField] private bool hasTransitioned = false;

        public event Action<int> OnPhaseTransition;

        public bool IsBoss => phases.Count > 0;
        public int CurrentPhase => currentPhaseIndex;
        public int TotalPhases => phases.Count;

        private void Start()
        {
            if (phases.Count > 0)
            {
                ApplyPhaseStats(0);
            }
        }

        public void CheckPhaseTransition()
        {
            if (hasTransitioned) return;
            if (currentPhaseIndex >= phases.Count - 1) return;

            float hpPercentage = (float)stats.CurrentHP / stats.MaxHP;

            // Check if HP threshold is met for next phase
            if (hpPercentage <= phases[currentPhaseIndex].nextPhaseHPThreshold)
            {
                TransitionToNextPhase();
            }
        }

        private void TransitionToNextPhase()
        {
            hasTransitioned = true;
            currentPhaseIndex++;

            if (currentPhaseIndex < phases.Count)
            {
                ApplyPhaseStats(currentPhaseIndex);
                OnPhaseTransition?.Invoke(currentPhaseIndex);

                Debug.Log($"{characterName} transitioned to Phase {currentPhaseIndex + 1}!");
            }

            hasTransitioned = false;
        }

        private void ApplyPhaseStats(int phaseIndex)
        {
            if (phaseIndex < 0 || phaseIndex >= phases.Count) return;

            BossPhase phase = phases[phaseIndex];

            // Reset HP, MP, and Shield Bar for new phase
            stats.MaxHP = phase.maxHP;
            stats.MaxMP = phase.maxMP;
            stats.MaxShieldBar = Mathf.Min(phase.maxShieldBar, phase.maxHP);

            stats.CurrentHP = stats.MaxHP;
            stats.CurrentMP = stats.MaxMP;
            stats.CurrentShieldBar = stats.MaxShieldBar;

            // Apply phase stats
            stats.Attack = phase.attack;
            stats.Defense = phase.defense;
            stats.MagicAttack = phase.magicAttack;
            stats.MagicDefense = phase.magicDefense;
            stats.Speed = phase.speed;

            // Update available skills
            if (phase.phaseSkills != null && phase.phaseSkills.Count > 0)
            {
                availableSkills = new List<Skill>(phase.phaseSkills);
            }
        }

        // Override TakeDamage to check for phase transitions
        public new int TakeDamage(int damage, ElementType element, bool targetsShieldBar)
        {
            int actualDamage = base.TakeDamage(damage, element, targetsShieldBar);
            CheckPhaseTransition();
            return actualDamage;
        }
    }

    [Serializable]
    public class BossPhase
    {
        [Header("Phase Info")]
        public string phaseName;
        [TextArea(2, 4)]
        public string phaseDescription;

        [Header("HP Threshold")]
        [Range(0f, 1f)]
        [Tooltip("HP percentage at which the boss transitions to the next phase (e.g., 0.5 = 50% HP)")]
        public float nextPhaseHPThreshold = 0.5f;

        [Header("Phase Stats")]
        public int maxHP = 1000;
        public int maxMP = 200;
        public int maxShieldBar = 800;

        public int attack = 50;
        public int defense = 30;
        public int magicAttack = 50;
        public int magicDefense = 30;
        public int speed = 20;

        [Header("Phase Skills")]
        public List<Skill> phaseSkills = new List<Skill>();

        [Header("Phase Visual")]
        public GameObject phaseModel;
        public Color phaseAuraColor = Color.red;
    }
}
