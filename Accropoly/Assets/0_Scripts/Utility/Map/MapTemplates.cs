using System.Collections.Generic;
using Components;
using Tags;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

public static class MapTemplates
{
    public static WorldData DefaultMap
    {
        get
        {
            WorldSaveBuilder builder = new();

            // Create tiles
            const int size = 20;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int entityId = builder.CreateEntity();
                    builder.AddComponent(entityId, new Tile(x, y, TileType.Plains, Directions.North));
                    builder.AddComponent(entityId, new ActiveTile(), enabled: false);
                    builder.AddComponent(entityId, new NewTile());
                    builder.AddComponent(entityId, LocalTransform.FromPosition(2 * new float3(x, 0, y)));
                }
            }

            // Set GameInfo
            builder.AddComponent(builder.CreateEntity(), new GameInfo()
            {
                balance = 5000,
                time = WorldTime.Zero
            });

            builder.AddComponent(builder.CreateEntity(), new CameraTransform()
            {
                pos = new float3(20, 0, 20),
                rot = new(70, 0, 0),
                camDist = 30,
                cursorLocked = false,
            });

            return new WorldData(size, builder.GetWorldSave());
        }
    }

    public static Dictionary<FixedString32Bytes, WorldData> mapTemplates = new()
    {
        { "DefaultMap", DefaultMap },
    };
}
