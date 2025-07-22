using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject upgradePanel;
    
    [Header("Upgrade Buttons")]
    [SerializeField] private Button reinforcedTanksButton;
    [SerializeField] private Button automatedRefineryButton;
    [SerializeField] private Button refinerySpeedButton;
    [SerializeField] private Button advancedSurveyButton;
    [SerializeField] private Button multiRigDrillingButton;

    [Header("Upgrade Status Text")]
    [SerializeField] private TMP_Text reinforcedTanksStatus;
    [SerializeField] private TMP_Text automatedRefineryStatus;
    [SerializeField] private TMP_Text refinerySpeedStatus;
    [SerializeField] private TMP_Text advancedSurveyStatus;
    [SerializeField] private TMP_Text multiRigDrillingStatus;

    [Header("Prices")]
    [SerializeField] private TMP_Text reinforcedTanksPrice;
    [SerializeField] private TMP_Text automatedRefineryPrice;
    [SerializeField] private TMP_Text refinerySpeedPrice;
    [SerializeField] private TMP_Text advancedSurveyPrice;
    [SerializeField] private TMP_Text multiRigDrillingPrice;
    
    [Header("Descriptions")]
    [SerializeField] private TMP_Text reinforcedTanksDesc;
    [SerializeField] private TMP_Text automatedRefineryDesc;
    [SerializeField] private TMP_Text refinerySpeedDesc;
    [SerializeField] private TMP_Text advancedSurveyDesc;
    [SerializeField] private TMP_Text multiRigDrillingDesc;

    private void Start()
    {
        // Setup button listeners
        reinforcedTanksButton.onClick.AddListener(() => PurchaseUpgrade(UpgradeType.ReinforcedTanks));
        automatedRefineryButton.onClick.AddListener(() => PurchaseUpgrade(UpgradeType.AutomatedRefinery));
        refinerySpeedButton.onClick.AddListener(() => PurchaseUpgrade(UpgradeType.RefinerySpeedUpgrade));
        advancedSurveyButton.onClick.AddListener(() => PurchaseUpgrade(UpgradeType.AdvancedSurveyDrones));
        multiRigDrillingButton.onClick.AddListener(() => PurchaseUpgrade(UpgradeType.MultiRigDrilling));

        // Setup price texts
        reinforcedTanksPrice.text = "$7.99";
        automatedRefineryPrice.text = "$4.99";
        refinerySpeedPrice.text = "$3.99";
        advancedSurveyPrice.text = "$2.99";
        multiRigDrillingPrice.text = "$5.99";
        
        // Setup detailed descriptions
        SetupDescriptions();

        // Listen for upgrade events
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradePurchased += UpdateUpgradeStatus;

        // Initialize UI
        UpdateAllUpgradeStatuses();
        
        // Hide panel initially
        upgradePanel.SetActive(false);
    }

    private void SetupDescriptions()
    {
        // Set detailed descriptions for each upgrade
        if (reinforcedTanksDesc != null)
            reinforcedTanksDesc.text = "Protects all tanks from damage during weather events!";
            
        if (automatedRefineryDesc != null)
            automatedRefineryDesc.text = "Automatically refines crude oil at the end of each turn!";
            
        if (refinerySpeedDesc != null)
            refinerySpeedDesc.text = "Doubles the output of refined fuel products!";
            
        if (advancedSurveyDesc != null)
            advancedSurveyDesc.text = "Survey 3 adjacent lots at once for the price of 1!";
            
        if (multiRigDrillingDesc != null)
            multiRigDrillingDesc.text = "Drill on 3 nearby lots simultaneously when drilling a single lot. Each additional lot still costs $250,000.";
    }

    public void ToggleUpgradePanel()
    {
        AudioManager.Instance.Play("Button");
        upgradePanel.SetActive(!upgradePanel.activeSelf);
        UpdateAllUpgradeStatuses();
    }

    private void UpdateAllUpgradeStatuses()
    {
        if (UpgradeManager.Instance == null)
            return;
            
        UpdateStatusText(reinforcedTanksStatus, UpgradeType.ReinforcedTanks);
        UpdateStatusText(automatedRefineryStatus, UpgradeType.AutomatedRefinery);
        UpdateStatusText(refinerySpeedStatus, UpgradeType.RefinerySpeedUpgrade);
        UpdateStatusText(advancedSurveyStatus, UpgradeType.AdvancedSurveyDrones);
        UpdateStatusText(multiRigDrillingStatus, UpgradeType.MultiRigDrilling);
    }

    private void UpdateStatusText(TMP_Text statusText, UpgradeType upgradeType)
    {
        if (statusText == null || UpgradeManager.Instance == null)
            return;
            
        bool hasUpgrade = UpgradeManager.Instance.HasUpgrade(upgradeType);
        statusText.text = hasUpgrade ? "OWNED" : "BUY";
        statusText.color = hasUpgrade ? Color.green : Color.white;
    }

    private void UpdateUpgradeStatus(UpgradeType upgradeType)
    {
        UpdateAllUpgradeStatuses();
    }

    private void PurchaseUpgrade(UpgradeType upgradeType)
    {
        AudioManager.Instance.Play("Button");

        if (UpgradeManager.Instance == null)
            return;

        // Skip if already purchased
        if (UpgradeManager.Instance.HasUpgrade(upgradeType))
            return;
            
        // For testing purposes in Editor, just enable the upgrade
        // Later, this will be replaced with IAP logic
        UpgradeManager.Instance.SetUpgradeStatus(upgradeType, true);
        
        // Show confirmation
        Debug.Log($"Purchased upgrade: {upgradeType}");
        
        // Show a confirmation popup or effect here when IAP is implemented
        ShowUpgradePurchaseConfirmation(upgradeType);
    }
    
    private void ShowUpgradePurchaseConfirmation(UpgradeType upgradeType)
    {
        // This would be expanded when implementing real IAP
        string upgradeName = "";
        switch (upgradeType)
        {
            case UpgradeType.ReinforcedTanks:
                upgradeName = "Reinforced Tanks";
                break;
            case UpgradeType.AutomatedRefinery:
                upgradeName = "Automated Refinery";
                break;
            case UpgradeType.RefinerySpeedUpgrade:
                upgradeName = "Refinery Speed Upgrade";
                break;
            case UpgradeType.AdvancedSurveyDrones:
                upgradeName = "Advanced Survey Drones";
                break;
            case UpgradeType.MultiRigDrilling:
                upgradeName = "Multi-Rig Drilling";
                break;
        }
        
        Debug.Log($"Purchased {upgradeName} upgrade! Effects are now active.");
    }
    
    private void OnDestroy()
    {
        // Clean up event listener
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradePurchased -= UpdateUpgradeStatus;
    }
}