using Systems;
using Unity.Entities;
using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    [Header("Waypoint system")]
    [SerializeField] private bool debugWaypoints;

    [Header("Movement system")]
    [SerializeField] private bool debugPath;
    [SerializeField] private bool debugRaycasts;

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

        movementSystem.DrawGizmos(debugPath, debugRaycasts);
    }
}
