using System;
using Components;
using Tags;

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
    public void Serialize(PersonData data)
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
            else if (type == typeof(Worker))
            {
                bw.Write((int)PersonComponents.Worker);
                Worker componentData = (Worker)component;

                Serialize(componentData.employer);
            }
            else if (type == typeof(Person))
            {
                bw.Write((int)PersonComponents.Person);
                Person componentData = (Person)component;

                Serialize(componentData.homeTile);
                bw.Write(componentData.age);
            }
            else if (type == typeof(Traveller))
            {
                bw.Write((int)PersonComponents.Traveller);
                Traveller componentData = (Traveller)component;

                bw.Write(componentData.nextWaypointIndex);
                Serialize(componentData.waypoints);
            }
            else if (type == typeof(Travelling)) bw.Write((int)PersonComponents.Travelling);
            else if (type == typeof(WantsToTravel)) bw.Write((int)PersonComponents.WantsToTravel);
            else throw new($"Cannot serialize component of type {type}");
        }
    }
    public void Serialize(Waypoint waypoint)
    {
        Serialize(waypoint.pos);
    }
    public void Serialize(MapData data)
    {
        Serialize(data.tiles);
    }
    public void Serialize(TileData data)
    {
        if (data.components == null) throw new("Failed to serialize tile because Tile.TileComponents is null");

        bw.Write(data.components.Count);
        foreach (var (component, isEnabled) in data.components)
        {
            bw.Write(isEnabled);
            Type type = component.GetType();
            if (type == typeof(Tile))
            {
                bw.Write((int)TileComponents.Tile);

                Tile componentData = (Tile)component;
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
                bw.Write((int)componentData.group);
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
            else if (type == typeof(IsConnected)) bw.Write((int)TileComponents.IsConnectedTag);
            else if (type == typeof(ActiveTile)) bw.Write((int)TileComponents.ActiveTileTag);
            else if (type == typeof(NewTile)) bw.Write((int)TileComponents.NewTileTag);
            else if (type == typeof(BuildingConnector)) bw.Write((int)TileComponents.BuildingConnectorTag);
            else throw new($"Cannot serialize component of type {type}");

        }
    }
    public void Serialize(UserData data)
    {
        bw.Write(data.worldName);
    }
}