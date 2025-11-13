using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JRPGGame.Core;
using JRPGGame.Progression;

namespace JRPGGame.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        private static InventoryManager instance;
        public static InventoryManager Instance => instance;

        [Header("Inventory")]
        [SerializeField] private List<InventorySlot> consumableItems = new List<InventorySlot>();
        [SerializeField] private List<InventorySlot> keyItems = new List<InventorySlot>();
        [SerializeField] private List<InventorySlot> equipmentItems = new List<InventorySlot>();
        [SerializeField] private List<InventorySlot> materials = new List<InventorySlot>();

        [Header("Capacity")]
        [SerializeField] private int maxConsumableSlots = 100;
        [SerializeField] private int maxEquipmentSlots = 200;
        [SerializeField] private int maxMaterialSlots = 200;

        // Events
        public event Action<Item, int> OnItemAdded;
        public event Action<Item, int> OnItemRemoved;
        public event Action OnInventoryChanged;

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

        public bool AddItem(Item item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            List<InventorySlot> targetList = GetInventoryList(item.itemType);
            int maxSlots = GetMaxSlots(item.itemType);

            // Check if item already exists (stackable)
            var existingSlot = targetList.FirstOrDefault(slot => slot.item == item);

            if (existingSlot != null)
            {
                // Stack with existing item
                int spaceAvailable = item.maxStackSize - existingSlot.quantity;
                int amountToAdd = Mathf.Min(quantity, spaceAvailable);

                existingSlot.quantity += amountToAdd;
                quantity -= amountToAdd;

                OnItemAdded?.Invoke(item, amountToAdd);
            }

            // Add remaining quantity to new slots
            while (quantity > 0)
            {
                if (targetList.Count >= maxSlots)
                {
                    Debug.LogWarning($"Inventory full! Cannot add more {item.itemName}");
                    return false;
                }

                int amountToAdd = Mathf.Min(quantity, item.maxStackSize);
                targetList.Add(new InventorySlot
                {
                    item = item,
                    quantity = amountToAdd
                });

                quantity -= amountToAdd;
                OnItemAdded?.Invoke(item, amountToAdd);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveItem(Item item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            List<InventorySlot> targetList = GetInventoryList(item.itemType);
            int totalAmount = GetItemCount(item);

            if (totalAmount < quantity)
            {
                Debug.LogWarning($"Not enough {item.itemName} to remove!");
                return false;
            }

            int remainingToRemove = quantity;

            for (int i = targetList.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                if (targetList[i].item == item)
                {
                    int amountToRemove = Mathf.Min(remainingToRemove, targetList[i].quantity);
                    targetList[i].quantity -= amountToRemove;
                    remainingToRemove -= amountToRemove;

                    if (targetList[i].quantity <= 0)
                    {
                        targetList.RemoveAt(i);
                    }
                }
            }

            OnItemRemoved?.Invoke(item, quantity);
            OnInventoryChanged?.Invoke();
            return true;
        }

        public int GetItemCount(Item item)
        {
            if (item == null) return 0;

            List<InventorySlot> targetList = GetInventoryList(item.itemType);
            return targetList.Where(slot => slot.item == item).Sum(slot => slot.quantity);
        }

        public bool HasItem(Item item, int quantity = 1)
        {
            return GetItemCount(item) >= quantity;
        }

        public List<InventorySlot> GetItems(ItemType itemType)
        {
            return new List<InventorySlot>(GetInventoryList(itemType));
        }

        public List<InventorySlot> GetAllItems()
        {
            List<InventorySlot> allItems = new List<InventorySlot>();
            allItems.AddRange(consumableItems);
            allItems.AddRange(keyItems);
            allItems.AddRange(equipmentItems);
            allItems.AddRange(materials);
            return allItems;
        }

        // Equipment specific methods
        public bool AddEquipment(Equipment equipment)
        {
            // Equipment is stored as items
            if (equipmentItems.Count >= maxEquipmentSlots)
            {
                Debug.LogWarning("Equipment inventory is full!");
                return false;
            }

            equipmentItems.Add(new InventorySlot
            {
                equipment = equipment,
                quantity = 1
            });

            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool RemoveEquipment(Equipment equipment)
        {
            var slot = equipmentItems.FirstOrDefault(s => s.equipment == equipment);
            if (slot != null)
            {
                equipmentItems.Remove(slot);
                OnInventoryChanged?.Invoke();
                return true;
            }

            return false;
        }

        public List<Equipment> GetAllEquipment()
        {
            return equipmentItems.Where(s => s.equipment != null).Select(s => s.equipment).ToList();
        }

        public List<Equipment> GetEquipmentBySlot(EquipmentSlot slot)
        {
            return equipmentItems
                .Where(s => s.equipment != null && s.equipment.equipmentSlot == slot)
                .Select(s => s.equipment)
                .ToList();
        }

        // Material specific methods
        public int GetMaterialCount(Material material)
        {
            return materials.Where(s => s.material == material).Sum(s => s.quantity);
        }

        public bool RemoveMaterial(Material material, int quantity)
        {
            int totalAmount = GetMaterialCount(material);
            if (totalAmount < quantity) return false;

            int remainingToRemove = quantity;

            for (int i = materials.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                if (materials[i].material == material)
                {
                    int amountToRemove = Mathf.Min(remainingToRemove, materials[i].quantity);
                    materials[i].quantity -= amountToRemove;
                    remainingToRemove -= amountToRemove;

                    if (materials[i].quantity <= 0)
                    {
                        materials.RemoveAt(i);
                    }
                }
            }

            OnInventoryChanged?.Invoke();
            return true;
        }

        // Helper methods
        private List<InventorySlot> GetInventoryList(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Consumable:
                    return consumableItems;
                case ItemType.KeyItem:
                    return keyItems;
                case ItemType.Equipment:
                    return equipmentItems;
                case ItemType.Material:
                    return materials;
                default:
                    return consumableItems;
            }
        }

        private int GetMaxSlots(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Consumable:
                case ItemType.KeyItem:
                case ItemType.QuestItem:
                    return maxConsumableSlots;
                case ItemType.Equipment:
                    return maxEquipmentSlots;
                case ItemType.Material:
                    return maxMaterialSlots;
                default:
                    return maxConsumableSlots;
            }
        }

        public void SortInventory(ItemType itemType)
        {
            List<InventorySlot> targetList = GetInventoryList(itemType);
            targetList.Sort((a, b) => string.Compare(a.GetName(), b.GetName()));
            OnInventoryChanged?.Invoke();
        }

        public void ClearInventory()
        {
            consumableItems.Clear();
            keyItems.Clear();
            equipmentItems.Clear();
            materials.Clear();
            OnInventoryChanged?.Invoke();
        }
    }

    [Serializable]
    public class InventorySlot
    {
        public Item item;
        public Equipment equipment;
        public Material material;
        public int quantity;

        public string GetName()
        {
            if (item != null) return item.itemName;
            if (equipment != null) return equipment.equipmentName;
            if (material != null) return material.materialName;
            return "Unknown";
        }
    }
}
