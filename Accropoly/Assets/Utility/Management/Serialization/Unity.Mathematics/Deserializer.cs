using Unity.Mathematics;

public partial class Deserializer
{
    public float2 Deserialize(float2 data)
    {
        data = new float2(br.ReadSingle(), br.ReadSingle());
        return data;
    }
    public float3 Deserialize(float3 data)
    {
        data = new float3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        return data;
    }
    public quaternion Deserialize(quaternion data)
    {
        data = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        return data;
    }
}
