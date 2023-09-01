using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : Singleton<PopulationManager>
{
    public event RefAction<float> PayTaxes;

    [SerializeField] GameObject personPrefab;
    [SerializeField] Transform populationParentObject;

    public List<GameObject> population = new();
    private void OnEnable()
    {
        GameLoopManager.Instance.InitWorld += InitPopulation;

        GameLoopManager.Instance.InitWorld += InitEconomy;
        GameLoopManager.Instance.SaveWorld += SaveEconomy;

        GameLoopManager.Instance.PayTaxes += CalculateTaxes;
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.InitWorld -= InitPopulation;

        GameLoopManager.Instance.InitWorld -= InitEconomy;
        GameLoopManager.Instance.SaveWorld -= SaveEconomy;

        GameLoopManager.Instance.PayTaxes -= CalculateTaxes;
    }

    private void InitPopulation(World world)
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
    }
    public void CalculateTaxes()
    {
        float taxes = 0;
        PayTaxes?.Invoke(ref taxes);
        balance += taxes;
    }
    public void NewHouse(HouseSize houseSize, Vector2 houseTilePos)
    {
        int personCount = 0;

        switch (houseSize)
        {
            case HouseSize.normal:
                personCount = Random.Range(2, 5); // Returns 2, 3 or 4
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
        population.Add(person);

        // Add the person to the house tile inhabitants list
        IHouseTile houseTileScript = MapHandler.Instance.map.GetValue(personScript.HomeTilePos).GetComponent<IHouseTile>();
        houseTileScript.Inhabitants.Add(person);

        return person;
    }
    public void RemovePerson(GameObject person)
    {
        population.Remove(person);
        Destroy(person);
    }

    public float balance;
    [SerializeField] private SerializableDictionary<TileType, int> tileBuyPrices;
    [SerializeField] private SerializableDictionary<TileType, int> tileSellPrices;

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

    private void InitEconomy(World world)
    {
        balance = world.balance;
    }
    private void SaveEconomy(ref World world)
    {
        world.balance = balance;
    }
}
public enum HouseSize
{
    normal
}