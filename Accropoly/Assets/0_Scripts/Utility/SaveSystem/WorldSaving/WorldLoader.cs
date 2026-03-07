using System.Collections.Generic;
using System.Reflection;
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
        entityMap = new();
        entityManager = _entityManager;

        types = SaveComponents.GetSaveComponentTypeInfos();
    }
    public void Load(WorldSave save)
    {
        foreach (var componentSave in save.componentSaves)
        {
            // TODO: Use attribute name
            TypeManager.TypeInfo type = types.Find((t) => t.Type.Name == componentSave.name);
            if (type.Type == null)
                throw new($"ComponentType \"{componentSave.name}\" does not exist");

            // this.LoadComponent<type.Type>(entityManager);
            MethodInfo method = GetType().GetMethod(nameof(LoadComponent), BindingFlags.NonPublic | BindingFlags.Instance);
            method = method.MakeGenericMethod(type.Type);
            method.Invoke(this, new object[] { componentSave });
        }
    }

    private void LoadComponent<T>(WorldSave.ComponentSave save) where T : unmanaged, IComponentData
    {
        Deserializer componentDeserializer = new(new NativeListReader(save.components));

        for (int i = 0; i < save.entityIds.Length; i++)
        {
            int entityId = save.entityIds[i];

            if (!entityMap.ContainsKey(entityId))
                entityMap[entityId] = entityManager.CreateEntity();

            T component = componentDeserializer.Deserialize<T>();
            if (component is Components.Tile tile)
                Debug.Log($"Tile pos: {tile.pos}");
            entityManager.AddComponentData(entityMap[entityId], component);

            if (component is IEnableableComponent)
                entityManager.SetComponentEnabled(entityMap[entityId], typeof(T), save.enabled[i]);
        }
    }
}