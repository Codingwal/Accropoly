using System;
using Components;
using Components.WaypointComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using WaypointData = WaypointConfigUnmanaged.WaypointData;

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
            throw new NotImplementedException();
        }
        else
        {
            type = "Building";
        }

        if (!ConfigData.waypointConfig.Data.tileToWaypoints.TryGetValue(type, out var tileWaypoints))
        {
            Debug.LogError($"No waypoint data for type \"{type}\"");
            return;
        }

        foreach (var waypoint in tileWaypoints.waypoints)
            CreateWaypoint(waypoint, ref ecb);
    }

    private void CreateWaypoint(WaypointData waypoint, ref EntityCommandBuffer ecb)
    {
        Entity entity = ecb.CreateEntity();
        ecb.AddComponent(entity, LocalTransform.FromPosition(ToWorldSpace(waypoint.position)));
        ecb.AddComponent(entity, new Waypoint { allowedObjects = waypoint.allowedObjects, velocity = waypoint.velocity });
        ecb.AddComponent(entity, new Connections(waypoint.entry, waypoint.exit));

        if (waypoint.junctionData != Junction.JunctionData.None)
            ecb.AddComponent(entity, new Junction(waypoint.junctionData));

        // Convert nextWaypoints to world space
        for (int i = 0; i < waypoint.newWaypointData.nextWaypoints.Size; i++)
        {
            if (math.isnan(waypoint.newWaypointData.nextWaypoints[i].x)) continue;
            waypoint.newWaypointData.nextWaypoints[i] = ToWorldSpace(waypoint.newWaypointData.nextWaypoints[i]);
        }
        ecb.AddComponent(entity, waypoint.newWaypointData);
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