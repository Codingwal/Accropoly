using System.IO;
using System.Text;

public interface IWriter
{
    public void Init(FileStream fs);
    public void Write(int data);
    public void Write(float data);
    public void Write(bool data);
    public void Write(string data);

}

public struct BinWriter : IWriter
{
    private BinaryWriter bw;
    public void Init(FileStream fs) { bw = new(fs); }
    public readonly void Write(int data) => bw.Write(data);
    public readonly void Write(float data) => bw.Write(data);
    public readonly void Write(bool data) => bw.Write(data);
    public readonly void Write(string data) => bw.Write(data);
}
public struct LineWriter : IWriter
{
    private FileStream fs;
    public void Init(FileStream fs) { this.fs = fs; }
    private readonly void WriteString(string data)
    {
        byte[] arr = new UTF8Encoding(true).GetBytes(data);
        fs.Write(arr, 0, arr.Length);

    }
    public readonly void Write(string data) => WriteString(data + "\n");
    public readonly void Write(int data) => Write(data.ToString());
    public readonly void Write(float data) => Write(data.ToString());
    public readonly void Write(bool data) => Write(data.ToString());
}