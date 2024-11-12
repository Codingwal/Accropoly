using Unity.Entities;

public struct UIInfo : IComponentData
{
    public float populationSize;
    public float happinessSum; // The sum of the happiness of all people. averageHappiness = happinessSum / populationSize
    public float unemployedCount;

    public float electricityProduction;
    public float maxElectricityConsumption; // The consumption if there was an infite supply of electricity
    public float actualElectricityConsumption; // The actual consumption, never bigger than the production

    public float pollution;
    public float electricityPollution; // The pollution caused by electricity producing tiles
}
