using Components;
using Tags;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public partial struct FollowCurveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameInfo>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.GetSingleton<GameInfo>().deltaTime;
            int gameSecondsPerMovementSecond = ConfigData.populationConfig.Data.movement.gameSecondsPerMovementSecond;
            new FollowCurveJob()
            {
                deltaTime = deltaTime / gameSecondsPerMovementSecond
            }.Schedule();
        }

        [BurstCompile]
        [WithAll(typeof(Travelling))]
        public partial struct FollowCurveJob : IJobEntity
        {
            public float deltaTime;
            public void Execute(ref LocalTransform transform, ref CurveFollower curveFollower, in Speed speed)
            {
                // The distance the object should travel this frame (arc length)
                float distance = speed.value * deltaTime;

                // Get new progress along curve
                curveFollower.timeAlongCurve += curveFollower.curve.GetDeltaTime(curveFollower.timeAlongCurve, distance);

                // Update position
                transform.Position = curveFollower.curve.GetPoint(curveFollower.timeAlongCurve);

                // Update rotation
                float3 velocity = curveFollower.curve.GetVelocity(curveFollower.timeAlongCurve);
                if (math.lengthsq(velocity.xz) > math.square(0.1)) // Only update if the object is moving (preserve current state otherwise)
                {
                    float rotY = math.atan2(velocity.x, velocity.z + 0.00001f);
                    transform.Rotation = quaternion.EulerXYZ(0, rotY, 0);
                }
            }
        }
    }
}