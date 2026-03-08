using System;
using Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Authoring
{
    public class Appearence : MonoBehaviour
    {
        private static Appearence instance;

        [Header("Tiles")]
        [SerializeField] private SerializableDictionary<TileType, MaterialMeshPair> simpleTiles = new();
        [SerializeField] private SerializableDictionary<TileType, MaterialMeshPairSet> connectingTiles = new();

        [Header("Population")]
        [SerializeField] private MaterialMeshPair person;

        [Header("Billboarding")]
        [SerializeField] private SerializableDictionary<BillboardInfo.Problems, Material> billboardMaterials = new();
        [SerializeField] private Mesh billboardMesh;

        private void Awake()
        {
            instance = this;
        }

        public static ConfigComponents.Appearence CreateAppearenceConfig()
        {
            var graphicsSystem = ECSUtility.World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

            ConfigComponents.Appearence data = new()
            {
                simpleTiles = new(1, Allocator.Persistent),
                connectingTiles = new(1, Allocator.Persistent),
                billboardMaterials = new(1, Allocator.Persistent)
            };

            // Simple tiles
            foreach ((TileType tileType, MaterialMeshPair pair) in instance.simpleTiles)
            {
                var materialMeshInfo = RegisterPair(pair, graphicsSystem, $"simple tile \"{tileType}\"");
                data.simpleTiles.Add((int)tileType, materialMeshInfo);
            }

            // Connecting tiles
            foreach ((TileType tileType, MaterialMeshPairSet managedSet) in instance.connectingTiles)
            {
                MaterialMeshInfoSet set = new(7, Allocator.Persistent);
                for (int i = 0; i < 7; i++)
                {
                    MaterialMeshPair pair = managedSet.pairs[i];
                    var materialMeshInfo = RegisterPair(pair, graphicsSystem, $"connecting tile \"{tileType}\" (index {i})");
                    set.pairs.Add(materialMeshInfo);
                }
                data.connectingTiles.Add((int)tileType, set);
            }

            // Population
            data.person = RegisterPair(instance.person, graphicsSystem, "person");

            // Billboarding
            foreach ((BillboardInfo.Problems problem, Material material) in instance.billboardMaterials)
            {
                var materialID = RegisterMaterial(material, graphicsSystem, $"billboard \"{problem}\"");
                data.billboardMaterials.Add((int)problem, materialID);
            }
            data.billboardMesh = RegisterMesh(instance.billboardMesh, graphicsSystem, $"billboard");

            return data;
        }

        private static MaterialMeshInfo RegisterPair(MaterialMeshPair pair, EntitiesGraphicsSystem graphicsSystem, string debugName)
        {
            BatchMaterialID matID = RegisterMaterial(pair.material, graphicsSystem, debugName);
            BatchMeshID meshID = RegisterMesh(pair.mesh, graphicsSystem, debugName);
            return new(matID, meshID);
        }
        private static BatchMaterialID RegisterMaterial(Material material, EntitiesGraphicsSystem graphicsSystem, string debugName)
        {
            Debug.Assert(material != null, $"Material for {debugName} is null.");
            BatchMaterialID matID = graphicsSystem.RegisterMaterial(material);
            return matID;
        }
        private static BatchMeshID RegisterMesh(Mesh mesh, EntitiesGraphicsSystem graphicsSystem, string debugName)
        {
            Debug.Assert(mesh != null, $"Mesh for {debugName} is null.");
            BatchMeshID meshID = graphicsSystem.RegisterMesh(mesh);
            return meshID;
        }
    }
}
[Serializable]
public struct MaterialMeshPair
{
    public Material material;
    public Mesh mesh;
}
[Serializable]
public class MaterialMeshPairSet
{
    public MaterialMeshPair[] pairs = new MaterialMeshPair[7];
}

public struct MaterialMeshInfoSet
{
    public UnsafeList<MaterialMeshInfo> pairs;
    public MaterialMeshInfoSet(int length, Allocator allocator)
    {
        pairs = new(length, allocator);
    }
}

namespace ConfigComponents
{
    public struct Appearence : IComponentData
    {
        // Tiles
        public NativeHashMap<int, MaterialMeshInfo> simpleTiles; // <TileType, pair>
        public NativeHashMap<int, MaterialMeshInfoSet> connectingTiles; // <TileType, set>

        // Population
        public MaterialMeshInfo person;

        // Billboarding
        public NativeHashMap<int, BatchMaterialID> billboardMaterials;
        public BatchMeshID billboardMesh;

        public void Dispose()
        {
            simpleTiles.Dispose();
            foreach (var pair in connectingTiles)
                pair.Value.pairs.Dispose();
            connectingTiles.Dispose();
            billboardMaterials.Dispose();
        }
    }
}