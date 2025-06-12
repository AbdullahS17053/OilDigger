using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneUI : MonoBehaviour
{
    [Header("Text Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI turnText;
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] private TMPro.TextMeshProUGUI barrelsText;
    [SerializeField] private Button buyTank;

    [SerializeField] private GameObject tankPrefab; // Prefab of UI Image
    [SerializeField] private Transform tankContainer; // Parent with Horizontal Layout

    private List<Image> tankImages = new List<Image>(); // UI images

    public void UpdateMoney(int amount)
    {
        moneyText.text = $"$ {amount}";
    }

    public void UpdateTurn(int turn)
    {
        turnText.text = $"Turn: {turn}";
    }

    public void UpdateGallons(int amount)
    {
        barrelsText.text = $"Gallons: {amount}";
    }

    public void BuyTank()
    {
        if (!GameManager.Instance.TrySpend(10000)) return;

        GameManager.Instance.AddTank();

        GameObject tankObj = Instantiate(tankPrefab, tankContainer);
        Image img = tankObj.GetComponent<Image>();
        img.color = Color.white;

        tankImages.Add(img);
    }

    public void DisableBuyTank()
    {
        buyTank.interactable = false;
    }
    public void EnableBuyTank()
    {
        buyTank.interactable = true;
    }
    
    public void UpdateTankColor(int remaining, int index)
    {
        Image img = tankImages[index];

        if (remaining == 660)
            img.color = Color.white; // Empty
        else if (remaining == 0)
            img.color = Color.red; // Full
        else
            img.color = Color.green; // Partially filled
    }
}
