public struct UserData
{
    public string worldName;
    public static UserData Default => new() { worldName = "" };
}
