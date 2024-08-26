using Unity.Collections;

public partial class Deserializer
{
    public NativeArray<T> Deserialize<T>(NativeArray<T> data) where T : struct
    {
        int size = br.ReadInt32();
        data = new(size, Allocator.Persistent);
        for (int i = 0; i < size; i++)
        {
            data[i] = Deserialize((dynamic)new T());
        }
        return data;
    }
}
