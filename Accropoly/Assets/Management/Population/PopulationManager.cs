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
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.InitWorld -= InitPopulation;
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
    public float CalculateTaxes()
    {
        float taxes = 0;
        PayTaxes?.Invoke(ref taxes);
        return taxes;
    }
    public List<GameObject> NewHouse(HouseSize houseSize, Vector2 houseTilePos)
    {
        int personCount = 0;

        switch (houseSize)
        {
            case HouseSize.normal:
                personCount = Random.Range(2, 5); // Returns 2, 3 or 4
                break;
        }

        // Add people to the house
        List<GameObject> houseInhabitants = new();

        for (int i = 0; i < personCount; i++)
        {
            PersonData personData = new()
            {
                homeTilePos = houseTilePos
            };
            houseInhabitants.Add(AddPerson(personData));
        }
        return houseInhabitants;
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

        population.Add(person);

        return person;
    }
    public void RemovePerson(GameObject person)
    {
        population.Remove(person);
        Destroy(person);
    }
}
public enum HouseSize
{
    normal
}