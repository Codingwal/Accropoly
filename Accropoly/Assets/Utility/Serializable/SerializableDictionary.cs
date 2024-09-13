using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : IEnumerable<SerializableKeyValuePair<TKey, TValue>>
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

    public IEnumerator<SerializableKeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return (IEnumerator<SerializableKeyValuePair<TKey, TValue>>)new Enumerator(dictionary);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> dict)
    {
        Dictionary<TKey, TValue> newDict = new();
        foreach (var pair in dict)
        {
            if (!newDict.TryAdd(pair.key, pair.value))
                Debug.LogError($"Duplicate key {pair.key} in SerializableDictionary<{typeof(TKey)}, {typeof(TValue)}>");
        }
        return newDict;
    }
    public class Enumerator : IEnumerator
    {
        private List<SerializableKeyValuePair<TKey, TValue>> dict;
        private int index = -1;
        public object Current => dict[index];
        public Enumerator(List<SerializableKeyValuePair<TKey, TValue>> dict)
        {
            this.dict = dict;
        }
        public bool MoveNext()
        {
            index++;
            return index < dict.Count;
        }

        public void Reset()
        {
            index = -1;
        }
    }
}

[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey key;
    public TValue value;
}
