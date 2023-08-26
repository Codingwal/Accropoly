using UnityEngine;

public class DataHandler : SingletonPersistant<DataHandler>
{
    public World LoadWorld()
    {
        string mapName = GetWorldName();
        return FileHandler.LoadObject<World>("Saves", mapName);
    }
    public void SaveWorld(World world)
    {
        string worldName = GetWorldName();

        FileHandler.SaveObject("Saves", worldName, world);
    }
    public void CreateWorld(string mapTemplateName)
    {
        Serializable2DArray<TileType> mapTemplate = FileHandler.LoadObject<Serializable2DArray<TileType>>("Templates", mapTemplateName);

        World world = new(mapTemplate);

        FileHandler.SaveObject("Saves", GetWorldName(), world);
    }
    public void ChangeWorldName(string newMapName)
    {
        UserData userData = GetUserData();

        userData.worldName = newMapName;

        SaveUserData(userData);
    }
    public string GetWorldName()
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
