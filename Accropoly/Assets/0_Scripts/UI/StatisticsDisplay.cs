using TMPro;
using UnityEngine;
using Unity.Mathematics;

public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text populationText;
    [SerializeField] private TMP_Text happinessText;
    [SerializeField] private TMP_Text unemployedText;
    [SerializeField] private TMP_Text electricityText;
    [SerializeField] private TMP_Text lastTaxIncomeText;

    [SerializeField] private TMP_Text timeText;

    private void Start()
    {
        MenuUtility.InitUIInfo();
    }
    private void Update()
    {
        var uiInfo = MenuUtility.GetUIInfo();
        var gameInfo = MenuUtility.GetGameInfo();

        balanceText.text = $"Balance: {Format(gameInfo.balance)}";

        populationText.text = $"Population size: {Format(uiInfo.populationSize)}";

        float averageHappiness = uiInfo.happinessSum / uiInfo.populationSize; // 0 - 100
        happinessText.text = (uiInfo.populationSize == 0) ? $" Average happiness: -" : $"Average happiness: {math.round(averageHappiness)}%";

        float unemploymentRate = uiInfo.unemployedCount / uiInfo.populationSize; // 0 - 1
        unemployedText.text = (uiInfo.populationSize == 0) ? $" Unemployment rate: -" : $"Unemployment rate: {math.round(unemploymentRate * 100)}%";

        string electricityConsumptionText = Format(uiInfo.maxElectricityConsumption);
        string electricityProductionText = Format(uiInfo.electricityProduction);
        electricityText.text = $"Electricity: {electricityConsumptionText}/{electricityProductionText}";

        lastTaxIncomeText.text = $"Last tax income: {uiInfo.lastTaxIncome}";

        timeText.text = gameInfo.time.ToString();
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
