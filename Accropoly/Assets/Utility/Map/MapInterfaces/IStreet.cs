using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStreet
{
    public List<Transform> ConnectableTiles { get; }
}
