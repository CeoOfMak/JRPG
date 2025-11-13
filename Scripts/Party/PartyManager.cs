using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JRPGGame.Core;
using JRPGGame.Progression;

namespace JRPGGame.Party
{
    public class PartyManager : MonoBehaviour
    {
        private static PartyManager instance;
        public static PartyManager Instance => instance;

        [Header("Party")]
        [SerializeField] private List<CharacterProgression> activeParty = new List<CharacterProgression>();
        [SerializeField] private List<CharacterProgression> reserves = new List<CharacterProgression>();

        [Header("Currency")]
        [SerializeField] private int gold = 0;

        [Header("Party Limits")]
        private const int MAX_ACTIVE_PARTY_SIZE = GameConstants.MAX_PLAYER_PARTY_SIZE;

        // Events
        public event Action<CharacterProgression> OnCharacterAdded;
        public event Action<CharacterProgression> OnCharacterRemoved;
        public event Action OnPartyChanged;
        public event Action<int> OnGoldChanged;

        public List<CharacterProgression> ActiveParty => activeParty;
        public List<CharacterProgression> Reserves => reserves;
        public int Gold => gold;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Party Management
        public bool AddCharacterToParty(CharacterProgression character)
        {
            if (character == null) return false;

            // Check if character already exists
            if (activeParty.Contains(character) || reserves.Contains(character))
            {
                Debug.LogWarning($"{character.characterName} is already in the party!");
                return false;
            }

            // Add to active party if there's space
            if (activeParty.Count < MAX_ACTIVE_PARTY_SIZE)
            {
                activeParty.Add(character);
                character.Initialize();
                OnCharacterAdded?.Invoke(character);
                OnPartyChanged?.Invoke();
                Debug.Log($"{character.characterName} joined the active party!");
                return true;
            }
            else
            {
                // Add to reserves
                reserves.Add(character);
                character.Initialize();
                OnCharacterAdded?.Invoke(character);
                OnPartyChanged?.Invoke();
                Debug.Log($"{character.characterName} joined the reserves!");
                return true;
            }
        }

        public bool RemoveCharacterFromParty(CharacterProgression character)
        {
            if (character == null) return false;

            bool removed = false;

            if (activeParty.Contains(character))
            {
                activeParty.Remove(character);
                removed = true;
            }
            else if (reserves.Contains(character))
            {
                reserves.Remove(character);
                removed = true;
            }

            if (removed)
            {
                OnCharacterRemoved?.Invoke(character);
                OnPartyChanged?.Invoke();
                Debug.Log($"{character.characterName} left the party!");
            }

            return removed;
        }

        public bool SwapToActiveParty(CharacterProgression character)
        {
            if (!reserves.Contains(character))
            {
                Debug.LogWarning($"{character.characterName} is not in reserves!");
                return false;
            }

            if (activeParty.Count >= MAX_ACTIVE_PARTY_SIZE)
            {
                Debug.LogWarning("Active party is full! Remove a character first.");
                return false;
            }

            reserves.Remove(character);
            activeParty.Add(character);
            OnPartyChanged?.Invoke();
            Debug.Log($"{character.characterName} moved to active party!");
            return true;
        }

        public bool SwapToReserves(CharacterProgression character)
        {
            if (!activeParty.Contains(character))
            {
                Debug.LogWarning($"{character.characterName} is not in active party!");
                return false;
            }

            if (activeParty.Count <= 1)
            {
                Debug.LogWarning("Cannot move the last active party member to reserves!");
                return false;
            }

            activeParty.Remove(character);
            reserves.Add(character);
            OnPartyChanged?.Invoke();
            Debug.Log($"{character.characterName} moved to reserves!");
            return true;
        }

        public bool SwapPartyMembers(CharacterProgression char1, CharacterProgression char2)
        {
            bool char1InActive = activeParty.Contains(char1);
            bool char2InActive = activeParty.Contains(char2);
            bool char1InReserves = reserves.Contains(char1);
            bool char2InReserves = reserves.Contains(char2);

            if (char1InActive && char2InReserves)
            {
                int index1 = activeParty.IndexOf(char1);
                activeParty.RemoveAt(index1);
                reserves.Remove(char2);

                activeParty.Insert(index1, char2);
                reserves.Add(char1);

                OnPartyChanged?.Invoke();
                Debug.Log($"Swapped {char1.characterName} and {char2.characterName}!");
                return true;
            }
            else if (char1InReserves && char2InActive)
            {
                return SwapPartyMembers(char2, char1);
            }
            else if (char1InActive && char2InActive)
            {
                // Swap positions in active party
                int index1 = activeParty.IndexOf(char1);
                int index2 = activeParty.IndexOf(char2);

                activeParty[index1] = char2;
                activeParty[index2] = char1;

                OnPartyChanged?.Invoke();
                return true;
            }

            return false;
        }

        public CharacterProgression GetCharacterByName(string name)
        {
            var character = activeParty.FirstOrDefault(c => c.characterName == name);
            if (character != null) return character;

            return reserves.FirstOrDefault(c => c.characterName == name);
        }

        public List<CharacterProgression> GetAllCharacters()
        {
            List<CharacterProgression> all = new List<CharacterProgression>();
            all.AddRange(activeParty);
            all.AddRange(reserves);
            return all;
        }

        public bool IsPartyFull()
        {
            return activeParty.Count >= MAX_ACTIVE_PARTY_SIZE;
        }

        // Gold Management
        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            gold += amount;
            OnGoldChanged?.Invoke(gold);
            Debug.Log($"Gained {amount} gold! Total: {gold}");
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0) return false;

            if (gold < amount)
            {
                Debug.LogWarning($"Not enough gold! Need {amount}, have {gold}");
                return false;
            }

            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            Debug.Log($"Spent {amount} gold! Remaining: {gold}");
            return true;
        }

        public bool HasGold(int amount)
        {
            return gold >= amount;
        }

        // Party Stats
        public int GetAverageLevel()
        {
            if (activeParty.Count == 0) return 1;
            return Mathf.RoundToInt(activeParty.Average(c => c.stats.Level));
        }

        public int GetTotalPartyHP()
        {
            return activeParty.Sum(c => c.stats.CurrentHP);
        }

        public int GetTotalPartyMaxHP()
        {
            return activeParty.Sum(c => c.stats.MaxHP);
        }

        public bool IsPartyAlive()
        {
            return activeParty.Any(c => c.stats.IsAlive);
        }

        public void HealParty(bool fullHeal = false)
        {
            foreach (var character in activeParty)
            {
                if (fullHeal)
                {
                    character.stats.CurrentHP = character.stats.MaxHP;
                    character.stats.CurrentMP = character.stats.MaxMP;
                    character.stats.CurrentShieldBar = character.stats.MaxShieldBar;
                }
                else
                {
                    character.stats.CurrentHP = Mathf.Min(
                        character.stats.CurrentHP + Mathf.RoundToInt(character.stats.MaxHP * 0.5f),
                        character.stats.MaxHP);
                    character.stats.CurrentMP = Mathf.Min(
                        character.stats.CurrentMP + Mathf.RoundToInt(character.stats.MaxMP * 0.5f),
                        character.stats.MaxMP);
                }
            }

            Debug.Log(fullHeal ? "Party fully healed!" : "Party partially healed!");
        }
    }
}
