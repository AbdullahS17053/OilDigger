using UnityEngine;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Debug Controls")]
    [Tooltip("Enable to test upgrades in the editor")]
    [SerializeField] private bool useDebugUpgrades = false;

    [Header("Available Upgrades")]
    [SerializeField] private bool reinforcedTanks = false;
    [SerializeField] private bool automatedRefinery = false;
    [SerializeField] private bool refinerySpeedUpgrade = false;
    [SerializeField] private bool advancedSurveyDrones = false;
    [SerializeField] private bool multiRigDrilling = false;

    [Header("Upgrade Settings")]
    [SerializeField] private int advancedSurveyCount = 3;
    [SerializeField] private int multiRigDrillingCount = 3;
    [SerializeField] private int refineryCapacityMultiplier = 2;

    // Events for when upgrades are purchased or activated
    public event Action<UpgradeType> OnUpgradePurchased;

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
    
    private void Start()
    {
        // Listen for cloud save events
        if (CloudSaveManager.Instance != null)
        {
            CloudSaveManager.Instance.OnUpgradesLoaded += OnUpgradesLoadedFromCloud;
        }
        else
        {
            Debug.LogWarning("CloudSaveManager not found, upgrades will not persist across devices");
        }
    }

    // Called when upgrades are loaded from the cloud
    private void OnUpgradesLoadedFromCloud()
    {
        Debug.Log("Upgrades loaded from cloud, updating UI");
        // We don't need to do anything here since the CloudSaveManager
        // already calls SetUpgradeStatus for each owned upgrade
    }

    public bool HasUpgrade(UpgradeType upgradeType)
    {
        if (useDebugUpgrades)
        {
            return GetDebugUpgradeStatus(upgradeType);
        }

        return GetDebugUpgradeStatus(upgradeType);
    }

    private bool GetDebugUpgradeStatus(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.ReinforcedTanks:
                return reinforcedTanks;
            case UpgradeType.AutomatedRefinery:
                return automatedRefinery;
            case UpgradeType.RefinerySpeedUpgrade:
                return refinerySpeedUpgrade;
            case UpgradeType.AdvancedSurveyDrones:
                return advancedSurveyDrones;
            case UpgradeType.MultiRigDrilling:
                return multiRigDrilling;
            default:
                return false;
        }
    }

    public void SetUpgradeStatus(UpgradeType upgradeType, bool status)
    {
        bool changed = false;
        
        switch (upgradeType)
        {
            case UpgradeType.ReinforcedTanks:
                if (reinforcedTanks != status)
                {
                    reinforcedTanks = status;
                    changed = true;
                }
                break;
            case UpgradeType.AutomatedRefinery:
                if (automatedRefinery != status)
                {
                    automatedRefinery = status;
                    changed = true;
                }
                break;
            case UpgradeType.RefinerySpeedUpgrade:
                if (refinerySpeedUpgrade != status)
                {
                    refinerySpeedUpgrade = status;
                    changed = true;
                }
                break;
            case UpgradeType.AdvancedSurveyDrones:
                if (advancedSurveyDrones != status)
                {
                    advancedSurveyDrones = status;
                    changed = true;
                }
                break;
            case UpgradeType.MultiRigDrilling:
                if (multiRigDrilling != status)
                {
                    multiRigDrilling = status;
                    changed = true;
                }
                break;
        }

        if (changed && status)
        {
            OnUpgradePurchased?.Invoke(upgradeType);
        }
    }

    public int GetAdvancedSurveyCount()
    {
        return HasUpgrade(UpgradeType.AdvancedSurveyDrones) ? advancedSurveyCount : 1;
    }

    public int GetMultiRigDrillingCount()
    {
        return HasUpgrade(UpgradeType.MultiRigDrilling) ? multiRigDrillingCount : 1;
    }

    public int GetRefineryCapacityMultiplier()
    {
        return HasUpgrade(UpgradeType.RefinerySpeedUpgrade) ? refineryCapacityMultiplier : 1;
    }

    public bool ShouldAutoRefine()
    {
        return HasUpgrade(UpgradeType.AutomatedRefinery);
    }

    public bool AreTanksReinforced()
    {
        return HasUpgrade(UpgradeType.ReinforcedTanks);
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (CloudSaveManager.Instance != null)
        {
            CloudSaveManager.Instance.OnUpgradesLoaded -= OnUpgradesLoadedFromCloud;
        }
    }
}

public enum UpgradeType
{
    ReinforcedTanks,
    AutomatedRefinery,
    RefinerySpeedUpgrade,
    AdvancedSurveyDrones,
    MultiRigDrilling
}