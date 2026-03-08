using Unity.Collections;

public struct UserData
{
    public FixedString32Bytes worldName;
    public static UserData Default => new() { worldName = "" };
}
