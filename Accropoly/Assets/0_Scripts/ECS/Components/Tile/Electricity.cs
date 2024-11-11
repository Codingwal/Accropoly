using Unity.Entities;

public struct ElectricityProducer : IComponentData
{
    public float production;
}
public struct ElectricityConsumer : IComponentData
{
    public float consumption;
    public bool disableIfElectroless;
}
public struct HasElectricityTag : IComponentData, IEnableableComponent { }