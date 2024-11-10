using Unity.Entities;
using Unity.Mathematics;

public partial struct CameraTransform : IComponentData
{
    public float3 pos;
    public float3 rot; // In degrees
    public float camDist;
    public bool cursorLocked;
    public readonly (float3, quaternion) GetCameraTransform()
    {
        float3 camOffset = math.rotate(quaternion.EulerXYZ(rot * math.PI / 180), new(0, 0, -camDist)); // rot needs to be converted to radians
        float3 camPos = pos + camOffset;
        quaternion camLookDir = new(float4x4.LookAt(camPos, pos, new(0, 1, 0)));
        return (camPos, camLookDir);
    }
}
