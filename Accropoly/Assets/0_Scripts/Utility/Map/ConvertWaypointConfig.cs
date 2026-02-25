using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using WaypointUnmanaged = WaypointConfigUnmanaged.WaypointData;
using WaypointManaged = WaypointConfigManaged.Waypoint;

public struct ConvertWaypointConfig
{
    private Dictionary<string, WaypointManaged> waypoints;
    public NativeList<WaypointUnmanaged> ConfigWaypointsToWaypointData(WaypointConfigManaged.TileWaypoints tileWaypoints)
    {
        NativeList<WaypointUnmanaged> result = new(Allocator.Persistent);

        waypoints = tileWaypoints.waypoints;

        foreach (var waypoint in waypoints)
            result.Add(WaypointConfigToWaypointData(waypoint.Value));
        return result;
    }
    private WaypointUnmanaged WaypointConfigToWaypointData(WaypointManaged waypoint)
    {
        WaypointUnmanaged data = new()
        {
            position = FloatListToFloat3(waypoint.position),
            allowedObjects = waypoint.allowedObjects,
            velocity = waypoint.velocity,
            junctionData = waypoint.junctionData,
            entry = waypoint.entry,
            exit = waypoint.exit
        };

        data.newWaypointData.nextWaypoints.Clear(float.NaN);
        for (int i = 0; i < waypoint.nextWaypoints.Count; i++)
        {
            if (!waypoints.TryGetValue(waypoint.nextWaypoints[i], out WaypointManaged nextWaypoint))
                Debug.LogError($"Waypoint \"{waypoint.nextWaypoints[i]}\" not found!");

            data.newWaypointData.nextWaypoints[i] = FloatListToFloat3(nextWaypoint.position);
        }

        return data;
    }

    private float3 FloatListToFloat3(List<float> list)
    {
        Debug.Assert(list.Count == 3, $"Expected 3 values but found {list.Count}.");

        return new float3(list[0], list[1], list[2]);
    }
}