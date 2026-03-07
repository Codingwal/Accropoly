using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public unsafe struct Serializer
{
    public delegate void TypeSerializer(Serializer serializer, void* data);
    public readonly Dictionary<Type, TypeSerializer> typeSerializers;
    public readonly IWriter writer;
    public Serializer(IWriter _writer)
    {
        writer = _writer;

        typeSerializers = new();
        DefaultSerializers.GetSerializers(typeSerializers);
    }

    public readonly void Serialize<T>(T data) where T : unmanaged
    {
        Serialize(typeof(T), UnsafeUtility.AddressOf(ref data));
    }
    public readonly void Serialize(Type type, void* data, int recursion = 0)
    {
        if (recursion > 10)
            throw new($"Encountered recursion bug while deserializing {type}");

        if (type.GetInterfaces().Contains(typeof(ICustomSaving)))
        {
            object obj = Marshal.PtrToStructure(new(data), type);
            ((ICustomSaving)obj).Save(this);
            return;
        }

        if (TryCustomSerialization(type, data))
            return;

        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.GetCustomAttribute(typeof(DontSaveAttribute)) != null)
                continue;

            int fieldOffset = UnsafeUtility.GetFieldOffset(field);
            Type fieldType = field.FieldType;
            void* fieldAddress = (byte*)data + fieldOffset;

            if (fieldType.IsPointer)
                Debug.LogWarning($"Serializing pointer type ({fieldType})");

            Serialize(fieldType, fieldAddress, recursion + 1);
        }
    }

    private readonly bool TryCustomSerialization(Type type, void* data)
    {
        if (typeSerializers.TryGetValue(type, out var typeSerializer))
        {
            typeSerializer(this, data);
            return true;
        }

        if (!type.IsGenericType)
            return false;

        // Handle generics

        if (type.GetGenericTypeDefinition() == typeof(UnsafeList<>))
        {
            Type elementType = type.GetGenericArguments()[0];

            // this.SerializeUnsafeList<elementType>(data);
            CallMethod(nameof(SerializeUnsafeList), elementType, data);

            return true;
        }
        else if (type.GetGenericTypeDefinition() == typeof(NativeList<>))
        {
            Type elementType = type.GetGenericArguments()[0];

            // this.SerializeNativeList<elementType>(data);
            CallMethod(nameof(SerializeNativeList), elementType, data);

            return true;
        }
        return false;
    }

    private readonly void SerializeNativeList<T>(IntPtr ptr)
        where T : unmanaged
    {
        var listPtr = (NativeList<T>*)ptr;
        Serialize(listPtr->Length);
        foreach (T element in *listPtr)
            Serialize(element);
    }
    private readonly void SerializeUnsafeList<T>(IntPtr ptr)
        where T : unmanaged
    {
        var listPtr = (UnsafeList<T>*)ptr;
        Serialize(listPtr->Length);
        foreach (T element in *listPtr)
            Serialize(element);
    }

    private readonly void CallMethod(string name, Type typeArgument, void* data)
    {
        MethodInfo method = GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        method = method.MakeGenericMethod(typeArgument);
        method.Invoke(this, new object[] { new IntPtr(data) });
    }

    public static class DefaultSerializers
    {
        public static void GetSerializers(Dictionary<Type, TypeSerializer> typeSerializers)
        {
            typeSerializers.Add(typeof(int), (s, data) => s.writer.Write(*(int*)data));
            typeSerializers.Add(typeof(float), (s, data) => s.writer.Write(*(float*)data));
            typeSerializers.Add(typeof(bool), (s, data) => s.writer.Write(*(bool*)data));
            typeSerializers.Add(typeof(byte), (s, data) => s.writer.Write(*(byte*)data));
            typeSerializers.Add(typeof(FixedString32Bytes), (s, data) => s.writer.Write(((FixedString32Bytes*)data)->ToString()));

            // TODO: Native containers
        }
    }
}