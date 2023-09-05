using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField] private List<SerializableKeyValuePair<TKey, TValue>> dictionary = new();

    public int Count { get { return dictionary.Count; } }

    public TValue this[TKey key]
    {
        get
        {
            foreach (SerializableKeyValuePair<TKey, TValue> keyValuePair in dictionary)
            {
                if (keyValuePair.key.Equals(key))
                {
                    return keyValuePair.value;
                }
            }
            Debug.LogWarning($"Key {key} does not exist");
            return default;
        }
    }
    public bool Contains(TKey key)
    {
        foreach (SerializableKeyValuePair<TKey, TValue> keyValuePair in dictionary)
        {
            if (keyValuePair.key.Equals(key))
            {
                return true;
            }
        }
        return false;
    }
}

[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey key;
    public TValue value;
}
