using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPerson
{
    public GameObject PersonObject { get; }
    public Vector2 HomeTilePos { get; set; }
    public Vector2? WorkplaceTilePos { get; set; }
    public float Happiness { get; }
}
