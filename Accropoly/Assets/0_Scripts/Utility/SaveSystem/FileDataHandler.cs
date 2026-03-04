using System.IO;
using UnityEngine;

public class FileHandler
{
    public static readonly string baseDir = Application.persistentDataPath + "/data/";
    public static string[] ListFiles(string directory)
    {
        string dataPath = $"{baseDir}{directory}/";

        string[] files = Directory.GetFiles(dataPath);

        for (int i = 0; i < files.Length; i++)
        {
            files[i] = GetFileName(files[i]);
        }
        return files;
    }
    public static void SaveObject<T>(string directory, string name, T obj, bool overwrite = true)
        where T : unmanaged
    {
        string dataPath = $"{baseDir}{directory}/{name}.bin";

        if (File.Exists(dataPath) && !overwrite)
            return;

        FileStream fs = File.Create(dataPath);
        IWriter writer = new BinWriter();
        writer.Init(fs);
        Serializer serializer = new(writer);

        serializer.Serialize(obj);

        fs.Close();
    }
    public static T LoadObject<T>(string directory, string name)
        where T : unmanaged
    {
        string dataPath = $"{baseDir}{directory}/{name}.bin";

        FileStream fs = File.Open(dataPath, FileMode.Open);
        IReader reader = new BinReader();
        reader.Init(fs);
        Deserializer deserializer = new(reader);

        T data = deserializer.Deserialize<T>();

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
    public static void InitFileSystem(string[] requiredDirectories)
    {
        foreach (string directory in requiredDirectories)
        {
            if (!Directory.Exists($"{baseDir}{directory}/"))
            {
                Directory.CreateDirectory($"{baseDir}{directory}/");
            }
        }
    }

    /// <summary>
    /// Get the fileName, without path or file type
    /// </summary>
    public static string GetFileName(string path)
    {
        int startPoint = path.LastIndexOf("/") + 1;
        int endPoint = path.LastIndexOf(".");
        return path[startPoint..endPoint];
    }
}
