using Systems;
using Unity.Entities;
using UnityEngine;

public class DrawGizmos : MonoBehaviour
{
    WaypointSystem waypointSystem = null;
    private void Start()
    {
        waypointSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<WaypointSystem>();
    }
    private void OnDestroy()
    {
        waypointSystem = null;
    }
    private void OnDrawGizmos()
    {
        if (waypointSystem == null) return;

        waypointSystem.DrawGizmos();
    }
}
