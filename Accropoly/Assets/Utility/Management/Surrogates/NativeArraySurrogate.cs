using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Mathematics;

public class NativeArraySurrogate<T> : ISerializationSurrogate where T : struct
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        var arr = (NativeArray<T>)obj;
        info.AddValue("length", arr.Length);
        for (int i = 0; i < arr.Length; i++)
        {
            info.AddValue($"{i}", arr[i]);
        }
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        NativeArray<T> arr = new((int)info.GetValue("length", typeof(int)), Allocator.Persistent);
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = (T)info.GetValue($"{i}", typeof(T));
        }
        return arr;
    }
}
