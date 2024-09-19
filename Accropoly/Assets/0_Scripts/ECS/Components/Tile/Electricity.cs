using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ElectricityProducer : IComponentData
{
    public float previousProduction;
    public float production;
    public void UpdateProduction(float newProduction)
    {
        previousProduction = production;
        production = newProduction;
    }
}
public struct ElectricityConsumer : IComponentData
{
    public float previousConsumption;
    public float consumption;
    public void UpdateConsumption(float newConsumption)
    {
        previousConsumption = consumption;
        consumption = newConsumption;
    }
}
public struct UpdatedElectricityTag : IComponentData { }
public struct HasElectricityTag : IComponentData, IEnableableComponent { }