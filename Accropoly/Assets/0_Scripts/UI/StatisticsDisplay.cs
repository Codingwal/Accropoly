using TMPro;
using Unity.Entities;
using UnityEngine;

public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text electricityText;

    private void Start()
    {
        MenuUtility.InitUIInfo();
    }
    private void Update()
    {
        UIInfo info = MenuUtility.GetUIInfo();

        string electricityConsumptionText = Format(info.maxElectricityConsumption);
        string electricityProductionText = Format(info.electricityProduction);
        electricityText.text = $"Electricity: {electricityConsumptionText}/{electricityProductionText}";

        populationText.text = $"Population size: {Format(info.populationSize)}";
    }
    private string Format(float value)
    {
        return value switch
        {
            > 1000000000 => $"{value / 1000000000}mrd",
            > 1000000 => $"{value / 1000000}mio",
            > 1000 => $"{value / 1000}k",
            _ => $"{value}",
        };
    }
    /*
    private void Update()
    {
        float balance = TownManager.Instance.balance;
        float population = TownManager.Instance.population.Count;
        float energyConsumption = TownManager.Instance.EnergyConsumption;
        float energyProduction = TownManager.Instance.EnergyProduction;
        float averageHappiness = TownManager.Instance.AverageHappiness;

        balanceText.text = balance switch
        {
            > 1000000000 => $"Guthaben: {Math.Round(balance / 1000000000, 2)}mrd",
            > 1000000 => $"Guthaben: {Math.Round(balance / 1000000, 2)}mio",
            > 1000 => $"Guthaben: {Math.Round(balance / 1000, 2)}k",
            _ => $"Guthaben: {Math.Round(balance, 2)}",
        };

        populationText.text = population switch
        {
            > 1000000000 => $"Einwohner: {Math.Round(population / 1000000000, 2)}mrd",
            > 1000000 => $"Einwohner: {Math.Round(population / 1000000, 2)}mio",
            > 1000 => $"Einwohner: {Math.Round(population / 1000, 2)}k",
            _ => $"Einwohner: {Math.Round(population, 2)}",
        };

        string energyConsumptionText = energyConsumption switch
        {
            > 1000000000 => $"{energyConsumption / 1000000000}mrd",
            > 1000000 => $"{energyConsumption / 1000000}mio",
            > 1000 => $"{energyConsumption / 1000}k",
            _ => $"{energyConsumption}",
        };
        string energyProductionText = energyProduction switch
        {
            > 1000000000 => $"{energyProduction / 1000000000}mrd",
            > 1000000 => $"{energyProduction / 1000000}mio",
            > 1000 => $"{energyProduction / 1000}k",
            _ => $"{energyProduction}",
        };
        electricityText.text = $"Energie: {energyProductionText}/{energyConsumptionText}";

        averageHappinessText.text = $"Glücklichkeit: {Math.Round(averageHappiness, 1)}";
    }
    */
}
