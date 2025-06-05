using Unity.Entities;
using Unity.Mathematics;
using Components;
using Tags;
using Unity.Collections.LowLevel.Unsafe;

public partial class Deserializer
{
    public WorldData Deserialize(WorldData data)
    {
        data.time = Deserialize(new WorldTime());
        data.cameraSystemPos = Deserialize(data.cameraSystemPos);
        data.cameraSystemRotation = Deserialize(data.cameraSystemRotation);
        data.cameraDistance = br.ReadSingle();
        data.balance = br.ReadSingle();
        data.population = Deserialize(data.population);
        data.map = Deserialize(data.map);
        return data;
    }
    public WorldTime Deserialize(WorldTime data)
    {
        data.day = br.ReadInt32();
        data.hours = br.ReadInt32();
        data.minutes = br.ReadInt32();
        data.seconds = br.ReadSingle();
        return data;
    }
    public PersonData Deserialize(PersonData data)
    {
        int count = br.ReadInt32();
        data.components = new(count);

        // For each component...
        for (int i = 0; i < count; i++)
        {
            bool isEnabled = br.ReadBoolean();
            IComponentData component;

            PersonComponents type = (PersonComponents)br.ReadInt32();
            component = type switch
            {
                PersonComponents.PosComponent => new PosComponent()
                {
                    pos = Deserialize(new float3()),
                },
                PersonComponents.Person => new Person()
                {
                    homeTile = Deserialize(new int2()),
                    age = br.ReadInt32(),
                },
                PersonComponents.Worker => new Worker()
                {
                    employer = Deserialize(new int2()),
                },
                PersonComponents.Traveller => new Traveller()
                {
                    nextWaypointIndex = br.ReadInt32(),
                    waypoints = Deserialize(new UnsafeList<Waypoint>()),
                },
                PersonComponents.Travelling => new Travelling(),
                PersonComponents.WantsToTravel => new WantsToTravel(),
                _ => throw new($"Cannot deserialize component of type {type}")
            };
            data.components.Add((component, isEnabled));
        }
        return data;
    }
    public Waypoint Deserialize(Waypoint data)
    {
        data.pos = Deserialize(new int2());
        return data;
    }
    public MapData Deserialize(MapData data)
    {
        data.tiles = Deserialize(data.tiles);
        return data;
    }
    public TileData Deserialize(TileData data)
    {
        int count = br.ReadInt32();
        data.components = new(count);

        // For each component...
        for (int i = 0; i < count; i++)
        {
            bool isEnabled = br.ReadBoolean();
            IComponentData component;

            TileComponents type = (TileComponents)br.ReadInt32();
            component = type switch
            {
                TileComponents.Tile => new Tile()
                {
                    tileType = (TileType)br.ReadInt32(),
                    pos = Deserialize(new int2()),
                    rotation = (Direction)br.ReadUInt32()
                },
                TileComponents.GrowingTile => new GrowingTile()
                {
                    age = br.ReadSingle()
                },
                TileComponents.ElectricityProducer => new ElectricityProducer()
                {
                    production = br.ReadSingle()
                },
                TileComponents.ElectricityConsumer => new ElectricityConsumer()
                {
                    consumption = br.ReadSingle(),
                    disableIfElectroless = br.ReadBoolean()
                },
                TileComponents.ConnectingTile => new ConnectingTile
                {
                    group = (ConnectingTileGroup)br.ReadInt32(),
                },
                TileComponents.Polluter => new Polluter()
                {
                    pollution = br.ReadSingle()
                },
                TileComponents.Habitat => new Habitat()
                {
                    totalSpace = br.ReadInt32(),
                    freeSpace = br.ReadInt32()
                },
                TileComponents.Employer => new Employer()
                {
                    totalSpace = br.ReadInt32(),
                    freeSpace = br.ReadInt32()
                },
                TileComponents.TransportTile => new TransportTile()
                {
                    speed = br.ReadSingle()
                },

                TileComponents.IsConnectedTag => new IsConnected(),
                TileComponents.ActiveTileTag => new ActiveTile(),
                TileComponents.NewTileTag => new NewTile(),
                TileComponents.BuildingConnectorTag => new BuildingConnector(),
                TileComponents.DisabledTileTag => new DisabledTile(),

                _ => throw new($"Cannot deserialize component of type {type}")
            };

            data.components.Add((component, isEnabled));
        }
        return data;
    }
    public UserData Deserialize(UserData data)
    {
        data.worldName = br.ReadString();
        return data;
    }
}
