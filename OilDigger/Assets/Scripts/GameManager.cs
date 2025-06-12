using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 30;
    [SerializeField] private int currentTurn = 1;
    [SerializeField] private int money = 1000000;

    private bool hasInteractedThisTurn = false;
    private List<Lot> producingLots = new List<Lot>();
    private int totalBarrels = 0;

    public int CurrentTurn => currentTurn;
    public int Money => money;
    public bool HasInteractedThisTurn => hasInteractedThisTurn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterInteraction()
    {
        hasInteractedThisTurn = true;
    }

    public void EndTurn()
    {
        if (currentTurn <= maxTurns)
        {
        
            // Accumulate barrels from all drilled lots
            int barrelsThisTurn = 0;
            foreach (Lot lot in producingLots)
            {
                barrelsThisTurn += lot.GetDailyProduction();
            }

            totalBarrels += barrelsThisTurn;
            Debug.Log($"Ending turn {currentTurn}. Money: {money}, Total Barrels: {totalBarrels}");
            Debug.Log($"Day {currentTurn}: +{barrelsThisTurn} barrels (Total: {totalBarrels})");

            currentTurn++;
            hasInteractedThisTurn = false;
        }
        else
        {
            Debug.Log("Game Over");
            // You can disable inputs or show game over screen here
        }
    }

    public bool TrySpend(int amount)
    {
        Debug.Log($"Trying to spend {amount}. Current money: {money}");
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"After spending {amount}. Current money: {money}");
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public void RegisterProducingLot(Lot lot)
    {
        if (!producingLots.Contains(lot))
        {
            producingLots.Add(lot);
        }
    }
}
