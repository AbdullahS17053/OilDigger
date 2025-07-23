using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }
    
    private const string UPGRADES_KEY = "player_upgrades";
    
    // Event fired when upgrades are loaded from the cloud
    public event Action OnUpgradesLoaded;
    
    private bool isInitialized = false;
    private bool isInitializing = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private async void Start()
    {
        await InitializeCloudServices();
        await LoadUpgrades();
    }
    
    /// <summary>
    /// Initialize Unity Cloud Services including Authentication
    /// </summary>
    public async Task<bool> InitializeCloudServices()
    {
        if (isInitialized)
            return true;
            
        if (isInitializing)
        {
            // Wait for initialization to complete
            while (isInitializing)
                await Task.Delay(100);
            return isInitialized;
        }
        
        isInitializing = true;
        
        try
        {
            // Initialize Unity Services
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Cloud Services initialized");
            }
            
            // Sign in anonymously if not already signed in
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in anonymously with Player ID: {AuthenticationService.Instance.PlayerId}");
            }
            
            isInitialized = true;
            Debug.Log("Cloud Save initialized successfully");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Cloud Services: {e.Message}");
            return false;
        }
        finally
        {
            isInitializing = false;
        }
    }
    
    /// <summary>
    /// Save upgrade status to the cloud
    /// </summary>
    public async Task SaveUpgrades()
    {
        if (!isInitialized && !await InitializeCloudServices())
        {
            Debug.LogError("Could not save upgrades: Cloud Services not initialized");
            return;
        }
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("Could not save upgrades: User not signed in");
            return;
        }
        
        try
        {
            // Create lists to hold upgrade names and values (JsonUtility can't serialize dictionaries)
            var saveData = new UpgradeSaveData();
            saveData.upgradeNames = new List<string>();
            saveData.upgradeValues = new List<bool>();
            
            // Get status for each upgrade from UpgradeManager
            if (UpgradeManager.Instance != null)
            {
                foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
                {
                    string upgradeName = upgradeType.ToString();
                    bool isOwned = UpgradeManager.Instance.HasUpgrade(upgradeType);
                    
                    saveData.upgradeNames.Add(upgradeName);
                    saveData.upgradeValues.Add(isOwned);
                    
                    Debug.Log($"Saving upgrade: {upgradeName} = {isOwned}");
                }
            }
            
            // Convert to JSON string
            string jsonData = JsonUtility.ToJson(saveData);
            Debug.Log($"Saving JSON data: {jsonData}");
            
            // Save to Cloud Save
            var data = new Dictionary<string, object> { { UPGRADES_KEY, jsonData } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
            
            Debug.Log("Upgrade data saved to cloud successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving upgrades to cloud: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load upgrade status from the cloud
    /// </summary>
    public async Task LoadUpgrades()
    {
        if (!isInitialized && !await InitializeCloudServices())
        {
            Debug.LogError("Could not load upgrades: Cloud Services not initialized");
            return;
        }
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("Could not load upgrades: User not signed in");
            return;
        }
        
        try
        {
            // Load data from Cloud Save
            Dictionary<string, string> savedData = await CloudSaveService.Instance.Data.LoadAllAsync();
            
            if (savedData != null && savedData.TryGetValue(UPGRADES_KEY, out string jsonData))
            {
                Debug.Log($"Loaded JSON data: {jsonData}");
                
                // Parse the JSON data
                UpgradeSaveData upgradeSaveData = JsonUtility.FromJson<UpgradeSaveData>(jsonData);
                
                if (upgradeSaveData != null && 
                    upgradeSaveData.upgradeNames != null && 
                    upgradeSaveData.upgradeValues != null && 
                    upgradeSaveData.upgradeNames.Count == upgradeSaveData.upgradeValues.Count)
                {
                    // Apply loaded upgrade status to UpgradeManager
                    if (UpgradeManager.Instance != null)
                    {
                        // Convert parallel lists back to dictionary-like lookup
                        for (int i = 0; i < upgradeSaveData.upgradeNames.Count; i++)
                        {
                            string upgradeName = upgradeSaveData.upgradeNames[i];
                            bool isOwned = upgradeSaveData.upgradeValues[i];
                            
                            // Only apply if the upgrade is owned
                            if (isOwned && Enum.TryParse(upgradeName, out UpgradeType upgradeType))
                            {
                                UpgradeManager.Instance.SetUpgradeStatus(upgradeType, true);
                                Debug.Log($"Loaded upgrade from cloud: {upgradeType} = {isOwned}");
                            }
                        }
                    }
                    
                    // Notify listeners that upgrades have been loaded
                    OnUpgradesLoaded?.Invoke();
                    Debug.Log("Upgrades loaded from cloud successfully");
                }
                else
                {
                    Debug.LogWarning("Invalid upgrade data format in cloud");
                }
            }
            else
            {
                Debug.Log("No saved upgrade data found in cloud");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading upgrades from cloud: {e.Message}");
        }
    }
    
    /// <summary>
    /// Helper class for JSON serialization of upgrade data
    /// </summary>
    [Serializable]
    private class UpgradeSaveData
    {
        // Using parallel lists instead of Dictionary since JsonUtility can't serialize dictionaries
        public List<string> upgradeNames;
        public List<bool> upgradeValues;
    }
}