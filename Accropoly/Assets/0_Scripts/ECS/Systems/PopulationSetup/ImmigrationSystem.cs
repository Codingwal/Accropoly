using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class ImmigrationSystem : SystemBase
{
    private const float immigrationProbability = 0.3f; // 1 = 100%
    protected override void OnCreate()
    {
        RequireForUpdate<TilePrefab>();
    }
    protected override void OnUpdate()
    {
        Entity prefab = SystemAPI.GetSingleton<TilePrefab>();

        // TODO: use ecb
        // Foreach active habitat with space
        Entities.WithAll<ActiveTileTag, HasSpaceTag>().ForEach((Entity habitatEntity, ref Habitat habitat, in MapTileComponent habitatTile) =>
        {
            if (UnityEngine.Random.Range(0f, 1f) <= immigrationProbability * SystemAPI.Time.DeltaTime) // Multiply with delta time bc immigrationProbability is per second, not per frame
            {
                Debug.Log("New person!");

                habitat.freeSpace--;
                if (habitat.freeSpace == 0) EntityManager.RemoveComponent<HasSpaceTag>(habitatEntity);

                // Create new inhabitant for this house ("immigrant")
                Entity entity = EntityManager.Instantiate(prefab);
                EntityManager.AddComponentData(entity, new PersonComponent
                {
                    homeTile = habitatTile.pos,
                    age = 0,
                });

                float offset = (habitat.totalSpace - habitat.freeSpace - 2.5f) * 0.2f;
                float3 pos = new(2 * habitatTile.pos.x + offset, 0.5f, 2 * habitatTile.pos.y);
                EntityManager.SetComponentData(entity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 0.1f));
            }
        }).WithoutBurst().WithStructuralChanges().Run();
    }
}
