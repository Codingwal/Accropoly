using Unity.Mathematics;

public partial class Serializer
{
    public void Serialize(float2 data)
    {
        bw.Write(data.x);
        bw.Write(data.y);
    }
    public void Serialize(float3 data)
    {
        bw.Write(data.x);
        bw.Write(data.y);
        bw.Write(data.z);
    }
    public void Serialize(quaternion data)
    {
        bw.Write(data.value.x);
        bw.Write(data.value.y);
        bw.Write(data.value.z);
        bw.Write(data.value.w);
    }
}