using System.IO;
using System.Text;

public interface IReader
{
    public void Init(FileStream fs);
    public int ReadInt();
    public float ReadFloat();
    public bool ReadBool();
    public string ReadStr();
    public byte ReadByte();

}

public struct BinReader : IReader
{
    private BinaryReader br;
    public void Init(FileStream fs) { br = new(fs); }
    public readonly int ReadInt() => br.ReadInt32();
    public readonly float ReadFloat() => br.ReadSingle();
    public readonly bool ReadBool() => br.ReadBoolean();
    public readonly string ReadStr() => br.ReadString();
    public readonly byte ReadByte() => br.ReadByte();
}