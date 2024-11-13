using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class TilePricesAuthoring : MonoBehaviour
{
    [SerializeField] private SerializableDictionary<TileType, float> tilePrices = new();
    public class Baker : Baker<TilePricesAuthoring>
    {
        public override void Bake(TilePricesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponentObject(entity, new TilePrices
            {
                prices = authoring.tilePrices
            });
        }
    }
}
public class TilePrices : IComponentData
{
    public Dictionary<TileType, float> prices;
    public TilePrices()
    {
        prices = new();
    }
}
