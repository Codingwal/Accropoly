using TMPro;
using UnityEngine;
using Unity.Mathematics;

public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text unemployedText;
    [SerializeField] private TMP_Text electricityText;

    private void Start()
    {
        MenuUtility.InitUIInfo();
    }
    private void Update()
    {
        UIInfo info = MenuUtility.GetUIInfo();

        populationText.text = $"Population size: {Format(info.populationSize)}";

        float unemploymentRate = info.unemployedCount / info.populationSize;
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
