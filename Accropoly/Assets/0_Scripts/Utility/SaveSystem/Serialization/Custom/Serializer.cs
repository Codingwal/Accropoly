using System;

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
        if (data.components == null) throw new("Failed to serialize tile because Tile.components is null");

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
                Serialize(componentData.rotation);
            }
            else if (type == typeof(AgingTile))
            {
                bw.Write((int)Components.AgingTile);

                AgingTile componentData = (AgingTile)component;
                bw.Write(componentData.age);
            }
            else if (type == typeof(ElectricityProducer))
            {
                bw.Write((int)Components.ElectricityProducer);

                ElectricityProducer componentData = (ElectricityProducer)component;
                bw.Write(componentData.production);
            }
            else if (type == typeof(ElectricityConsumer))
            {
                bw.Write((int)Components.ElectricityConsumer);

                ElectricityConsumer componentData = (ElectricityConsumer)component;
                bw.Write(componentData.consumption);
            }
            else if (type == typeof(HasElectricityTag))
            {
                
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
