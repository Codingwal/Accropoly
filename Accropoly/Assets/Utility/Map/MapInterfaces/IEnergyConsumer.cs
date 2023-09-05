using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnergyConsumer : IMapTile
{
    public float EnergyConsumption { get; }
}
