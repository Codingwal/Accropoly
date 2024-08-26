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
        data.size = Deserialize(data.size);
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
        data.tileType = (TileType)br.ReadInt32();
        data.direction = br.ReadInt32();
        return data;
    }
    public UserData Deserialize(UserData data)
    {
        data.worldName = br.ReadString();
        return data;
    }
}
