using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class TilePrices : MonoBehaviour
    {
        [SerializeField] private SerializableDictionary<TileType, float> tilePrices = new();
        public class Baker : Baker<TilePrices>
        {
            public override void Bake(TilePrices authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponentObject(entity, new ConfigComponents.TilePrices
                {
                    prices = authoring.tilePrices
                });
            }
        }
    }
}
namespace ConfigComponents
{
    public class TilePrices : IComponentData
    {
        public Dictionary<TileType, float> prices;
        public TilePrices()
        {
            prices = new();
        }
    }
}