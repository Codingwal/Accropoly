using Unity.Entities;

namespace Components
{
    public unsafe struct TransportTile : IComponentData
    {
        public const int maxWaypoints = 5;
        public const int maxConnections = 10;
        public float speed;
        public fixed int waypoints[maxWaypoints];
        public fixed int connections[maxWaypoints];
        public TransportTile(float speed)
        {
            this.speed = speed;

            for (int i = 0; i < maxWaypoints; i++)
                waypoints[i] = -1;

            for (int i = 0; i < maxConnections; i++)
                connections[i] = -1;
        }
        public int GetWaypoint(int index)
        {
            return waypoints[index];
        }
        public int GetConnection(int index)
        {
            return connections[index];
        }
    }
}
