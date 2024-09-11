using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileHandler
{
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
        string dataPath = $"{Application.persistentDataPath}/data/{directory}/{name}.bin";

        FileStream fs = File.Create(dataPath);
        Serializer serializer = new(new(fs));
        serializer.Serialize((dynamic)obj);
        fs.Close();
    }
    public static T LoadObject<T>(string directory, string name) where T : new()
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
        string filePath = $"{Application.persistentDataPath}/data/{directory}/{name}.bin";

        File.Delete(filePath);
    }
    public static void InitFileSystem(string[] requiredDirectories, Dictionary<string, object> requiredFiles)
    {
#if true // use true normally, false only if the whole directory structure including all requiredFiles should be reset
        foreach (string directory in requiredDirectories)
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/data/{directory}/"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/data/{directory}/");
            }
        }
        foreach (var fileDataPair in requiredFiles)
        {
            if (!File.Exists($"{Application.persistentDataPath}/data/{fileDataPair.Key}.bin"))
            {
                SaveObject("", fileDataPair.Key, fileDataPair.Value);
            }
        }
#else
        foreach (string directory in requiredDirectories)
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/data/{directory}/");
        }
        foreach (var fileDataPair in requiredFiles)
        {
            SaveObject("", fileDataPair.Key, fileDataPair.Value);
        }
#endif
    }
}
