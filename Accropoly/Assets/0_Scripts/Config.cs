
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
    public static readonly SharedStatic<WaypointConfig> waypointConfig = SharedStatic<WaypointConfig>.GetOrCreate<WaypointConfigKey>();

    private class SaveSystemConfigKey { }
    private class TileConfigContextKey { }
    private class PopulationConfigKey { }
    private class CameraConfigKey { }
    private class TimeConfigKey { }
    private class WaypointConfigKey { }
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

public struct WaypointConfig
{
    public struct WaypointData
    {
        public float3 position;
        public Waypoint waypointData; // allowedObjects & velocity
        public NativeList<Connection> connections; // next waypoints
        public JunctionData junctionData;
    }
    public struct ElementData
    {
        public NativeList<WaypointData> waypoints;
    }
    public struct TileData
    {
        public struct Element
        {
            public FixedString32Bytes name;
            public Direction rotation;
        }
        public NativeList<Element> elements;
    }

    public NativeHashMap<FixedString32Bytes, TileData> tiles;
    public NativeHashMap<FixedString32Bytes, ElementData> elements;
}

[Serializable]
public struct WaypointConfigSerialized
{
    [Serializable]
    public struct Connection
    {
        public List<float> position;
        public List<float> controlPoint;
    }

    [Serializable]
    public struct WaypointData
    {
        public List<float> position; // float3
        public TravelObjects allowedObjects;
        public float velocity;
        public List<Connection> nextWaypoints;
        public JunctionData junctionData;
    }

    [Serializable]
    public struct ElementData
    {
        public List<WaypointData> waypoints;
    }

    [Serializable]
    public struct TileData
    {
        public struct Element
        {
            public string name;
            public int rotation;
        }
        public List<Element> elements;
    }

    public Dictionary<string, ElementData> elements;
    public Dictionary<string, TileData> tiles;
}