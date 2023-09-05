public class SolarPanelTile : MapTileScript, IEnergyProducer
{
    public float EnergyProduction
    {
        get
        {
            return TownManager.Instance.tileEnergyProduction[tileType];
        }
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
