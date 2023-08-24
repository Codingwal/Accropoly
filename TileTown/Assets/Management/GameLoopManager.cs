public class GameLoopManager : Singleton<GameLoopManager>
{
    private void Awake()
    {
        FileHandler.Init();
        LoadWorld();
    }
    private void OnApplicationQuit()
    {
        SaveWorld();
    }

    public void LoadWorld()
    {
        Serializable2DArray<TileType> map = DataHandler.Instance.LoadMap();

        MapHandler.Instance.GenerateTileMap(map);
    }
    public void SaveWorld()
    {
        Serializable2DArray<TileType> map = MapHandler.Instance.SaveTileMap();

        DataHandler.Instance.SaveMap(map);
    }

}