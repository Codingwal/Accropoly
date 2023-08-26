using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class FileHandler
{
    public static void Init()
    {
        string[] requiredDirectories =
        {
            "UserData",
            "Templates",
            "Saves"
        };
        Dictionary<string, IInstantiatable> requiredFiles = new Dictionary<string, IInstantiatable>()
        {
            {"UserData/userdata", new UserData()},
        };

        foreach (KeyValuePair<string, Serializable2DArray<TileType>> keyValuePair in MapTemplates.mapTemplates)
        {
            requiredFiles.Add("Templates/" + keyValuePair.Key, keyValuePair.Value);
        }

        // Generate folders and files
        foreach (string directory in requiredDirectories)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/data/{directory}/"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/data/{directory}/");
            }
        }
        foreach (string file in requiredFiles.Keys)
        {
            if (!File.Exists($"{Application.persistentDataPath}/data/{file}.json"))
            {
                // File.Create($"{Application.persistentDataPath}/data/{file}.json");
                File.WriteAllText($"{Application.persistentDataPath}/data/{file}.json", JsonUtility.ToJson(requiredFiles[file]));
            }
        }
    }
    public static string[] ListFiles(string directory)
    {
        string dataPath = $"{Application.persistentDataPath}/data/{directory}/";

        string[] files = Directory.GetFiles(dataPath);

        for (int i = 0; i < files.Length; i++)
        {
            int startPoint = files[i].LastIndexOf("/") + 1;
            int endPoint = files[i].IndexOf(".");

            files[i] = files[i][startPoint..endPoint];
        }
        return files;
    }
    public static void SaveObject<T>(string directory, string name, T obj)
    {
        string content = JsonUtility.ToJson(obj);

        string dataPath = $"{Application.persistentDataPath}/data/{directory}/{name}.json";

        File.WriteAllText(dataPath, content);
    }
    public static T LoadObject<T>(string directory, string name)
    {
        string dataPath = $"{Application.persistentDataPath}/data/{directory}/{name}.json";

        T obj;

        try
        {
            string fileContent = File.ReadAllText(dataPath);
            obj = JsonUtility.FromJson<T>(fileContent);
        }
        catch (Exception e)
        {
            obj = default;
            Debug.LogWarning("[FileHandler] Loaded object is null: " + e.Message);
        }

        return obj;
    }
    public static void DeleteFile(string directory, string name)
    {
        string filePath = $"{Application.persistentDataPath}/data/{directory}/{name}.json";

        File.Delete(filePath);
    }


    ////////////////////////////////////////////////////////////////////////////////


    public static World LoadWorld()
    {
        string mapName = GetWorldName();
        return FileHandler.LoadObject<World>("Saves", mapName);
    }
    public static void SaveWorld(World world)
    {
        string worldName = GetWorldName();

        FileHandler.SaveObject("Saves", worldName, world);
    }
    public static void CreateWorld(string mapTemplateName)
    {
        Serializable2DArray<TileType> mapTemplate = FileHandler.LoadObject<Serializable2DArray<TileType>>("Templates", mapTemplateName);

        World world = new(mapTemplate);

        FileHandler.SaveObject("Saves", GetWorldName(), world);
    }
    public static void ChangeWorldName(string newMapName)
    {
        UserData userData = GetUserData();

        userData.worldName = newMapName;

        SaveUserData(userData);
    }
    public static string GetWorldName()
    {
        return GetUserData().worldName;
    }
    private static UserData GetUserData()
    {
        return FileHandler.LoadObject<UserData>("UserData", "userdata");
    }
    private static void SaveUserData(UserData userData)
    {
        FileHandler.SaveObject("UserData", "userdata", userData);
    }
}
