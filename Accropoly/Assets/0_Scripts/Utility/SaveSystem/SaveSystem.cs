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
        LoadWaypointConfig();


        Debug.Log("Initializing user data");

        string[] requiredDirectories =
        {
            "UserData",
            "Templates",
            "Saves"
        };

        if (ConfigData.saveSystemConfig.Data.deleteTemplates)
            DeleteDirectoryContent("Templates");

        InitFileSystem(requiredDirectories);

        SaveObject("UserData", "userdata", UserData.Default, ConfigData.saveSystemConfig.Data.overwriteFiles);

        if (ConfigData.saveSystemConfig.Data.deleteSaves)
            DeleteDirectoryContent("Saves");
    }

    private void LoadWaypointConfig()
    {
        WaypointConfigSerialized waypointConfigSerialized = new() { elements = new(), tiles = new() };

        // Load tile data
        foreach (string filePath in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "WaypointConfig", "Tiles")))
        {
            if (filePath.Contains(".meta"))
                continue;

            string fileName = GetFileName(filePath);
            string fileText = File.ReadAllText(filePath);
            var tileData = JsonConvert.DeserializeObject<WaypointConfigSerialized.TileData>(fileText);
            waypointConfigSerialized.tiles.Add(fileName, tileData);
        }

        // Load element data
        foreach (string filePath in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "WaypointConfig", "Elements")))
        {
            if (filePath.Contains(".meta"))
                continue;

            string fileName = GetFileName(filePath);
            string fileText = File.ReadAllText(filePath);
            var elementData = JsonConvert.DeserializeObject<WaypointConfigSerialized.ElementData>(fileText);
            waypointConfigSerialized.elements.Add(fileName, elementData);
        }

        // Convert serialized config data to unmanaged config data
        ConfigData.waypointConfig.Data = WaypointConversionUtility.ConvertWaypointConfig(waypointConfigSerialized);
    }

    public static void Initialize() { _instance = new(); }
    public WorldData GetWorldData(string worldName) { return LoadObject<WorldData>("Saves", worldName); }
    public WorldData GetWorldData() { return GetWorldData(GetWorldName()); }
    public UserData GetUserData() { return LoadObject<UserData>("UserData", "userdata"); }
    public string GetWorldName() { return GetUserData().worldName.ToString(); }
    public void SaveWorldData(string worldName, WorldData worldData) { SaveObject("Saves", worldName, worldData); }
    public void SaveWorldData(WorldData worldData) { SaveWorldData(GetWorldName(), worldData); }
    public void SaveUserData(UserData userData) { SaveObject("UserData", "userdata", userData); }
    public void UpdateWorldName(string newWorldName)
    {
        var userData = GetUserData();
        userData.worldName = newWorldName;
        SaveUserData(userData);
    }
    public void CreateWorld(string worldName, string templateName) { SaveWorldData(worldName, LoadObject<WorldData>("Templates", templateName)); }
    public void SaveTemplate(WorldData template, string name) { SaveObject("Templates", name, template); }

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
