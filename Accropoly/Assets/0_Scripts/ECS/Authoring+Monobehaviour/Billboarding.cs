using Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Authoring
{
    public class Billboarding : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private float billboardHeightOffset;

        public class Baker : Baker<Billboarding>
        {
            public override void Bake(Billboarding authoring)
            {
                Debug.Assert(authoring.prefab != null, "tilePrefab is null");

                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new ConfigComponents.Billboarding
                {
                    prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    billboardHeightOffset = authoring.billboardHeightOffset
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
        public FixedList128Bytes<BatchMaterialID> materialIDs; // supports ~30 BatchMaterialIDs
        public float billboardHeightOffset;
    }
}