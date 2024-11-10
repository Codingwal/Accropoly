using Unity.Entities;
using Unity.Mathematics;

// This should not be added to an entity! Only for serialization purposes!
public struct PosComponent : IComponentData
{
    public float3 pos;
}
