using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public unsafe struct TransportTile : IComponentData
    {
        public float speed;
        public FixedFloat3Array10 waypoints;
        public FixedFloat3Array10 connections;
        public TransportTile(float speed)
        {
            this.speed = speed;

            waypoints.Initialize(float.NaN);
            connections.Initialize(float.NaN);
        }
        public void AddWaypoint(float3 pos)
        {
            for (int i = 0; i < waypoints.Size; i++)
            {
                if (math.isnan(waypoints[i].x))
                {
                    waypoints[i] = pos;
                    return;
                }
            }
        }
        public void AddConnection(float3 pos)
        {
            for (int i = 0; i < connections.Size; i++)
            {
                if (math.isnan(connections[i].x))
                {
                    connections[i] = pos;
                    return;
                }
            }
        }
    }
}
