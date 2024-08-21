using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class TileSpawnerConfigAuthoring : MonoBehaviour
{
    public GameObject baseTilePrefab;
    public class Baker : Baker<TileSpawnerConfigAuthoring>
    {
        public override void Bake(TileSpawnerConfigAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TileSpawnerConfig
            {
                baseTilePrefabEntity = GetEntity(authoring.baseTilePrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}
public struct TileSpawnerConfig : IComponentData
{
    public Entity baseTilePrefabEntity;
}