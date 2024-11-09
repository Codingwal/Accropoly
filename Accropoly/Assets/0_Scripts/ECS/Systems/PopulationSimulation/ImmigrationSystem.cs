using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(CreationSystemGroup))]
public partial struct ImmigrationSystem : ISystem
{
    private const float immigrationProbability = 0.3f; // 1 = 100%
    private EntityQuery query;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PrefabEntity>();
        query = state.GetEntityQuery(typeof(Habitat), typeof(MapTileComponent), typeof(ActiveTileTag), typeof(HasSpaceTag));
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Foreach active habitat with space
        new Job()
        {
            prefab = SystemAPI.GetSingleton<PrefabEntity>(),
            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            rnd = new((uint)UnityEngine.Random.Range(1, 1000)),
            deltaTime = SystemAPI.Time.DeltaTime,
        }.Schedule(query);
    }
    [BurstCompile]
    private partial struct Job : IJobEntity
    {
        public Entity prefab;
        public EntityCommandBuffer ecb;
        public Random rnd;
        public float deltaTime;
        public void Execute(Entity habitatEntity, ref Habitat habitat, in MapTileComponent habitatTile)
        {
            if (rnd.NextFloat() <= immigrationProbability * deltaTime) // Multiply with delta time bc immigrationProbability is per second, not per frame
            {
                habitat.freeSpace--;
                if (habitat.freeSpace == 0) ecb.RemoveComponent<HasSpaceTag>(habitatEntity);

                // Create new inhabitant for this house ("immigrant")
                Entity entity = ecb.Instantiate(prefab);
                ecb.AddComponent(entity, new NewPersonTag());
                ecb.AddComponent(entity, new PersonComponent
                {
                    homeTile = habitatTile.pos,
                    age = 0,
                });
                ecb.AddComponent(entity, new Worker { employer = new(-1) });

                float offset = (habitat.totalSpace - habitat.freeSpace - 2.5f) * 0.2f;
                float3 pos = new(2 * habitatTile.pos.x + offset, 0.5f, 2 * habitatTile.pos.y);
                ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
            }
        }
    }
}
