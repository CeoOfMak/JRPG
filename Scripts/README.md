# Unity 6 JRPG Game Scripts

This is a comprehensive collection of game-ready scripts for a 3D turn-based JRPG game in Unity 6.

## 📁 Project Structure

### Core Systems
- **GameEnums.cs** - All game enumerations (ElementType, ItemType, QuestStatus, etc.)
- **GameConstants.cs** - Game-wide constants and configuration values
- **CharacterStats.cs** - Character statistics management with events
- **ElementEffectiveness.cs** - Element type effectiveness calculations

### Battle System
- **Skill.cs** - ScriptableObject for skills and abilities
- **BattleCombatant.cs** - Base combatant class with shield bar and break state mechanics
- **BossCombatant.cs** - Boss-specific combatant with multi-phase support
- **TurnOrderSystem.cs** - Turn-based battle order management
- **DamageCalculator.cs** - Damage calculation with elements, criticals, and accuracy
- **EnemyAI.cs** - AI behavior patterns (Aggressive, Defensive, Balanced, Support)
- **BattleManager.cs** - Main battle controller with victory/defeat conditions

### Progression System
- **Equipment.cs** - Equipment ScriptableObject with stat bonuses
- **CharacterProgression.cs** - Character leveling, skill unlocks, and equipment
- **EquipmentUpgradeSystem.cs** - Equipment upgrade system using materials

### Inventory System
- **Item.cs** - Base item ScriptableObject (consumables, key items, etc.)
- **InventoryManager.cs** - Central inventory management with item stacking

### Party System
- **PartyManager.cs** - Party management (4 active, unlimited reserves) and gold

### Quest System
- **Quest.cs** - Quest ScriptableObject with objectives and rewards
- **QuestManager.cs** - Quest tracking, objective updates, and completion

### Dialogue System
- **DialogueData.cs** - Dialogue tree ScriptableObject with branching
- **DialogueManager.cs** - Dialogue execution with variables and conditions
- **NPCInteraction.cs** - NPC interaction trigger component

### Cutscene System
- **CutsceneData.cs** - Cutscene ScriptableObject with Timeline support
- **CutsceneManager.cs** - Cutscene playback and control

### World Navigator
- **WorldNavigator.cs** - Scene transitions, random encounters, and teleportation
- **LocationData.cs** - Location ScriptableObject with encounter data

### Economy System
- **ShopManager.cs** - Buy/sell system with price calculations
- **ShopData.cs** - Shop inventory ScriptableObject

### Save/Load System
- **SaveData.cs** - Complete save data structure
- **SaveManager.cs** - Save/Load with auto-save functionality (5-minute intervals)

### Menu Systems
- **MainMenu.cs** - Main menu controller
- **InGameMenu.cs** - In-game pause menu with sub-menus
- **SaveLoadMenu.cs** - Save/load slot UI

### Player Controller
- **PlayerController.cs** - Third-person character movement with CharacterController
- **CameraController.cs** - Cinemachine 3 camera control with zoom and rotation

## 🎮 Key Features

### Battle System
✅ Turn-based combat with speed-based turn order
✅ Shield Bar system (takes damage first, affected by elements)
✅ Break State (1 turn stun when shield depleted)
✅ HP damage (10% bonus in break state, not affected by elements)
✅ Boss multi-phase system with HP/MP/SB reset
✅ Element effectiveness system (Fire, Water, Earth, Wind, etc.)
✅ Enemy AI with multiple behavior patterns
✅ Victory/Defeat conditions with reward distribution

### Progression System
✅ Experience and leveling with customizable stat growth
✅ Skill unlocks by level
✅ Equipment system (Weapon, Armor, 4 Accessories)
✅ Equipment upgrades using materials from overworld/drops

### Inventory System
✅ Items: Healing, Buffs, Debuffs, Consumables, Key Items
✅ Equipment management with stat bonuses
✅ Material collection for upgrades
✅ Item stacking with max stack sizes

### Party System
✅ 4 active party members
✅ Unlimited reserve characters
✅ Party management and swapping
✅ Gold/currency system

### Quest System
✅ Main quests and side quests
✅ Quest objectives with tracking
✅ NPC interactions and triggers
✅ Event flags and conditions
✅ Rewards: EXP, Gold, Items

### Dialogue System
✅ Branching dialogue with choices
✅ Variables and conditions
✅ Event triggers (start quest, give item, etc.)
✅ NPC interaction system

### World System
✅ World map and overworld
✅ Town and dungeon transitions
✅ Random encounters (step-based)
✅ Visible enemy encounters
✅ Teleportation system

### Save/Load System
✅ Multiple save slots (10 slots)
✅ Auto-save every 5 minutes
✅ Complete game state saving
✅ Save file management

