using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public unsafe struct TransportTile : IComponentData
    {
        public float speed;
        public float timer; // Used by JunctionSystem
        public FixedFloat3Array20 waypoints;
        public TransportTile(float speed)
        {
            this.speed = speed;
            timer = 0;

            waypoints.Clear(float.NaN);
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
            Debug.LogError("Reached the limit of waypoints per tile");
        }
    }
}
