using Unity.Entities;

public class Bootstrap : ICustomBootstrap
{
    public bool Initialize(string defaultWorldName)
    {
        SaveSystem.Initialize();

        return false;
    }
}
