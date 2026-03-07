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

    private void OnDrawGizmos()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        World world = World.DefaultGameObjectInjectionWorld;

        if (debugWaypoints)
            world.GetExistingSystemManaged<WaypointSystem>().DrawGizmos(displayJunctionInfo);

        world.GetExistingSystemManaged<MovementSystem>().DrawGizmos(debugPath, debugCurrentTarget);

        if (debugRaycasts) 
            world.Unmanaged.GetUnsafeSystemRef<CollisionPreventionSystem>(world.GetExistingSystem<CollisionPreventionSystem>())
                .DrawGizmos();
    }
}
