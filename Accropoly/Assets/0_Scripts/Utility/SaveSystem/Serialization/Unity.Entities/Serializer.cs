using Unity.Entities;

public partial class Serializer
{
    public void Serialize(ComponentType data)
    {
        ulong hash = TypeManager.GetTypeInfo(data.TypeIndex).StableTypeHash;
        bw.Write(hash);
    }
}
