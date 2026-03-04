using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public struct WorldSave
{
    public struct ComponentSave : ICustomSaving
    {
        public FixedString32Bytes name;
        public UnsafeList<int> entityIds;
        public UnsafeList<byte> components;
        public UnsafeList<bool> enabled;

        public void Load(Deserializer deserializer)
        {
            name = deserializer.Deserialize<FixedString32Bytes>();

            Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "Entities");
            entityIds = deserializer.Deserialize<UnsafeList<int>>();

            Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "Components");
            components = deserializer.Deserialize<UnsafeList<byte>>();

            Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "Enabled");
            enabled = deserializer.Deserialize<UnsafeList<bool>>();
        }

        public void Save(Serializer serializer)
        {
            serializer.Serialize(name);

            serializer.Serialize<FixedString32Bytes>("Entities");
            serializer.Serialize(entityIds);

            serializer.Serialize<FixedString32Bytes>("Components");
            serializer.Serialize(components);

            serializer.Serialize<FixedString32Bytes>("Enabled");
            serializer.Serialize(enabled);
        }
    }
    public UnsafeList<ComponentSave> componentSaves;
}
