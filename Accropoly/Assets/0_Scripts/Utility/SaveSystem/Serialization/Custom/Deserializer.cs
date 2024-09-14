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

    public MapData Deserialize(MapData data)
    {
        data.tiles = Deserialize(data.tiles);
        return data;
    }
    public PersonData Deserialize(PersonData data)
    {
        data.position = Deserialize(data.position);
        data.homeTilePos = Deserialize(data.homeTilePos);
        data.workplaceTilePos = Deserialize(data.workplaceTilePos);
        data.hasWorkplace = br.ReadBoolean();
        return data;
    }
    public Tile Deserialize(Tile data)
    {
        int count = br.ReadInt32();
        data.components = new(count);
        for (int i = 0; i < count; i++)
        {
            Components type = (Components)br.ReadInt32();
            if (type == Components.MapTileComponent)
            {
                data.components.Add(new MapTileComponent()
                {
                    tileType = (TileType)br.ReadInt32(),
                    pos = Deserialize(new int2()),
                    rotation = br.ReadInt32()
                });
            }
            else if (type == Components.AgingTile)
            {
                data.components.Add(new AgingTile()
                {
                    age = br.ReadSingle()
                });
            }
            else
                throw new($"Cannot deserialize component of type {type}");

        }
        return data;
    }
    public UserData Deserialize(UserData data)
    {
        data.worldName = br.ReadString();
        return data;
    }
}