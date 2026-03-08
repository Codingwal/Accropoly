using Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public static class InitEntityGrid
{
    public static void CreateEntityGrid(EntityManager entityManager)
    {
        var buffer = TileGridUtility.CreateEntityGridBuffer();

        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<Tile>().Build(entityManager);

        int tileCount = query.CalculateEntityCount();

        buffer.Resize(tileCount, NativeArrayOptions.UninitializedMemory);

        var tiles = query.ToComponentDataArray<Tile>(Allocator.Temp);
        var tileEntities = query.ToEntityArray(Allocator.Temp);

        for (int i = 0; i < tileCount; i++)
        {
            int index = TileGridUtility.GetIndex(tiles[i].pos, tileCount);
            buffer.ElementAt(index).entity = tileEntities[i];
        }
    }
}