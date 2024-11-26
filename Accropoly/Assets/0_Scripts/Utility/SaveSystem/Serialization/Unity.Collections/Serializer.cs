using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public partial class Serializer
{
    public void Serialize<T>(NativeArray<T> data) where T : struct
    {
        bw.Write(data.Length);
        foreach (var e in data)
        {
            Serialize((dynamic)e);
        }
        data.Dispose();
    }
    public void Serialize<T>(UnsafeList<T> data) where T : unmanaged
    {
        bw.Write(data.Length);
        foreach (var e in data)
        {
            Serialize((dynamic)e);
        }
        data.Dispose();
    }
}
