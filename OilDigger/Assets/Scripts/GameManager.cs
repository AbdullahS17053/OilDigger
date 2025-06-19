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
    [SerializeField] private GameObject tankPrefab;

    private bool hasInteractedThisTurn = false;
    private List<Lot> producingLots = new List<Lot>();
    private Dictionary<TankType, int> globalFuelTotals = new Dictionary<TankType, int>
    {
        { TankType.Crude_Oil, 0 },
        { TankType.Gasoline, 0 },
        { TankType.Diesel_Fuel, 0 },
        { TankType.Jet_Fuel, 0 }
    };

    private List<Tank> tanks = new List<Tank>();
    private int tankCapacity = 630;
    public List<Transform> tankTransforms = new List<Transform>();
    public int CurrentTurn => currentTurn;
    public int Money => money;

    public int GetGlobalCrudeOilTotal()
    {
        return globalFuelTotals[TankType.Crude_Oil];
    }

    public int GetGlobalGasolineTotal()
    {
        return globalFuelTotals[TankType.Gasoline];
    }

    public int GetGlobalDieselTotal()
    {
        return globalFuelTotals[TankType.Diesel_Fuel];
    }

    public int GetGlobalJetFuelTotal()
    {
        return globalFuelTotals[TankType.Jet_Fuel];
    }

    public bool HasInteractedThisTurn => hasInteractedThisTurn;
    public bool isInteractionGoing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        gameSceneUIRef.UpdateMoney(money);
        gameSceneUIRef.UpdateTurn(currentTurn);
        gameSceneUIRef.UpdateBarrelsPanel();
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
            foreach (Lot lot in producingLots)
            {
                int barrels = lot.GetDailyProduction();
                if (barrels > 0)
                {
                    AddToTanks(barrels * 42, (int)TankType.Crude_Oil);
                }
            }

            currentTurn++;
            gameSceneUIRef.UpdateTurn(currentTurn);
            gameSceneUIRef.UpdateBarrelsPanel();
            hasInteractedThisTurn = false;
        }
        else
        {
            Debug.Log("Game Over");
        }
    }


    public bool TrySpend(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            gameSceneUIRef.UpdateMoney(money);
            if (money < 10000) gameSceneUIRef.DisableBuyTank();
            if (money < 30) gameSceneUIRef.DisableRefine();
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        gameSceneUIRef.UpdateMoney(money);
        if (money >= 10000) gameSceneUIRef.EnableBuyTank();
        if (money >= 30) gameSceneUIRef.EnableRefine();
    }

    public void RegisterProducingLot(Lot lot)
    {
        if (!producingLots.Contains(lot))
            producingLots.Add(lot);
    }

    public void AddTank()
    {
        Tank newTank = new Tank(tankCapacity);
        tanks.Add(newTank);

        if (tankTransforms == null || tankTransforms.Count == 0)
        {
            Debug.Log("Added new tank to list, but no transform available for placement.");
            return;
        }

        Transform spawnPoint = tankTransforms[UnityEngine.Random.Range(0, tankTransforms.Count)];
        if (spawnPoint != null)
        {
            GameObject tankInstance = Instantiate(tankPrefab, spawnPoint.position, spawnPoint.rotation);
            tankTransforms.Remove(spawnPoint);
            Debug.Log("Spawned new tank at position: " + spawnPoint.name);
        }
        else
        {
            Debug.LogWarning("Selected spawn point is null.");
        }
    }

    public bool AddToTanks(int gallonsThisTurn, int _type)
    {
        TankType type = (TankType)_type;
        Debug.Log($"Received {gallonsThisTurn} gallons of {type}");

        if (tanks.Count == 0)
        {
            Debug.Log($"Wasting {gallonsThisTurn} gallons — no tanks available");
            return false;
        }

        int gallonsRemaining = gallonsThisTurn;

        if (type != TankType.Crude_Oil)
        {
            if (globalFuelTotals[TankType.Crude_Oil] < gallonsThisTurn)
            {
                Debug.LogWarning($"Not enough Crude Oil to convert {gallonsThisTurn} gallons into {type}");
                return false;
            }

            int required = gallonsThisTurn;
            for (int i = tanks.Count - 1; i >= 0 && required > 0; i--)
            {
                Tank tank = tanks[i];
                int available = tank.GetFuelAmount(TankType.Crude_Oil);
                int extract = Mathf.Min(available, required);

                if (extract > 0)
                {
                    tank.FuelStored[TankType.Crude_Oil] -= extract;
                    tank.RemainingCapacity += extract;
                    required -= extract;
                    gameSceneUIRef.UpdateTankColor(tank.RemainingCapacity, i);
                    Debug.Log($"Extracted {extract} gallons of Crude Oil from Tank {i + 1}");
                }
            }

            globalFuelTotals[TankType.Crude_Oil] -= gallonsThisTurn;
        }

        for (int i = 0; i < tanks.Count && gallonsRemaining > 0; i++)
        {
            Tank tank = tanks[i];
            int toStore = Mathf.Min(gallonsRemaining, tank.RemainingCapacity);
            if (toStore > 0)
            {
                tank.StoreFuel(type, toStore);
                gallonsRemaining -= toStore;
                globalFuelTotals[type] += toStore;
                gameSceneUIRef.UpdateTankColor(tank.RemainingCapacity, i);
                Debug.Log($"Stored {toStore} gallons of {type} in Tank {i + 1}");
            }
        }

        if (gallonsRemaining > 0)
        {
            Debug.LogWarning($"Wasting {gallonsRemaining} gallons of {type} — no space left in tanks");
        }

        gameSceneUIRef.UpdateBarrelsPanel();
        // PrintTanks();
        // PrintFuelSummary();
        // Debug.Log($"Total Remaining Capacity: {totalRemaining} gallons");
        return true;
    }

    private void PrintTanks()
    {
        Debug.Log("Current Tanks Status:");
        for (int i = 0; i < tanks.Count; i++)
        {
            Tank tank = tanks[i];
            Debug.Log($"Tank {i + 1}:");
            foreach (var kvp in tank.FuelStored)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value} gallons");
            }
            Debug.Log($"  Remaining Capacity: {tank.RemainingCapacity} gallons");
        }
    }

    private void PrintFuelSummary()
    {
        Debug.Log("=== GLOBAL FUEL TOTALS ===");
        int totalRemaining = 0;
        foreach (var tank in tanks)
        {
            totalRemaining += tank.RemainingCapacity;
        }
        foreach (var kvp in globalFuelTotals)
        {
            Debug.Log($"- {kvp.Key}: {kvp.Value} gallons");
        }
        Debug.Log($"Total Remaining Capacity: {totalRemaining} gallons");
    }

}
