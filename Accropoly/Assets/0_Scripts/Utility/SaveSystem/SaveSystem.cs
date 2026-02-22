using System.Collections.Generic;
using System.IO;
using Unity.Entities;
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

        Debug.Log("Loading config data");
        ConfigData.saveSystemConfig = ReadJsonConfig<SaveSystemConfig>("SaveSystemConfig");
        ConfigData.tileConfig = ReadJsonConfig<TileConfig>("TileConfig");   
        ConfigData.populationConfig = ReadJsonConfig<PopulationConfig>("PopulationConfig");
        ConfigData.cameraConfig = ReadJsonConfig<CameraConfig>("CameraConfig");
        ConfigData.timeConfig = ReadJsonConfig<TimeConfig>("TimeConfig");

        Debug.Log("Initializing user data");

        string[] requiredDirectories =
        {
            "UserData",
            "Templates",
            "Saves"
        };
        UserData defaultUserData = UserData.Default;
        Dictionary<string, object> requiredFiles = new()
        {
            {"UserData/userdata", defaultUserData},
        };

        if (ConfigData.saveSystemConfig.deleteTemplates)
            DeleteDirectoryContent("Templates");

        InitFileSystem(requiredDirectories, requiredFiles, ConfigData.saveSystemConfig.overwriteFiles);

        if (ConfigData.saveSystemConfig.deleteSaves)
            DeleteDirectoryContent("Saves");
    }
    public static void Initialize() { _instance = new(); }
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
    public void SaveTemplate(MapData templateData, string newTemplateName) { SaveObject("Templates", newTemplateName, templateData); }

    private T ReadJsonConfig<T>(string fileName)
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, fileName + ".json");

        if (!File.Exists(configPath))
        {
            Debug.LogError($"Config file \"{fileName}.json\" not found!");
            return default;
        }

        string str = File.ReadAllText(configPath);
        return JsonUtility.FromJson<T>(str);
    }

    private void WriteJsonConfig(string fileName, object config)
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, fileName + ".json");
        string str = JsonUtility.ToJson(config, true);
        File.WriteAllText(configPath, str);
    }
}
