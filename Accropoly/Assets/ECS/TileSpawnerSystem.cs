using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

public partial class TileSpawnerSystem : SystemBase
{
    Serializable2DArray<Tile> selectedMap = new(10, 10);
    protected override void OnUpdate()
    {
        Enabled = false;

        TileSpawnerConfig tileSpawnerConfig = SystemAPI.GetSingleton<TileSpawnerConfig>();

        int amountToSpawn = selectedMap.GetLength(0) * selectedMap.GetLength(1);
        NativeArray<Entity> spawnedEntities = new(amountToSpawn, Allocator.Temp);
        EntityManager.Instantiate(tileSpawnerConfig.baseTilePrefabEntity, spawnedEntities);

        for (int x = 0; x < selectedMap.GetLength(0); x++)
        {
            for (int y = 0; y < selectedMap.GetLength(1); y++)
            {
                int i = x * selectedMap.GetLength(0) + y;
                Entity entity = spawnedEntities[i];

                SystemAPI.SetComponent(entity, LocalTransform.FromPosition(2 * x, 0, 2 * y));
            }
        }
        spawnedEntities.Dispose();
    }
}
