using System;
using UnityEngine;

public partial class Serializer
{
    public void Serialize(WorldData data)
    {
        bw.Write(data.playTime);
        Serialize(data.cameraSystemPos);
        Serialize(data.cameraSystemRotation);
        bw.Write(data.cameraDistance);
        bw.Write(data.balance);
        Serialize(data.population);
        Serialize(data.map);
    }
    public void Serialize(MapData data)
    {
        Serialize(data.tiles);
    }
    public void Serialize(PersonData data)
    {
        Serialize(data.position);
        Serialize(data.homeTilePos);
        Serialize(data.workplaceTilePos);
        bw.Write(data.hasWorkplace);
    }
    public void Serialize(Tile data)
    {
        bw.Write(data.components.Count);
        foreach (var component in data.components)
        {
            Type type = component.GetType();
            if (type == typeof(MapTileComponent))
            {
                bw.Write((int)Components.MapTileComponent);

                MapTileComponent componentData = (MapTileComponent)component;
                bw.Write((int)componentData.tileType);
                Serialize(componentData.pos);
            }
            else if (type == typeof(AgingTile))
            {
                bw.Write((int)Components.AgingTile);

                AgingTile componentData = (AgingTile)component;
                bw.Write(componentData.age);
            }
            else
                throw new($"Cannot serialize component of type {type}");

        }
    }
    public void Serialize(UserData data)
    {
        bw.Write(data.worldName);
    }
}
