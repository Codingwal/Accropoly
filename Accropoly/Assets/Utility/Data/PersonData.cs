using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct PersonData
{
    public float3 position;
    public float2 homeTilePos;
    public float2 workplaceTilePos;
    public bool hasWorkplace;
}
