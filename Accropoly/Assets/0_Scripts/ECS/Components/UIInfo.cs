using Unity.Entities;

namespace Components
{
    public struct UIInfo : IComponentData
    {
        public float populationSize; // The total population size. This also includes homeless people
        public float happinessSum; // The sum of the happiness of all people. averageHappiness = happinessSum / populationSize
        public float unemployedCount;// unemploymentRate = unemployedCount / populationSize

        public float electricityProduction;
        public float maxElectricityConsumption; // The consumption if there was an infite supply of electricity
        public float actualElectricityConsumption; // The actual consumption, never bigger than the production

        public float pollution;
        public float electricityPollution; // The pollution caused by electricity producing tiles

        public float lastTaxIncome; // The sum of all taxes from the last taxing frame
    }
}