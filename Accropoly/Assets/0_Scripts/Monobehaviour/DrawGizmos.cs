using Systems;
using Unity.Entities;
using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    [Header("Waypoint system")]
    [SerializeField] private bool debugWaypoints;
    [SerializeField] private bool highlightTileExits;
    [SerializeField] private bool displayJunctionInfo;

    [Header("Movement system")]
    [SerializeField] private bool debugPath;
    [SerializeField] private bool debugRaycasts;
    [SerializeField] private bool debugCurrentTarget;

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
            waypointSystem.DrawGizmos(highlightTileExits, displayJunctionInfo);

        movementSystem.DrawGizmos(debugPath, debugRaycasts, debugCurrentTarget);
    }
}
