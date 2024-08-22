using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public static class WorldDataParser
{
    private static BinaryReader br;
    public static WorldData ReadWorldData(string fileName)
    {
        FileStream file = File.Open(fileName, FileMode.Open);
        br = new(file);

        return new()
        {
            playTime = br.ReadSingle(),
            cameraSystemPos = ReadFloat2(),
            cameraSystemRotation = ReadQuaternion(),
            followOffsetY = br.ReadSingle(),
            balance = br.ReadSingle(),
            population = ReadList(ReadPersonData),
        };
    }
    private static List<T> ReadList<T>(Func<T> callback)
    {
        List<T> list = new();
        int size = br.ReadInt32();
        for (int i = 0; i < size; i++)
        {
            list.Add(callback());
        }
        return list;
    }
    private static float2 ReadFloat2()
    {
        return new float2(br.ReadSingle(), br.ReadSingle());
    }
    private static float3 ReadFloat3()
    {
        return new float3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
    }
    private static quaternion ReadQuaternion()
    {
        return new quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
    }
    private static PersonData ReadPersonData()
    {
        return new()
        {
            position = ReadFloat3(),
            homeTilePos = ReadFloat2(),
            workplaceTilePos = ReadFloat2(),
            hasWorkplace = br.ReadBoolean()
        };
    }
    private static int2 ReadInt2()
    {
        return new(br.ReadInt32(), br.ReadInt32());
    }
    private static MapData ReadMapData()
    {
        return new()
        {
            size = ReadInt2(),
            
        };
    }
}
