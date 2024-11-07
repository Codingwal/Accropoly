using Unity.Entities;
using Unity.Mathematics;
public partial class Deserializer
{
    public WorldData Deserialize(WorldData data)
    {
        data.playTime = br.ReadSingle();
        data.cameraSystemPos = Deserialize(data.cameraSystemPos);
        data.cameraSystemRotation = Deserialize(data.cameraSystemRotation);
        data.cameraDistance = br.ReadSingle();
        data.balance = br.ReadSingle();
        data.population = Deserialize(data.population);
        data.map = Deserialize(data.map);
        return data;
    }
    public Person Deserialize(Person data)
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
                PersonComponents.PersonComponent => new PersonComponent()
                {
                    homeTile = Deserialize(new int2()),
                    age = br.ReadInt32(),
                },
                _ => throw new($"Cannot deserialize component of type {type}")
            };
            data.components.Add((component, isEnabled));
        }
        return data;
    }
    public MapData Deserialize(MapData data)
    {
        data.tiles = Deserialize(data.tiles);
        return data;
    }
    public Tile Deserialize(Tile data)
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
                TileComponents.MapTileComponent => new MapTileComponent()
                {
                    tileType = (TileType)br.ReadInt32(),
                    pos = Deserialize(new int2()),
                    rotation = (Direction)br.ReadUInt32()
                },
                TileComponents.AgingTile => new AgingTile()
                {
                    age = br.ReadSingle()
                },
                TileComponents.ElectricityProducer => new ElectricityProducer()
                {
                    production = br.ReadSingle()
                },
                TileComponents.ElectricityConsumer => new ElectricityConsumer()
                {
                    consumption = br.ReadSingle()
                },
                TileComponents.BuildingConnector => BuildingConnector.Deserialize(br.ReadInt32()),
                TileComponents.Polluter => new Polluter()
                {
                    pollution = br.ReadSingle()
                },
                TileComponents.Habitat => new Habitat()
                {
                    totalSpace = br.ReadInt32(),
                    freeSpace = br.ReadInt32()
                },

                TileComponents.IsConnectedTag => new IsConnectedTag(),
                TileComponents.ActiveTileTag => new ActiveTileTag(),
                TileComponents.NewTileTag => new NewTileTag(),

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
