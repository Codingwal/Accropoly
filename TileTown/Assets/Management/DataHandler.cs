using UnityEngine;

public class DataHandler : Singleton<DataHandler>
{
    public Serializable2DArray<TileType> LoadMap()
    {
        string mapName = GetMapName();
        return FileHandler.LoadObject<Serializable2DArray<TileType>>("Saves", mapName);
    }
    public void SaveMap(Serializable2DArray<TileType> map)
    {
        string mapName = GetMapName();

        FileHandler.SaveObject("Saves", mapName, map);
    }
    public void ChangeMapName(string newMapName)
    {
        UserData userData = GetUserData();

        userData.worldName = newMapName;

        SaveUserData(userData);
    }
    public string GetMapName()
    {
        return GetUserData().worldName;
    }
    private UserData GetUserData()
    {
        return FileHandler.LoadObject<UserData>("UserData", "userdata");
    }
    private void SaveUserData(UserData userData)
    {
        FileHandler.SaveObject("UserData", "userdata", userData);
    }
}
