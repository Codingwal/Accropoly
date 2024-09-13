using System;
using System.Collections.Generic;
using System.IO;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class FileHandler
{
    protected static readonly string baseDir = Application.persistentDataPath + "/data/";
    public static string[] ListFiles(string directory)
    {
        string dataPath = $"{baseDir}{directory}/";

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
        string dataPath = $"{baseDir}{directory}/{name}.bin";

        FileStream fs = File.Create(dataPath);
        Serializer serializer = new(new(fs));

        try
        {
            serializer.Serialize((dynamic)obj);
            fs.Close();
        }
        catch (Exception e)
        {
            fs.Close();
            throw new($"Failed to serialize object of type '{obj.GetType()}' with the following error message:\n{e.Message}");
        }
    }
    public static T LoadObject<T>(string directory, string name) where T : new()
    {
        string dataPath = $"{baseDir}{directory}/{name}.bin";

        FileStream fs = File.Open(dataPath, FileMode.Open);
        Deserializer deserializer = new(new(fs));
        T data;
        try
        {
            data = deserializer.Deserialize((dynamic)new T());
        }
        catch (Exception e)
        {
            throw new($"Failed to deserialize object of type '{typeof(T)}' with the following error message:\n{e.Message}");
        }
        fs.Close();
        return data;
    }
    public static void DeleteFile(string directory, string name)
    {
        string filePath = $"{baseDir}{directory}/{name}.bin";

        File.Delete(filePath);
    }
    public static void DeleteDirectoryContent(string directory)
    {
        if (!Directory.Exists($"{baseDir}{directory}")) return;

        DirectoryInfo dir = new($"{baseDir}{directory}");

        foreach (FileInfo file in dir.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo subDir in dir.GetDirectories())
            subDir.Delete(true);
    }
    public static void InitFileSystem(string[] requiredDirectories, Dictionary<string, object> requiredFiles, bool overwriteFiles)
    {
        foreach (string directory in requiredDirectories)
        {
            if (!Directory.Exists($"{baseDir}{directory}/"))
            {
                Directory.CreateDirectory($"{baseDir}{directory}/");
            }
        }
        foreach (var fileDataPair in requiredFiles)
        {
            if (overwriteFiles || !File.Exists($"{baseDir}{fileDataPair.Key}.bin"))
            {
                SaveObject("", fileDataPair.Key, fileDataPair.Value);
            }
        }
    }
}
