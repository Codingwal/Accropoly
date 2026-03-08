using System;
using Unity.Collections;
using UnityEngine;

[Serializable]
public struct WorldData : IDisposable, ICustomSaving
{
    public int version;
    public int mapSize;
    public WorldSave worldSave;
    public void Dispose()
    {
        worldSave.Dispose();
    }

    public void Load(Deserializer deserializer)
    {
        Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "Accropoly WorldSave!");

        version = deserializer.Deserialize<int>();
        mapSize = deserializer.Deserialize<int>();

        Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "WorldSave:");
        worldSave = deserializer.Deserialize<WorldSave>();
    }

    public readonly void Save(Serializer serializer)
    {
        serializer.Serialize<FixedString32Bytes>("Accropoly WorldSave!");

        serializer.Serialize(version);
        serializer.Serialize(mapSize);

        serializer.Serialize<FixedString32Bytes>("WorldSave:");
        serializer.Serialize(worldSave);
    }
}