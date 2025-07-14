using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class OpsHandler : MonoBehaviour
{
    public static OpsHandler Instance { get; private set; }

    [SerializeField] private int refineryCap = 1260;// gallons
    [SerializeField] private int refiningGCost = 4;// gasoline cost per gallon
    [SerializeField] private int refiningJFCost = 5;// jet fuel cost per gallon
    [SerializeField] private int refiningDCost = 6;// diesel cost per gallon
    [SerializeField] private GameObject refineOptionsPanel;
    [SerializeField] private GameObject refineInputPanel;
    [SerializeField] private GameObject opsPanel;


    [SerializeField] private Button surveyButton;
    [SerializeField] private Button drillButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button buyTankButton;
    [SerializeField] private Button refineButton;
    [SerializeField] private Button refineAction;

    [SerializeField] private TMP_Text refineInputTitle;
    [SerializeField] private TMP_Text refineInputMoney;
    [SerializeField] private TMP_Text refineInputAmount;
    [SerializeField] private TMP_Text surveyText;

    [SerializeField] private Slider refineInputAmountSlider;
    [SerializeField] private GameObject feedbackPrefab;
    [SerializeField] private Canvas canvas;

    private Lot currentLot;
    private int refineInputType = 0; // 0: Gasoline, 1: Jet Fuel, 2: Diesel
    private int moneyToSpend = 0;
    private int nGallonsToRefine = 0;

    public bool surveyedThisTurn = false;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {

    }

    public void Show(Lot _lot)
    {
        CloseRefineInput();
        CloseRefineOptions();
        if (GameManager.Instance.HasInteractedThisTurn) return;

        currentLot = _lot;

        // if (!currentLot)
        // {
        //     Debug.LogWarning(" N G00d");
        //     return;
        // }
        TabManager.Instance.SwitchToTab(2);
        TabManager.Instance.SetStartAxisHorizontal();

        UpdateStatus();
    }

    public void UpdateStatus()
    {
        surveyButton.interactable = !currentLot.IsSurveyed && !currentLot.IsDrilled && GameManager.Instance.Money >= 40000;
        drillButton.interactable = !currentLot.IsDrilled && GameManager.Instance.Money >= 250000;

        // if (currentLot.IsSurveyed)
        //     surveyText.text = currentLot.oilChance.ToString() + " % Chance";
        // else
        //     surveyText.text = "$ 40,000";
    }

    public void Survey()
    {
        surveyedThisTurn = true;
        CloseRefineInput();
        CloseRefineOptions();
        GameObject popup = Instantiate(feedbackPrefab, canvas.transform);

        popup.transform.position = surveyButton.transform.position;
        if (currentLot.Survey())
        {
            // GameManager.Instance.EndTurn();
            popup.GetComponent<SimpleFeedback>().Show("- $ 40,000", new Color32(255, 0, 0, 255));
        }
        else
        {
            popup.GetComponent<SimpleFeedback>().Show("ERROR", new Color32(255, 0, 0, 255));
            AudioManager.Instance.Play("Error");

        }

        UpdateStatus();

        // Hide();
    }

    public void Drill()
    {
        CloseRefineInput();
        CloseRefineOptions();
        GameObject popup = Instantiate(feedbackPrefab, canvas.transform);

        popup.transform.position = drillButton.transform.position;
        if (currentLot.Drill())
        {
            //GameManager.Instance.EndTurn();

            // Set custom text
            popup.GetComponent<SimpleFeedback>().Show("- $ 250,000", new Color32(255, 0, 0, 255));
        }
        else
        {
            popup.GetComponent<SimpleFeedback>().Show("ERROR", new Color32(255, 0, 0, 255));
            AudioManager.Instance.Play("Error");

        }
        UpdateStatus();
    }

    public void Skip()
    {
        CloseRefineInput();
        CloseRefineOptions();

        GameManager.Instance.EndTurn();
        GameManager.Instance.RegisterInteraction();
        if (!surveyedThisTurn)
            DaySummaryHandler.Instance.UpdateSurveyChance("No Survey");
        // DaySummaryHandler.Instance.UpdateWastedOil(0);
        UpdateStatus();

        // Hide();
    }

    public void BuyTank()
    {
        CloseRefineInput();
        CloseRefineOptions();
        GameObject popup = Instantiate(feedbackPrefab, canvas.transform);

        popup.transform.position = buyTankButton.transform.position;
        if (!GameManager.Instance.TrySpend(10000))
        {
            popup.GetComponent<SimpleFeedback>().Show("- $ 10,000", new Color32(255, 0, 0, 255));
            return;
        }

        TankManager.Instance.AddTank();
        popup.GetComponent<SimpleFeedback>().Show("ADDED", new Color32(30, 110, 30, 255));
    }

    public void OpenRefineOptions()
    {
        AudioManager.Instance.Play("Button");

        refineOptionsPanel.SetActive(!refineOptionsPanel.activeSelf);
    }
    public void CloseRefineOptions()
    {
        refineOptionsPanel.SetActive(false);
    }

    public void OpenRefineInputPanel(int _type)
    {
        AudioManager.Instance.Play("Button");

        refineInputPanel.SetActive(true);
        refineInputAmountSlider.value = 0;
        CloseRefineOptions();
        refineInputType = _type;
        refineInputAmountSlider.maxValue = Mathf.Min(refineryCap, TankManager.Instance.GetGlobalCrudeOilTotal()) / 10;

        switch (_type)
        {
            case 0: // Oil
                refineInputTitle.text = "Gasoline";

                break;
            case 1: // Gas
                refineInputTitle.text = "Jet Fuel";

                break;
            case 2: // Water
                refineInputTitle.text = "Diesel";

                break;
        }
        SliderValueChanged();
        if (refineInputAmountSlider.maxValue == 0)
        {
            refineInputAmount.text = "No Crude Oil";
        }
        else
            refineInputAmount.text = "0 Gallons";

    }

    public void CloseRefineInput()
    {
        refineInputPanel.SetActive(false);
    }

    public void SliderValueChanged()
    {

        AudioManager.Instance.Play("Slider");
        // refineInputAmountSlider.value *= 10;
        if (refineInputAmountSlider.value < 0)
            refineInputAmountSlider.value = 0;
        // Always ensure slider value is whole number
        int sliderStep = Mathf.FloorToInt(refineInputAmountSlider.value);
        nGallonsToRefine = sliderStep * 10;

        // Clamp to max crude oil available
        int maxGallonsAvailable = TankManager.Instance.GetGlobalCrudeOilTotal();
        if (nGallonsToRefine > maxGallonsAvailable)
        {
            nGallonsToRefine = maxGallonsAvailable;
            sliderStep = nGallonsToRefine / 10;
            refineInputAmountSlider.value = sliderStep;
        }

        refineInputAmount.text = nGallonsToRefine.ToString() + " Gallons";

        switch (refineInputType)
        {
            case 0: moneyToSpend = nGallonsToRefine * refiningGCost; break;
            case 1: moneyToSpend = nGallonsToRefine * refiningJFCost; break;
            case 2: moneyToSpend = nGallonsToRefine * refiningDCost; break;
        }

        refineInputMoney.text = moneyToSpend.ToString();
    }

    public void AddSlider()
    {
        refineInputAmountSlider.value += 1;
        // SliderValueChanged();
    }

    public void SubtractSlider()
    {
        refineInputAmountSlider.value -= 1;
        // SliderValueChanged();
    }

    public void SubmitRefine()
    {
        GameObject popup = Instantiate(feedbackPrefab, canvas.transform);

        popup.transform.position = refineAction.transform.position;
        if (TankManager.Instance.GetGlobalCrudeOilTotal() < nGallonsToRefine)
        {
            Debug.Log("Not enough Crude Oil to refine.");
            popup.GetComponent<SimpleFeedback>().Show("ERROR", new Color32(255, 0, 0, 255));
            AudioManager.Instance.Play("Error");

            CloseRefineInput();
            CloseRefineOptions();
            return;
        }

        if (!GameManager.Instance.TrySpend(moneyToSpend))
        {
            Debug.Log("Not enough money to refine.");
            popup.GetComponent<SimpleFeedback>().Show("ERROR", new Color32(255, 0, 0, 255));
            AudioManager.Instance.Play("Error");

            CloseRefineInput();
            CloseRefineOptions();
            return;
        }
        bool success = TankManager.Instance.AddToTanks(nGallonsToRefine, (int)refineInputType + 1);
        popup.GetComponent<SimpleFeedback>().Show("REFINED", new Color32(30, 110, 30, 255));

        CloseRefineInput();
        CloseRefineOptions();

    }

    public void DisableBuyTank()
    {
        buyTankButton.interactable = false;
    }
    public void EnableBuyTank()
    {
        buyTankButton.interactable = true;
    }

    public void DisableRefine()
    {
        refineButton.interactable = false;
    }
    public void EnableRefine()
    {
        refineButton.interactable = true;
    }

}
