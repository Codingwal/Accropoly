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

                var world = World.DefaultGameObjectInjectionWorld;
                var entitiesGraphicsSystem = world.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
                BatchMeshID meshID = entitiesGraphicsSystem.RegisterMesh(authoring.mesh);
                var configComponent = new ConfigComponents.Billboarding
                {
                    prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    meshID = meshID,
                };
                foreach (var pair in authoring.materials)
                {
                    int index = (int)pair.key;

                    // Change the length (if necessary) so that the element can be added at the correct positon
                    if (index >= configComponent.materialIDs.Length)
                        configComponent.materialIDs.Length = index + 1;

                    configComponent.materialIDs[(int)pair.key] = entitiesGraphicsSystem.RegisterMaterial(pair.value);
                }

                AddComponent(entity, configComponent);
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
        public FixedList128Bytes<BatchMaterialID> materialIDs; // supports ~30 BatchMaterialIDs
    }
}