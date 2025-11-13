using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.Progression
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "JRPG/Progression/Equipment")]
    public class Equipment : ScriptableObject
    {
        [Header("Basic Info")]
        public string equipmentName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Equipment Type")]
        public EquipmentSlot equipmentSlot;

        [Header("Requirements")]
        public int requiredLevel = 1;
        public List<string> requiredClasses = new List<string>();

        [Header("Stats Bonuses")]
        public int hpBonus;
        public int mpBonus;
        public int shieldBarBonus;
        public int attackBonus;
        public int defenseBonus;
        public int magicAttackBonus;
        public int magicDefenseBonus;
        public int speedBonus;
        public int accuracyBonus;
        public int evasionBonus;
        public float criticalRateBonus;
        public float criticalDamageBonus;

        [Header("Special Properties")]
        public ElementType elementalAffinity = ElementType.None;
        public List<BuffDebuffType> grantedPassives = new List<BuffDebuffType>();

        [Header("Upgrade System")]
        public int upgradeLevel = 0;
        public int maxUpgradeLevel = 5;
        public List<UpgradeMaterial> upgradeMaterials = new List<UpgradeMaterial>();

        [Header("Economy")]
        public int buyPrice = 100;
        public int sellPrice = 50;

        public bool CanEquip(CharacterStats stats, string characterClass)
        {
            if (stats.Level < requiredLevel) return false;
            if (requiredClasses.Count > 0 && !requiredClasses.Contains(characterClass)) return false;
            return true;
        }

        public void ApplyStatBonuses(CharacterStats stats)
        {
            stats.MaxHP += hpBonus;
            stats.MaxMP += mpBonus;
            stats.MaxShieldBar += shieldBarBonus;
            stats.Attack += attackBonus;
            stats.Defense += defenseBonus;
            stats.MagicAttack += magicAttackBonus;
            stats.MagicDefense += magicDefenseBonus;
            stats.Speed += speedBonus;
            stats.Accuracy += accuracyBonus;
            stats.Evasion += evasionBonus;
            stats.CriticalRate += criticalRateBonus;
            stats.CriticalDamage += criticalDamageBonus;
        }

        public void RemoveStatBonuses(CharacterStats stats)
        {
            stats.MaxHP -= hpBonus;
            stats.MaxMP -= mpBonus;
            stats.MaxShieldBar -= shieldBarBonus;
            stats.Attack -= attackBonus;
            stats.Defense -= defenseBonus;
            stats.MagicAttack -= magicAttackBonus;
            stats.MagicDefense -= magicDefenseBonus;
            stats.Speed -= speedBonus;
            stats.Accuracy -= accuracyBonus;
            stats.Evasion -= evasionBonus;
            stats.CriticalRate -= criticalRateBonus;
            stats.CriticalDamage -= criticalDamageBonus;
        }

        public bool CanUpgrade()
        {
            return upgradeLevel < maxUpgradeLevel;
        }

        public Equipment CreateUpgradedCopy()
        {
            Equipment upgraded = Instantiate(this);
            upgraded.upgradeLevel++;

            // Increase stats by 10% per upgrade level
            float multiplier = 1.1f;
            upgraded.hpBonus = Mathf.RoundToInt(upgraded.hpBonus * multiplier);
            upgraded.mpBonus = Mathf.RoundToInt(upgraded.mpBonus * multiplier);
            upgraded.shieldBarBonus = Mathf.RoundToInt(upgraded.shieldBarBonus * multiplier);
            upgraded.attackBonus = Mathf.RoundToInt(upgraded.attackBonus * multiplier);
            upgraded.defenseBonus = Mathf.RoundToInt(upgraded.defenseBonus * multiplier);
            upgraded.magicAttackBonus = Mathf.RoundToInt(upgraded.magicAttackBonus * multiplier);
            upgraded.magicDefenseBonus = Mathf.RoundToInt(upgraded.magicDefenseBonus * multiplier);
            upgraded.speedBonus = Mathf.RoundToInt(upgraded.speedBonus * multiplier);

            return upgraded;
        }
    }

    [Serializable]
    public class UpgradeMaterial
    {
        public CraftingMaterial material;
        public int quantity;
    }

    [CreateAssetMenu(fileName = "New Material", menuName = "JRPG/Progression/Material")]
    public class CraftingMaterial : ScriptableObject
    {
        public string materialName;
        public string description;
        public Sprite icon;
        public int stackLimit = 99;
    }
}
