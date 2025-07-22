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

        // Listen for upgrade events
        UpgradeManager.Instance.OnUpgradePurchased += UpdateUpgradeStatus;

        // Initialize UI
        UpdateAllUpgradeStatuses();
        
        // Hide panel initially
        upgradePanel.SetActive(false);
    }

    public void ToggleUpgradePanel()
    {
        AudioManager.Instance.Play("Button");
        upgradePanel.SetActive(!upgradePanel.activeSelf);
        UpdateAllUpgradeStatuses();
    }

    private void UpdateAllUpgradeStatuses()
    {
        UpdateStatusText(reinforcedTanksStatus, UpgradeType.ReinforcedTanks);
        UpdateStatusText(automatedRefineryStatus, UpgradeType.AutomatedRefinery);
        UpdateStatusText(refinerySpeedStatus, UpgradeType.RefinerySpeedUpgrade);
        UpdateStatusText(advancedSurveyStatus, UpgradeType.AdvancedSurveyDrones);
        UpdateStatusText(multiRigDrillingStatus, UpgradeType.MultiRigDrilling);
    }

    private void UpdateStatusText(TMP_Text statusText, UpgradeType upgradeType)
    {
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

        // Skip if already purchased
        if (UpgradeManager.Instance.HasUpgrade(upgradeType))
            return;

        UpgradeManager.Instance.SetUpgradeStatus(upgradeType, true);

        Debug.Log($"Purchased upgrade: {upgradeType}");
    }
}