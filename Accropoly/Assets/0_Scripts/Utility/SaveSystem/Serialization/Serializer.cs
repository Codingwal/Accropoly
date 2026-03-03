using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe struct Serializer
{
    private delegate void TypeSerializer(Serializer serializer, void* data);
    private readonly Dictionary<Type, TypeSerializer> typeSerializers;
    private readonly IWriter writer;
    public Serializer(IWriter _writer)
    {
        writer = _writer;

        typeSerializers = new()
        {
            { typeof(int), (serializer, data) => serializer.writer.Write(*(int*)data) },
            { typeof(float), (serializer, data) => serializer.writer.Write(*(float*)data) },
            { typeof(FixedString32Bytes), (serializer, data) => serializer.writer.Write((*(FixedString32Bytes*)data).ToString()) }
            // TODO: Native containers
        };
    }

    public readonly void Serialize<T>(T data) where T : unmanaged
    {
        Serialize(typeof(T), UnsafeUtility.AddressOf(ref data));
    }
    private readonly void Serialize(Type type, void* data, int recursion = 0)
    {
        if (recursion > 10)
            throw new($"Encountered recursion bug while serializing {type}");

        if (type.GetInterfaces().Contains(typeof(ICustomSaving)))
        {
            object obj = Marshal.PtrToStructure(new(data), type);
            ((ICustomSaving)obj).Save(this);
            return;
        }

        if (typeSerializers.TryGetValue(type, out var typeSerializer))
        {
            typeSerializer(this, data);
            return;
        }

        var fields = type.GetFields();
        foreach (FieldInfo field in fields)
        {
            if (field.IsStatic)
                continue;

            if (field.GetCustomAttribute(typeof(DontSaveAttribute)) != null)
                continue;

            int fieldOffset = UnsafeUtility.GetFieldOffset(field);
            Type fieldType = field.FieldType;
            void* fieldAddress = (byte*)data + fieldOffset;

            Serialize(fieldType, fieldAddress, recursion + 1);
        }
    }
}