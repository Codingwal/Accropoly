using System.IO;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Bootstrap : ICustomBootstrap
{
    public bool Initialize(string defaultWorldName)
    {
        Test();
        EditorApplication.ExitPlaymode();

        // SaveSystem.Initialize();

        return false;
    }

    private struct TestStruct
    {
        public float3 a;
    }

    private void Test()
    {
        string path = Path.Combine(Application.persistentDataPath, "Test.bin");

        TestStruct testStruct = new()
        {
            a = new(1, 2, 3)
        };
        Save(testStruct, path);

        // var testStruct2 = Load(path);

        // Debug.Log(testStruct2.a);

        // Debug.Assert(testStruct.Equals(testStruct2), ":(");
    }

    private void Save(TestStruct testStruct, string path)
    {
        FileStream fs = File.Create(path);

        Serializer serializer = new(new BinWriter(new(fs)));

        serializer.Serialize(testStruct);

        fs.Close();
    }
    private TestStruct Load(string path)
    {
        FileStream fs = File.Open(path, FileMode.Open);

        Deserializer deserializer = new(new BinReader(new(fs)));
        TestStruct testStruct = deserializer.Deserialize<TestStruct>();

        fs.Close();

        return testStruct;
    }
}
