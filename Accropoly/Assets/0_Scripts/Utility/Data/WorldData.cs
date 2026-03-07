using System;

[Serializable]
public struct WorldData : IDisposable
{
    public WorldSave worldSave;
    public void Dispose()
    {
        worldSave.Dispose();
    }
}