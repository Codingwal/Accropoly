using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public interface IWriter
{
    public void Write(int data);
    public void Write(float data);
    public void Write(bool data);
    public void Write(byte data);
    public void Write(char data);
    public void Write(string data);

}

public readonly struct BinWriter : IWriter
{
    private readonly BinaryWriter bw;
    public BinWriter(FileStream fs)
    {
        bw = new BinaryWriter(fs);
    }
    public readonly void Write(int data) => bw.Write(data);
    public readonly void Write(float data) => bw.Write(data);
    public readonly void Write(bool data) => bw.Write(data);
    public readonly void Write(byte data) => bw.Write(data);
    public readonly void Write(char data) => bw.Write(data);
    public readonly void Write(string data) => bw.Write(data);
}
public readonly struct LineWriter : IWriter
{
    private readonly FileStream fs;
    public LineWriter(FileStream fs)
    {
        this.fs = fs;
    }
    private readonly void WriteString(string data)
    {
        byte[] arr = new UTF8Encoding(true).GetBytes(data);
        fs.Write(arr, 0, arr.Length);

    }
    public readonly void Write(int data) => Write(data.ToString());
    public readonly void Write(float data) => Write(data.ToString());
    public readonly void Write(bool data) => Write(data.ToString());
    public readonly void Write(byte data) => Write(data.ToString());
    public readonly void Write(char data) => Write(data.ToString());
    public readonly void Write(string data) => WriteString(data + "\n");
}
public readonly unsafe struct NativeListWriter : IWriter
{
    private readonly NativeList<byte> data;
    public NativeListWriter(NativeList<byte> data)
    {
        this.data = data;
    }

    public readonly void Write(int value) => WriteGeneric(value);
    public readonly void Write(float value) => WriteGeneric(value);
    public readonly void Write(bool value) => WriteGeneric(value);
    public readonly void Write(char value) => WriteGeneric(value);
    public readonly void Write(byte value) => WriteGeneric(value);
    public readonly void Write(string str)
    {
        foreach (char c in str)
            Write(c);
        Write('\0');
    }

    private readonly void WriteGeneric<T>(T value)
        where T : unmanaged
    {
        data.AddRange(&value, sizeof(T));
    }

}