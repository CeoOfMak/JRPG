using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Inventory;
using JRPGGame.Progression;

namespace JRPGGame.Economy
{
    [CreateAssetMenu(fileName = "New Shop", menuName = "JRPG/Economy/Shop")]
    public class ShopData : ScriptableObject
    {
        [Header("Shop Info")]
        public string shopID;
        public string shopName;
        [TextArea(3, 5)]
        public string description;

        [Header("Shop Settings")]
        public bool buyItems = true;
        [Range(0f, 1f)]
        public float sellPriceMultiplier = 0.5f;

        [Header("Inventory")]
        public List<Item> itemsForSale = new List<Item>();
        public List<Equipment> equipmentForSale = new List<Equipment>();

        [Header("Shopkeeper")]
        public string shopkeeperName;
        public Sprite shopkeeperPortrait;

        public bool HasItem(Item item)
        {
            return itemsForSale.Contains(item);
        }

        public bool HasEquipment(Equipment equipment)
        {
            return equipmentForSale.Contains(equipment);
        }
    }
}
