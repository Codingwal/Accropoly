using System.Runtime.Serialization;
using Unity.Mathematics;

public class Float3Surrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        float3 f3 = (float3)obj;
        info.AddValue("x", f3.x);
        info.AddValue("y", f3.y);
        info.AddValue("z", f3.z);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        float3 f3 = (float3)obj;
        f3.x = (float)info.GetValue("x", typeof(float));
        f3.y = (float)info.GetValue("y", typeof(float));
        f3.z = (float)info.GetValue("z", typeof(float));
        return f3;
    }
}
