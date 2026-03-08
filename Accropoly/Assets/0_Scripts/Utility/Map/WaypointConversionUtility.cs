using System.Collections.Generic;
using Components.WaypointComponents;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class WaypointConversionUtility
{
    public static WaypointConfig ConvertWaypointConfig(WaypointConfigSerialized configSerialized)
    {
        WaypointConfig waypointConfig = new() { tiles = new(10, Allocator.Persistent), elements = new(10, Allocator.Persistent) };

        // Covert tile data
        foreach (var (tileName, tileDataSerialized) in configSerialized.tiles)
        {
            WaypointConfig.TileData tileData = new() { elements = new(Allocator.Persistent) };

            // Convert element references
            foreach (var element in tileDataSerialized.elements)
            {
                tileData.elements.Add(new()
                {
                    name = new(element.name),
                    rotation = (Direction)element.rotation,
                    flip = new(element.flipX, element.flipY)
                });
            }

            waypointConfig.tiles.Add(new(tileName), tileData);
        }

        // Convert element data
        foreach (var (elementName, elementSerialized) in configSerialized.elements)
        {
            WaypointConfig.ElementData elementData = new() { waypoints = new(Allocator.Persistent) };

            foreach (var waypointSerialized in elementSerialized.waypoints)
                elementData.waypoints.Add(ConvertWaypointData(waypointSerialized));

            waypointConfig.elements.Add(new(elementName), elementData);
        }

        return waypointConfig;
    }
    private static WaypointConfig.WaypointData ConvertWaypointData(WaypointConfigSerialized.WaypointData waypointSerialized)
    {
        WaypointConfig.WaypointData waypoint = new()
        {
            position = FloatListToFloat3(waypointSerialized.position),
            waypointData = new(waypointSerialized.allowedObjects, waypointSerialized.velocity),
            connections = new(Allocator.Persistent),
            junctionData = waypointSerialized.junctionData
        };

        // convert waypoint.connections
        foreach (var nextPointSerialized in waypointSerialized.nextWaypoints)
        {
            Connection connection = new()
            {
                nextWaypoint = FloatListToFloat3(nextPointSerialized.position),
            };

            if (nextPointSerialized.controlPoint != null)
                connection.controlPoint = FloatListToFloat3(nextPointSerialized.controlPoint);
            else
                connection.controlPoint = connection.nextWaypoint;

            waypoint.connections.Add(connection);
        }

        return waypoint;
    }

    private static float3 FloatListToFloat3(List<float> list)
    {
        Debug.Assert(list.Count == 3, $"Expected 3 values but found {list.Count}.");

        return new float3(list[0], list[1], list[2]);
    }
}