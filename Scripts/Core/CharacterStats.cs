using System;
using UnityEngine;

namespace JRPGGame.Core
{
    [Serializable]
    public class CharacterStats
    {
        [Header("Base Stats")]
        [SerializeField] private int level = 1;
        [SerializeField] private int currentHP;
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int currentMP;
        [SerializeField] private int maxMP = 50;
        [SerializeField] private int currentShieldBar;
        [SerializeField] private int maxShieldBar = 80;

        [Header("Offensive Stats")]
        [SerializeField] private int attack = 10;
        [SerializeField] private int magicAttack = 10;
        [SerializeField] private int accuracy = 95;
        [SerializeField] private float criticalRate = 5f;
        [SerializeField] private float criticalDamage = 150f;

        [Header("Defensive Stats")]
        [SerializeField] private int defense = 5;
        [SerializeField] private int magicDefense = 5;
        [SerializeField] private int evasion = 5;
        [SerializeField] private int speed = 10;

        [Header("Experience")]
        [SerializeField] private int currentExp;
        [SerializeField] private int expToNextLevel = 100;

        // Events
        public event Action OnStatsChanged;
        public event Action OnLevelUp;
        public event Action OnHPChanged;
        public event Action OnMPChanged;
        public event Action OnShieldBarChanged;

        // Properties
        public int Level { get => level; set => level = value; }
        public int CurrentHP
        {
            get => currentHP;
            set
            {
                currentHP = Mathf.Clamp(value, 0, maxHP);
                OnHPChanged?.Invoke();
            }
        }
        public int MaxHP
        {
            get => maxHP;
            set
            {
                maxHP = value;
                OnStatsChanged?.Invoke();
            }
        }
        public int CurrentMP
        {
            get => currentMP;
            set
            {
                currentMP = Mathf.Clamp(value, 0, maxMP);
                OnMPChanged?.Invoke();
            }
        }
        public int MaxMP
        {
            get => maxMP;
            set
            {
                maxMP = value;
                OnStatsChanged?.Invoke();
            }
        }
        public int CurrentShieldBar
        {
            get => currentShieldBar;
            set
            {
                currentShieldBar = Mathf.Clamp(value, 0, maxShieldBar);
                OnShieldBarChanged?.Invoke();
            }
        }
        public int MaxShieldBar
        {
            get => maxShieldBar;
            set
            {
                maxShieldBar = Mathf.Min(value, maxHP); // SB must be less than HP
                OnStatsChanged?.Invoke();
            }
        }

        public int Attack { get => attack; set { attack = value; OnStatsChanged?.Invoke(); } }
        public int MagicAttack { get => magicAttack; set { magicAttack = value; OnStatsChanged?.Invoke(); } }
        public int Defense { get => defense; set { defense = value; OnStatsChanged?.Invoke(); } }
        public int MagicDefense { get => magicDefense; set { magicDefense = value; OnStatsChanged?.Invoke(); } }
        public int Speed { get => speed; set { speed = value; OnStatsChanged?.Invoke(); } }
        public int Accuracy { get => accuracy; set { accuracy = value; OnStatsChanged?.Invoke(); } }
        public int Evasion { get => evasion; set { evasion = value; OnStatsChanged?.Invoke(); } }
        public float CriticalRate { get => criticalRate; set { criticalRate = value; OnStatsChanged?.Invoke(); } }
        public float CriticalDamage { get => criticalDamage; set { criticalDamage = value; OnStatsChanged?.Invoke(); } }
        public int CurrentExp { get => currentExp; set => currentExp = value; }
        public int ExpToNextLevel { get => expToNextLevel; set => expToNextLevel = value; }

        public bool IsAlive => currentHP > 0;
        public bool IsShieldBarDepleted => currentShieldBar <= 0;

        public void Initialize()
        {
            currentHP = maxHP;
            currentMP = maxMP;
            currentShieldBar = maxShieldBar;
        }

        public void ResetAfterBattle()
        {
            currentHP = maxHP;
            currentMP = maxMP;
        }

        public void AddExperience(int amount)
        {
            currentExp += amount;
            while (currentExp >= expToNextLevel && level < GameConstants.MAX_LEVEL)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            level++;
            currentExp -= expToNextLevel;
            expToNextLevel = CalculateExpForNextLevel();

            // Apply stat growth (can be customized per character)
            maxHP += UnityEngine.Random.Range(8, 15);
            maxMP += UnityEngine.Random.Range(3, 7);
            maxShieldBar = Mathf.Min(maxShieldBar + UnityEngine.Random.Range(5, 10), maxHP);
            attack += UnityEngine.Random.Range(2, 5);
            defense += UnityEngine.Random.Range(1, 4);
            magicAttack += UnityEngine.Random.Range(2, 5);
            magicDefense += UnityEngine.Random.Range(1, 4);
            speed += UnityEngine.Random.Range(1, 3);

            // Restore HP/MP/SB on level up
            currentHP = maxHP;
            currentMP = maxMP;
            currentShieldBar = maxShieldBar;

            OnLevelUp?.Invoke();
            OnStatsChanged?.Invoke();
        }

        private int CalculateExpForNextLevel()
        {
            // Formula: Base * Level^2 * 1.5
            return Mathf.RoundToInt(100 * Mathf.Pow(level, 2) * 1.5f);
        }

        public CharacterStats Clone()
        {
            return (CharacterStats)this.MemberwiseClone();
        }
    }
}
