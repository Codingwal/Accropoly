using Unity.Entities;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Bootstrap : ICustomBootstrap
{
    public bool Initialize(string defaultWorldName)
    {
        // Test();
        // EditorApplication.ExitPlaymode();

        SaveSystem.Initialize();

        return false;
    }

    // private struct TestStruct
    // {
    //     private UnsafeList<int> list;
    //     public void Init()
    //     {
    //     }
    // }

    // private void Test()
    // {
    //     string path = Path.Combine(Application.persistentDataPath, "Test.bin");

    //     TestStruct testStruct = new();
    //     testStruct.Init();
    //     Save(testStruct, path);

    //     var testStruct2 = Load(path);

    //     Debug.Assert(testStruct.Equals(testStruct2), ":(");
    // }

    // private void Save(TestStruct testStruct, string path)
    // {
    //     FileStream fs = File.Create(path);

    //     Serializer serializer = new(new BinWriter(new(fs)));

    //     serializer.Serialize(testStruct);

    //     fs.Close();
    // }
    // private TestStruct Load(string path)
    // {
    //     FileStream fs = File.Open(path, FileMode.Open);

    //     Deserializer deserializer = new(new BinReader(new(fs)));
    //     TestStruct testStruct = deserializer.Deserialize<TestStruct>();

    //     fs.Close();

    //     return testStruct;
    // }
}
