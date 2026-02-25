using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.Collections;
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
        ConfigData.saveSystemConfig.Data = ReadJsonConfig<SaveSystemConfig>("SaveSystemConfig");
        ConfigData.tileConfig.Data = ReadJsonConfig<TileConfig>("TileConfig");
        ConfigData.populationConfig.Data = ReadJsonConfig<PopulationConfig>("PopulationConfig");
        ConfigData.cameraConfig.Data = ReadJsonConfig<CameraConfig>("CameraConfig");
        ConfigData.timeConfig.Data = ReadJsonConfig<TimeConfig>("TimeConfig");

        // Get config data for each type of tile, convert it to unmanaged data and store it together with the type name
        ConfigData.waypointConfig.Data.tileToWaypoints = new(10, Allocator.Persistent);
        foreach (string file in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "WaypointConfig")))
        {
            if (file.Contains(".meta"))
                continue;

            string fileText = File.ReadAllText(file);
            var tileWaypointsManaged = JsonConvert.DeserializeObject<WaypointConfigManaged.TileWaypoints>(fileText);
            var tileWaypointsUnmanaged = new ConvertWaypointConfig().ConfigWaypointsToWaypointData(tileWaypointsManaged);

            Debug.Assert(GetFileName(file).Length < 32, $"File name \"{GetFileName(file)}\"is too long");
            FixedString32Bytes type = GetFileName(file);

            ConfigData.waypointConfig.Data.tileToWaypoints.Add(type, new WaypointConfigUnmanaged.TileWaypoints { waypoints = tileWaypointsUnmanaged });
        }

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

        if (ConfigData.saveSystemConfig.Data.deleteTemplates)
            DeleteDirectoryContent("Templates");

        InitFileSystem(requiredDirectories, requiredFiles, ConfigData.saveSystemConfig.Data.overwriteFiles);

        if (ConfigData.saveSystemConfig.Data.deleteSaves)
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
        return JsonConvert.DeserializeObject<T>(str);
    }

    private void WriteJsonConfig(string fileName, object config)
    {
        string configPath = Path.Combine(Application.streamingAssetsPath, fileName + ".json");
        string str = JsonConvert.SerializeObject(config);
        File.WriteAllText(configPath, str);
    }
}
