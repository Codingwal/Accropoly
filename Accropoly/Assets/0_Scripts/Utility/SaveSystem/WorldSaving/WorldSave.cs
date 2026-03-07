using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public struct WorldSave
{
    public struct ComponentSave : ICustomSaving
    {
        public FixedString32Bytes name;
        public NativeList<int> entityIds;
        public NativeList<byte> components;
        public NativeList<bool> enabled;

        public void Load(Deserializer deserializer)
        {
            name = deserializer.Deserialize<FixedString32Bytes>();

            Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "Entities");
            entityIds = deserializer.Deserialize<NativeList<int>>();

            Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "Components", $"ERROR! {name}");
            components = deserializer.Deserialize<NativeList<byte>>();

            Debug.Assert(deserializer.Deserialize<FixedString32Bytes>() == "Enabled");
            enabled = deserializer.Deserialize<NativeList<bool>>();
        }

        public readonly void Save(Serializer serializer)
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
