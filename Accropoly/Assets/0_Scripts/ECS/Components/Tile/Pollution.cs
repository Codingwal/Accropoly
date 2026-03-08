using Unity.Entities;

namespace Components
{
    [Save("Polluter")]
    public struct Polluter : IComponentData
    {
        public float pollution;
    }
}