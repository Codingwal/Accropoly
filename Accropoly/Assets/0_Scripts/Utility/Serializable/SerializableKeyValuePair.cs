using System;

[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey key;
    public TValue value;
    public void Deconstruct(out TKey key, out TValue value)
    {
        key = this.key;
        value = this.value;
    }
}
