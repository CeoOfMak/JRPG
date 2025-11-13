using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;
using JRPGGame.BattleSystem;

namespace JRPGGame.Progression
{
    [Serializable]
    public class CharacterProgression
    {
        [Header("Character Identity")]
        public string characterName;
        public string characterClass;
        public Sprite characterPortrait;

        [Header("Stats")]
        public CharacterStats stats;

        [Header("Equipment")]
        public EquipmentLoadout equipment;

        [Header("Skills")]
        public List<Skill> learnedSkills = new List<Skill>();
        public List<SkillUnlock> skillUnlocks = new List<SkillUnlock>();

        [Header("Growth Rates")]
        public StatGrowthRates growthRates;

        public event Action<Skill> OnSkillLearned;
        public event Action<int> OnLevelUp;

        public CharacterProgression()
        {
            stats = new CharacterStats();
            equipment = new EquipmentLoadout();
            growthRates = new StatGrowthRates();
        }

        public void Initialize()
        {
            stats.Initialize();
            stats.OnLevelUp += HandleLevelUp;
        }

        private void HandleLevelUp()
        {
            ApplyStatGrowth();
            CheckSkillUnlocks();
            OnLevelUp?.Invoke(stats.Level);
        }

        private void ApplyStatGrowth()
        {
            stats.MaxHP += CalculateStatIncrease(growthRates.hpGrowth);
            stats.MaxMP += CalculateStatIncrease(growthRates.mpGrowth);
            stats.MaxShieldBar += CalculateStatIncrease(growthRates.shieldBarGrowth);
            stats.Attack += CalculateStatIncrease(growthRates.attackGrowth);
            stats.Defense += CalculateStatIncrease(growthRates.defenseGrowth);
            stats.MagicAttack += CalculateStatIncrease(growthRates.magicAttackGrowth);
            stats.MagicDefense += CalculateStatIncrease(growthRates.magicDefenseGrowth);
            stats.Speed += CalculateStatIncrease(growthRates.speedGrowth);
        }

        private int CalculateStatIncrease(StatGrowth growth)
        {
            return UnityEngine.Random.Range(growth.minGrowth, growth.maxGrowth + 1);
        }

        private void CheckSkillUnlocks()
        {
            foreach (var unlock in skillUnlocks)
            {
                if (unlock.unlockLevel == stats.Level && !learnedSkills.Contains(unlock.skill))
                {
                    LearnSkill(unlock.skill);
                }
            }
        }

        public void LearnSkill(Skill skill)
        {
            if (!learnedSkills.Contains(skill))
            {
                learnedSkills.Add(skill);
                OnSkillLearned?.Invoke(skill);
                Debug.Log($"{characterName} learned {skill.skillName}!");
            }
        }

        public bool CanEquip(Equipment equipmentItem)
        {
            return equipmentItem.CanEquip(stats, characterClass);
        }

        public void EquipItem(Equipment equipmentItem)
        {
            if (!CanEquip(equipmentItem))
            {
                Debug.LogWarning($"{characterName} cannot equip {equipmentItem.equipmentName}!");
                return;
            }

            // Unequip current item in that slot
            Equipment currentItem = equipment.GetEquipment(equipmentItem.equipmentSlot);
            if (currentItem != null)
            {
                UnequipItem(equipmentItem.equipmentSlot);
            }

            // Equip new item
            equipment.SetEquipment(equipmentItem.equipmentSlot, equipmentItem);
            equipmentItem.ApplyStatBonuses(stats);

            Debug.Log($"{characterName} equipped {equipmentItem.equipmentName}!");
        }

        public void UnequipItem(EquipmentSlot slot)
        {
            Equipment currentItem = equipment.GetEquipment(slot);
            if (currentItem != null)
            {
                currentItem.RemoveStatBonuses(stats);
                equipment.SetEquipment(slot, null);
                Debug.Log($"{characterName} unequipped {currentItem.equipmentName}!");
            }
        }

        public int GetTotalStats(StatType statType)
        {
            // Base stat + equipment bonuses
            int baseStat = 0;

            switch (statType)
            {
                case StatType.HP: baseStat = stats.MaxHP; break;
                case StatType.MP: baseStat = stats.MaxMP; break;
                case StatType.ShieldBar: baseStat = stats.MaxShieldBar; break;
                case StatType.Attack: baseStat = stats.Attack; break;
                case StatType.Defense: baseStat = stats.Defense; break;
                case StatType.MagicAttack: baseStat = stats.MagicAttack; break;
                case StatType.MagicDefense: baseStat = stats.MagicDefense; break;
                case StatType.Speed: baseStat = stats.Speed; break;
            }

            return baseStat;
        }
    }

    [Serializable]
    public class EquipmentLoadout
    {
        public Equipment weapon;
        public Equipment armor;
        public Equipment accessory1;
        public Equipment accessory2;
        public Equipment accessory3;
        public Equipment accessory4;

        public Equipment GetEquipment(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: return weapon;
                case EquipmentSlot.Armor: return armor;
                case EquipmentSlot.Accessory1: return accessory1;
                case EquipmentSlot.Accessory2: return accessory2;
                case EquipmentSlot.Accessory3: return accessory3;
                case EquipmentSlot.Accessory4: return accessory4;
                default: return null;
            }
        }

        public void SetEquipment(EquipmentSlot slot, Equipment equipment)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: weapon = equipment; break;
                case EquipmentSlot.Armor: armor = equipment; break;
                case EquipmentSlot.Accessory1: accessory1 = equipment; break;
                case EquipmentSlot.Accessory2: accessory2 = equipment; break;
                case EquipmentSlot.Accessory3: accessory3 = equipment; break;
                case EquipmentSlot.Accessory4: accessory4 = equipment; break;
            }
        }

        public List<Equipment> GetAllEquipment()
        {
            List<Equipment> allEquipment = new List<Equipment>();
            if (weapon != null) allEquipment.Add(weapon);
            if (armor != null) allEquipment.Add(armor);
            if (accessory1 != null) allEquipment.Add(accessory1);
            if (accessory2 != null) allEquipment.Add(accessory2);
            if (accessory3 != null) allEquipment.Add(accessory3);
            if (accessory4 != null) allEquipment.Add(accessory4);
            return allEquipment;
        }
    }

    [Serializable]
    public class SkillUnlock
    {
        public Skill skill;
        public int unlockLevel;
    }

    [Serializable]
    public class StatGrowthRates
    {
        public StatGrowth hpGrowth = new StatGrowth(8, 15);
        public StatGrowth mpGrowth = new StatGrowth(3, 7);
        public StatGrowth shieldBarGrowth = new StatGrowth(5, 10);
        public StatGrowth attackGrowth = new StatGrowth(2, 5);
        public StatGrowth defenseGrowth = new StatGrowth(1, 4);
        public StatGrowth magicAttackGrowth = new StatGrowth(2, 5);
        public StatGrowth magicDefenseGrowth = new StatGrowth(1, 4);
        public StatGrowth speedGrowth = new StatGrowth(1, 3);
    }

    [Serializable]
    public class StatGrowth
    {
        public int minGrowth;
        public int maxGrowth;

        public StatGrowth(int min, int max)
        {
            minGrowth = min;
            maxGrowth = max;
        }
    }
}
