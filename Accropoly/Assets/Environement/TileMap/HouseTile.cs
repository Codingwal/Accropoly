using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;

public class HouseTile : MapTileScript, IHouseTile
{
    // Inhabitants
    public List<GameObject> Inhabitants { get; set; } = new();
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
        TownManager.Instance.NewHouse(HouseSize.normal, TilePos);
    }
    public override void OnRemove()
    {
        base.OnRemove();
        for (int i = 0; i < Inhabitants.Count; i++)
        {
            TownManager.Instance.RemovePerson(Inhabitants[i]);
        }
    }
}
