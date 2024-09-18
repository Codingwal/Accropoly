using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ElectricityProducer : IComponentData
{
    public float previousProduction;
    public float production;
}
public struct ElectricityConsumer : IComponentData
{
    public float previousConsumption;
    public float consumption;
}
public struct UpdatedElectricityTag : IComponentData { }
public struct HasElectricityTag : IComponentData { }