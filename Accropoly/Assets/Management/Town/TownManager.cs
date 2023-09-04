using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class TownManager : Singleton<TownManager>
{
    // Economy
    public event RefAction<float> CollectInvoice;

    [Header("Economy")]
    public float balance;
    [SerializeField] private SerializableDictionary<TileType, int> tileBuyPrices;
    [SerializeField] private SerializableDictionary<TileType, int> tileSellPrices;
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
    public float electricityUsagePerPerson;

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
            Debug.Log("1: " + happinessSum);
            Debug.Log("2: " + population.Count);
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

        GameLoopManager.Instance.Invoice += CalculateInvoice;
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.InitWorld -= Init;
        GameLoopManager.Instance.SaveWorld -= Save;

        GameLoopManager.Instance.Invoice -= CalculateInvoice;
    }

    // -------------------------------- Initialization & Saving -------------------------------- //
    private void Init(World world)
    {
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
        Debug.Log(balance);
    }

    private void Save(ref World world)
    {
        // Save balance
        world.balance = balance;
        Debug.Log(world.balance);
    }

    // -------------------------------- Economy -------------------------------- //
    public void CalculateInvoice()
    {
        Debug.Log("Before Invoice: " + balance);

        float Invoice = 0;

        CollectInvoice?.Invoke(ref Invoice);

        Debug.Log(AverageHappiness);
        Debug.Log(population.Count);
        Debug.Log(taxPerHappiness);

        Invoice += AverageHappiness * population.Count * taxPerHappiness;
        Debug.Log(Invoice);
        balance -= Invoice / (60 / GameLoopManager.Instance.invoiceInterval);

        Debug.Log("After Invoice: " + balance);
    }

    /// <returns>Can the tile be bought</returns>
    public bool BuyTile(TileType tileType)
    {
        balance -= tileBuyPrices[tileType];
        if (balance < 0)
        {
            balance += tileBuyPrices[tileType];
            return false;
        }
        return true;
    }
    public void SellTile(TileType tileType)
    {
        balance += tileSellPrices[tileType];
    }

    // -------------------------------- Population -------------------------------- //
    public void NewHouse(TileType tileType, Vector2 houseTilePos)
    {
        int personCount = 0;

        switch (tileType)
        {
            case TileType.House:
                personCount = Random.Range(2, 5); // Returns 2, 3 or 4
                break;
            case TileType.Skyscraper:
                personCount = Random.Range(10, 16);
                break;
        }

        // Add people to the house
        for (int i = 0; i < personCount; i++)
        {
            PersonData personData = new()
            {
                homeTilePos = houseTilePos
            };
            AddPerson(personData);
        }
    }
    public GameObject AddPerson(PersonData personData)
    {
        // Create the person
        GameObject person = Instantiate(personPrefab, populationParentObject);

        // Get the person script
        IPerson personScript = person.GetComponent<IPerson>();

        // Set the person data
        person.transform.position = personData.position;
        personScript.HomeTilePos = personData.homeTilePos;
        personScript.WorkplaceTilePos = personData.workplaceTilePos;

        // Add the person to the population list
        population.Add(personScript);

        // Add the person to the house tile inhabitants list
        IHouseTile houseTileScript = MapHandler.Instance.map.GetValue(personScript.HomeTilePos).GetComponent<IHouseTile>();
        houseTileScript.Inhabitants.Add(person);

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
}
public enum HouseSize
{
    normal
}