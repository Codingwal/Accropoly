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
        SaveSystem.Initialize();

        return true;
    }

    // private struct TestStruct
    // {
    //     private UnsafeList<int> list;
    //     public void Init()
    //     {
    //         list = new(3, Allocator.Persistent)
    //         {
    //             5, 25, 125
    //         };
    //     }
    //     public void Print()
    //     {
    //         Debug.Assert(list.IsCreated, "Not created");
    //         Debug.Log($"{list[0]}, {list[1]}, {list[2]}");
    //     }
    // }

    // private void Test()
    // {
    //     string path = Path.Combine(Application.persistentDataPath, "Test.bin");

    //     Debug.Log(path);

    //     TestStruct testStruct = new();
    //     testStruct.Init();
    //     Save(testStruct, path);

    //     var testStruct2 = Load(path);

    //     testStruct2.Print();

    //     // Debug.Assert(testStruct.Equals(testStruct2), ":(");
    // }

    // private void Save(TestStruct testStruct, string path)
    // {
    //     FileStream fs = File.Create(path);
    //     IWriter writer = new BinWriter();
    //     writer.Init(fs);
    //     Serializer serializer = new(writer);

    //     serializer.Serialize(testStruct);

    //     fs.Close();
    // }
    // private TestStruct Load(string path)
    // {
    //     FileStream fs = File.Open(path, FileMode.Open);
    //     IReader reader = new BinReader();
    //     reader.Init(fs);
    //     Deserializer deserializer = new(reader);

    //     TestStruct testStruct = deserializer.Deserialize<TestStruct>();

    //     fs.Close();

    //     return testStruct;
    // }
}
