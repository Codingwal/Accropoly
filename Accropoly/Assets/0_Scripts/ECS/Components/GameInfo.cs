using Unity.Entities;

namespace Components
{
    public struct GameInfo : IComponentData
    {
        public float balance;
        public float deltaTime;
        public float fixedDeltaTime;
        public WorldTime time;
    }
}