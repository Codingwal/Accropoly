public class SolarPanelTile : MapTileScript, IEnergyProducer
{
    public float EnergyProduction
    {
        get
        {
            return 30;
        }
    }

    public override void Init()
    {
        base.Init();
        TownManager.Instance.energyProducers.Add(this);
    }
    public override void OnRemove()
    {
        base.OnRemove();
        TownManager.Instance.energyProducers.Remove(this);
    }
}
