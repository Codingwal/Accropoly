using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHouseTile : IMapTile
{
    public List<GameObject> Inhabitants { get; set; }
}
