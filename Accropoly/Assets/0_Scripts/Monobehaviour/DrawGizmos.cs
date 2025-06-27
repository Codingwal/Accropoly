using Systems;
using Unity.Entities;
using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    [SerializeField] private bool debugWaypoints;
    [SerializeField] private bool debugTravelling;

    WaypointSystem waypointSystem = null;
    MovementSystem movementSystem = null;
    private void Start()
    {
        waypointSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<WaypointSystem>();
        movementSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<MovementSystem>();
    }
    private void OnDestroy()
    {
        waypointSystem = null;
        movementSystem = null;
    }
    private void OnDrawGizmos()
    {
        if (waypointSystem == null) return;
        if (movementSystem == null) return;

        if (debugWaypoints)
            waypointSystem.DrawGizmos();

        if (debugTravelling)
            movementSystem.DrawGizmos();
    }
}
