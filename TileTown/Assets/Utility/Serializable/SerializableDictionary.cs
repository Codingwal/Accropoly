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
            return values[keys.IndexOf(key)];
        }
    }
}
