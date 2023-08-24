using System;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

public static class FileHandler
{
    public static void Init()
    {
        string[] requiredDirectories =
        {
            "UserData",
            "Saves"
        };
        string[] requiredFiles =
        {
            "UserData/userdata"
        };

        foreach (string directory in requiredDirectories)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/data/{directory}/"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/data/{directory}/");
            }
        }
        foreach (string file in requiredFiles)
        {
            if (!File.Exists($"{Application.persistentDataPath}/data/{file}.json"))
            {
                File.Create($"{Application.persistentDataPath}/data/{file}.json/");
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
}
