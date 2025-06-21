using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUI : MonoBehaviour
{
    [Header("Text Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI turnText;
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] private TMPro.TextMeshProUGUI barrelsText;
    [SerializeField] private Button buyTank;
    [SerializeField] private Button refineOilButton; // Button to open refine panel
    [SerializeField] private GameObject buyTankPanel; // Panel to show when buying tanks
    [SerializeField] private GameObject refinePanel; // Panel to show refining options
    [SerializeField] private GameObject barrelsInputPanel; // Panel to input number of barrels

    [SerializeField] private GameObject barrelsPanel;
    [SerializeField] private GameObject buyTankFeedbackPanel; // Panel to show feedback after buying a tank
    [SerializeField] private TMP_Text buyTankFeedbackMsg; // Panel to show feedback after buying a tank
    [SerializeField] private TMP_Text crudeOil;
    [SerializeField] private TMP_Text jetFuel;
    [SerializeField] private TMP_Text gasoline;
    [SerializeField] private TMP_Text diesel;
    [SerializeField] private TMP_Text capacityText; // Text to show tank capacity

    [SerializeField] private GameObject tankPrefab; // Prefab of UI Image
    [SerializeField] private Transform tankContainer; // Parent with Horizontal Layout
    [SerializeField] public int maxCapacity;
    [SerializeField] private int refiningCost = 30;

    [SerializeField] private TMP_InputField barrelInputField;
    // [SerializeField] private Button submitButton;

    private List<Image> tankImages = new List<Image>(); // UI images
    private int nBarrelsToRefine = 0;
    private TankType refiningTankType;
    public void UpdateMoney(int amount)
    {
        moneyText.text = $"$ {amount}";
    }

    public void UpdateTurn(int turn)
    {
        turnText.text = $"Day: {turn}";
    }

    public void ToggleBarrelsPanel()
    {
        barrelsPanel.SetActive(!barrelsPanel.activeInHierarchy);

        if (refinePanel.activeInHierarchy)
        {
            CloseRefine();
        }
    }

    public void UpdateBarrelsPanel()
    {
        int crudeOilBarrels = GameManager.Instance.GetGlobalCrudeOilTotal() / 42;
        int gasolineBarrels = GameManager.Instance.GetGlobalGasolineTotal() / 42;
        int dieselBarrels = GameManager.Instance.GetGlobalDieselTotal() / 42;
        int jetFuelBarrels = GameManager.Instance.GetGlobalJetFuelTotal() / 42;

        crudeOil.text = crudeOilBarrels.ToString();
        gasoline.text = gasolineBarrels.ToString();
        diesel.text = dieselBarrels.ToString();
        jetFuel.text = jetFuelBarrels.ToString();
    }

    public void UpdateBarrelCapacity(int _capacity)
    {
        capacityText.text = _capacity.ToString();
    }
    public void CloseBarrelsPanel()
    {
        barrelsPanel.SetActive(false);
    }


    public void ToggleRefinePanel()
    {
        refinePanel.SetActive(!refinePanel.activeInHierarchy);

        if (barrelsInputPanel.activeInHierarchy)
        {
            CloseBarrelsPanel();
        }
    }

    public void CloseRefine()
    {
        refinePanel.SetActive(false);
    }

    public void OpenBarrelsInput()
    {
        barrelsInputPanel.SetActive(true); // Panel to input number of barrels.SetActive(true);
        if (barrelsPanel.activeInHierarchy)
        {
            CloseBarrelsPanel();
        }

        if (refinePanel.activeInHierarchy)
        {
            CloseRefine();
        }
    }

    public void CloseBarrelsInput()
    {
        barrelsInputPanel.SetActive(false); // Panel to input number of barrels.SetActive(false);
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

        // TankType tankType = (TankType)_type;

        GameManager.Instance.AddTank();
        buyTankFeedbackMsg.text = "Tank Added!";
        Invoke("closeFeedbackPanel", 0.5f);

    }

    void closeFeedbackPanel()
    {
        Debug.Log("Closing feedback panel");
        buyTankFeedbackPanel.SetActive(false);
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

                GameManager.Instance.AddToTanks(nBarrelsToRefine * 42, (int)refiningTankType); // 1 barrel = 42 gallons
            }

            // Clear input field
            barrelInputField.text = "";
        }
        else
        {
            Debug.LogWarning("Invalid number of barrels entered!");
        }
        barrelsInputPanel.SetActive(false); // Close the input panel after submission
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


    // public void UpdateTankColor(int remaining, int index)
    // {
    //     Image img = tankImages[index];

    //     if (remaining == 630)
    //         img.color = Color.white; // Empty
    //     else if (remaining == 0)
    //         img.color = Color.red; // Full
    // }
}
