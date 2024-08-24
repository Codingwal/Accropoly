using System.Runtime.Serialization;
using Unity.Mathematics;

public class QuaternionSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        quaternion q = (quaternion)obj;
        info.AddValue("x", q.value.x);
        info.AddValue("y", q.value.y);
        info.AddValue("z", q.value.z);
        info.AddValue("w", q.value.w);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        var q = (quaternion)obj;

        q.value.x = (float)info.GetValue("x", typeof(float));
        q.value.y = (float)info.GetValue("y", typeof(float));
        q.value.z = (float)info.GetValue("z", typeof(float));
        q.value.w = (float)info.GetValue("w", typeof(float));
        return q;
    }
}
