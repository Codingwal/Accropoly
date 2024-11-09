using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

[UpdateInGroup(typeof(PreCreationSystemGroup), OrderLast = true)]
public partial class EndPreCreationECBSystem : EntityCommandBufferSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();

        this.RegisterSingleton<Singleton>(ref PendingBuffers, World.Unmanaged);
    }
    public unsafe struct Singleton : IComponentData, IECBSingleton
    {
        internal UnsafeList<EntityCommandBuffer>* pendingBuffers;
        internal AllocatorManager.AllocatorHandle allocator;
        public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
        {
            return EntityCommandBufferSystem.CreateCommandBuffer(ref *pendingBuffers, allocator, world);
        }

        /// <summary>
        /// Sets the list of command buffers to play back when this system updates.
        /// </summary>
        /// <remarks>This method is only intended for internal use, but must be in the public API due to language
        public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
        {
            pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
        }
        public void SetAllocator(Allocator allocatorIn)
        {
            allocator = allocatorIn;
        }
        public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
        {
            allocator = allocatorIn;
        }
    }
}

[UpdateInGroup(typeof(CreationSystemGroup), OrderLast = true)]
public partial class EndCreationECBSystem : EntityCommandBufferSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();

        this.RegisterSingleton<Singleton>(ref PendingBuffers, World.Unmanaged);
    }
    public unsafe struct Singleton : IComponentData, IECBSingleton
    {
        internal UnsafeList<EntityCommandBuffer>* pendingBuffers;
        internal AllocatorManager.AllocatorHandle allocator;
        public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
        {
            return EntityCommandBufferSystem.CreateCommandBuffer(ref *pendingBuffers, allocator, world);
        }

        /// <summary>
        /// Sets the list of command buffers to play back when this system updates.
        /// </summary>
        /// <remarks>This method is only intended for internal use, but must be in the public API due to language
        public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
        {
            pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
        }
        public void SetAllocator(Allocator allocatorIn)
        {
            allocator = allocatorIn;
        }
        public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
        {
            allocator = allocatorIn;
        }
    }
}

[UpdateInGroup(typeof(ComponentInitializationSystemGroup), OrderLast = true)]
public partial class EndComponentInitializationECBSystem : EntityCommandBufferSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();

        this.RegisterSingleton<Singleton>(ref PendingBuffers, World.Unmanaged);
    }
    public unsafe struct Singleton : IComponentData, IECBSingleton
    {
        internal UnsafeList<EntityCommandBuffer>* pendingBuffers;
        internal AllocatorManager.AllocatorHandle allocator;
        public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
        {
            return EntityCommandBufferSystem.CreateCommandBuffer(ref *pendingBuffers, allocator, world);
        }

        /// <summary>
        /// Sets the list of command buffers to play back when this system updates.
        /// </summary>
        /// <remarks>This method is only intended for internal use, but must be in the public API due to language
        public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
        {
            pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
        }
        public void SetAllocator(Allocator allocatorIn)
        {
            allocator = allocatorIn;
        }
        public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
        {
            allocator = allocatorIn;
        }
    }
}