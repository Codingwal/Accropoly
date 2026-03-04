using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

public class WorldLoader
{
    public List<TypeManager.TypeInfo> types;
    private Dictionary<int, Entity> entityMap; // old id -> new id
    private EntityManager entityManager;
    public WorldLoader(EntityManager _entityManager)
    {
        types = new();
        entityMap = new();
        entityManager = _entityManager;

        var typeInfos = TypeManager.GetAllTypes();
        foreach (TypeManager.TypeInfo type in typeInfos)
        {
            if (type.TypeIndex == TypeIndex.Null)
                continue;

            if (type.Category == TypeManager.TypeCategory.ComponentData
                && type.Type.GetCustomAttribute(typeof(SaveAttribute)) != null)
            {
                types.Add(type);
            }
        }
    }
    public void Load(WorldSave save)
    {
        foreach (var componentSave in save.componentSaves)
        {
            // TODO: Use attribute name
            TypeManager.TypeInfo? type = types.Find((t) => t.Type.Name == componentSave.name);
            if (!type.HasValue)
                throw new($"ComponentType \"{componentSave.name}\" does not exist");

            // this.LoadComponent<type.Type>(entityManager);
            MethodInfo method = GetType().GetMethod(nameof(LoadComponent), BindingFlags.NonPublic | BindingFlags.Instance);
            method = method.MakeGenericMethod(type.Value.Type);
            method.Invoke(this, new object[] { componentSave });
        }
    }

    private unsafe void LoadComponent<T>(WorldSave.ComponentSave save) where T : unmanaged, IComponentData
    {
        for (int i = 0; i < save.entityIds.Length; i++)
        {
            int entityId = save.entityIds[i];
            T component = UnsafeUtility.ReadArrayElement<T>(save.components.Ptr, i);

            if (!entityMap.ContainsKey(entityId))
                entityMap[entityId] = entityManager.CreateEntity();

            entityManager.AddComponentData(entityMap[entityId], component);

            if (component is IEnableableComponent)
                entityManager.SetComponentEnabled(entityMap[entityId], typeof(T), save.enabled[i]);
        }
    }
}