public partial class Serializer
{
    public void Serialize(WorldData data)
    {
        bw.Write(data.playTime);
        Serialize(data.cameraSystemPos);
        Serialize(data.cameraSystemRotation);
        bw.Write(data.followOffsetY);
        bw.Write(data.balance);
        Serialize(data.population);
        Serialize(data.map);
    }
    public void Serialize(MapData data)
    {
        Serialize(data.size);
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
        bw.Write((int)data.tileType);
        bw.Write(data.direction);
    }
    public void Serialize(UserData data)
    {
        bw.Write(data.worldName);
    }
}
