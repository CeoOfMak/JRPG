using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Inventory;

namespace JRPGGame.Progression
{
    public class EquipmentUpgradeSystem : MonoBehaviour
    {
        private static EquipmentUpgradeSystem instance;
        public static EquipmentUpgradeSystem Instance => instance;

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

        public bool CanUpgradeEquipment(Equipment equipment)
        {
            if (equipment == null) return false;
            if (!equipment.CanUpgrade()) return false;

            // Check if player has required materials
            return HasRequiredMaterials(equipment);
        }

        private bool HasRequiredMaterials(Equipment equipment)
        {
            if (InventoryManager.Instance == null) return false;

            foreach (var materialReq in equipment.upgradeMaterials)
            {
                int ownedAmount = InventoryManager.Instance.GetMaterialCount(materialReq.material);
                if (ownedAmount < materialReq.quantity)
                {
                    return false;
                }
            }

            return true;
        }

        public Equipment UpgradeEquipment(Equipment equipment)
        {
            if (!CanUpgradeEquipment(equipment))
            {
                Debug.LogWarning("Cannot upgrade equipment: requirements not met.");
                return null;
            }

            // Consume materials
            foreach (var materialReq in equipment.upgradeMaterials)
            {
                InventoryManager.Instance.RemoveMaterial(materialReq.material, materialReq.quantity);
            }

            // Create upgraded version
            Equipment upgradedEquipment = equipment.CreateUpgradedCopy();

            Debug.Log($"Successfully upgraded {equipment.equipmentName} to +{upgradedEquipment.upgradeLevel}!");

            return upgradedEquipment;
        }

        public List<UpgradeMaterial> GetMissingMaterials(Equipment equipment)
        {
            List<UpgradeMaterial> missing = new List<UpgradeMaterial>();

            if (InventoryManager.Instance == null) return missing;

            foreach (var materialReq in equipment.upgradeMaterials)
            {
                int ownedAmount = InventoryManager.Instance.GetMaterialCount(materialReq.material);
                int neededAmount = materialReq.quantity - ownedAmount;

                if (neededAmount > 0)
                {
                    missing.Add(new UpgradeMaterial
                    {
                        material = materialReq.material,
                        quantity = neededAmount
                    });
                }
            }

            return missing;
        }

        public string GetUpgradePreview(Equipment equipment)
        {
            if (!equipment.CanUpgrade())
            {
                return "Maximum upgrade level reached.";
            }

            Equipment preview = equipment.CreateUpgradedCopy();

            string previewText = $"Upgrade to +{preview.upgradeLevel}:\n";
            previewText += $"HP: {equipment.hpBonus} → {preview.hpBonus}\n";
            previewText += $"Attack: {equipment.attackBonus} → {preview.attackBonus}\n";
            previewText += $"Defense: {equipment.defenseBonus} → {preview.defenseBonus}\n";

            return previewText;
        }
    }
}
