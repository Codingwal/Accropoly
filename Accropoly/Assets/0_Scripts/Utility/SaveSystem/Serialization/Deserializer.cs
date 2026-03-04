using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public unsafe struct Deserializer
{
    public delegate void TypeDeserializer(Deserializer deserializer, void* data);
    public readonly Dictionary<Type, TypeDeserializer> typeDeserializers;
    private readonly IReader reader;
    public Deserializer(IReader _reader)
    {
        reader = _reader;

        typeDeserializers = new();
        DefaultDeserializers.GetDeserializers(typeDeserializers);
    }

    public readonly T Deserialize<T>() where T : unmanaged
    {
        T data = new();
        Deserialize(typeof(T), UnsafeUtility.AddressOf(ref data));
        return data;
    }
    private readonly void Deserialize(Type type, void* data, int recursion = 0)
    {
        if (recursion > 10)
            throw new($"Encountered recursion bug while serializing {type}");

        if (type.GetInterfaces().Contains(typeof(ICustomSaving)))
        {
            object obj = Marshal.PtrToStructure((IntPtr)data, type);
            ((ICustomSaving)obj).Load(this);
            Marshal.StructureToPtr(obj, (IntPtr)data, false);
            return;
        }

        if (TryCustomDeserialization(type, data))
            return;

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.GetCustomAttribute(typeof(DontSaveAttribute)) != null)
                continue;

            int fieldOffset = UnsafeUtility.GetFieldOffset(field);
            Type fieldType = field.FieldType;
            void* fieldAddress = (byte*)data + fieldOffset;

            Deserialize(fieldType, fieldAddress, recursion + 1);
        }
    }

    private readonly bool TryCustomDeserialization(Type type, void* data)
    {
        if (typeDeserializers.TryGetValue(type, out var typeSerializer))
        {
            typeSerializer(this, data);
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(UnsafeList<>))
        {
            Type elementType = type.GetGenericArguments()[0];

            // object obj = this.DeserializeUnsafeList<elementType>(obj);
            GetType().GetMethod(nameof(DeserializeUnsafeList), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(elementType)
                .Invoke(this, new object[] { new IntPtr(data) });

            return true;
        }

        return false;
    }

    private readonly void DeserializeUnsafeList<T>(IntPtr ptr)
        where T : unmanaged
    {
        var listPtr = (UnsafeList<T>*)ptr;

        int length = Deserialize<int>();
        *listPtr = new(length, Allocator.Persistent);
        for (int i = 0; i < length; i++)
            listPtr->Add(Deserialize<T>());
    }

    public static class DefaultDeserializers
    {
        public static void GetDeserializers(Dictionary<Type, TypeDeserializer> typeDeserializers)
        {
            typeDeserializers.Add(typeof(int), (d, data) => *(int*)data = d.reader.ReadInt());
            typeDeserializers.Add(typeof(float), (d, data) => *(float*)data = d.reader.ReadFloat());
            typeDeserializers.Add(typeof(bool), (d, data) => *(bool*)data = d.reader.ReadBool());
            typeDeserializers.Add(typeof(FixedString32Bytes), (d, data) =>
            {
                string str = d.reader.ReadStr();
                *(FixedString32Bytes*)data = str;
            });
            typeDeserializers.Add(typeof(byte), (d, data) => *(byte*)data = d.reader.ReadByte());

            // TODO: Native containers
        }
    }
}