using Unity.Collections;

public partial class Serializer
{
    public void Serialize<T>(NativeArray<T> data) where T : struct
    {
        bw.Write(data.Length);
        foreach (var e in data)
        {
            Serialize((dynamic)e);
        }
    }
}
