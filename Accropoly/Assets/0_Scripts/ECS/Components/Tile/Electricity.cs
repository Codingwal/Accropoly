using Unity.Entities;

namespace Components
{
    [Save("ElectricityProducer")]
    public struct ElectricityProducer : IComponentData
    {
        public float production;
    }

    [Save("ElectricityConsumer")]
    public struct ElectricityConsumer : IComponentData
    {
        public float consumption;
        public bool disableIfElectroless;
    }
}
namespace Tags
{
    [Save("HasElectricity")]
    public struct HasElectricity : IComponentData, IEnableableComponent { }
}