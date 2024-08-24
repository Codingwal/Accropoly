using System.Runtime.Serialization;
using Unity.Mathematics;

public class Float2Surrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        float2 f2 = (float2)obj;
        info.AddValue("x", f2.x);
        info.AddValue("y", f2.y);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        float2 f2 = (float2)obj;
        f2.x = (float)info.GetValue("x", typeof(float));
        f2.y = (float)info.GetValue("y", typeof(float));
        return f2;
    }
}
