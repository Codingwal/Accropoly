using Unity.Entities;

namespace Components
{
    public struct TransportTile : IComponentData
    {
        public float speed;
        public TransportTile(float speed)
        {
            this.speed = speed;
        }
    }
}