### Player Controls
✅ Third-person character controller
✅ Cinemachine 3 camera support
✅ Camera rotation and zoom
✅ Sprint functionality

## 🔧 Setup Instructions

### 1. Import Required Packages
Install these packages via Unity Package Manager:
- **Cinemachine** (version 3.x) - For camera control
- **TextMeshPro** (included with Unity) - For UI text

### 2. Add Scripts to Unity
1. Copy all script folders to your Unity project's `Assets/Scripts/` directory
2. Wait for Unity to compile the scripts

### 3. Create Manager GameObjects
Create empty GameObjects in your scene with these scripts:
- PartyManager
- InventoryManager
- QuestManager
- DialogueManager
- SaveManager
- BattleManager (in battle scenes)
- WorldNavigator
- ShopManager
- CutsceneManager
- InGameMenu

### 4. Setup Player
1. Create a player GameObject with:
   - CharacterController component
   - PlayerController script
   - Animator (optional)
2. Tag it as "Player"

### 5. Setup Camera
1. Create a Cinemachine Camera (GameObject > Cinemachine > Camera)
2. Add CameraController script
3. Configure the OrbitalFollow component
4. Set the player as Follow and LookAt targets

### 6. Create ScriptableObjects
Create your game data using the ScriptableObject menus:
- Skills: Assets > Create > JRPG > Battle > Skill
- Equipment: Assets > Create > JRPG > Progression > Equipment
- Items: Assets > Create > JRPG > Inventory > Item
- Quests: Assets > Create > JRPG > Quest > Quest
- Dialogues: Assets > Create > JRPG > Dialogue > Dialogue
- Locations: Assets > Create > JRPG > World > Location
- Shops: Assets > Create > JRPG > Economy > Shop

## 📋 Important Notes

### Shield Bar Mechanics
- Shield Bar (SB) must be less than HP
- SB takes damage first and is affected by elemental effectiveness
- When SB is depleted, combatant enters Break State (stunned for 1 turn)
- HP damage occurs after SB is depleted and receives 10% bonus damage
- HP damage is NOT affected by elements
- SB can be healed with skills/items, but NOT HP during battle
- HP resets after battle, SB does not

### Boss Phase System
- Bosses can have multiple phases
- HP, MP, and SB reset when entering a new phase
- Each phase can have unique stats and skills
- Phase transitions triggered by HP thresholds

### Save System
- Auto-save occurs every 5 minutes
- 10 save slots available
- Saves include complete game state (party, inventory, quests, flags)
- Save files stored in Application.persistentDataPath

### Cinemachine 3 Compatibility
- Uses CinemachineCamera (not Virtual Camera from Cinemachine 2)
- Uses CinemachineOrbitalFollow for third-person camera
- Camera rotation controlled via script

## 🎯 Combat Flow
1. Battle starts → Initialize turn order based on Speed stats
2. Each turn:
   - Character selects action (Skill/Item/Defend/Flee)
   - Damage calculated with elemental effectiveness (if targeting SB)
   - Shield Bar depleted → Break State → HP damage with bonus
   - Status effects processed
3. Victory/Defeat conditions checked
4. Rewards distributed (EXP, Gold, Items)
5. HP restored after battle

## 🔄 Quest Flow
1. Player talks to NPC → Quest offered
2. Quest started → Objectives tracked
3. Objectives completed (kill enemies, collect items, talk to NPCs)
4. Quest completed → Rewards given
5. Quest logged in completed quests

## 💾 Namespace Structure
```
JRPGGame.Core - Core systems and data
JRPGGame.BattleSystem - Battle mechanics
JRPGGame.Progression - Character progression
JRPGGame.Inventory - Inventory and items
JRPGGame.Party - Party management
JRPGGame.Quest - Quest system
JRPGGame.Dialogue - Dialogue system
JRPGGame.Cutscene - Cutscene system
JRPGGame.WorldNavigator - World navigation
JRPGGame.Economy - Shop and trading
JRPGGame.SaveSystem - Save/Load
JRPGGame.Menu - Menu UI
JRPGGame.PlayerController - Player controls
```

## ⚠️ Unity 6 Compatibility
All scripts are compatible with Unity 6:
- Use Unity.Cinemachine (Cinemachine 3)
- Standard MonoBehaviour patterns
- No deprecated APIs
- Proper namespace organization
- Each MonoBehaviour in separate file

## 🚀 Getting Started
1. Set up all Manager GameObjects in your main scene
2. Create your first character, skill, and equipment ScriptableObjects
3. Set up the player with PlayerController and camera with Cinemachine
4. Create a test battle scene with BattleManager
5. Test the complete flow: Overworld → Battle → Victory → Rewards

## 📝 License
These scripts are provided as-is for your JRPG project.

---
**Compatible with Unity 6 and Cinemachine 3**
