using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnergyProducer : IMapTile
{
    public float EnergyProduction { get; }
}
