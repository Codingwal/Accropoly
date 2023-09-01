using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    public TValue this[TKey key]
    {
        get
        {
            int valueIndex = keys.IndexOf(key);
            if (valueIndex == -1)
            {
                Debug.LogWarning("Key does not exist");
                return default;
            }
            return values[valueIndex];
        }
    }
}
