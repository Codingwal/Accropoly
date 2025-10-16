using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public unsafe interface IFixedArrayHelper<T> : IAspect where T : unmanaged
{
    public int Size { get; } // Element count
    public int ElementSize { get; }
    public T this[int index] { get; set; }
}
public struct FixedArray<THelper, T> where THelper : unmanaged, IFixedArrayHelper<T> where T : unmanaged
{
    private THelper helper;
    public int Size => helper.Size;
    public T this[int index]
    {
        get
        {
            OutOfRangeCheck(index);
            return helper[index];
        }
        set
        {
            OutOfRangeCheck(index);
            helper[index] = value;
        }
    }

    public FixedArray(T defaultValue)
    {
        helper = new();
        Clear(defaultValue);
    }

    public void Clear(T value)
    {
        for (int i = 0; i < Size; i++)
            this[i] = value;
    }

    public bool Contains(T value)
    {
        for (int i = 0; i < Size; i++)
        {
            if (this[i].Equals(value))
                return true;
        }
        return false;
    }

    public unsafe Enumerator GetEnumerator()
    {
        fixed (FixedArray<THelper, T>* ptr = &this)
        {
            return new Enumerator(ptr);
        }
    }

    private void OutOfRangeCheck(int index)
    {
        Debug.Assert(index < Size, "Index out of range");
    }

    public unsafe struct Enumerator : IEnumerator
    {
        private readonly FixedArray<THelper, T>* data;
        private int index;
        public readonly T Current => (*data)[index];
        readonly object IEnumerator.Current => Current;
        public Enumerator(FixedArray<THelper, T>* data)
        {
            this.data = data;
            index = -1;
        }
        public bool MoveNext()
        {
            index++;
            return index < (*data).Size;
        }
        public void Reset()
        {
            index = -1;
        }
    }
}

public unsafe struct FixedFloat3Array5Helper : IFixedArrayHelper<float3>
{
    public fixed float array[5 * 3];
    public readonly int Size => 5;
    public readonly int ElementSize => 3;

    public float3 this[int index]
    {
        get => new(array[index * 3], array[index * 3 + 1], array[index * 3 + 2]);
        set
        {
            array[index * 3] = value.x;
            array[index * 3 + 1] = value.y;
            array[index * 3 + 2] = value.z;
        }
    }
}
public unsafe struct FixedFloat3Array5 : IEnumerable
{
    private FixedArray<FixedFloat3Array5Helper, float3> data;
    public int Size => data.Size;
    public float3 this[int index]
    {
        get => data[index];
        set => data[index] = value;
    }
    public void Clear(float3 value) => data.Clear(value);
    public bool Contains(float3 value) => data.Contains(value);
    public unsafe FixedArray<FixedFloat3Array5Helper, float3>.Enumerator GetEnumerator() => data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public unsafe struct FixedEntityArray5Helper : IFixedArrayHelper<Entity>
{
    public fixed int array[5 * 2];
    public readonly int Size => 5;
    public readonly int ElementSize => 2;

    public Entity this[int index]
    {
        get => new() { Index = array[index * 2], Version = array[index * 2 + 1] };
        set
        {
            array[index * 2] = value.Index;
            array[index * 2 + 1] = value.Version;
        }
    }
}
public unsafe struct FixedEntityArray5 : IEnumerable
{
    private FixedArray<FixedEntityArray5Helper, Entity> data;
    public int Size => data.Size;
    public Entity this[int index]
    {
        get => data[index];
        set => data[index] = value;
    }
    public void Clear(Entity value) => data.Clear(value);
    public bool Contains(Entity value) => data.Contains(value);
    public unsafe FixedArray<FixedEntityArray5Helper, Entity>.Enumerator GetEnumerator() => data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
