using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    public static TankManager Instance { get; private set; }

    [Header("Tank Settings")]
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private GameSceneUI gameSceneUIRef;
    [SerializeField] private int tankCapacity = 630; // gallons

    private List<Lot> producingLots = new List<Lot>();
    private Dictionary<TankType, int> globalFuelTotals = new Dictionary<TankType, int>
    {
        { TankType.Crude_Oil, 0 },
        { TankType.Gasoline, 0 },
        { TankType.Diesel_Fuel, 0 },
        { TankType.Jet_Fuel, 0 }
    };

    private List<Tank> tanks = new List<Tank>();
    public List<Transform> tankTransforms = new List<Transform>();
    private List<int> weatherEventDays = new List<int>();
    private List<int> drillMalDays = new List<int>();
    public int RemainingCapacity { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GenerateWeatherEventDays();
        GenerateDrillMalfunctionDays();
    }
    private void GenerateWeatherEventDays()
    {
        HashSet<int> uniqueDays = new HashSet<int>();
        int numEvents = Random.Range(1, 6); // 1 to 5 events

        while (uniqueDays.Count < numEvents)
        {
            int day = Random.Range(0, 30);
            uniqueDays.Add(day);
        }

        weatherEventDays = uniqueDays.ToList();
        Debug.Log("Weather Event Days: " + string.Join(", ", weatherEventDays));
    }
    private void GenerateDrillMalfunctionDays()
    {
        HashSet<int> uniqueDays = new HashSet<int>();
        int numMalfunctions = Random.Range(1, 6); // 1 to 5 malfunctions

        while (uniqueDays.Count < numMalfunctions)
        {
            int day = Random.Range(0, 30);
            uniqueDays.Add(day);
        }

        drillMalDays = uniqueDays.ToList();
        Debug.Log("Drill Malfunction Days: " + string.Join(", ", drillMalDays));
    }

    public void RegisterProducingLot(Lot lot)
    {
        if (!producingLots.Contains(lot))
            producingLots.Add(lot);
    }

    public void EndDay(int _currentDay)
    {
        foreach (Lot lot in producingLots)
        {
            int barrels = lot.GetDailyProduction();
            if (barrels > 0)
            {
                AddToTanks(barrels * 42, (int)TankType.Crude_Oil);
            }
        }

        if (weatherEventDays.Contains(_currentDay))
        {
            TriggerWeatherEvent();
        }

        if (drillMalDays.Contains(_currentDay))
        {
            TriggerDrillMalfunction();
        }
    }
    private void TriggerWeatherEvent()
    {
        int tanksToDestroy = Mathf.Min(UnityEngine.Random.Range(1, 5), tanks.Count);

        Debug.Log($"Weather event triggered! Destroying {tanksToDestroy} tanks.");

        for (int i = 0; i < tanksToDestroy; i++)
        {
            int index = UnityEngine.Random.Range(0, tanks.Count);
            Tank tank = tanks[index];

            // Free up spawn point
            if (tank.AssignedSpawnPoint != null)
                tankTransforms.Add(tank.AssignedSpawnPoint);

            // Destroy GameObject
            if (tank.VisualInstance != null)
                Destroy(tank.VisualInstance);

            // Deduct oil stored in this tank from globalFuelTotals
            foreach (var entry in tank.FuelStored)
            {
                if (globalFuelTotals.ContainsKey(entry.Key))
                    globalFuelTotals[entry.Key] -= entry.Value;
            }

            RemainingCapacity -= tankCapacity / 42;
            tanks.RemoveAt(index);
        }

        gameSceneUIRef.UpdateBarrelsPanel();
        gameSceneUIRef.UpdateBarrelCapacity(RemainingCapacity);
    }

    private void TriggerDrillMalfunction()
    {
        Lot lot = producingLots[UnityEngine.Random.Range(0, producingLots.Count)];
        if (lot.IsDrilled && lot.IsProducing())
        {
            Transform drillChild = lot.transform.Find("Drill");
            drillChild.gameObject.SetActive(false);
            producingLots.Remove(lot);
            Debug.Log($"Drill malfunction on {lot.name}! Stopping production.");
        }
    }

    public void AddTank()
    {
        if (tankTransforms == null || tankTransforms.Count == 0)
            return;

        Transform spawnPoint = tankTransforms[UnityEngine.Random.Range(0, tankTransforms.Count)];
        if (spawnPoint == null)
            return;

        GameObject tankInstance = Instantiate(tankPrefab, spawnPoint.position, spawnPoint.rotation);
        tankTransforms.Remove(spawnPoint);

        Tank newTank = new Tank(tankCapacity)
        {
            VisualInstance = tankInstance,
            AssignedSpawnPoint = spawnPoint
        };

        tanks.Add(newTank);

        RemainingCapacity += tankCapacity / 42;
        gameSceneUIRef.UpdateBarrelCapacity(RemainingCapacity);
    }

    private int ExtractFromTanks(TankType type, int gallonsRequired)
    {
        int remaining = gallonsRequired;

        for (int i = tanks.Count - 1; i >= 0 && remaining > 0; i--)
        {
            Tank tank = tanks[i];
            int available = tank.GetFuelAmount(type);
            int extract = Mathf.Min(available, remaining);

            if (extract > 0)
            {
                tank.FuelStored[type] -= extract;
                tank.RemainingCapacity += extract;
                remaining -= extract;
            }
        }

        return remaining;
    }

    private int StoreInTanks(TankType type, int gallonsToStore)
    {
        int remaining = gallonsToStore;

        foreach (Tank tank in tanks)
        {
            int toStore = Mathf.Min(remaining, tank.RemainingCapacity);
            if (toStore > 0)
            {
                tank.StoreFuel(type, toStore);
                remaining -= toStore;
                globalFuelTotals[type] += toStore;
            }

            if (remaining <= 0) break;
        }

        return remaining;
    }

    private void UpdateTankUI()
    {
        gameSceneUIRef.UpdateBarrelsPanel();
        RemainingCapacity = RecalculateTotalBarrelCapacity();
        gameSceneUIRef.UpdateBarrelCapacity(RemainingCapacity);
    }

    public bool AddToTanks(int gallonsThisTurn, int _type, bool isBuy = false)
    {
        TankType type = (TankType)_type;

        if (tanks.Count == 0) return false;

        int gallonsRemaining = gallonsThisTurn;

        // Handle Crude Oil conversion for refined fuels
        if (type != TankType.Crude_Oil && !isBuy)
        {
            if (globalFuelTotals[TankType.Crude_Oil] < gallonsThisTurn)
            {
                Debug.LogWarning($"Not enough Crude Oil to convert {gallonsThisTurn} gallons into {type}");
                return false;
            }

            gallonsRemaining = ExtractFromTanks(TankType.Crude_Oil, gallonsThisTurn);
            globalFuelTotals[TankType.Crude_Oil] -= (gallonsThisTurn - gallonsRemaining);
        }

        // Store in tanks
        gallonsRemaining = StoreInTanks(type, gallonsRemaining);

        if (gallonsRemaining > 0)
        {
            Debug.LogWarning($"Wasting {gallonsRemaining} gallons of {type} — no space left in tanks");
        }

        UpdateTankUI();
        return true;
    }

    public int SellOil(int gallonsToSell, int type)
    {
        TankType tankType = (TankType)type;

        if (gallonsToSell <= 0 || globalFuelTotals.GetValueOrDefault(tankType, 0) < gallonsToSell)
        {
            Debug.LogWarning($"Not enough {tankType} to sell {gallonsToSell} gallons");
            return -1;
        }

        int gallonsRemaining = ExtractFromTanks(tankType, gallonsToSell);
        int gallonsSold = gallonsToSell - gallonsRemaining;
        globalFuelTotals[tankType] -= gallonsSold;

        if (gallonsRemaining > 0)
        {
            Debug.LogWarning($"Could not sell all requested {tankType} — only sold {gallonsSold} gallons");
        }

        UpdateTankUI();
        return gallonsSold;
    }

    private int RecalculateTotalBarrelCapacity()
    {
        int totalGallons = 0;
        foreach (var tank in tanks)
        {
            totalGallons += tank.RemainingCapacity;
        }
        return totalGallons / 42;
    }

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
}
