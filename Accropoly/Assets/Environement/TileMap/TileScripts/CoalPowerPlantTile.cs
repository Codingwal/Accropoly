using UnityEngine;

public class CoalPowerPlantTile : MapTileScript, IEnergyProducer
{
    public float EnergyProduction
    {
        get
        {
            return TownManager.Instance.tileEnergyProduction[tileType];
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
    public override void Load()
    {
        TownManager.Instance.energyProducers.Add(this);
    }
    public override void OnRemove()
    {
        base.OnRemove();
        TownManager.Instance.energyProducers.Remove(this);
    }

}
