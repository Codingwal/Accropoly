using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficeBuildingTile : MapTileScript, IWorkplace, IEnergyConsumer
{
    public List<IPerson> Workers { get; set; } = new();
    private int maxWorkerCount = 10;
    public float EnergyConsumption
    {
        get
        {
            return 10 + Workers.Count * TownManager.Instance.electricityUsagePerWorker;
        }
    }
    public bool AddWorker(IPerson person)
    {
        if (Workers.Count == maxWorkerCount) return false;

        Workers.Add(person);
        return true;
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
        base.Init();

        TownManager.Instance.workplaces.Add(this);
    }
    public override void Load()
    {
        TownManager.Instance.energyConsumers.Add(this);
    }
    public override void OnRemove()
    {
        TownManager.Instance.energyConsumers.Remove(this);
        TownManager.Instance.workplaces.Remove(this);

        foreach (IPerson worker in Workers)
        {
            worker.LeaveWorkplace();
        }
    }
}
