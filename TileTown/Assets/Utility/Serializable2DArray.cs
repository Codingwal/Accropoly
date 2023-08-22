using System;
using UnityEngine;

[Serializable]
public class ArrayWrapper<T>
{
    public T[] array;

    public ArrayWrapper(int size)
    {
        array = new T[size];
    }

    public T this[int index]
    {
        get
        {
            return array[index];
        }
        set
        {
            array[index] = value;
        }
    }

    public int GetLength()
    {
        return array.Length;
    }
}

[Serializable]
public class Serializable2DArray<T>
{
    public ArrayWrapper<T>[] array2D;
    public Serializable2DArray(int x, int y)
    {
        array2D = new ArrayWrapper<T>[x];
        for (int i = 0; i < array2D.Length; i++)
        {
            array2D[i] = new ArrayWrapper<T>(y);
        }
    }

    public T this[int x, int y]
    {
        get
        {
            return array2D[x][y];
        }
        set
        {
            try
            {
                array2D[x][y] = value;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.Log($"[{x}, {y}], array size: [{GetLength(0)}, {GetLength(1)}]");
            }

        }
    }
    public int GetLength(int dimension)
    {
        return dimension switch
        {
            0 => array2D.Length,
            1 => array2D[0].GetLength(),
            _ => -1
        };
    }
}
