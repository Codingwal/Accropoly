using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TileAgeingConfigAuthoring : MonoBehaviour
{
    [SerializeField] private uint seed;
    [SerializeField] private int2 randomAgeRange;
    [SerializeField] private int maxAge;
    [SerializeField] private TileType newTileType;
    public class Baker : Baker<TileAgeingConfigAuthoring>
    {
        public override void Bake(TileAgeingConfigAuthoring authoring)
        {
            Debug.Assert(authoring.seed != 0, "Seed should not be 0");
            Debug.Assert(authoring.randomAgeRange.x < authoring.randomAgeRange.y, "First parameter of randomAgeRange is the min value");

            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TileAgeingConfig
            {
                seed = authoring.seed,
                randomAgeRange = authoring.randomAgeRange,
                maxAge = authoring.maxAge,
                newTileType = authoring.newTileType
            });
        }
    }
}
public struct TileAgeingConfig : IComponentData
{
    public uint seed;
    public int2 randomAgeRange;
    public int maxAge;
    public TileType newTileType;
}