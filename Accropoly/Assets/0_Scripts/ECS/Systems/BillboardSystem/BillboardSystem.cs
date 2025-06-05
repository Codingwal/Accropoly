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
    /// <summary>
    /// Handles billboards (icons over buildings indicating problems)
    /// Billboard entities aren't created and deleted (at least not often) but reused using a queue
    /// </summary>
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

            // The game needs to be running or saving
            RequireAnyForUpdate(new EntityQuery[] { GetEntityQuery(typeof(RunGame)), GetEntityQuery(typeof(SaveGame)) });
        }
        protected override void OnDestroy()
        {
            unusedBillboards.Dispose();
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var config = SystemAPI.GetSingleton<Billboarding>();

            if (SystemAPI.HasSingleton<SaveGame>())
            {
                // Dispose BillboardOwners
                Entities.ForEach((ref BillboardOwner billboardOwner) =>
                {
                    if (!billboardOwner.billboards.IsCreated)
                        return;

                    foreach (BillboardInfo billboard in billboardOwner.billboards)
                    {
                        ecb.SetComponent(billboard.entity, LocalTransform.FromPosition(new(0, -5, 0))); // Hide unused billboards
                        unusedBillboards.Enqueue(billboard.entity);
                    }
                    billboardOwner.billboards.Dispose();
                }).Schedule();
                return;
            }

            // Make sure all tiles with problems have the BillboardOwner component, this simplifies the Entities.ForEach 
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

                // Handle electricity
                bool noElectricity = hasElectricityLookup.HasComponent(entity) && !hasElectricityLookup.IsComponentEnabled(entity); // .IsComponentDisabled()
                if (noElectricity && !ContainsProblem(billboardOwner.billboards, Problems.NoElectricity))
                {
                    AddProblem(ref billboardOwner, Problems.NoElectricity, ecb, tile.pos, config);
                }
                else if (!noElectricity && ContainsProblem(billboardOwner.billboards, Problems.NoElectricity))
                {
                    RemoveProblem(ref billboardOwner, Problems.NoElectricity, ecb, tile.pos);
                }

                // Handle connection
                bool notConnected = isConnectedLookup.HasComponent(entity) && !isConnectedLookup.IsComponentEnabled(entity); // .IsComponentDisabled()
                if (notConnected && !ContainsProblem(billboardOwner.billboards, Problems.NotConnected))
                {
                    AddProblem(ref billboardOwner, Problems.NotConnected, ecb, tile.pos, config);
                }
                else if (!notConnected && ContainsProblem(billboardOwner.billboards, Problems.NotConnected))
                {
                    RemoveProblem(ref billboardOwner, Problems.NotConnected, ecb, tile.pos);
                }

                // Update the component
                ecb.SetComponent(entity, billboardOwner); // Needed because the data is passed to sub-functions
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

            // Get an entity and update its appearence (transform is handled later)
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

                // Recycle the billboard
                billboardOwner.billboards.RemoveAt(i);
                unusedBillboards.Enqueue(billboard.entity);
                ecb.SetComponent(billboard.entity, LocalTransform.FromPosition(new(0, -5, 0))); // Hide unused billboards

                RepositionBillboards(ref billboardOwner.billboards, ecb, pos);
                return;
            }
            Debug.LogError("Billboard not present");
        }

        private static void RepositionBillboards(ref UnsafeList<BillboardInfo> billboards, EntityCommandBuffer ecb, int2 pos)
        {
            for (int i = 0; i < billboards.Length; i++)
            {
                // Billboards will be shown as a vertical stack
                var transform = LocalTransform.FromPositionRotationScale(new(pos.x * 2, i * 0.7f + 1, pos.y * 2), quaternion.identity, 0.5f);
                ecb.SetComponent(billboards[i].entity, transform);
            }
        }
    }
}