using System;
using System.Collections.Generic;
using UnityEngine;

public class TownManager : Singleton<TownManager>
{
    /*
    // Economy
    public event RefAction<float> CollectInvoice;

    [Header("Economy")]
    public float balance;
    [SerializeField] private SerializableDictionary<TileType, TilePrice> tilePrices;
    public SerializableDictionary<TileType, float> tileExpenditure;
    public float taxPerHappiness;

    // Population
    [Header("Population")]
    [SerializeField] GameObject personPrefab;
    [SerializeField] Transform populationParentObject;

    public List<IPerson> population = new();

    // Electricity
    [Header("Electricity")]
    public SerializableDictionary<TileType, float> tileEnergyProduction;
    public List<IEnergyProducer> energyProducers = new();
    public List<IEnergyConsumer> energyConsumers = new();
    public float electricityUsagePerPerson = 4;
    public float electricityUsagePerWorker = 2;

    // Workplaces
    [Header("Workplaces")]
    public List<IWorkplace> workplaces = new();

    public float EnergyConsumption
    {
        get
        {
            float consumption = 0;
            foreach (IEnergyConsumer energyConsumer in energyConsumers)
            {
                consumption += energyConsumer.EnergyConsumption;
            }
            return consumption;
        }
    }
    public float EnergyProduction
    {
        get
        {
            float production = 0;
            foreach (IEnergyProducer energyProducer in energyProducers)
            {
                production += energyProducer.EnergyProduction;
            }
            return production;
        }
    }
    public float AverageHappiness
    {
        get
        {
            float happinessSum = 0;
            foreach (IPerson person in population)
            {
                happinessSum += person.Happiness;
            }
            if (population.Count == 0)
            {
                return 0;
            }
            return happinessSum / population.Count;
        }
    }

    private void OnEnable()
    {
        GameLoopManager.Instance.InitWorld += Init;
        GameLoopManager.Instance.SaveWorld += Save;

        transform.GetComponent<TimeManager>().NextDay += CalculateInvoice;
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.InitWorld -= Init;
        GameLoopManager.Instance.SaveWorld -= Save;

        transform.GetComponent<TimeManager>().NextDay -= CalculateInvoice;
    }

    // -------------------------------- Initialization & Saving -------------------------------- //
    private void Init(WorldData world)
    {
        return;
        // Delete all existing childs
        List<GameObject> childs = new();
        foreach (Transform child in populationParentObject)
        {
            childs.Add(child.gameObject);
        }
        foreach (GameObject child in childs)
        {
            Destroy(child);
        }

        // Create all saved people
        foreach (PersonData personData in world.population)
        {
            AddPerson(personData);
        }

        // Load balance
        balance = world.balance;
    }

    private void Save(ref WorldData world)
    {
        // Save balance
        world.balance = balance;
    }

    // -------------------------------- Economy -------------------------------- //
    public void CalculateInvoice()
    {
        float invoice = 0;

        CollectInvoice?.Invoke(ref invoice);

        invoice += AverageHappiness * population.Count * taxPerHappiness;
        balance += invoice;
    }

    /// <returns>Can the tile be bought</returns>
    public bool BuyTile(TileType tileType)
    {
        balance -= tilePrices[tileType].buyPrice;
        if (balance < 0)
        {
            balance += tilePrices[tileType].buyPrice;
            return false;
        }
        return true;
    }
    public void SellTile(TileType tileType)
    {
        balance += tilePrices[tileType].sellPrice;
    }

    // -------------------------------- Population -------------------------------- //
    public GameObject AddPerson(PersonData personData)
    {
        // Create the person
        GameObject person = Instantiate(personPrefab, populationParentObject);

        // Get the person script
        IPerson personScript = person.GetComponent<IPerson>();

        // Set the person data
        person.transform.position = personData.position;
        personScript.HomeTilePos = personData.homeTilePos;
        personScript.HasWorkplace = personData.hasWorkplace;
        personScript.WorkplaceTilePos = personData.workplaceTilePos;

        // Add the person to the population list
        population.Add(personScript);

        // Add the person to the house tile inhabitants list
        IHouseTile houseTileScript = MapHandler.Instance.map.GetValue(personScript.HomeTilePos).GetComponent<IHouseTile>();
        houseTileScript.Inhabitants.Add(person);

        if (personScript.HasWorkplace)
        {
            IWorkplace workplaceTileScript = MapHandler.Instance.map.GetValue(personScript.WorkplaceTilePos).GetComponent<IWorkplace>();
            workplaceTileScript.Workers.Add(personScript);
        }

        personScript.Init();

        return person;
    }
    public void RemovePerson(IPerson person)
    {
        population.Remove(person);
        Destroy(person.PersonObject);
    }

    // -------------------------------- Electricity -------------------------------- //
    public bool HasElectricity()
    {
        return EnergyProduction > EnergyConsumption;
    }
    */
}
[Serializable]
public struct TilePrice
{
    public int buyPrice;
    public int sellPrice;
}
