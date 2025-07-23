using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

public class IAPListener : MonoBehaviour
{
    // Dictionary mapping product IDs to upgrade types
    private Dictionary<string, UpgradeType> productToUpgradeMap;
    
    private void Awake()
    {
        // Initialize the product ID to upgrade type mapping
        productToUpgradeMap = new Dictionary<string, UpgradeType>
        {
            { "com.test.iap.tanksreinforcement", UpgradeType.ReinforcedTanks },
            { "com.test.iap.automatedrefinery", UpgradeType.AutomatedRefinery },
            { "com.test.iap.multirigdrilling", UpgradeType.MultiRigDrilling },
            { "com.test.iap.doublecapactiyrefinery", UpgradeType.RefinerySpeedUpgrade },
            { "com.test.iap.advancedsurvey", UpgradeType.AdvancedSurveyDrones }
        };
    }
    
    // Called when a purchase completes
    public void OnPurchaseComplete(Product product)
    {
        if (product != null)
        {
            string productId = product.definition.id;
            Debug.Log($"Purchase complete for product: {productId}");
            
            // Check if the product ID is in our mapping
            if (productToUpgradeMap.TryGetValue(productId, out UpgradeType upgradeType))
            {
                // Update the upgrade status in our UpgradeManager
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.SetUpgradeStatus(upgradeType, true);
                    Debug.Log($"Upgrade {upgradeType} activated");
                    
                    // Save the upgrade to the cloud
                    SaveUpgradeToCloud();
                }
            }
            else
            {
                Debug.LogWarning($"Product ID {productId} not found in mapping");
            }
        }
    }
    
    // Called when a purchase fails
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogError($"Purchase failed for product {product.definition.id}: {failureReason}");
    }
    
    // Save upgrades to the cloud
    private async void SaveUpgradeToCloud()
    {
        if (CloudSaveManager.Instance != null)
        {
            await CloudSaveManager.Instance.SaveUpgrades();
        }
        else
        {
            Debug.LogWarning("CloudSaveManager not found, could not save upgrades to cloud");
        }
    }
}