using System;

namespace JRPGGame.Core
{
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Earth,
        Wind,
        Lightning,
        Ice,
        Light,
        Dark
    }

    public enum DamageType
    {
        Physical,
        Magical,
        True
    }

    public enum TargetType
    {
        Self,
        SingleAlly,
        AllAllies,
        SingleEnemy,
        AllEnemies,
        RandomEnemy,
        All
    }

    public enum SkillType
    {
        Attack,
        Heal,
        Buff,
        Debuff,
        Special
    }

    public enum ItemType
    {
        Consumable,
        KeyItem,
        Equipment,
        Material,
        QuestItem
    }

    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Accessory1,
        Accessory2,
        Accessory3,
        Accessory4
    }

    public enum StatType
    {
        HP,
        MP,
        ShieldBar,
        Attack,
        Defense,
        MagicAttack,
        MagicDefense,
        Speed,
        Accuracy,
        Evasion,
        CriticalRate,
        CriticalDamage
    }

    public enum BattleState
    {
        Start,
        TurnStart,
        SelectingAction,
        ExecutingAction,
        TurnEnd,
        Victory,
        Defeat
    }

    public enum CombatantState
    {
        Normal,
        Break,
        Dead,
        Defending
    }

    public enum QuestStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    public enum QuestType
    {
        MainQuest,
        SideQuest
    }

    public enum EncounterType
    {
        Random,
        Visible,
        Boss,
        Scripted
    }

    public enum LocationType
    {
        Town,
        Dungeon,
        Overworld,
        WorldMap
    }

    public enum DialogueEventType
    {
        StartQuest,
        CompleteQuest,
        GiveItem,
        TakeItem,
        SetFlag,
        StartBattle,
        Teleport,
        Custom
    }

    [Flags]
    public enum BuffDebuffType
    {
        None = 0,
        AttackUp = 1 << 0,
        AttackDown = 1 << 1,
        DefenseUp = 1 << 2,
        DefenseDown = 1 << 3,
        SpeedUp = 1 << 4,
        SpeedDown = 1 << 5,
        Poison = 1 << 6,
        Regen = 1 << 7,
        Stun = 1 << 8,
        Shield = 1 << 9
    }
}
