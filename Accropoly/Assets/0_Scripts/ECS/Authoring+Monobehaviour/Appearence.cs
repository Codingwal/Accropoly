using System;
using Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Authoring
{
    public class Appearence : MonoBehaviour
    {
        private static Appearence instance;
        [SerializeField] private SerializableDictionary<TileType, MaterialMeshPair> simpleTiles = new();
        [SerializeField] private SerializableDictionary<TileType, MaterialMeshPairSet> connectingTiles = new();
        [SerializeField] private MaterialMeshPair person;

        private void Awake()
        {
            instance = this;
        }

        public static ConfigComponents.Appearence CreateAppearenceConfig()
        {
            var graphicsSystem = ECSUtility.World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();

            ConfigComponents.Appearence data = new()
            {
                simpleTiles = new(4, Allocator.Persistent),
                connectingTiles = new(1, Allocator.Persistent)
            };

            // Copy simpleTiles
            foreach ((TileType tileType, MaterialMeshPair pair) in instance.simpleTiles)
            {
                var materialMeshInfo = RegisterPair(pair, graphicsSystem, $"simple tile \"{tileType}\"");
                data.simpleTiles.Add((int)tileType, materialMeshInfo);
            }

            // Copy connectingTiles
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

            data.person = RegisterPair(instance.person, graphicsSystem, "person");

            return data;
        }

        private static MaterialMeshInfo RegisterPair(MaterialMeshPair pair, EntitiesGraphicsSystem graphicsSystem, string debugName)
        {
            Debug.Assert(pair.material != null, $"Material for {debugName} is null.");
            Debug.Assert(pair.mesh != null, $"Mesh for {debugName} is null.");

            BatchMaterialID matID = graphicsSystem.RegisterMaterial(pair.material);
            BatchMeshID meshID = graphicsSystem.RegisterMesh(pair.mesh);
            return new(matID, meshID);
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
    [ChunkSerializable]
    public struct Appearence : IComponentData
    {
        public NativeHashMap<int, MaterialMeshInfo> simpleTiles; // <TileType, pair>
        public NativeHashMap<int, MaterialMeshInfoSet> connectingTiles; // <TileType, set>
        public MaterialMeshInfo person;
    }
}