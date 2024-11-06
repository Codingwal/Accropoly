using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct Habitat : IComponentData
{
    public int totalSpace;
    public int freeSpace;
}

public struct HasSpaceTag : IComponentData { }