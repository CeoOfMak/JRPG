using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;
using JRPGGame.BattleSystem;

namespace JRPGGame.Inventory
{
    [CreateAssetMenu(fileName = "New Item", menuName = "JRPG/Inventory/Item")]
    public class Item : ScriptableObject
    {
        [Header("Basic Info")]
        public string itemName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Item Type")]
        public ItemType itemType;

        [Header("Usability")]
        public bool usableInBattle = true;
        public bool usableInField = true;
        public bool consumable = true;

        [Header("Target")]
        public TargetType targetType = TargetType.SingleAlly;

        [Header("Effects")]
        public List<ItemEffect> effects = new List<ItemEffect>();

        [Header("Economy")]
        public int buyPrice = 50;
        public int sellPrice = 25;

        [Header("Inventory")]
        public int maxStackSize = 99;

        public virtual void Use(BattleCombatant target)
        {
            foreach (var effect in effects)
            {
                ApplyEffect(effect, target);
            }
        }

        protected void ApplyEffect(ItemEffect effect, BattleCombatant target)
        {
            switch (effect.effectType)
            {
                case ItemEffectType.RestoreHP:
                    int hpRestore = effect.isPercentage
                        ? Mathf.RoundToInt(target.stats.MaxHP * (effect.power / 100f))
                        : effect.power;
                    target.Heal(hpRestore, false);
                    break;

                case ItemEffectType.RestoreMP:
                    int mpRestore = effect.isPercentage
                        ? Mathf.RoundToInt(target.stats.MaxMP * (effect.power / 100f))
                        : effect.power;
                    target.stats.CurrentMP += mpRestore;
                    break;

                case ItemEffectType.RestoreShieldBar:
                    int sbRestore = effect.isPercentage
                        ? Mathf.RoundToInt(target.stats.MaxShieldBar * (effect.power / 100f))
                        : effect.power;
                    target.Heal(sbRestore, true);
                    break;

                case ItemEffectType.RemoveStatusEffect:
                    // Status effect removal logic would go here
                    break;

                case ItemEffectType.ApplyBuff:
                    if (effect.statusEffect != null)
                    {
                        target.ApplyStatusEffect(effect.statusEffect);
                    }
                    break;

                case ItemEffectType.ReviveAlly:
                    if (!target.IsAlive)
                    {
                        target.stats.CurrentHP = effect.isPercentage
                            ? Mathf.RoundToInt(target.stats.MaxHP * (effect.power / 100f))
                            : effect.power;
                    }
                    break;
            }
        }
    }

    [Serializable]
    public class ItemEffect
    {
        public ItemEffectType effectType;
        public int power;
        public bool isPercentage;
        public StatusEffect statusEffect;
    }

    public enum ItemEffectType
    {
        RestoreHP,
        RestoreMP,
        RestoreShieldBar,
        RemoveStatusEffect,
        ApplyBuff,
        ReviveAlly,
        Custom
    }
}
