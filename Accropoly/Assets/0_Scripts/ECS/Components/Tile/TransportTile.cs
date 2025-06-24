using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;

namespace Components
{
    public unsafe struct TransportTile : IComponentData
    {
        public const int maxWaypoints = 10;
        public const int maxConnections = 8;
        public float speed;
        public fixed int waypoints[maxWaypoints];
        public fixed float connections[maxConnections * 3]; // float3 connections[maxConnections]
        public TransportTile(float speed)
        {
            this.speed = speed;

            for (int i = 0; i < maxWaypoints; i++)
                waypoints[i] = -1;

            for (int i = 0; i < maxConnections * 3; i++)
                connections[i] = -1;
        }
        public void AddWaypoint(int waypointIndex)
        {
            for (int i = 0; i < maxWaypoints; i++)
            {
                if (waypoints[i] == -1)
                {
                    waypoints[i] = waypointIndex;
                    return;
                }
            }
        }
        public int GetWaypoint(int index)
        {
            return waypoints[index];
        }
        public void AddConnection(float3 connectionPos)
        {
            for (int i = 0; i < maxConnections * 3; i += 3)
            {
                if (connections[i] == -1)
                {
                    connections[i] = connectionPos.x;
                    connections[i] = connectionPos.y;
                    connections[i] = connectionPos.z;
                    return;
                }
            }
        }
        public float3 GetConnection(int index)
        {
            return new(connections[index * 3], connections[index * 3 + 1], connections[index * 3 + 2]);
        }
    }
}
