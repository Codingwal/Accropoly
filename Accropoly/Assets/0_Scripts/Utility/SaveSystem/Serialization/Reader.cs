using System.IO;
using System.Text;

public interface IReader
{
    public int ReadInt();
    public float ReadFloat();
    public bool ReadBool();
    public string ReadStr();

}

public struct BinReader : IReader
{
    private BinaryReader br;
    public BinReader(BinaryReader _br)
    {
        br = _br;
    }
    public readonly int ReadInt() => br.ReadInt32();
    public readonly float ReadFloat() => br.ReadSingle();
    public readonly bool ReadBool() => br.ReadBoolean();
    public readonly string ReadStr() => br.ReadString();
}