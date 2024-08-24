using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;

public class FileHandler
{
    public static FileHandler Instance { get; private set; }
    public FileHandler()
    {
        if (Instance != null) Debug.LogError("Multiple instances of singleton FileHandler");
        Instance = this;

        InitFileSystem();
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
    public void SaveObject<T>(string directory, string name, T obj)
    {
        string dataPath = $"{Application.persistentDataPath}/data/{directory}/{name}.bin";

        FileStream fs = File.Create(dataPath);
        Serializer serializer = new(new(fs));
        serializer.Serialize((dynamic)obj);
        fs.Close();
    }
    public T LoadObject<T>(string directory, string name) where T : new()
    {
        string dataPath = $"{Application.persistentDataPath}/data/{directory}/{name}.bin";

        FileStream fs = File.Open(dataPath, FileMode.Open);
        Deserializer deserializer = new(new(fs));
        T data = deserializer.Deserialize((dynamic)new T());
        fs.Close();
        return data;
    }
    public static void DeleteFile(string directory, string name)
    {
        string filePath = $"{Application.persistentDataPath}/data/{directory}/{name}.json";

        File.Delete(filePath);
    }
    private void InitFileSystem()
    {
        string[] requiredDirectories =
                {
            "UserData",
            "Templates",
            "Saves"
        };
        Dictionary<string, IInstantiatable> requiredFiles = new()
        {
            {"UserData/userdata", new UserData()},
        };

        foreach (KeyValuePair<string, Serializable2DArray<Tile>> keyValuePair in MapTemplates.mapTemplates)
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
}
