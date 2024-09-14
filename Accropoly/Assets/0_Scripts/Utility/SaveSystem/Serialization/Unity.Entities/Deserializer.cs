using Unity.Entities;

public partial class Deserializer
{
    public ComponentType Deserialize(ComponentType data)
    {
        ulong hash = br.ReadUInt64();
        data = ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(hash));
        return data;
    }
}

