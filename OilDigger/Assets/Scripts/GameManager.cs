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
    private List<Tank> tanks = new List<Tank>();
    private int tankCapacity = 630; // Each tank can hold 660 gallons

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
        tankCapacity = gameSceneUIRef.maxCapacity;
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
                AddToTanks(barrelsThisTurn * 42, 0); // Assuming each barrel is 42 gallons
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

            if (money < 30)
            {
                gameSceneUIRef.DisableRefine();
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

        if (money >= 30)
        {
            gameSceneUIRef.EnableRefine();
        }
    }

    public void RegisterProducingLot(Lot lot)
    {
        if (!producingLots.Contains(lot))
        {
            producingLots.Add(lot);
        }
    }

    public void AddTank(TankType type)
    {
        Debug.Log("Adding new tank of type: " + type);
        Tank newTank = new Tank(type, tankCapacity);
        tanks.Add(newTank);
    }

    public void AddToTanks(int gallonsThisTurn, int _type)
    {
        TankType tankType = (TankType)_type;
        Debug.Log($"Received {gallonsThisTurn} gallons for {tankType}");

        if (tanks.Count == 0)
        {
            Debug.Log($"Wasting {gallonsThisTurn} gallons — no tanks available");
            return;
        }

        int gallonsRemaining = gallonsThisTurn;

        if (tankType == TankType.Crude_Oil)
        {
            // Store normally in crude oil tanks
            for (int i = 0; i < tanks.Count; i++)
            {
                Tank tank = tanks[i];

                if (tank.Type != TankType.Crude_Oil)
                    continue;

                if (gallonsRemaining == 0)
                    break;

                int toStore = Mathf.Min(gallonsRemaining, tank.RemainingCapacity);

                tank.RemainingCapacity -= toStore;
                totalGallons += toStore;
                gallonsRemaining -= toStore;

                gameSceneUIRef.UpdateTankColor(tank.RemainingCapacity, i);
                Debug.Log($"Stored {toStore} gallons in Tank {i + 1} ({tank.Type})");
            }

            if (gallonsRemaining > 0)
            {
                Debug.Log($"Wasting {gallonsRemaining} gallons — no space left in Crude Oil tanks");
            }
        }
        else
        {
            // Ensure we have enough crude oil to convert
            if (totalGallons < gallonsThisTurn)
            {
                Debug.LogWarning($"Not enough Crude Oil to convert! Required: {gallonsThisTurn}, Available: {totalGallons}");
                return;
            }

            // Add to desired type
            for (int i = 0; i < tanks.Count; i++)
            {
                Tank tank = tanks[i];

                if (tank.Type != tankType)
                    continue;

                if (gallonsRemaining == 0)
                    break;

                int toStore = Mathf.Min(gallonsRemaining, tank.RemainingCapacity);

                tank.RemainingCapacity -= toStore;
                gallonsRemaining -= toStore;

                gameSceneUIRef.UpdateTankColor(tank.RemainingCapacity, i);
                Debug.Log($"Converted & stored {toStore} gallons into Tank {i + 1} ({tankType})");
            }

            if (gallonsRemaining > 0)
            {
                Debug.LogWarning($"Wasting {gallonsRemaining} gallons — not enough space in {tankType} tanks");
            }

            int usedGallons = gallonsThisTurn - gallonsRemaining;
            totalGallons -= usedGallons;

            // Recover crude oil capacity from last tanks (reverse)
            for (int i = tanks.Count - 1; i >= 0 && usedGallons > 0; i--)
            {
                Tank tank = tanks[i];

                if (tank.Type != TankType.Crude_Oil || tank.RemainingCapacity == tank.MaxCapacity)
                    continue;

                int usedInThis = Mathf.Min(tank.MaxCapacity - tank.RemainingCapacity, usedGallons);

                tank.RemainingCapacity += usedInThis;
                usedGallons -= usedInThis;

                gameSceneUIRef.UpdateTankColor(tank.RemainingCapacity, i);
                Debug.Log($"Recovered {usedInThis} gallons in Crude Oil Tank {i + 1}");
            }
        }
        gameSceneUIRef.UpdateGallons(totalGallons);
        PrintTanks();
    }
    private void PrintTanks()
    {
        Debug.Log("Current Tanks Status:");
        for (int i = 0; i < tanks.Count; i++)
        {
            Tank tank = tanks[i];
            int used = tank.MaxCapacity - tank.RemainingCapacity;
            Debug.Log($"Tank {i + 1} ({tank.Type}): {used} gallons stored, {tank.RemainingCapacity} gallons remaining");
        }
        Debug.Log($"Total Gallons Stored: {totalGallons}\n");
    }

}
