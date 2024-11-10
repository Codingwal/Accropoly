using System.Collections.Generic;
using System.IO;

public partial class Serializer
{
    readonly BinaryWriter bw;
    public Serializer(BinaryWriter bw)
    {
        this.bw = bw;
    }
    public void Serialize(byte data) { bw.Write(data); }
    public void Serialize(bool data) { bw.Write(data); }
    public void Serialize(int data) { bw.Write(data); }
    public void Serialize(uint data) { bw.Write(data); }
    public void Serialize(float data) { bw.Write(data); }
    public void Serialize(char data) { bw.Write(data); }
    public void Serialize(string data) { bw.Write(data); }

    public void Serialize<T>(T[] data)
    {
        bw.Write(data.Length);
        foreach (var e in data)
        {
            Serialize((dynamic)e);
        }
    }
    public void Serialize<T>(T[,] data)
    {
        bw.Write(data.GetLength(0));
        bw.Write(data.GetLength(1));
        for (int x = 0; x < data.GetLength(0); x++)
        {
            for (int y = 0; y < data.GetLength(1); y++)
            {
                Serialize((dynamic)data[x, y]);
            }
        }
    }
    public void Serialize<T>(List<T> data)
    {
        bw.Write(data.Count);
        foreach (var e in data)
        {
            Serialize((dynamic)e);
        }
    }
    public void Serialize<TKey, TValue>(Dictionary<TKey, TValue> data) where TKey : notnull
    {
        bw.Write(data.Count);
        foreach (var pair in data)
        {
            Serialize((dynamic)pair.Key);
            Serialize((dynamic)pair.Value);
        }
    }
}
