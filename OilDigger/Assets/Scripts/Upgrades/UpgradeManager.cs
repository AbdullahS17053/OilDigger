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

    public bool HasUpgrade(UpgradeType upgradeType)
    {
        if (useDebugUpgrades)
        {
            return GetDebugUpgradeStatus(upgradeType);
        }

        // In the future, this will check cloud-saved data
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
        switch (upgradeType)
        {
            case UpgradeType.ReinforcedTanks:
                reinforcedTanks = status;
                break;
            case UpgradeType.AutomatedRefinery:
                automatedRefinery = status;
                break;
            case UpgradeType.RefinerySpeedUpgrade:
                refinerySpeedUpgrade = status;
                break;
            case UpgradeType.AdvancedSurveyDrones:
                advancedSurveyDrones = status;
                break;
            case UpgradeType.MultiRigDrilling:
                multiRigDrilling = status;
                break;
        }

        if (status)
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
}

public enum UpgradeType
{
    ReinforcedTanks,
    AutomatedRefinery,
    RefinerySpeedUpgrade,
    AdvancedSurveyDrones,
    MultiRigDrilling
}