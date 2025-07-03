using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    public static TankManager Instance { get; private set; }

    [Header("Tank Settings")]
    [SerializeField] private GameObject tankPrefab;
    [SerializeField] private int tankCapacity = 630; // gallons
    [SerializeField] private int tankPrice = 1000;
    [SerializeField] private int refineryPrice = 10000;

    private List<Lot> producingLots = new List<Lot>();
    public Dictionary<TankType, int> globalFuelTotals = new Dictionary<TankType, int>
    {
        { TankType.Crude_Oil, 0 },
        { TankType.Gasoline, 0 },
        { TankType.Jet_Fuel, 0 },
        { TankType.Diesel_Fuel, 0 }
    };

    private List<Tank> tanks = new List<Tank>();
    public List<Transform> tankTransforms = new List<Transform>();
    private List<int> weatherEventDays = new List<int>();
    private List<int> drillMalDays = new List<int>();

    public Dictionary<int, List<string>> notificationDays = new();
    private List<string> weatherEvents = new List<string>
    {
        "Hurricane",
        "Tornado",
        "Flood",
        "Blizzard",
        "Heatwave"
    };

    private int weatherNotificationIndex = 0;
    private int weatherEventIndex = 0;

    private List<string> notificationMessages = new List<string>();
    public List<string> notfiMessagesSuffix = new List<string>
    {
        " in coming days",
        " in the near future",
        " soon",
        " in this week"
    };
    private int TotalGallonsCapacity { get; set; }
    private int RemainingGallonsCapacity { get; set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateWeatherEventDays();
        GenerateDrillMalfunctionDays();
        ShuffleWeatherEvents();
    }
    private void ShuffleWeatherEvents()
    {
        for (int i = weatherEvents.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string temp = weatherEvents[i];
            weatherEvents[i] = weatherEvents[j];
            weatherEvents[j] = temp;
        }
        // Debug.Log("Weather Events shuffled.");
    }
    private void GenerateWeatherEventDays()
    {
        HashSet<int> uniqueDays = new HashSet<int>();
        int numEvents = Random.Range(1, 6); // 1 to 5 events

        while (uniqueDays.Count < numEvents)
        {
            int day = Random.Range(6, 30);
            uniqueDays.Add(day);
        }

        weatherEventDays = uniqueDays.ToList();
        weatherEventDays.Sort(); // Sort the days in ascending order
        Debug.Log("Weather Event Days: " + string.Join(", ", weatherEventDays));

        foreach (int day in weatherEventDays)
        {
            string weatherEvent = weatherEvents[weatherEventIndex];
            weatherEventIndex = (weatherEventIndex + 1) % weatherEvents.Count;
            string weatherMessage = $"Weather event: {weatherEvent} is hitting today, Day no {day + 1}";
            AddNotification(day, weatherMessage);

            int notificationDay = Random.Range(day - 5, day);
            string eventType = weatherEvents[weatherNotificationIndex];
            weatherNotificationIndex = (weatherNotificationIndex + 1) % weatherEvents.Count;
            string suffix = notfiMessagesSuffix[Random.Range(0, notfiMessagesSuffix.Count)];

            string message = $"Weather notification: {eventType} {suffix}";
            AddNotification(notificationDay, message);
        }
    }
    public void AddNotification(int day, string message)
    {
        if (!notificationDays.ContainsKey(day))
        {
            notificationDays[day] = new List<string>();
        }
        notificationDays[day].Add(message);
    }
    private void GenerateDrillMalfunctionDays()
    {
        HashSet<int> uniqueDays = new HashSet<int>();
        int numMalfunctions = Random.Range(1, 4); // 1 to 5 malfunctions

        while (uniqueDays.Count < numMalfunctions)
        {
            int day = Random.Range(15, 30);
            uniqueDays.Add(day);
        }

        drillMalDays = uniqueDays.ToList();
        drillMalDays.Sort(); // Sort the days in ascending order
        Debug.Log("Drill Malfunction Days: " + string.Join(", ", drillMalDays));
    }

    public void RegisterProducingLot(Lot lot)
    {
        if (!producingLots.Contains(lot))
            producingLots.Add(lot);
    }

    public void EndDay(int _currentDay)
    {
        // Debug.Log("Ending day: " + _currentDay);
        int barrelsTotal = 0;
        foreach (Lot lot in producingLots)
        {
            int barrels = lot.GetDailyProduction();
            if (barrels > 0)
            {
                barrelsTotal += barrels;
                AddToTanks(barrels * 42, (int)TankType.Crude_Oil);
            }
        }
        DaySummaryHandler.Instance.UpdateDailyProduction(barrelsTotal * 42);

        StartCoroutine(LateUpdateEndDay(_currentDay));
    }
    IEnumerator LateUpdateEndDay(int _currentDay)
    {
        yield return new WaitForSeconds(2f);
        if (weatherEventDays.Contains(_currentDay))
        {
            TriggerWeatherEvent(_currentDay);
        }

        if (drillMalDays.Contains(_currentDay))
        {
            TriggerDrillMalfunction(_currentDay);
        }

        if (notificationDays.ContainsKey(_currentDay))
        {
            NotificationUIManager.Instance.AddNotifications(_currentDay + 1, notificationDays[_currentDay]);

            StartCoroutine("SwitchToNotification");
        }
    }

    private IEnumerator SwitchToNotification()
    {
        yield return new WaitForSeconds(2f);
        TabManager.Instance.SwitchToTab(0);
        AudioManager.Instance.Play("Notification");
    }
    private void TriggerWeatherEvent(int currentDay)
    {
        if (tanks.Count == 0)
        {
            Debug.Log("No tanks available to destroy.");
            return;
        }

        int tanksToDestroy = Mathf.Min(Random.Range(1, 5), tanks.Count);
        int tanksDestroyed = 0;
        List<Tank> selectedTanks = new List<Tank>();

        // Pick unique tanks to destroy
        while (selectedTanks.Count < tanksToDestroy)
        {
            Tank tank = tanks[Random.Range(0, tanks.Count)];
            if (!selectedTanks.Contains(tank))
                selectedTanks.Add(tank);
        }

        foreach (Tank tank in selectedTanks)
        {
            // Free up spawn point
            if (tank.AssignedSpawnPoint != null)
                tankTransforms.Add(tank.AssignedSpawnPoint);

            // Destroy visual
            if (tank.VisualInstance != null)
                Destroy(tank.VisualInstance);

            // Adjust global totals
            foreach (var entry in tank.FuelStored)
            {
                if (globalFuelTotals.ContainsKey(entry.Key))
                    globalFuelTotals[entry.Key] -= entry.Value;
            }

            TotalGallonsCapacity -= tankCapacity;
            RemainingGallonsCapacity -= tank.RemainingCapacity;

            tanks.Remove(tank);
            tanksDestroyed++;
        }

        MarketManager.Instance.UpdateBarrelsPanel();
        TopUIHandler.Instance.SetCapacity(TotalGallonsCapacity, RemainingGallonsCapacity);

        string message = $"Weather event on Day {currentDay + 1}: Destroyed {tanksDestroyed} tank(s).";

        AddNotification(currentDay, message);
    }

    private void TriggerDrillMalfunction(int currentDay)
    {
        if (producingLots.Count == 0)
        {
            Debug.Log("No producing lots to trigger malfunction.");
            return;
        }
        Lot lot = producingLots[UnityEngine.Random.Range(0, producingLots.Count)];
        if (lot.IsDrilled && lot.IsProducing())
        {
            Transform drillChild = lot.transform.Find("Drill");
            drillChild.gameObject.SetActive(false);
            producingLots.Remove(lot);
            string message = $"Drill malfunction on {lot.name}! Stopping production.";
            AddNotification(currentDay, message);
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
        AudioManager.Instance.Play("Tank");

        Debug.Log($"Added new tank at {spawnPoint.position}");

        TotalGallonsCapacity += tankCapacity;
        RemainingGallonsCapacity += tankCapacity;

        TopUIHandler.Instance.SetCapacity(TotalGallonsCapacity, RemainingGallonsCapacity);
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
                RemainingGallonsCapacity += extract;
            }
        }

        return remaining;
    }

    private int StoreInTanks(TankType type, int gallonsToStore)
    {
        Debug.Log($"Storing {type} gallons {gallonsToStore}");
        int remaining = gallonsToStore;

        foreach (Tank tank in tanks)
        {
            int toStore = Mathf.Min(remaining, tank.RemainingCapacity);
            if (toStore > 0)
            {
                tank.StoreFuel(type, toStore);
                Debug.Log($"[StoreInTanks] Stored {toStore} gallons of {type} in tank.");

                remaining -= toStore;
                globalFuelTotals[type] += toStore;
                RemainingGallonsCapacity -= toStore;

                Debug.Log($"[StoreInTanks] Updated globalFuelTotals[{type}] = {globalFuelTotals[type]}");
            }

            if (remaining <= 0) break;
        }

        return remaining;
    }


    private void UpdateTankUI()
    {
        MarketManager.Instance.UpdateBarrelsPanel();
        TotalGallonsCapacity = tanks.Count * tankCapacity;
        RemainingGallonsCapacity = RecalculateTotalBarrelCapacity();
        TopUIHandler.Instance.SetCapacity(TotalGallonsCapacity, RemainingGallonsCapacity);
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

            int remainingAfterExtract = ExtractFromTanks(TankType.Crude_Oil, gallonsThisTurn);
            int actuallyExtracted = gallonsThisTurn - remainingAfterExtract;

            globalFuelTotals[TankType.Crude_Oil] -= actuallyExtracted;

            gallonsRemaining = actuallyExtracted;
        }

        // Store in tanks
        int leftover = StoreInTanks(type, gallonsRemaining);

        if (leftover > 0)
        {
            Debug.LogWarning($"Wasting {leftover} gallons of {type} — no space left in tanks");
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
        return totalGallons;
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

    public void GameOver()
    {
        GameOverManager.Instance.UpdateTanks(tanks.Count, tankPrice);

        GameOverManager.Instance.UpdateRefinery(producingLots.Count, refineryPrice);

    }
}
