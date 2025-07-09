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

    /// <summary>
    /// Initializes all elements to defaultValue
    /// </summary>
    public FixedArray(T defaultValue)
    {
        helper = new();
        Clear(defaultValue);
    }

    /// <summary>
    /// Cleares all elements using the specified value
    /// </summary>
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

    public unsafe IEnumerator GetEnumerator()
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
        public readonly object Current => (*data)[index];

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
    public int Size => 5;
    public int ElementSize => 3;

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
    public unsafe IEnumerator GetEnumerator() => data.GetEnumerator();
}

public unsafe struct FixedFloat3Array10Helper : IFixedArrayHelper<float3>
{
    public fixed float array[10 * 3];
    public int Size => 10;
    public int ElementSize => 3;

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
public unsafe struct FixedFloat3Array10 : IEnumerable
{
    private FixedArray<FixedFloat3Array10Helper, float3> data;
    public int Size => data.Size;
    public float3 this[int index]
    {
        get => data[index];
        set => data[index] = value;
    }
    public void Clear(float3 value) => data.Clear(value);
    public bool Contains(float3 value) => data.Contains(value);
    public unsafe IEnumerator GetEnumerator() => data.GetEnumerator();
}

public unsafe struct FixedFloat3Array20Helper : IFixedArrayHelper<float3>
{
    public fixed float array[20 * 3];
    public int Size => 20;
    public int ElementSize => 3;

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
public unsafe struct FixedFloat3Array20 : IEnumerable
{
    private FixedArray<FixedFloat3Array20Helper, float3> data;
    public int Size => data.Size;
    public float3 this[int index]
    {
        get => data[index];
        set => data[index] = value;
    }
    public void Clear(float3 value) => data.Clear(value);
    public bool Contains(float3 value) => data.Contains(value);
    public unsafe IEnumerator GetEnumerator() => data.GetEnumerator();
}

public unsafe struct FixedEntityArray5Helper : IFixedArrayHelper<Entity>
{
    public fixed int array[5 * 2];
    public int Size => 5;
    public int ElementSize => 2;

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
    public unsafe IEnumerator GetEnumerator() => data.GetEnumerator();
}

public unsafe struct FixedEntityArray10Helper : IFixedArrayHelper<Entity>
{
    public fixed int array[10 * 2];
    public int Size => 10;
    public int ElementSize => 2;

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
public unsafe struct FixedEntityArray10 : IEnumerable
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
    public unsafe IEnumerator GetEnumerator() => data.GetEnumerator();
}

public unsafe struct FixedEntityArray20Helper : IFixedArrayHelper<Entity>
{
    public fixed int array[20 * 2];
    public int Size => 20;
    public int ElementSize => 2;

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
public unsafe struct FixedEntityArray20 : IEnumerable
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
    public unsafe IEnumerator GetEnumerator() => data.GetEnumerator();
}
