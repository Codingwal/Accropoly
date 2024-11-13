using System;

public partial class Serializer
{
    public void Serialize(WorldData data)
    {
        Serialize(data.time);
        Serialize(data.cameraSystemPos);
        Serialize(data.cameraSystemRotation);
        bw.Write(data.cameraDistance);
        bw.Write(data.balance);
        Serialize(data.population);
        Serialize(data.map);
    }
    public void Serialize(WorldTime data)
    {
        bw.Write(data.day);
        bw.Write(data.seconds);
    }
    public void Serialize(MapData data)
    {
        Serialize(data.tiles);
    }
    public void Serialize(Person data)
    {
        if (data.components == null) throw new("Failed to serialize tile because Tile.TileComponents is null");

        bw.Write(data.components.Count);
        foreach (var (component, isEnabled) in data.components)
        {
            bw.Write(isEnabled);
            Type type = component.GetType();
            if (type == typeof(PosComponent))
            {
                bw.Write((int)PersonComponents.PosComponent);
                PosComponent componentData = (PosComponent)component;

                Serialize(componentData.pos);
            }
            else if (type == typeof(PersonComponent))
            {
                bw.Write((int)PersonComponents.PersonComponent);
                PersonComponent componentData = (PersonComponent)component;

                Serialize(componentData.homeTile);
                bw.Write(componentData.age);
            }
            else if (type == typeof(Worker))
            {
                bw.Write((int)PersonComponents.Worker);
                Worker componentData = (Worker)component;

                Serialize(componentData.employer);
            }
            else throw new($"Cannot serialize component of type {type}");
        }
    }
    public void Serialize(Tile data)
    {
        if (data.components == null) throw new("Failed to serialize tile because Tile.TileComponents is null");

        bw.Write(data.components.Count);
        foreach (var (component, isEnabled) in data.components)
        {
            bw.Write(isEnabled);
            Type type = component.GetType();
            if (type == typeof(MapTileComponent))
            {
                bw.Write((int)TileComponents.MapTileComponent);

                MapTileComponent componentData = (MapTileComponent)component;
                bw.Write((int)componentData.tileType);
                Serialize(componentData.pos);
                bw.Write((uint)componentData.rotation);
            }
            else if (type == typeof(AgingTile))
            {
                bw.Write((int)TileComponents.AgingTile);

                AgingTile componentData = (AgingTile)component;
                bw.Write(componentData.age);
            }
            else if (type == typeof(ElectricityProducer))
            {
                bw.Write((int)TileComponents.ElectricityProducer);

                ElectricityProducer componentData = (ElectricityProducer)component;
                bw.Write(componentData.production);
            }
            else if (type == typeof(ElectricityConsumer))
            {
                bw.Write((int)TileComponents.ElectricityConsumer);

                ElectricityConsumer componentData = (ElectricityConsumer)component;
                bw.Write(componentData.consumption);
                bw.Write(componentData.disableIfElectroless);
            }
            else if (type == typeof(ConnectingTile))
            {
                bw.Write((int)TileComponents.ConnectingTile);

                ConnectingTile componentData = (ConnectingTile)component;
                bw.Write(componentData.Serialize());
            }
            else if (type == typeof(Polluter))
            {
                bw.Write((int)TileComponents.Polluter);

                Polluter componentData = (Polluter)component;
                bw.Write(componentData.pollution);
            }
            else if (type == typeof(Habitat))
            {
                bw.Write((int)TileComponents.Habitat);

                Habitat componentData = (Habitat)component;
                bw.Write(componentData.totalSpace);
                bw.Write(componentData.freeSpace);
            }
            else if (type == typeof(Employer))
            {
                bw.Write((int)TileComponents.Employer);

                Employer componentData = (Employer)component;
                bw.Write(componentData.totalSpace);
                bw.Write(componentData.freeSpace);
            }
            else if (type == typeof(IsConnectedTag)) bw.Write((int)TileComponents.IsConnectedTag);
            else if (type == typeof(ActiveTileTag)) bw.Write((int)TileComponents.ActiveTileTag);
            else if (type == typeof(NewTileTag)) bw.Write((int)TileComponents.NewTileTag);
            else throw new($"Cannot serialize component of type {type}");

        }
    }
    public void Serialize(UserData data)
    {
        bw.Write(data.worldName);
    }
}