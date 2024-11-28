using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;


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
    public UnsafeList<T> Deserialize<T>(UnsafeList<T> data) where T : unmanaged
    {
        int size = br.ReadInt32();
        data = new(size, Allocator.Persistent);
        for (int i = 0; i < size; i++)
        {
            data.Add(Deserialize((dynamic)new T()));
        }
        return data;
    }
}
