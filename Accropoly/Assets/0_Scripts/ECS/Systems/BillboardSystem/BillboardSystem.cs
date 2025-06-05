using Components;
using ConfigComponents;
using Tags;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Problems = Components.BillboardInfo.Problems;

namespace Systems
{
    public partial class BillboardSystem : SystemBase
    {
        private static NativeQueue<Entity> unusedBillboards;
        private static EntityQuery tilesWithProblemsQuery;
        protected override void OnCreate()
        {
            unusedBillboards = new(Allocator.Persistent);

            // Query should contain all tiles with at least one problem
            var notConnectedQuery = new EntityQueryDesc
            {
                Disabled = new ComponentType[] { typeof(IsConnected) }
            };
            var noElectricityQuery = new EntityQueryDesc
            {
                Disabled = new ComponentType[] { typeof(HasElectricity) }
            };
            tilesWithProblemsQuery = GetEntityQuery(new EntityQueryDesc[] { notConnectedQuery, noElectricityQuery });

            RequireForUpdate<Billboarding>();
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var config = SystemAPI.GetSingleton<Billboarding>();

            ecb.AddComponent(tilesWithProblemsQuery, new BillboardOwner());

            // Create a few new billboard entity if we're running short
            // The numbers (currently 2 and 5) are arbitrary
            if (unusedBillboards.Count < 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    Entity billboard = EntityManager.Instantiate(config.prefab);
                    ecb.AddComponent<Billboard>(billboard); // Tag component for debugging
                    ecb.SetComponent(billboard, LocalTransform.FromPosition(new(0, -5, 0))); // Hide unused billboards
                    unusedBillboards.Enqueue(billboard);
                }
            }

            var hasElectricityLookup = GetComponentLookup<HasElectricity>(true);
            var isConnectedLookup = GetComponentLookup<IsConnected>(true);

            // Update billboards / billboard owners
            Entities.ForEach((Entity entity, ref BillboardOwner billboardOwner, in Tile tile) =>
            {
                if (!billboardOwner.IsInitialized)
                    billboardOwner.Initialize();

                bool noElectricity = hasElectricityLookup.HasComponent(entity) && !hasElectricityLookup.IsComponentEnabled(entity); // .IsComponentDisabled()
                if (noElectricity && !ContainsProblem(billboardOwner.billboards, Problems.NoElectricity))
                {
                    AddProblem(ref billboardOwner, Problems.NoElectricity, ecb, tile.pos, config);
                }
                else if (!noElectricity && ContainsProblem(billboardOwner.billboards, Problems.NoElectricity))
                {
                    RemoveProblem(ref billboardOwner, Problems.NoElectricity, ecb, tile.pos);
                }

                bool notConnected = isConnectedLookup.HasComponent(entity) && !isConnectedLookup.IsComponentEnabled(entity); // .IsComponentDisabled()
                if (notConnected && !ContainsProblem(billboardOwner.billboards, Problems.NotConnected))
                {
                    AddProblem(ref billboardOwner, Problems.NotConnected, ecb, tile.pos, config);
                }
                else if (!notConnected && ContainsProblem(billboardOwner.billboards, Problems.NotConnected))
                {
                    RemoveProblem(ref billboardOwner, Problems.NotConnected, ecb, tile.pos);
                }

                ecb.SetComponent(entity, billboardOwner); // TODO: Why is this needed?
            }).WithReadOnly(hasElectricityLookup).WithReadOnly(isConnectedLookup).Schedule();
        }

        private static bool ContainsProblem(UnsafeList<BillboardInfo> billboards, Problems problem)
        {
            foreach (BillboardInfo billboard in billboards)
            {
                if (billboard.problem == problem)
                    return true;
            }
            return false;
        }
        private static void AddProblem(ref BillboardOwner billboardOwner, Problems problem, EntityCommandBuffer ecb, int2 pos, Billboarding config)
        {
            if (unusedBillboards.Count == 0) return; // Wait for next frame, new billboards will be created

            Entity billboard = unusedBillboards.Dequeue();
            ecb.SetComponent(billboard, new MaterialMeshInfo(config.materialIDs[(int)problem], config.meshID));

            billboardOwner.billboards.Add(new BillboardInfo(billboard, problem));

            RepositionBillboards(ref billboardOwner.billboards, ecb, pos);
        }
        private static void RemoveProblem(ref BillboardOwner billboardOwner, Problems problem, EntityCommandBuffer ecb, int2 pos)
        {
            for (int i = 0; i < billboardOwner.billboards.Length; i++)
            {
                BillboardInfo billboard = billboardOwner.billboards[i];

                if (billboard.problem != problem)
                    continue;

                billboardOwner.billboards.RemoveAt(i);
                unusedBillboards.Enqueue(billboard.entity);
                ecb.SetComponent(billboard.entity, LocalTransform.FromPosition(new(0, -5, 0))); // Hide unused billboards

                RepositionBillboards(ref billboardOwner.billboards, ecb, pos);
            }
            Debug.LogError("Billboard not present");
        }
        private static void RepositionBillboards(ref UnsafeList<BillboardInfo> billboards, EntityCommandBuffer ecb, int2 pos)
        {
            for (int i = 0; i < billboards.Length; i++)
            {
                var transform = LocalTransform.FromPositionRotationScale(new(pos.x * 2, i * 0.7f + 1, pos.y * 2), quaternion.identity, 0.5f);
                ecb.SetComponent(billboards[i].entity, transform);
            }
        }
    }
}