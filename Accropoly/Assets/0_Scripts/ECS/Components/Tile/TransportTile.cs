using Unity.Entities;
using UnityEngine;

namespace Components
{
    public unsafe struct TransportTile : IComponentData
    {
        public float speed;
        public float timer; // Used by JunctionSystem
        public FixedEntityArray30 waypoints;
        public TransportTile(float speed)
        {
            this.speed = speed;
            timer = 0;

            waypoints.Clear(Entity.Null);
        }
        public void AddWaypoint(Entity entity)
        {
            for (int i = 0; i < waypoints.Size; i++)
            {
                if (waypoints[i] == Entity.Null)
                {
                    waypoints[i] = entity;
                    return;
                }
            }
            Debug.LogError("Reached the limit of waypoints per tile");
        }
    }
}
