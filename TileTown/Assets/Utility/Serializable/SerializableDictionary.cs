using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new();
    [SerializeField] private List<TValue> values = new();
    public Dictionary<TKey, TValue> dictionary;
    public void OnAfterDeserialize()
    {
        dictionary.Clear();

        for (int i = 0; i < keys.Count; i++)
        {
            dictionary.Add(keys[i], values[i]);
        }
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        for (int i = 0; i < dictionary.Count; i++)
        {
            keys.Add(dictionary.ElementAt(i).Key);
            values.Add(dictionary.ElementAt(i).Value);
        }
    }
}
