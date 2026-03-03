using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public unsafe struct Deserializer
{
    private delegate void TypeSerializer(Deserializer deserializer, void* data);
    private readonly Dictionary<Type, TypeSerializer> typeDeserializers;
    private readonly IReader reader;
    public Deserializer(IReader _reader)
    {
        reader = _reader;

        typeDeserializers = new()
        {
            { typeof(int), (deserializer, data) => *(int*)data = deserializer.reader.ReadInt()},
            { typeof(float), (deserializer, data) => *(float*)data = deserializer.reader.ReadFloat()},
            { typeof(FixedString32Bytes), (deserializer, data) =>*(FixedString32Bytes*)data = deserializer.reader.ReadStr() }
        };
    }

    public readonly T Deserialize<T>() where T : unmanaged
    {
        T data = new();
        Deserialize(typeof(T), UnsafeUtility.AddressOf(ref data));
        return data;
    }
    private readonly void Deserialize(Type type, void* data)
    {
        if (type.GetInterfaces().Contains(typeof(ICustomSaving)))
        {
            object obj = Marshal.PtrToStructure((IntPtr)data, type);
            ((ICustomSaving)obj).Load(this);
            Marshal.StructureToPtr(obj, (IntPtr)data, false);
            return;
        }

        if (typeDeserializers.TryGetValue(type, out var typeDeserializer))
        {
            typeDeserializer(this, data);
            return;
        }

        var fields = type.GetFields();
        foreach (FieldInfo field in fields)
        {
            if (field.GetCustomAttribute(typeof(DontSaveAttribute)) != null)
                continue;

            int fieldOffset = UnsafeUtility.GetFieldOffset(field);
            Type fieldType = field.FieldType;
            void* fieldAddress = (byte*)data + fieldOffset;

            Deserialize(fieldType, fieldAddress);
        }
    }
}