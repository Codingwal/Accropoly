using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public class WorldSaver
{
    public List<TypeManager.TypeInfo> types;
    private EntityManager entityManager;
    public WorldSaver(EntityManager _entityManager)
    {
        types = new();
        entityManager = _entityManager;

        foreach (var type in TypeManager.AllTypes)
        {
            if (type.Category == TypeManager.TypeCategory.ComponentData
                && type.Type.GetCustomAttribute(typeof(SaveAttribute)) != null)
            {
                types.Add(type);
            }
        }
    }
    public WorldSave Save()
    {
        WorldSave save = new() { componentSaves = new(5, Allocator.Persistent) };

        foreach (var type in types)
        {
            // this.SaveComponent<type.Type>(entityManager);
            var componentSave = GetType().GetMethod(nameof(SaveComponent), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(type.Type)
                .Invoke(this, new object[] { });

            save.componentSaves.Add((WorldSave.ComponentSave)componentSave);
        }

        return save;
    }

    private unsafe WorldSave.ComponentSave SaveComponent<T>()
        where T : unmanaged, IComponentData
    {
        EntityQuery query = new EntityQueryBuilder(Allocator.Temp).WithPresent<T>().Build(entityManager);
        WorldSave.ComponentSave componentSave = new()
        {
            entityIds = new(0, Allocator.Persistent),
            components = new(0, Allocator.Persistent),
            enabled = new(0, Allocator.Persistent)
        };

        NativeArray<T> components = query.ToComponentDataArray<T>(Allocator.Temp);
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);


        bool isEnableable = TypeManager.GetTypeIndex(typeof(T)).IsEnableable;

        componentSave.name = nameof(T); // TODO: Use attribute

        for (int i = 0; i < components.Length; i++)
        {
            componentSave.entityIds.Add(entities[i].Index);

            ref T dataRef = ref UnsafeUtility.ArrayElementAsRef<T>(components.GetUnsafePtr(), i);
            void* ptr = UnsafeUtility.AddressOf(ref dataRef);
            componentSave.components.AddRange(ptr, sizeof(T));

            // If isEnableable, check if enabled (otherwise just set as true)
            componentSave.enabled.Add(!isEnableable || entityManager.IsComponentEnabled(entities[i], typeof(T)));
        }

        return componentSave;
    }
}