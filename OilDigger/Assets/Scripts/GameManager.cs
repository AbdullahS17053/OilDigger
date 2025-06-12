using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 30;
    [SerializeField] private int currentTurn = 1;
    [SerializeField] private int money = 1000000;
    [SerializeField] private GameSceneUI gameSceneUIRef;

    private bool hasInteractedThisTurn = false;
    private List<Lot> producingLots = new List<Lot>();
    private int totalGallons = 0;
    public bool isInteractionGoing = false;
    private List<int> tanks = new List<int>();

    public int CurrentTurn => currentTurn;
    public int Money => money;
    public bool HasInteractedThisTurn => hasInteractedThisTurn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameSceneUIRef.UpdateMoney(money);
        gameSceneUIRef.UpdateTurn(currentTurn);
        gameSceneUIRef.UpdateGallons(totalGallons);
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

            if (barrelsThisTurn > 0)
                AddToTanks(barrelsThisTurn * 42); // Assuming each barrel is 42 gallons
            // Debug.Log($"Day {currentTurn}: +{barrelsThisTurn} barrels (Total: {totalGallons})");

            currentTurn++;
            gameSceneUIRef.UpdateTurn(currentTurn);
            gameSceneUIRef.UpdateGallons(totalGallons);
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
        if (money >= amount)
        {
            money -= amount;
            gameSceneUIRef.UpdateMoney(money);
            if (money < 10000)
            {
                gameSceneUIRef.DisableBuyTank();
            }
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        gameSceneUIRef.UpdateMoney(money);
        if (money >= 10000)
        {
            gameSceneUIRef.EnableBuyTank();
        }
    }

    public void RegisterProducingLot(Lot lot)
    {
        if (!producingLots.Contains(lot))
        {
            producingLots.Add(lot);
        }
    }

    public void AddTank()
    {
        tanks.Add(660);
    }

    public void AddToTanks(int gallonsThisTurn)
    {
        Debug.Log($"Received {gallonsThisTurn} gallons today");

        if (tanks.Count == 0)
        {
            Debug.Log($"Wasting {gallonsThisTurn} gallons — no tanks available");
            return;
        }

        int gallonsRemaining = gallonsThisTurn;

        for (int i = 0; i < tanks.Count; i++)
        {
            if (gallonsRemaining == 0)
                break;

            int capacity = tanks[i];
            int toStore = Mathf.Min(gallonsRemaining, capacity);

            tanks[i] -= toStore;
            totalGallons += toStore;
            gallonsRemaining -= toStore;

            gameSceneUIRef.UpdateTankColor(tanks[i], i);

            Debug.Log($"Stored {toStore} gallons in Tank {i + 1}");
        }

        if (gallonsRemaining > 0)
        {
            Debug.Log($"Wasting {gallonsRemaining} gallons — no space left in tanks");
        }

        PrintTanks();
    }

    
    private void PrintTanks()
    {
        Debug.Log("Current Tanks Status:");
        for (int i = 0; i < tanks.Count; i++)
        {
            int used = 660 - tanks[i];
            Debug.Log($"Tank {i + 1}: {used} gallons stored, {tanks[i]} gallons remaining");
        }
        Debug.Log($"Total Gallons Stored: {totalGallons}\n");
    }
}
