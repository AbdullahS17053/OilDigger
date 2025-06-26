using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUI : MonoBehaviour
{
    #region Variables
    [Header("Text Elements")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text crudeOil;
    [SerializeField] private TMP_Text jetFuel;
    [SerializeField] private TMP_Text gasoline;
    [SerializeField] private TMP_Text diesel;
    [SerializeField] private TMP_Text capacityText; // Text to show remaining tank capacity
    [SerializeField] private TMP_Text buyTankFeedbackMsg;
    [SerializeField] private TMP_Text crudeOilChange;
    [SerializeField] private TMP_Text gasolineChange;
    [SerializeField] private TMP_Text jetFuelChange;
    [SerializeField] private TMP_Text dieselChange;
    [SerializeField] private TMP_Text crudeOilPrice;
    [SerializeField] private TMP_Text gasolinePrice;
    [SerializeField] private TMP_Text jetFuelPrice;
    [SerializeField] private TMP_Text dieselPrice;
    [SerializeField] private TMP_Text maxChange;
    [SerializeField] private TMP_Text marketEventText;



    [Header("Buttons")]
    [SerializeField] private Button buyTank;
    [SerializeField] private Button refineOilButton;

    [Header("Panels")]
    [SerializeField] private GameObject refinePanel; // Panel to show refining options
    [SerializeField] private GameObject barrelsInputPanel;
    // [SerializeField] private GameObject buyCrudeInputPanel;
    // [SerializeField] private GameObject sellCrudeInputPanel;
    [SerializeField] private GameObject barrelsPanel;
    [SerializeField] private GameObject buyTankFeedbackPanel;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField barrelInputField;
    [SerializeField] private TMP_InputField buyCrudeInputField;
    [SerializeField] private TMP_InputField sellCrudeInputField;

    [Header("Initial Prices")]
    public int curdeOilIP = 50;
    public int gasolineIP = 100;
    public int jetFuelIP = 150;
    public int dieselIP = 200;

    private int curdeOilCP;
    private int gasolineCP;
    private int jetFuelCP;
    private int dieselCP;

    private int refiningCost = 30;
    private int nBarrelsToRefine = 0;
    private TankType refiningTankType;

    #endregion

    void Awake()
    {
        curdeOilCP = curdeOilIP;
        gasolineCP = gasolineIP;
        jetFuelCP = jetFuelIP;
        dieselCP = dieselIP;
    }

    #region UI Updates
    public void UpdateMoney(int amount)
    {
        moneyText.text = $"$ {amount}";
    }

    public void UpdateDay(int day)
    {
        dayText.text = $"Day: {day}";
    }
    public void UpdateBarrelsPanel()
    {
        crudeOil.text = (TankManager.Instance.GetGlobalCrudeOilTotal() / 42).ToString();
        gasoline.text = (TankManager.Instance.GetGlobalGasolineTotal() / 42).ToString();
        diesel.text = (TankManager.Instance.GetGlobalDieselTotal() / 42).ToString();
        jetFuel.text = (TankManager.Instance.GetGlobalJetFuelTotal() / 42).ToString();
    }

    public void UpdateBarrelCapacity(int _capacity)
    {
        capacityText.text = _capacity.ToString();
    }

    public void UpdateMarketChange(int _type, string text)
    {
        switch (_type)
        {
            case 0: // Crude Oil
                crudeOilChange.text = text;
                break;
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
            case 0: // Crude Oil
                curdeOilCP = price;
                crudeOilPrice.text = $"{curdeOilCP}";
                break;
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

    public void UpdateMaxChange(string text)
    {
        maxChange.text = text;
    }
    public void UpdateMarketEventText(string text)
    {
        marketEventText.text = text;
    }


    #endregion

    #region Panel Togglers

    public void ToggleBarrelsPanel()
    {
        barrelsPanel.SetActive(!barrelsPanel.activeInHierarchy);

        if (refinePanel.activeInHierarchy)
        {
            CloseRefine();
        }
    }

    public void ToggleRefinePanel()
    {
        refinePanel.SetActive(!refinePanel.activeInHierarchy);

        if (barrelsInputPanel.activeInHierarchy)
        {
            CloseBarrelsPanel();
        }
    }

    #endregion

    #region Panel Closers
    public void CloseBarrelsPanel()
    {
        barrelsPanel.SetActive(false);
    }
    public void CloseRefine()
    {
        refinePanel.SetActive(false);
    }
    public void CloseBarrelsInput()
    {
        barrelsInputPanel.SetActive(false);
    }
    void closeFeedbackPanel()
    {
        buyTankFeedbackPanel.SetActive(false);
    }
    #endregion

    #region Button Handlers
    public void OpenBarrelsInput()
    {
        barrelsInputPanel.SetActive(true);
        if (barrelsPanel.activeInHierarchy)
        {
            CloseBarrelsPanel();
        }

        if (refinePanel.activeInHierarchy)
        {
            CloseRefine();
        }
    }
    public void BuyTank(int _type)
    {
        buyTankFeedbackPanel.SetActive(true);
        if (!GameManager.Instance.TrySpend(10000))
        {
            buyTankFeedbackMsg.text = "Error!";
            Invoke("closeFeedbackPanel", 1f);
            return;
        }

        TankManager.Instance.AddTank();
        buyTankFeedbackMsg.text = "Tank Added!";
        Invoke("closeFeedbackPanel", 0.5f);

    }

    public void RefineOil(int _type)
    {
        refiningTankType = (TankType)_type;
        OpenBarrelsInput();

    }

    public void SubmitBarrels()
    {
        string inputText = barrelInputField.text;

        if (int.TryParse(inputText, out int barrels))
        {
            nBarrelsToRefine = barrels;

            if (nBarrelsToRefine > 0)
            {
                if (!GameManager.Instance.TrySpend(refiningCost * nBarrelsToRefine))
                {
                    CloseBarrelsInput();
                    return;
                }

                TankManager.Instance.AddToTanks(nBarrelsToRefine * 42, (int)refiningTankType); // 1 barrel = 42 gallons
            }

            // Clear input field
            barrelInputField.text = "";
        }

        barrelsInputPanel.SetActive(false);
    }

    public void SubmitBuyCrude(int _type)
    {
        string inputText = buyCrudeInputField.text;

        if (int.TryParse(inputText, out int barrels))
        {
            TankType type = (TankType)_type;
            int costPerBarrel = 0;

            // Get the correct price per barrel based on type
            switch (type)
            {
                case TankType.Crude_Oil:
                    costPerBarrel = curdeOilCP;
                    break;
                case TankType.Gasoline:
                    costPerBarrel = gasolineCP;
                    break;
                case TankType.Jet_Fuel:
                    costPerBarrel = jetFuelCP;
                    break;
                case TankType.Diesel_Fuel:
                    costPerBarrel = dieselCP;
                    break;
                default:
                    Debug.LogError("Invalid fuel type");
                    return;
            }

            int totalCost = barrels * costPerBarrel;

            if (!GameManager.Instance.TrySpend(totalCost))
                return;

            // Add gallons to tanks (1 barrel = 42 gallons)
            TankManager.Instance.AddToTanks(barrels * 42, _type, isBuy: true);

            // Clear input
            buyCrudeInputField.text = "";
        }

        // buyCrudeInputPanel.SetActive(false);
    }

    public void SubmitSellCrude(int _type)
    {
        string inputText = sellCrudeInputField.text;

        if (int.TryParse(inputText, out int barrels))
        {
            TankType type = (TankType)_type;
            int costPerBarrel = 0;

            // Get the correct price per barrel based on type
            switch (type)
            {
                case TankType.Crude_Oil:
                    costPerBarrel = curdeOilCP;
                    break;
                case TankType.Gasoline:
                    costPerBarrel = gasolineCP;
                    break;
                case TankType.Jet_Fuel:
                    costPerBarrel = jetFuelCP;
                    break;
                case TankType.Diesel_Fuel:
                    costPerBarrel = dieselCP;
                    break;
                default:
                    Debug.LogError("Invalid fuel type");
                    return;
            }

            int gallonsSold = TankManager.Instance.SellOil(barrels * 42, _type);
            int totalRevenue = gallonsSold / 42 * costPerBarrel;


            GameManager.Instance.AddMoney(totalRevenue);

            // Clear input
            sellCrudeInputField.text = "";
        }

        // sellCrudeInputPanel.SetActive(false);
    }
    public void DisableBuyTank()
    {
        buyTank.interactable = false;
    }
    public void EnableBuyTank()
    {
        buyTank.interactable = true;
    }

    public void DisableRefine()
    {
        refineOilButton.interactable = false;
    }
    public void EnableRefine()
    {
        refineOilButton.interactable = true;
    }
    public void GameOver()
    {
        int remaingCrudeMoney = TankManager.Instance.GetGlobalCrudeOilTotal() / 42 * curdeOilCP;
        int remaingGasolineMoney = TankManager.Instance.GetGlobalGasolineTotal() / 42 * gasolineCP;
        int remaingJetFuelMoney = TankManager.Instance.GetGlobalJetFuelTotal() / 42 * jetFuelCP;
        int remaingDieselMoney = TankManager.Instance.GetGlobalDieselTotal() / 42 * dieselCP;

        Debug.Log($"Game Over! You have ${GameManager.Instance.Money + remaingCrudeMoney + remaingGasolineMoney + remaingJetFuelMoney + remaingDieselMoney} left.");
        
        Debug.Log("All Money: " +
            $"{GameManager.Instance.Money} + {remaingCrudeMoney} + {remaingGasolineMoney} + {remaingJetFuelMoney} + {remaingDieselMoney}");
    }
    #endregion
}
