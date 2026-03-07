using Systems;
using Unity.Entities;
using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    [Header("Waypoint system")]
    [SerializeField] private bool debugWaypoints;
    [SerializeField] private bool displayJunctionInfo;

    [Header("Movement system")]
    [SerializeField] private bool debugPath;
    [SerializeField] private bool debugRaycasts;
    [SerializeField] private bool debugCurrentTarget;

    WaypointSystem waypointSystem = null;
    MovementSystem movementSystem = null;
    SystemHandle? collisionPreventionSystem = null;
    // private void Start()
    // {
    //     waypointSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<WaypointSystem>();
    //     movementSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<MovementSystem>();
    //     collisionPreventionSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<CollisionPreventionSystem>();
    // }
    // private void OnDestroy()
    // {
    //     waypointSystem = null;
    //     movementSystem = null;
    //     collisionPreventionSystem = null;
    // }
    // private void OnDrawGizmos()
    // {
    //     if (waypointSystem == null) return;
    //     if (movementSystem == null) return;
    //     if (collisionPreventionSystem == null) return;

    //     if (debugWaypoints)
    //         waypointSystem.DrawGizmos(displayJunctionInfo);

    //     movementSystem.DrawGizmos(debugPath, debugCurrentTarget);

    //     if (debugRaycasts)
    //         World.DefaultGameObjectInjectionWorld.Unmanaged.GetUnsafeSystemRef<CollisionPreventionSystem>(collisionPreventionSystem.Value)
    //             .DrawGizmos();
    // }
}
