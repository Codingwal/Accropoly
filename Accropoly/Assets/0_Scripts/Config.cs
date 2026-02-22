
using System;

public static class ConfigData
{
    public static SaveSystemConfig saveSystemConfig;

    public static TileConfig tileConfig;
}

[Serializable]
public struct SaveSystemConfig
{
    public bool deleteTemplates;
    public bool deleteSaves;
    public bool overwriteFiles;
}

[Serializable]
public struct TileConfig
{
    [Serializable]
    public struct TileGrowing
    {
        // In in-game hours
        public int maxAge1;
        public int maxAge2;
    }
    public TileGrowing tileGrowing;
}