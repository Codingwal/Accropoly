
using System;
using System.Collections.Generic;
using Components.WaypointComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Components.WaypointComponents.Junction;

public abstract class ConfigData
{
    public static readonly SharedStatic<SaveSystemConfig> saveSystemConfig = SharedStatic<SaveSystemConfig>.GetOrCreate<SaveSystemConfigKey>();
    public static readonly SharedStatic<TileConfig> tileConfig = SharedStatic<TileConfig>.GetOrCreate<TileConfigContextKey>();
    public static readonly SharedStatic<PopulationConfig> populationConfig = SharedStatic<PopulationConfig>.GetOrCreate<PopulationConfigKey>();
    public static readonly SharedStatic<CameraConfig> cameraConfig = SharedStatic<CameraConfig>.GetOrCreate<CameraConfigKey>();
    public static readonly SharedStatic<TimeConfig> timeConfig = SharedStatic<TimeConfig>.GetOrCreate<TimeConfigKey>();
    public static readonly SharedStatic<WaypointConfigUnmanaged> waypointConfig = SharedStatic<WaypointConfigUnmanaged>.GetOrCreate<WaypointConfigUnmanagedKey>();

    private class SaveSystemConfigKey { }
    private class TileConfigContextKey { }
    private class PopulationConfigKey { }
    private class CameraConfigKey { }
    private class TimeConfigKey { }
    private class WaypointConfigUnmanagedKey { }
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

[Serializable]
public struct WaypointConfigManaged
{
    [Serializable]
    public struct Waypoint
    {
        public List<float> position; // float3
        public TravelObjects allowedObjects;
        public float velocity;
        public List<string> nextWaypoints;
        public JunctionData junctionData;
        public bool entry;
        public bool exit;
    }

    [Serializable]
    public struct TileWaypoints
    {
        public Dictionary<string, Waypoint> waypoints;
    }

    public Dictionary<string, TileWaypoints> tileToWaypoints;
}

public struct WaypointConfigUnmanaged
{
    public struct WaypointData
    {
        public float3 position;
        public TravelObjects allowedObjects;
        public float velocity;
        public NewWaypoint newWaypointData;
        public JunctionData junctionData;
        public bool entry;
        public bool exit;
    }

    public struct TileWaypoints
    {
        public NativeList<WaypointData> waypoints;
    }

    public NativeHashMap<FixedString32Bytes, TileWaypoints> tileToWaypoints;
}