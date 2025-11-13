using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.BattleSystem
{
    [CreateAssetMenu(fileName = "New Skill", menuName = "JRPG/Battle/Skill")]
    public class Skill : ScriptableObject
    {
        [Header("Basic Info")]
        public string skillName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Skill Properties")]
        public SkillType skillType;
        public TargetType targetType;
        public ElementType elementType = ElementType.None;
        public DamageType damageType = DamageType.Physical;

        [Header("Costs")]
        public int mpCost;
        public int hpCost;

        [Header("Power & Effects")]
        public int basePower = 50;
        public int accuracy = 100;
        public bool canCrit = true;

        [Header("Shield Damage")]
        public int shieldDamage = 30;

        [Header("Healing")]
        public int healAmount;
        public bool healsShieldBar;

        [Header("Status Effects")]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();

        [Header("Unlock Requirements")]
        public int unlockLevel = 1;

        [Header("Animation")]
        public string animationTrigger;
        public GameObject effectPrefab;

        public bool CanUse(CharacterStats userStats)
        {
            if (userStats.CurrentMP < mpCost) return false;
            if (userStats.CurrentHP <= hpCost) return false;
            return true;
        }

        public void PayCost(CharacterStats userStats)
        {
            userStats.CurrentMP -= mpCost;
            if (hpCost > 0)
            {
                userStats.CurrentHP -= hpCost;
            }
        }
    }

    [Serializable]
    public class StatusEffect
    {
        public BuffDebuffType effectType;
        public int duration; // in turns
        public int power; // for DoT/HoT effects
        public float statModifier; // percentage modifier for stat buffs/debuffs
    }
}
