
using System;

public static class ConfigData
{
    public static SaveSystemConfig saveSystemConfig;
    public static TileConfig tileConfig;
    public static PopulationConfig populationConfig;
    public static CameraConfig cameraConfig;
    public static TimeConfig timeConfig;
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

[Serializable]
public struct PopulationConfig
{
    [Serializable]
    public struct Happiness
    {
        public float defaultHappiness; // The happiness before any other factor is taken into account

        // The impact on happiness of different factors
        public float homeless;
        public float hasElectricity;
        public float noElectricity;
        public float employed;
        public float unemployed;
    }
    public Happiness happiness;

    [Serializable]
    public struct Taxes
    {
        public float taxPerHappiness;
    }
    public Taxes taxes;
}

[Serializable]
public struct CameraConfig
{
    // Movement
    public float moveSpeed;

    // Rotation
    public float rotationSpeed;

    // Zooming
    public float zoomSpeed;
    public float minDistance;
    public float maxDistance;

    // Looking
    public float lookSpeed;
    public float minAngle;
    public float maxAngle;

    // Sprinting
    public float sprintSpeedMultiplier;
}

[Serializable]
public struct TimeConfig
{
    public float secondsPerDay;
    public readonly float TimeSpeed => 24 * 60 * 60 / secondsPerDay;
}