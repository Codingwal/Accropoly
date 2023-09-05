using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStreet : IMapTile
{
    public List<Transform> ConnectableTiles { get; }
}
