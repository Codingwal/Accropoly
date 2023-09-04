using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();

    public int Count { get { return keys.Count; } }

    public TValue this[TKey key]
    {
        get
        {
            int valueIndex = keys.IndexOf(key);
            if (valueIndex == -1)
            {
                Debug.LogWarning($"Key {key} does not exist");
                return default;
            }
            if (values.Count < keys.Count)
            {
                Debug.LogWarning("There are more keys than values");
            }
            return values[valueIndex];
        }
    }
    public bool Contains(TKey key)
    {
        return keys.IndexOf(key) != -1;
    }
}
