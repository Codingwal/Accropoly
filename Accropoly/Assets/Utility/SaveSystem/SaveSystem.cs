using System.Collections.Generic;

public class SaveSystem : FileHandler
{
    private static SaveSystem _instance = null;
    public static SaveSystem Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }
    private SaveSystem()
    {
        string[] requiredDirectories =
        {
            "UserData",
            "Templates",
            "Saves"
        };
        Dictionary<string, object> requiredFiles = new()
        {
            {"UserData/userdata", UserData.Default},
        };
        foreach (KeyValuePair<string, Serializable2DArray<Tile>> keyValuePair in MapTemplates.mapTemplates)
        {
            requiredFiles.Add("Templates/" + keyValuePair.Key, keyValuePair.Value);
        }
        InitFileSystem(requiredDirectories, requiredFiles);
    }
    public WorldData GetWorldData(string worldName) { return LoadObject<WorldData>("Saves", worldName); }
    public WorldData GetWorldData() { return GetWorldData(GetUserData().worldName); }
    public UserData GetUserData() { return LoadObject<UserData>("UserData", "userdata"); }
    public void SaveWorldData(string worldName, WorldData worldData) { SaveObject("Saves", worldName, worldData); }
    public void SaveWorldData(WorldData worldData) { SaveWorldData(GetUserData().worldName, worldData); }
    public void SaveUserData(UserData userData) { SaveObject("UserData", "userdata", userData); }
}
