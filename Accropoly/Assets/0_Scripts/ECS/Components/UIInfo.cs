using Unity.Entities;

public struct UIInfo : IComponentData
{
    public float populationSize;
    public float unemployedCount;

    public float electricityProduction;
    public float maxElectricityConsumption;
    public float actualElectricityConsumption;

    public float pollution;
    public float pollutionPerElectricity;
}
