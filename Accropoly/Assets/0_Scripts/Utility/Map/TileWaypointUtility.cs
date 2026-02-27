using Components;
using Components.WaypointComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct TileWaypointUtility
{
    public Tile tile;
    public LocalTransform transform;
    public ConnectingTile? connectingTile;
    public TileWaypointUtility(Tile tile, LocalTransform transform, ConnectingTile? connectingTile)
    {
        this.tile = tile;
        this.transform = transform;
        this.connectingTile = connectingTile;
    }
    public void CreateWaypoints(ref EntityCommandBuffer ecb)
    {
        FixedString32Bytes type;
        if (connectingTile.HasValue)
        {
            type = connectingTile.Value.GetIndex() switch
            {
                ConnectingTile.notConnected => "NotConnected",
                ConnectingTile.deadEnd => "DeadEnd",
                ConnectingTile.straight => "Straight",
                ConnectingTile.curve => "Curve",
                ConnectingTile.tJunction => "TJunction",
                ConnectingTile.junction => "Junction",
                _ => throw new("Invalid connecting tile index")
            };
        }
        else if (tile.tileType == TileType.Bitumen)
        {
            type = "Plaza";
        }
        else
        {
            type = "Building";
        }

        if (!ConfigData.waypointConfig.Data.tiles.TryGetValue(type, out var tileData))
        {
            Debug.LogError($"No waypoint data for type \"{type}\"");
            return;
        }

        foreach (var element in tileData.elements)
        {
            if (!ConfigData.waypointConfig.Data.elements.TryGetValue(element.name, out var elementData))
                Debug.LogError($"Element {element.name} does not exist.");

            foreach (var waypoint in elementData.waypoints)
                CreateWaypoint(waypoint, element.rotation, ref ecb);
        }
    }

    private void CreateWaypoint(WaypointConfig.WaypointData waypointData, Direction elementRotation, ref EntityCommandBuffer ecb)
    {
        Entity entity = ecb.CreateEntity();

        // Rotate position according to elementRotation and convert it to world space, then set the transform
        float3 position = ToWorldSpace(Rotate(waypointData.position, elementRotation));
        ecb.AddComponent(entity, LocalTransform.FromPosition(position));

        // Add waypointData (velocity & allowedObjects)
        ecb.AddComponent(entity, waypointData.waypointData);

        // Add junction component (optional)
        if (waypointData.junctionData != Junction.JunctionData.None)
            ecb.AddComponent(entity, new Junction(waypointData.junctionData));

        // Update next waypoints data: Rotate according to elementRotation and convert to world space
        for (int i = 0; i < waypointData.connections.nextWaypoints.Length; i++)
        {
            NextPoint next = waypointData.connections.nextWaypoints[i];
            next.position = ToWorldSpace(Rotate(next.position, elementRotation));
            next.controlPoint = ToWorldSpace(Rotate(next.controlPoint, elementRotation));

            waypointData.connections.nextWaypoints[i] = next;
        }
        ecb.AddComponent(entity, waypointData.connections);

        ecb.AddComponent<NewWaypoint>(entity);
    }

    private float3 ToWorldSpace(float3 pos)
    {
        pos = Rotate(pos, tile.rotation);
        pos += transform.Position;
        return pos;
    }

    private float3 Rotate(float3 pos, Direction dir)
    {
        pos = math.rotate(quaternion.EulerXYZ(0, dir.ToRadians(), 0), pos);
        pos = math.round(pos * 100) / 100; // Round to precision of 0.01
        return pos;
    }
}