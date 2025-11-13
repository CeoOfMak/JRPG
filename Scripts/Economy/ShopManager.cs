using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using JRPGGame.Inventory;
using JRPGGame.Party;
using JRPGGame.Progression;

namespace JRPGGame.Economy
{
    public class ShopManager : MonoBehaviour
    {
        private static ShopManager instance;
        public static ShopManager Instance => instance;

        [Header("Current Shop")]
        [SerializeField] private ShopData currentShop;
        [SerializeField] private bool isShopOpen;

        [Header("Events")]
        public UnityEvent<ShopData> OnShopOpened;
        public UnityEvent OnShopClosed;
        public UnityEvent<Item, int> OnItemPurchased;
        public UnityEvent<Item, int> OnItemSold;

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

        public void OpenShop(ShopData shop)
        {
            if (shop == null)
            {
                Debug.LogWarning("Cannot open null shop!");
                return;
            }

            currentShop = shop;
            isShopOpen = true;

            OnShopOpened?.Invoke(shop);
            Debug.Log($"Opened shop: {shop.shopName}");
        }

        public void CloseShop()
        {
            isShopOpen = false;
            OnShopClosed?.Invoke();
            currentShop = null;
        }

        public bool BuyItem(Item item, int quantity = 1)
        {
            if (!isShopOpen || currentShop == null)
            {
                Debug.LogWarning("No shop is open!");
                return false;
            }

            if (!currentShop.HasItem(item))
            {
                Debug.LogWarning($"Shop doesn't sell {item.itemName}!");
                return false;
            }

            int totalCost = item.buyPrice * quantity;

            // Check if player has enough gold
            if (PartyManager.Instance == null || !PartyManager.Instance.HasGold(totalCost))
            {
                Debug.LogWarning($"Not enough gold! Need {totalCost}, have {PartyManager.Instance?.Gold ?? 0}");
                return false;
            }

            // Check if inventory has space
            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("Inventory manager not found!");
                return false;
            }

            // Process purchase
            PartyManager.Instance.SpendGold(totalCost);
            InventoryManager.Instance.AddItem(item, quantity);

            OnItemPurchased?.Invoke(item, quantity);
            Debug.Log($"Purchased {item.itemName} x{quantity} for {totalCost} gold!");

            return true;
        }

        public bool BuyEquipment(Equipment equipment)
        {
            if (!isShopOpen || currentShop == null)
            {
                Debug.LogWarning("No shop is open!");
                return false;
            }

            if (!currentShop.HasEquipment(equipment))
            {
                Debug.LogWarning($"Shop doesn't sell {equipment.equipmentName}!");
                return false;
            }

            int totalCost = equipment.buyPrice;

            if (PartyManager.Instance == null || !PartyManager.Instance.HasGold(totalCost))
            {
                Debug.LogWarning($"Not enough gold!");
                return false;
            }

            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("Inventory manager not found!");
                return false;
            }

            // Process purchase
            PartyManager.Instance.SpendGold(totalCost);
            InventoryManager.Instance.AddEquipment(equipment);

            Debug.Log($"Purchased {equipment.equipmentName} for {totalCost} gold!");

            return true;
        }

        public bool SellItem(Item item, int quantity = 1)
        {
            if (!isShopOpen || currentShop == null)
            {
                Debug.LogWarning("No shop is open!");
                return false;
            }

            if (!currentShop.buyItems)
            {
                Debug.LogWarning("This shop doesn't buy items!");
                return false;
            }

            if (InventoryManager.Instance == null || !InventoryManager.Instance.HasItem(item, quantity))
            {
                Debug.LogWarning($"Don't have enough {item.itemName} to sell!");
                return false;
            }

            int totalValue = item.sellPrice * quantity;

            // Process sale
            InventoryManager.Instance.RemoveItem(item, quantity);
            if (PartyManager.Instance != null)
            {
                PartyManager.Instance.AddGold(totalValue);
            }

            OnItemSold?.Invoke(item, quantity);
            Debug.Log($"Sold {item.itemName} x{quantity} for {totalValue} gold!");

            return true;
        }

        public bool SellEquipment(Equipment equipment)
        {
            if (!isShopOpen || currentShop == null)
            {
                Debug.LogWarning("No shop is open!");
                return false;
            }

            if (!currentShop.buyItems)
            {
                Debug.LogWarning("This shop doesn't buy items!");
                return false;
            }

            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("Inventory manager not found!");
                return false;
            }

            int totalValue = equipment.sellPrice;

            // Process sale
            InventoryManager.Instance.RemoveEquipment(equipment);
            if (PartyManager.Instance != null)
            {
                PartyManager.Instance.AddGold(totalValue);
            }

            Debug.Log($"Sold {equipment.equipmentName} for {totalValue} gold!");

            return true;
        }

        public ShopData GetCurrentShop()
        {
            return currentShop;
        }

        public bool IsShopOpen()
        {
            return isShopOpen;
        }
    }
}
