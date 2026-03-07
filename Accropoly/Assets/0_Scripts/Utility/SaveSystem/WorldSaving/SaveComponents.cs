using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using Unity.Transforms;
using static Unity.Entities.TypeManager;

public static class SaveComponents
{
    public static List<TypeManager.TypeInfo> GetSaveComponentTypeInfos()
    {
        List<TypeManager.TypeInfo> types = new();

        var typeInfos = GetAllTypes();
        foreach (TypeManager.TypeInfo typeInfo in typeInfos)
        {
            if (typeInfo.TypeIndex == TypeIndex.Null)
                continue;

            if (typeInfo.Type.GetCustomAttribute(typeof(SaveAttribute)) == null)
                continue;

            if (typeInfo.Category != TypeCategory.ComponentData)
                continue;

            types.Add(typeInfo);
        }

        types.Add(GetTypeInfo<LocalTransform>());

        return types;
    }
}