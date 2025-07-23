using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Purchasing;

public class UpgradesUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject upgradePanel;
    
    [Header("IAP References")]
    [SerializeField] private IAPListener iapListener;
    [SerializeField] private CodelessIAPButton reinforcedTanksButton;
    [SerializeField] private CodelessIAPButton automatedRefineryButton;
    [SerializeField] private CodelessIAPButton refinerySpeedButton;
    [SerializeField] private CodelessIAPButton advancedSurveyButton;
    [SerializeField] private CodelessIAPButton multiRigDrillingButton;

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
        
        // Setup event handlers programmatically for each button
        SetupIAPButtonEvents(reinforcedTanksButton);
        SetupIAPButtonEvents(automatedRefineryButton);
        SetupIAPButtonEvents(refinerySpeedButton);
        SetupIAPButtonEvents(advancedSurveyButton);
        SetupIAPButtonEvents(multiRigDrillingButton);
        
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

    private void SetupIAPButtonEvents(CodelessIAPButton button)
    {
        if (button == null || iapListener == null)
            return;

        // Clear existing listeners to avoid duplicates  
        button.onPurchaseComplete.RemoveAllListeners();
        button.onPurchaseFailed.RemoveAllListeners();

        button.onPurchaseComplete.AddListener(iapListener.OnPurchaseComplete);

        button.onPurchaseFailed.AddListener((product, description) =>
        {
            iapListener.OnPurchaseFailed(product, description.reason);
        });
    }

    private void SetupDescriptions()
    {
        if (reinforcedTanksDesc != null)
            reinforcedTanksDesc.text = "Protects all tanks from damage during weather events!";
            
        if (automatedRefineryDesc != null)
            automatedRefineryDesc.text = "Automatically refines crude oil at the end of each turn!";
            
        if (refinerySpeedDesc != null)
            refinerySpeedDesc.text = "Doubles the output of refined fuel products!.";
            
        if (advancedSurveyDesc != null)
            advancedSurveyDesc.text = "Survey 3 adjacent lots at once for the price of 1!.";
            
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
        
        // Disable IAP buttons for already purchased upgrades
        UpdateIAPButtonStatus(reinforcedTanksButton, UpgradeType.ReinforcedTanks);
        UpdateIAPButtonStatus(automatedRefineryButton, UpgradeType.AutomatedRefinery);
        UpdateIAPButtonStatus(refinerySpeedButton, UpgradeType.RefinerySpeedUpgrade);
        UpdateIAPButtonStatus(advancedSurveyButton, UpgradeType.AdvancedSurveyDrones);
        UpdateIAPButtonStatus(multiRigDrillingButton, UpgradeType.MultiRigDrilling);
    }
    
    private void UpdateIAPButtonStatus(CodelessIAPButton button, UpgradeType upgradeType)
    {
        if (button == null || UpgradeManager.Instance == null)
            return;
            
        // Disable the button if the upgrade is already owned
        button.button.interactable = !UpgradeManager.Instance.HasUpgrade(upgradeType);
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
    
    private void OnDestroy()
    {
        // Clean up event listener
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradePurchased -= UpdateUpgradeStatus;
    }
    
    // For editor testing only - directly simulate purchases
    public void EditorBuyUpgrade(int upgradeTypeIndex)
    {
        if (Application.isEditor && UpgradeManager.Instance != null)
        {
            UpgradeType upgradeType = (UpgradeType)upgradeTypeIndex;
            UpgradeManager.Instance.SetUpgradeStatus(upgradeType, true);
            Debug.Log($"DEBUG: Editor purchase of {upgradeType}");
        }
    }
}