using Components;
using ConfigComponents;
using Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Problems = Components.BillboardInfo.Problems;
using BillboardList = Unity.Collections.LowLevel.Unsafe.UnsafeList<Components.BillboardInfo>;

namespace Systems
{
    /// <summary>
    /// Handles billboards (icons over buildings indicating problems)
    /// Billboard entities aren't created and deleted (at least not often) but reused using a queue
    /// </summary>
    public partial class BillboardSystem : SystemBase
    {
        public NativeQueue<Entity> unusedBillboards;
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

            BillboardUtility utility = new()
            {
                unusedBillboards = unusedBillboards,
                appearenceConfig = appearenceConfig,
                transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
                materialMeshInfoLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>()
            };

            new DisposeBillboardOwnersJob { utility = utility }
                .Schedule();

            // Update billboards / billboard owners
            new UpdateBillboardsJob
            {
                ecb = ecb,
                hasElectricityLookup = GetComponentLookup<HasElectricity>(isReadOnly: true),
                isConnectedLookup = GetComponentLookup<IsConnected>(isReadOnly: true),
                utility = utility
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

        [BurstCompile]
        [WithAll(typeof(Replace))]
        private partial struct DisposeBillboardOwnersJob : IJobEntity
        {
            public BillboardUtility utility;
            public void Execute(ref BillboardOwner billboardOwner)
            {
                utility.DisposeBillboardOwner(ref billboardOwner.billboards);
            }
        }

        [BurstCompile]
        [WithNone(typeof(Replace))]
        private partial struct UpdateBillboardsJob : IJobEntity
        {
            public EntityCommandBuffer ecb;
            [ReadOnly] public ComponentLookup<HasElectricity> hasElectricityLookup;
            [ReadOnly] public ComponentLookup<IsConnected> isConnectedLookup;
            public BillboardUtility utility;
            public void Execute(Entity entity, ref BillboardOwner billboardOwner, in Tile tile)
            {
                if (!billboardOwner.IsInitialized)
                    billboardOwner.Initialize();

                // Handle electricity
                bool noElectricity = hasElectricityLookup.HasComponent(entity) && !hasElectricityLookup.IsComponentEnabled(entity); // .IsComponentDisabled()
                if (noElectricity && !utility.ContainsProblem(billboardOwner.billboards, Problems.NoElectricity))
                {
                    utility.AddProblem(ref billboardOwner.billboards, Problems.NoElectricity, tile.pos);
                }
                else if (!noElectricity && utility.ContainsProblem(billboardOwner.billboards, Problems.NoElectricity))
                {
                    utility.RemoveProblem(ref billboardOwner.billboards, Problems.NoElectricity, tile.pos);
                }

                // Handle connection
                bool notConnected = isConnectedLookup.HasComponent(entity) && !isConnectedLookup.IsComponentEnabled(entity); // .IsComponentDisabled()
                if (notConnected && !utility.ContainsProblem(billboardOwner.billboards, Problems.NotConnected))
                {
                    utility.AddProblem(ref billboardOwner.billboards, Problems.NotConnected, tile.pos);
                }
                else if (!notConnected && utility.ContainsProblem(billboardOwner.billboards, Problems.NotConnected))
                {
                    utility.RemoveProblem(ref billboardOwner.billboards, Problems.NotConnected, tile.pos);
                }

                // Update the component
                ecb.SetComponent(entity, billboardOwner); // Needed because the data is passed to sub-functions (ref doesn't work as intended)
            }
        }

        private struct BillboardUtility
        {
            public NativeQueue<Entity> unusedBillboards;
            public Appearence appearenceConfig;
            public ComponentLookup<LocalTransform> transformLookup;
            public ComponentLookup<MaterialMeshInfo> materialMeshInfoLookup;
            public bool ContainsProblem(BillboardList billboards, Problems problem)
            {
                foreach (BillboardInfo billboard in billboards)
                {
                    if (billboard.problem == problem)
                        return true;
                }
                return false;
            }
            public void AddProblem(ref BillboardList billboards, Problems problem, int2 pos)
            {
                if (unusedBillboards.Count == 0) return; // Wait for next frame, new billboards will be created

                // Get an entity and update its appearence (transform is handled later)
                Entity billboard = unusedBillboards.Dequeue();
                materialMeshInfoLookup.GetRefRW(billboard).ValueRW.MaterialID = appearenceConfig.billboardMaterials[(int)problem];

                billboards.Add(new BillboardInfo(billboard, problem));

                RepositionBillboards(ref billboards, pos);
            }
            public void RemoveProblem(ref BillboardList billboards, Problems problem, int2 pos)
            {
                for (int i = 0; i < billboards.Length; i++)
                {
                    BillboardInfo billboard = billboards[i];

                    if (billboard.problem != problem)
                        continue;

                    // Recycle the billboard
                    billboards.RemoveAt(i);
                    unusedBillboards.Enqueue(billboard.entity);
                    transformLookup.GetRefRW(billboard.entity).ValueRW.Position = new(0, -5, 0);

                    RepositionBillboards(ref billboards, pos);
                    return;
                }
                Debug.LogError("Billboard not present");
            }
            public void RepositionBillboards(ref BillboardList billboards, int2 pos)
            {
                for (int i = 0; i < billboards.Length; i++)
                {
                    // Billboards will be shown as a vertical stack
                    float billboardHeightOffset = ConfigData.tileConfig.Data.billboarding.billboardHeightOffset;
                    float3 position = new(pos.x * 2, i * 0.7f + billboardHeightOffset, pos.y * 2);
                    transformLookup[billboards[i].entity] = LocalTransform.FromPositionRotationScale(position, quaternion.identity, 0.5f);
                }
            }
            public void DisposeBillboardOwner(ref BillboardList billboards)
            {
                if (!billboards.IsCreated)
                    return;

                foreach (BillboardInfo billboard in billboards)
                {
                    transformLookup.GetRefRW(billboard.entity).ValueRW.Position = new(0, -5, 0); // Hide unused billboards
                    unusedBillboards.Enqueue(billboard.entity);
                }
                billboards.Dispose();
            }
        }
    }
}