using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct UIInfo : IComponentData
{
    public float populationSize;

    public float electricityProduction;
    public float maxElectricityConsumption;
    public float actualElectricityConsumption;

    public float pollution;
    public float pollutionPerElectricity;
}
