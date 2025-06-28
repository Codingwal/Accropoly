using Unity.Mathematics;
using UnityEngine;

public unsafe struct FixedFloat3Array5
{
    private const int size = 5;
    private fixed float array[size * 3];
    public readonly int Size => size; // Element count
    public float3 this[int index]
    {
        get
        {
            Debug.Assert(index < size, "Index out of range");
            return new float3()
            {
                x = array[index * 3],
                y = array[index * 3 + 1],
                z = array[index * 3 + 2],
            };
        }
        set
        {
            Debug.Assert(index < size, "Index out of range");
            array[index * 3] = value.x;
            array[index * 3 + 1] = value.y;
            array[index * 3 + 2] = value.z;
        }
    }

    /// <summary>
    /// Cleares all elements using the specified value
    /// </summary>
    public void Clear(float3 value)
    {
        for (int i = 0; i < Size; i++)
            this[i] = value;
    }
    public bool Contains(float3 value)
    {
        for (int i = 0; i < Size; i++)
        {
            if (this[i].Equals(value))
                return true;
        }
        return false;
    }
}

public unsafe struct FixedFloat3Array10
{
    private const int size = 10;
    private fixed float array[size * 3];
    public readonly int Size => size; // Element count
    public float3 this[int index]
    {
        get
        {
            Debug.Assert(index < size, "Index out of range");
            return new float3()
            {
                x = array[index * 3],
                y = array[index * 3 + 1],
                z = array[index * 3 + 2],
            };
        }
        set
        {
            Debug.Assert(index < size, "Index out of range");
            array[index * 3] = value.x;
            array[index * 3 + 1] = value.y;
            array[index * 3 + 2] = value.z;
        }
    }

    /// <summary>
    /// Cleares all elements using the specified value
    /// </summary>
    public void Clear(float3 value)
    {
        for (int i = 0; i < Size; i++)
            this[i] = value;
    }
    public bool Contains(float3 value)
    {
        for (int i = 0; i < Size; i++)
        {
            if (this[i].Equals(value))
                return true;
        }
        return false;
    }
}

public unsafe struct FixedFloat3Array20
{
    private const int size = 20;
    private fixed float array[size * 3];
    public readonly int Size => size; // Element count
    public float3 this[int index]
    {
        get
        {
            Debug.Assert(index < size, "Index out of range");
            return new float3()
            {
                x = array[index * 3],
                y = array[index * 3 + 1],
                z = array[index * 3 + 2],
            };
        }
        set
        {
            Debug.Assert(index < size, "Index out of range");
            array[index * 3] = value.x;
            array[index * 3 + 1] = value.y;
            array[index * 3 + 2] = value.z;
        }
    }

    /// <summary>
    /// Cleares all elements using the specified value
    /// </summary>
    public void Clear(float3 value)
    {
        for (int i = 0; i < Size; i++)
            this[i] = value;
    }
    public bool Contains(float3 value)
    {
        for (int i = 0; i < Size; i++)
        {
            if (this[i].Equals(value))
                return true;
        }
        return false;
    }
}