using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public interface IReader
{
    public int ReadInt();
    public float ReadFloat();
    public bool ReadBool();
    public byte ReadByte();
    public char ReadChar();
    public string ReadStr();
}

public readonly struct BinReader : IReader
{
    private readonly BinaryReader br;
    public BinReader(FileStream fs)
    {
        br = new BinaryReader(fs);
    }
    public readonly int ReadInt() => br.ReadInt32();
    public readonly float ReadFloat() => br.ReadSingle();
    public readonly bool ReadBool() => br.ReadBoolean();
    public readonly byte ReadByte() => br.ReadByte();
    public readonly char ReadChar() => br.ReadChar();
    public readonly string ReadStr() => br.ReadString();
}
public struct NativeListReader : IReader
{
    private readonly NativeList<byte> data;
    private int index;
    public NativeListReader(NativeList<byte> data)
    {
        this.data = data;
        index = 0;
    }

    public int ReadInt() => ReadGeneric<int>();
    public float ReadFloat() => ReadGeneric<float>();
    public bool ReadBool() => ReadGeneric<bool>();
    public byte ReadByte() => ReadGeneric<byte>();
    public char ReadChar() => ReadGeneric<char>();

    public string ReadStr()
    {
        string str = "";
        while (true)
        {
            char c = ReadChar();
            if (c == '\0') break;
            str += c;
        }
        return str;
    }

    private unsafe T ReadGeneric<T>()
        where T : unmanaged
    {
        T value = UnsafeUtility.ReadArrayElement<T>(data.GetUnsafePtr(), index);
        index++;
        return value;
    }
}