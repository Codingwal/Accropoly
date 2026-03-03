using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public struct WorldSave : ICustomSaving
{
    public void Load(Deserializer deserializer)
    {
        WorldLoader loader = new(deserializer);
        loader.Load();
    }

    public void Save(Serializer serializer)
    {
        WorldSaver saver = new(serializer);
        saver.Save();
    }
}

public struct WorldSave2
{
    public struct ComponentInfo
    {
        public FixedString32Bytes name;
        public int size;
    }
    public struct EntityData
    {
        public struct Component
        {
            public int id;
        }
        public UnsafeList<Component> components;
    }

    public UnsafeList<ComponentInfo> componentInfos;
    public UnsafeList<EntityData> entities;
}