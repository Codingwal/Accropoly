using TMPro;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities.UniversalDelegates;

public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text happinessText;
    [SerializeField] private TMP_Text unemployedText;
    [SerializeField] private TMP_Text electricityText;

    private UIInfo previousInfo;
    private void Start()
    {
        MenuUtility.InitUIInfo();
        previousInfo = new UIInfo { populationSize = -1 }; // Causes update on first frame
    }
    private void Update()
    {
        UIInfo info = MenuUtility.GetUIInfo();

        if (info.Equals(previousInfo)) return;
        previousInfo = info;

        populationText.text = $"Population size: {Format(info.populationSize)}";

        float averageHappiness = info.happinessSum / info.populationSize; // 0 - 100
        happinessText.text = (info.populationSize == 0) ? $" Average happiness: -" : $"Average happiness: {math.round(averageHappiness)}%";

        float unemploymentRate = info.unemployedCount / info.populationSize; // 0 - 1
        unemployedText.text = (info.populationSize == 0) ? $" Unemployment rate: -" : $"Unemployment rate: {math.round(unemploymentRate * 100)}%";


        string electricityConsumptionText = Format(info.maxElectricityConsumption);
        string electricityProductionText = Format(info.electricityProduction);
        electricityText.text = $"Electricity: {electricityConsumptionText}/{electricityProductionText}";
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
}
