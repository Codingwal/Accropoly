using Unity.Entities;
using Unity.Mathematics;

public partial class Deserializer
{
    public WorldData Deserialize(WorldData data)
    {
        data.playTime = br.ReadSingle();
        data.cameraSystemPos = Deserialize(data.cameraSystemPos);
        data.cameraSystemRotation = Deserialize(data.cameraSystemRotation);
        data.followOffsetY = br.ReadSingle();
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
        int count = br.Read();
        data.components = new(count);
        for (int i = 0; i < count; i++)
        {
            Components type = (Components)br.Read();
            if (type == Components.MapTileComponent)
            {
                data.components[i] = new MapTileComponent()
                {
                    tileType = (TileType)br.Read(),
                    pos = Deserialize(new float2())
                };
            }
        }
        return data;
    }
    public UserData Deserialize(UserData data)
    {
        data.worldName = br.ReadString();
        return data;
    }
}
