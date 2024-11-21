using System;
using System.Linq;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Authoring
{
    public class Billboarding : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Mesh mesh;
        [SerializeField] private SerializableDictionary<BillboardInfo.Problems, Material> materials;
        public class Baker : Baker<Billboarding>
        {
            public override void Bake(Billboarding authoring)
            {
                Debug.Assert(authoring.prefab != null, "tilePrefab is null");

                Entity entity = GetEntity(TransformUsageFlags.None);

                var entitiesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

                BatchMeshID meshID = entitiesGraphicsSystem.RegisterMesh(authoring.mesh);

                int largestEnumValue = Enum.GetValues(typeof(BillboardInfo.Problems)).Cast<int>().Max();
                NativeArray<BatchMaterialID> materialIDs = new(largestEnumValue, Allocator.Persistent);
                foreach (var pair in authoring.materials)
                {
                    materialIDs[(int)pair.key] = entitiesGraphicsSystem.RegisterMaterial(pair.value);
                }

                AddComponent(entity, new ConfigComponents.Billboarding
                {
                    prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    meshID = meshID,
                    materialIDs = materialIDs,
                });
            }
        }
    }
}
namespace ConfigComponents
{
    public struct Billboarding : IComponentData
    {
        public Entity prefab;
        public BatchMeshID meshID;
        public NativeArray<BatchMaterialID> materialIDs;
    }
}