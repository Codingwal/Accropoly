using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatisticsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text populationText;

    private void Update()
    {
        int balance = (int)Math.Round(EconomyManager.Instance.balance);
        int population = PopulationManager.Instance.population.Count;

        balanceText.text = "Balance: " + balance;
        populationText.text = "Population: " + population;
    }
}
