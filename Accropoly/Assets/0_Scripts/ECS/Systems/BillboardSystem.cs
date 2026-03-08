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
        private EntityQuery tilesWithProblemsQuery;

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

            RequireForUpdate<Appearence>();
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            Appearence appearenceConfig = SystemAPI.GetSingleton<Appearence>();

            // Make sure all tiles with problems have the BillboardOwner component, this simplifies the UpdateBillboardsJob
            ecb.AddComponent(tilesWithProblemsQuery, new BillboardOwner());

            // Create a few new billboard entities if we're running short
            // The numbers (currently 2 and 5) are arbitrary
            if (unusedBillboards.Count < 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    Entity billboard = EntityManager.CreateEntity();

                    // Add rendering components
                    RenderMeshDescription renderMeshDesc = new(UnityEngine.Rendering.ShadowCastingMode.Off);
                    MaterialMeshInfo materialMeshInfo = new(appearenceConfig.billboardMaterials[0], appearenceConfig.billboardMesh);
                    RenderMeshUtility.AddComponents(billboard, EntityManager, renderMeshDesc, materialMeshInfo);

                    EntityManager.AddComponentData(billboard, LocalTransform.FromPosition(new(0, -5, 0))); // Hide unused billboards
                    EntityManager.AddComponent<Billboard>(billboard); // Tag component for debugging

                    unusedBillboards.Enqueue(billboard);
                }
            }

            new DisposeBillboardOwnersJob { ecb = ecb }
                .Schedule(SystemAPI.QueryBuilder().WithAll<Replace, BillboardOwner>().Build());

            // Update billboards / billboard owners
            new UpdateBillboardsJob
            {
                ecb = ecb,
                hasElectricityLookup = GetComponentLookup<HasElectricity>(isReadOnly: true),
                isConnectedLookup = GetComponentLookup<IsConnected>(isReadOnly: true),
                config = appearenceConfig
            }.Schedule();
        }

        protected override void OnDestroy()
        {
            unusedBillboards.Dispose();

            EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithAll<BillboardOwner>().Build(this);

            foreach (var billboardOwner in query.ToComponentDataArray<BillboardOwner>(Allocator.Temp))
            {
                if (billboardOwner.billboards.IsCreated)
                    billboardOwner.billboards.Dispose();
            }
        }

        // Can't BurstCompile because of static RW field unusedBillboards
        private partial struct DisposeBillboardOwnersJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            public void Execute(ref BillboardOwner billboardOwner)
            {
                DisposeBillboardOwner(ref billboardOwner, ref ecb);
            }
        }

        // Can't BurstCompile because of static RW field unusedBillboards
        private partial struct UpdateBillboardsJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            [ReadOnly] public ComponentLookup<HasElectricity> hasElectricityLookup;
            [ReadOnly] public ComponentLookup<IsConnected> isConnectedLookup;
            public Appearence config;
            public void Execute(Entity entity, ref BillboardOwner billboardOwner, in Tile tile)
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
                ecb.SetComponent(entity, billboardOwner); // Needed because the data is passed to sub-functions (ref doesn't work as intended)
            }
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
        private static void AddProblem(ref BillboardOwner billboardOwner, Problems problem, EntityCommandBuffer ecb, int2 pos, Appearence config)
        {
            if (unusedBillboards.Count == 0) return; // Wait for next frame, new billboards will be created

            // Get an entity and update its appearence (transform is handled later)
            Entity billboard = unusedBillboards.Dequeue();
            var info = ECSUtility.EntityManager.GetComponentData<MaterialMeshInfo>(billboard);
            info.MaterialID = config.billboardMaterials[(int)problem];
            ecb.SetComponent(billboard, info);

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
                float billboardHeightOffset = ConfigData.tileConfig.Data.billboarding.billboardHeightOffset;
                float3 position = new(pos.x * 2, i * 0.7f + billboardHeightOffset, pos.y * 2);
                var transform = LocalTransform.FromPositionRotationScale(position, quaternion.identity, 0.5f);
                ecb.SetComponent(billboards[i].entity, transform);
            }
        }
        private static void DisposeBillboardOwner(ref BillboardOwner billboardOwner, ref EntityCommandBuffer ecb)
        {
            if (!billboardOwner.billboards.IsCreated)
                return;

            foreach (BillboardInfo billboard in billboardOwner.billboards)
            {
                ecb.SetComponent(billboard.entity, LocalTransform.FromPosition(new(0, -5, 0))); // Hide unused billboards
                unusedBillboards.Enqueue(billboard.entity);
            }
            billboardOwner.billboards.Dispose();
        }
    }
}