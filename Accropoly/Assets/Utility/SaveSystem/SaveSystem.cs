using System.Collections.Generic;
using UnityEngine;

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
        Debug.Log("Initializing SaveSystem");

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
        foreach (KeyValuePair<string, MapData> keyValuePair in MapTemplates.mapTemplates)
        {
            requiredFiles.Add("Templates/" + keyValuePair.Key, keyValuePair.Value);
        }
        InitFileSystem(requiredDirectories, requiredFiles);
    }
    public WorldData GetWorldData(string worldName) { return LoadObject<WorldData>("Saves", worldName); }
    public WorldData GetWorldData() { return GetWorldData(GetWorldName()); }
    public UserData GetUserData() { return LoadObject<UserData>("UserData", "userdata"); }
    public string GetWorldName() { return GetUserData().worldName; }
    public void SaveWorldData(string worldName, WorldData worldData) { SaveObject("Saves", worldName, worldData); }
    public void SaveWorldData(WorldData worldData) { SaveWorldData(GetWorldName(), worldData); }
    public void SaveUserData(UserData userData) { SaveObject("UserData", "userdata", userData); }
    public void UpdateWorldName(string newWorldName)
    {
        var userData = GetUserData();
        userData.worldName = newWorldName;
        SaveUserData(userData);
    }
    public void CreateWorld(string worldName, MapData mapTemplate)
    {
        WorldData worldData = new(mapTemplate);
        SaveWorldData(worldName, worldData);
    }
    public void CreateWorld(string worldName, string mapTemplateName) { CreateWorld(worldName, LoadObject<MapData>("Templates", mapTemplateName)); }
}
