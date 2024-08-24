using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public partial class Deserializer
{
    readonly BinaryReader br;
    public Deserializer(BinaryReader br)
    {
        this.br = br;
    }
    public byte Deserialize(byte data) { return br.ReadByte(); }
    public bool Deserialize(bool data) { return br.ReadBoolean(); }
    public int Deserialize(int data) { return br.ReadInt32(); }
    public uint Deserialize(uint data) { return br.ReadUInt32(); }
    public float Deserialize(float data) { return br.ReadSingle(); }
    public char Deserialize(char data) { return br.ReadChar(); }
    public string Deserialize(string data) { return br.ReadString(); }

    public List<T> Deserialize<T>(List<T> data) where T : new()
    {
        int size = br.ReadInt32();
        data = new(size);
        for (int i = 0; i < size; i++)
        {
            data.Add(Deserialize((dynamic)new T()));
        }
        return data;
    }
    public Dictionary<TKey, TValue> Deserialize<TKey, TValue>(Dictionary<TKey, TValue> data) where TKey : notnull, new() where TValue : new()
    {
        int size = br.ReadInt32();
        data = new(size);
        for (int i = 0; i < size; i++)
        {
            data.Add(Deserialize((dynamic)new TKey()), Deserialize((dynamic)new TValue()));
        }
        return data;
    }
}
