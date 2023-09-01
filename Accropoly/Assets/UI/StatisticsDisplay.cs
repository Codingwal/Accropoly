using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text electricityText;

    private void Update()
    {
        int balance = (int)Math.Round(TownManager.Instance.balance);
        int population = TownManager.Instance.population.Count;
        float energyConsumption = TownManager.Instance.EnergyConsumption;
        float energyProduction = TownManager.Instance.EnergyProduction;

        balanceText.text = balance switch
        {
            > 1000000000 => $"Balance: {balance / 1000000000}mrd",
            > 1000000 => $"Balance: {balance / 1000000}mio",
            > 1000 => $"Balance: {balance / 1000}k",
            _ => $"Balance: {balance}",
        };

        populationText.text = population switch
        {
            > 1000000000 => $"Population: {population / 1000000000}mrd",
            > 1000000 => $"Population: {population / 1000000}mio",
            > 1000 => $"Population: {population / 1000}k",
            _ => $"Population: {population}",
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
        electricityText.text = $"Energy: {energyProductionText}/{energyConsumptionText}";
    }
}
