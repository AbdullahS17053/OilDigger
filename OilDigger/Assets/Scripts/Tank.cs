using System.Collections.Generic;
using UnityEngine;

public enum TankType
{
    Crude_Oil,
    Gasoline,
    Jet_Fuel,
    Diesel_Fuel
}

public class Tank
{
    public int MaxCapacity { get; private set; }
    public int RemainingCapacity { get; set; }
// New: track how many gallons of each type this tank holds
    public Dictionary<TankType, int> FuelStored { get; private set; }

    public GameObject VisualInstance;   
    public Transform AssignedSpawnPoint;

    public Tank(int maxCapacity)
    {
        MaxCapacity = maxCapacity;
        RemainingCapacity = maxCapacity;
        FuelStored = new Dictionary<TankType, int>();
    }

    public void StoreFuel(TankType type, int gallons)
    {
        if (!FuelStored.ContainsKey(type))
            FuelStored[type] = 0;

        FuelStored[type] += gallons;
        RemainingCapacity -= gallons;
    }

    public int GetFuelAmount(TankType type)
    {
        return FuelStored.TryGetValue(type, out int value) ? value : 0;
    }
}
