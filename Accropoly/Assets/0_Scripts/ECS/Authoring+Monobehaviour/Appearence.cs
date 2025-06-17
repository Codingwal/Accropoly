using System;
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
                Debug.Assert(pair.material != null, $"Material for tileType {tileType} is null");
                Debug.Assert(pair.mesh != null, $"Mesh for tileType {tileType} is null");

                BatchMaterialID matID = graphicsSystem.RegisterMaterial(pair.material);
                BatchMeshID meshID = graphicsSystem.RegisterMesh(pair.mesh);

                data.simpleTiles.Add((int)tileType, new(matID, meshID));
            }

            // Copy connectingTiles
            foreach ((TileType tileType, MaterialMeshPairSet managedSet) in instance.connectingTiles)
            {
                MaterialMeshInfoSet set = new(6, Allocator.Persistent);
                for (int i = 0; i < 6; i++)
                {
                    MaterialMeshPair pair = managedSet.pairs[i];

                    Debug.Assert(pair.material != null, $"Material for tileType {tileType} is null");
                    Debug.Assert(pair.mesh != null, $"Mesh for tileType {tileType} is null");

                    BatchMaterialID matID = graphicsSystem.RegisterMaterial(pair.material);
                    BatchMeshID meshID = graphicsSystem.RegisterMesh(pair.mesh);

                    set.pairs.Add(new(matID, meshID));
                }
                data.connectingTiles.Add((int)tileType, set);
            }

            return data;
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
    public MaterialMeshPair[] pairs = new MaterialMeshPair[6];
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
    }
}