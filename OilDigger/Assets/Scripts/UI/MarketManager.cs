using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance { get; private set; }
    [SerializeField] private TMP_Text jetFuel;
    [SerializeField] private TMP_Text gasoline;
    [SerializeField] private TMP_Text diesel;
    [SerializeField] private TMP_Text crude;

    [SerializeField] private TMP_Text gasolineChange;
    [SerializeField] private TMP_Text jetFuelChange;
    [SerializeField] private TMP_Text dieselChange;

    [SerializeField] private TMP_Text gasolinePrice;
    [SerializeField] private TMP_Text jetFuelPrice;
    [SerializeField] private TMP_Text dieselPrice;
    [SerializeField] private TMP_Text crudePrice;

    [SerializeField] private TMP_Text optionsHeading;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject buySellInputPanel;
    [SerializeField] private TMP_Text buySellButtonText;
    [SerializeField] private Button buySellButton;

    [SerializeField] private TMP_Text buySellInputTitle;
    [SerializeField] private TMP_Text buySellInputType;
    [SerializeField] private TMP_Text buySellInputSign;
    [SerializeField] private TMP_Text buySellInputMoney;
    [SerializeField] private TMP_Text buySellInputAmount;

    [SerializeField] private Slider buySellInputAmountSlider;
    [SerializeField] private int maxGallonsToBuy = 1000;
    [SerializeField] private GameObject feedbackPrefab;
    [SerializeField] private Canvas canvas;



    [Header("Initial Prices")]
    public int curdeOilIP = 10;
    public int gasolineIP = 20;
    public int jetFuelIP = 40;
    public int dieselIP = 60;

    private int curdeOilCP;
    private int gasolineCP;
    private int jetFuelCP;
    private int dieselCP;

    private int buySell = 0;
    private int type = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        curdeOilCP = curdeOilIP;
        gasolineCP = gasolineIP;
        jetFuelCP = jetFuelIP;
        dieselCP = dieselIP;

        crudePrice.text = curdeOilCP.ToString();

        UpdateMarketPrices(1, gasolineIP);
        UpdateMarketPrices(2, jetFuelIP);
        UpdateMarketPrices(3, dieselIP);
    }

    public void OpenBuySellInputPanel(int option)
    {
        AudioManager.Instance.Play("Button");

        type = option;
        buySellInputPanel.SetActive(true);
        CloseBuySellPanel();

        if (buySell == 0)
        {
            buySellInputSign.text = "-";
            buySellInputType.text = "Buy";
            buySellInputAmountSlider.maxValue = maxGallonsToBuy;
            buySellButtonText.text = "Buy";
        }
        else
        {
            buySellInputSign.text = "+";
            buySellInputType.text = "Sell";
            buySellInputAmountSlider.maxValue = Mathf.Min(TankManager.Instance.globalFuelTotals[(TankType)type + 1], maxGallonsToBuy);
            buySellButtonText.text = "Sell";
        }

        switch (type)
        {
            case 0: // Oil
                buySellInputTitle.text = "Gasoline";

                break;
            case 1: // Gas
                buySellInputTitle.text = "Jet Fuel";

                break;
            case 2: // Water
                buySellInputTitle.text = "Diesel";

                break;
        }

    }

    public void SliderValueChanged()
    {
        AudioManager.Instance.Play("Slider");
        int step = Mathf.FloorToInt(buySellInputAmountSlider.value);
        int gallons = step * 10;

        int pricePerGallon = 0;

        switch (type)
        {
            case 0: pricePerGallon = gasolineCP; break;
            case 1: pricePerGallon = jetFuelCP; break;
            case 2: pricePerGallon = dieselCP; break;
        }

        int totalCost = gallons * pricePerGallon;

        buySellInputAmount.text = gallons.ToString();
        buySellInputMoney.text = totalCost.ToString();
    }

    public void SubmitBuySell()
    {
        AudioManager.Instance.Play("Button");

        GameObject popup = Instantiate(feedbackPrefab, canvas.transform);

        popup.transform.position = buySellButton.transform.position;
        int gallons = (int)buySellInputAmountSlider.value * 10;

        if (gallons <= 0)
        {
            Debug.LogWarning("Gallons must be greater than zero.");
            popup.GetComponent<SimpleFeedback>().Show("ERROR", new Color32(255, 0, 0, 255));
            AudioManager.Instance.Play("Error");

            return;
        }

        int pricePerGallon = 0;
        switch (type)
        {
            case 0: pricePerGallon = gasolineCP; break;
            case 1: pricePerGallon = jetFuelCP; break;
            case 2: pricePerGallon = dieselCP; break;
        }

        int totalCost = gallons * pricePerGallon;

        if (buySell == 0) // BUY
        {
            if (GameManager.Instance.GetMoney() < totalCost)
            {
                Debug.LogWarning("Not enough money to buy.");
                popup.GetComponent<SimpleFeedback>().Show("ERROR", new Color32(255, 0, 0, 255));
                AudioManager.Instance.Play("Error");
                return;
            }
            GameManager.Instance.TrySpend(totalCost);

            bool added = TankManager.Instance.AddToTanks(gallons, type + 1, true);

            if (added)
            {
                popup.GetComponent<SimpleFeedback>().Show($"- $ {totalCost}", new Color32(255, 0, 0, 255));
            }
            else
            {
                popup.GetComponent<SimpleFeedback>().Show($"ERROR!", new Color32(255, 0, 0, 255));
                AudioManager.Instance.Play("Error");
            }

        }
        else // SELL
        {
            int soldGallons = TankManager.Instance.SellOil(gallons, type + 1);
            if (soldGallons > 0)
            {
                int totalMoney = soldGallons * pricePerGallon;
                GameManager.Instance.AddMoney(totalMoney);

                popup.GetComponent<SimpleFeedback>().Show(totalMoney.ToString(), new Color32(30, 110, 30, 255));
            }
            else
            {
                Debug.LogWarning("Not enough fuel to sell.");
                popup.GetComponent<SimpleFeedback>().Show("ERROR", new Color32(255, 0, 0, 255));
                AudioManager.Instance.Play("Error");

                return;
            }
        }

        // Update UI and close panel
        UpdateBarrelsPanel();
        CloseBuySellInputPanel();
    }

    public void AddSlider()
    {
        buySellInputAmountSlider.value += 1;
        SliderValueChanged();
    }

    public void SubtractSlider()
    {
        buySellInputAmountSlider.value -= 1;
        SliderValueChanged();
    }
    public void CloseBuySellInputPanel()
    {
        buySellInputPanel.SetActive(false);
    }

    public void OpenBuySellPanel(int option)
    {
        AudioManager.Instance.Play("Button");

        optionsPanel.SetActive(true);
        buySell = option;
        if (option == 0)
        {
            optionsHeading.text = "Buy";
        }
        else
            optionsHeading.text = "Sell";


    }
    public void CloseBuySellPanel()
    {
        optionsPanel.SetActive(false);
    }

    public void UpdateBarrelsPanel()
    {
        crude.text = TankManager.Instance.GetGlobalCrudeOilTotal().ToString();
        gasoline.text = TankManager.Instance.GetGlobalGasolineTotal().ToString();
        diesel.text = TankManager.Instance.GetGlobalDieselTotal().ToString();
        jetFuel.text = TankManager.Instance.GetGlobalJetFuelTotal().ToString();
    }

    public void UpdateMarketChange(int _type, string text)
    {
        switch (_type)
        {
            case 1: // Gasoline
                gasolineChange.text = text;
                break;
            case 2: // Jet Fuel
                jetFuelChange.text = text;
                break;
            case 3: // Diesel
                dieselChange.text = text;
                break;
            default:
                Debug.LogError("Invalid fuel type for market change update.");
                break;
        }
    }
    public void UpdateMarketPrices(int _type, int price)
    {
        switch (_type)
        {
            case 1: // Gasoline
                gasolineCP = price;
                gasolinePrice.text = $"{gasolineCP}";
                break;
            case 2: // Jet Fuel
                jetFuelCP = price;
                jetFuelPrice.text = $"{jetFuelCP}";
                break;
            case 3: // Diesel
                dieselCP = price;
                dieselPrice.text = $"{dieselCP}";
                break;
            default:
                Debug.LogError("Invalid fuel type for market price update.");
                break;
        }
    }

    public void GameOver()
    {
        int remaingCrude = TankManager.Instance.GetGlobalCrudeOilTotal();
        int remaingGasoline = TankManager.Instance.GetGlobalGasolineTotal();
        int remaingJetFuel = TankManager.Instance.GetGlobalJetFuelTotal();
        int remaingDiesel = TankManager.Instance.GetGlobalDieselTotal();

        GameOverManager.Instance.UpdateCrudeOil(remaingCrude, curdeOilCP);

        Debug.Log($"Gas {gasolineCP}");
        GameOverManager.Instance.UpdateGasoline(remaingGasoline, gasolineCP);

        GameOverManager.Instance.UpdateJetFuel(remaingJetFuel, jetFuelCP);

        GameOverManager.Instance.UpdateDiesel(remaingDiesel, dieselCP);

    }

}

