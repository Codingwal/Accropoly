using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;

public class HouseTile : MapTileScript, IHouseTile, IEnergyConsumer
{
    // Inhabitants
    public List<GameObject> Inhabitants { get; set; } = new();

    // Energy consumption
    public float EnergyConsumption
    {
        get
        {
            return Inhabitants.Count * TownManager.Instance.electricityUsagePerPerson;
        }
    }

    public override bool CanPersist()
    {
        GameObject neighbour;

        neighbour = MapHandler.GetTileFromNeighbour(TilePos, transform.eulerAngles.y);

        if (neighbour == null) return false;

        if (!neighbour.TryGetComponent(out IHouseConnectable houseConnectableScript))
        {
            return false;
        }
        if (houseConnectableScript.ArableTiles.Contains(TilePos))
        {
            return true;
        }
        return false;
    }
    public override void Init()
    {
        TownManager.Instance.NewHouse(tileType, TilePos);
        Load();
    }
    public override void Load()
    {
        TownManager.Instance.energyConsumers.Add(this);
    }
    public override void OnRemove()
    {
        base.OnRemove();

        for (int i = 0; i < Inhabitants.Count; i++)
        {
            TownManager.Instance.RemovePerson(Inhabitants[i].GetComponent<IPerson>());
        }

        TownManager.Instance.energyConsumers.Remove(this);
    }
}
