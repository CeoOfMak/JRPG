using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.BattleSystem
{
    [CreateAssetMenu(fileName = "New Enemy AI", menuName = "JRPG/Battle/Enemy AI")]
    public class EnemyAI : ScriptableObject
    {
        [Header("AI Behavior")]
        public AIBehaviorType behaviorType = AIBehaviorType.Balanced;

        [Header("Targeting Priority")]
        [Range(0f, 1f)]
        public float targetLowestHPChance = 0.4f;
        [Range(0f, 1f)]
        public float targetHighestThreatChance = 0.3f;
        [Range(0f, 1f)]
        public float targetRandomChance = 0.3f;

        [Header("Skill Selection")]
        [Range(0f, 1f)]
        public float useStrongSkillChance = 0.3f;
        [Range(0f, 1f)]
        public float useBuffDebuffChance = 0.2f;

        public BattleAction DecideAction(BattleCombatant enemyCombatant, List<BattleCombatant> allPlayers, List<BattleCombatant> allEnemies)
        {
            BattleAction action = new BattleAction();
            action.actor = enemyCombatant;

            // Filter available skills
            var usableSkills = enemyCombatant.availableSkills.Where(s => s.CanUse(enemyCombatant.stats)).ToList();

            if (usableSkills.Count == 0)
            {
                // Defend if no skills available
                action.actionType = BattleActionType.Defend;
                return action;
            }

            // Decide action type based on behavior
            switch (behaviorType)
            {
                case AIBehaviorType.Aggressive:
                    action.actionType = BattleActionType.Skill;
                    action.selectedSkill = SelectOffensiveSkill(usableSkills);
                    break;

                case AIBehaviorType.Defensive:
                    action.actionType = Random.value < 0.3f ? BattleActionType.Defend : BattleActionType.Skill;
                    if (action.actionType == BattleActionType.Skill)
                    {
                        action.selectedSkill = SelectDefensiveSkill(usableSkills, enemyCombatant, allEnemies);
                    }
                    break;

                case AIBehaviorType.Balanced:
                    action.actionType = BattleActionType.Skill;
                    action.selectedSkill = SelectBalancedSkill(usableSkills, enemyCombatant, allEnemies);
                    break;

                case AIBehaviorType.Support:
                    action.actionType = BattleActionType.Skill;
                    action.selectedSkill = SelectSupportSkill(usableSkills, enemyCombatant, allEnemies);
                    break;
            }

            // Select target based on skill
            if (action.actionType == BattleActionType.Skill && action.selectedSkill != null)
            {
                action.targets = SelectTargets(action.selectedSkill, allPlayers, allEnemies);
            }

            return action;
        }

        private Skill SelectOffensiveSkill(List<Skill> usableSkills)
        {
            var offensiveSkills = usableSkills.Where(s =>
                s.skillType == SkillType.Attack ||
                s.skillType == SkillType.Debuff).ToList();

            if (offensiveSkills.Count == 0)
                return usableSkills[Random.Range(0, usableSkills.Count)];

            // Prefer high-power skills
            if (Random.value < useStrongSkillChance)
            {
                return offensiveSkills.OrderByDescending(s => s.basePower).First();
            }

            return offensiveSkills[Random.Range(0, offensiveSkills.Count)];
        }

        private Skill SelectDefensiveSkill(List<Skill> usableSkills, BattleCombatant self, List<BattleCombatant> allies)
        {
            // Check if healing is needed
            var healSkills = usableSkills.Where(s => s.skillType == SkillType.Heal).ToList();
            if (healSkills.Count > 0)
            {
                bool needsHealing = allies.Any(a =>
                    a.stats.CurrentHP < a.stats.MaxHP * 0.5f ||
                    a.stats.CurrentShieldBar < a.stats.MaxShieldBar * 0.3f);

                if (needsHealing)
                {
                    return healSkills[Random.Range(0, healSkills.Count)];
                }
            }

            // Use buff skills
            var buffSkills = usableSkills.Where(s => s.skillType == SkillType.Buff).ToList();
            if (buffSkills.Count > 0 && Random.value < useBuffDebuffChance)
            {
                return buffSkills[Random.Range(0, buffSkills.Count)];
            }

            // Default to attack
            return SelectOffensiveSkill(usableSkills);
        }

        private Skill SelectBalancedSkill(List<Skill> usableSkills, BattleCombatant self, List<BattleCombatant> allies)
        {
            // Check for critical HP situations
            bool criticalSituation = allies.Any(a => a.stats.CurrentHP < a.stats.MaxHP * 0.3f);

            if (criticalSituation)
            {
                var healSkills = usableSkills.Where(s => s.skillType == SkillType.Heal).ToList();
                if (healSkills.Count > 0)
                {
                    return healSkills[Random.Range(0, healSkills.Count)];
                }
            }

            // Mix of offense and support
            float roll = Random.value;
            if (roll < 0.6f)
            {
                return SelectOffensiveSkill(usableSkills);
            }
            else if (roll < 0.8f)
            {
                var debuffSkills = usableSkills.Where(s => s.skillType == SkillType.Debuff).ToList();
                if (debuffSkills.Count > 0)
                    return debuffSkills[Random.Range(0, debuffSkills.Count)];
            }
            else
            {
                var buffSkills = usableSkills.Where(s => s.skillType == SkillType.Buff).ToList();
                if (buffSkills.Count > 0)
                    return buffSkills[Random.Range(0, buffSkills.Count)];
            }

            return usableSkills[Random.Range(0, usableSkills.Count)];
        }

        private Skill SelectSupportSkill(List<Skill> usableSkills, BattleCombatant self, List<BattleCombatant> allies)
        {
            var supportSkills = usableSkills.Where(s =>
                s.skillType == SkillType.Heal ||
                s.skillType == SkillType.Buff).ToList();

            if (supportSkills.Count > 0 && Random.value < 0.7f)
            {
                return supportSkills[Random.Range(0, supportSkills.Count)];
            }

            return usableSkills[Random.Range(0, usableSkills.Count)];
        }

        private List<BattleCombatant> SelectTargets(Skill skill, List<BattleCombatant> players, List<BattleCombatant> enemies)
        {
            List<BattleCombatant> targets = new List<BattleCombatant>();
            var alivePlayers = players.Where(p => p.IsAlive).ToList();
            var aliveEnemies = enemies.Where(e => e.IsAlive).ToList();

            switch (skill.targetType)
            {
                case TargetType.Self:
                    // Target the enemy itself
                    targets.Add(enemies.First());
                    break;

                case TargetType.SingleAlly:
                    if (aliveEnemies.Count > 0)
                    {
                        // Target ally with lowest HP percentage
                        var target = aliveEnemies.OrderBy(e =>
                            (float)e.stats.CurrentHP / e.stats.MaxHP).First();
                        targets.Add(target);
                    }
                    break;

                case TargetType.AllAllies:
                    targets.AddRange(aliveEnemies);
                    break;

                case TargetType.SingleEnemy:
                    if (alivePlayers.Count > 0)
                    {
                        targets.Add(SelectPlayerTarget(alivePlayers));
                    }
                    break;

                case TargetType.AllEnemies:
                    targets.AddRange(alivePlayers);
                    break;

                case TargetType.RandomEnemy:
                    if (alivePlayers.Count > 0)
                    {
                        targets.Add(alivePlayers[Random.Range(0, alivePlayers.Count)]);
                    }
                    break;

                case TargetType.All:
                    targets.AddRange(alivePlayers);
                    targets.AddRange(aliveEnemies);
                    break;
            }

            return targets;
        }

        private BattleCombatant SelectPlayerTarget(List<BattleCombatant> alivePlayers)
        {
            float roll = Random.value;

            if (roll < targetLowestHPChance)
            {
                // Target player with lowest HP percentage
                return alivePlayers.OrderBy(p => (float)p.stats.CurrentHP / p.stats.MaxHP).First();
            }
            else if (roll < targetLowestHPChance + targetHighestThreatChance)
            {
                // Target player with highest attack (threat)
                return alivePlayers.OrderByDescending(p => p.stats.Attack).First();
            }
            else
            {
                // Random target
                return alivePlayers[Random.Range(0, alivePlayers.Count)];
            }
        }
    }

    public enum AIBehaviorType
    {
        Aggressive,
        Defensive,
        Balanced,
        Support
    }

    public class BattleAction
    {
        public BattleCombatant actor;
        public BattleActionType actionType;
        public Skill selectedSkill;
        public List<BattleCombatant> targets = new List<BattleCombatant>();
    }

    public enum BattleActionType
    {
        Skill,
        Item,
        Defend,
        Flee
    }
}
