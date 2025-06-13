using UnityEngine;

public enum TankType
{
    Crude_Oil,
    Gasoline,
    Diesel_Fuel,
    Jet_Fuel
}

public class Tank
{
    public TankType Type { get; private set; }
    public int MaxCapacity { get; private set; }
    public int RemainingCapacity { get; set; }

    public Tank(TankType _type, int _maxCapacity)
    {
        Type = _type;
        MaxCapacity = _maxCapacity;
        RemainingCapacity = _maxCapacity;

    }

    public int GallonsStored => MaxCapacity - RemainingCapacity;
}
