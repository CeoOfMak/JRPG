using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.BattleSystem
{
    public static class DamageCalculator
    {
        public static DamageResult CalculateDamage(BattleCombatant attacker, BattleCombatant target, Skill skill)
        {
            DamageResult result = new DamageResult();

            // Check if attack hits
            if (!CheckAccuracy(attacker, target, skill))
            {
                result.missed = true;
                return result;
            }

            // Calculate base damage
            int baseDamage = skill.basePower;
            float attackStat = 0;
            float defenseStat = 0;

            switch (skill.damageType)
            {
                case DamageType.Physical:
                    attackStat = attacker.stats.Attack * attacker.GetStatModifier(StatType.Attack);
                    defenseStat = target.stats.Defense * target.GetStatModifier(StatType.Defense);
                    break;
                case DamageType.Magical:
                    attackStat = attacker.stats.MagicAttack * attacker.GetStatModifier(StatType.MagicAttack);
                    defenseStat = target.stats.MagicDefense * target.GetStatModifier(StatType.MagicDefense);
                    break;
                case DamageType.True:
                    // True damage ignores defense
                    result.totalDamage = baseDamage;
                    result.targetsShieldBar = true;
                    return result;
            }

            // Damage formula: ((BasePower * AttackStat) / DefenseStat) * Modifiers
            float damage = ((baseDamage * attackStat) / Mathf.Max(defenseStat, 1)) * 2;

            // Check for critical hit
            if (skill.canCrit && CheckCritical(attacker))
            {
                result.isCritical = true;
                damage *= (attacker.stats.CriticalDamage / 100f);
            }

            // Add variance (90% - 110%)
            damage *= Random.Range(0.9f, 1.1f);

            result.totalDamage = Mathf.Max(1, Mathf.RoundToInt(damage));
            result.shieldDamage = skill.shieldDamage;
            result.elementType = skill.elementType;
            result.targetsShieldBar = target.stats.CurrentShieldBar > 0;

            return result;
        }

        public static bool CheckAccuracy(BattleCombatant attacker, BattleCombatant target, Skill skill)
        {
            float baseAccuracy = skill.accuracy;
            float attackerAccuracy = attacker.stats.Accuracy;
            float targetEvasion = target.stats.Evasion * target.GetStatModifier(StatType.Evasion);

            float hitChance = (baseAccuracy * attackerAccuracy) / (attackerAccuracy + targetEvasion);
            hitChance = Mathf.Clamp(hitChance, 5f, 100f); // Min 5%, Max 100% hit chance

            return Random.Range(0f, 100f) < hitChance;
        }

        public static bool CheckCritical(BattleCombatant attacker)
        {
            float critChance = attacker.stats.CriticalRate * attacker.GetStatModifier(StatType.CriticalRate);
            return Random.Range(0f, 100f) < critChance;
        }

        public static int CalculateHealing(BattleCombatant healer, Skill skill)
        {
            int baseHeal = skill.healAmount;

            // Healing can scale with magic attack
            if (skill.skillType == SkillType.Heal)
            {
                float magicAttack = healer.stats.MagicAttack * healer.GetStatModifier(StatType.MagicAttack);
                baseHeal += Mathf.RoundToInt(magicAttack * 0.5f);
            }

            // Add variance (95% - 105%)
            int finalHeal = Mathf.RoundToInt(baseHeal * Random.Range(0.95f, 1.05f));

            return Mathf.Max(1, finalHeal);
        }
    }

    public class DamageResult
    {
        public int totalDamage;
        public int shieldDamage;
        public bool isCritical;
        public bool missed;
        public ElementType elementType;
        public bool targetsShieldBar;
    }
}
