using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : Singleton<PopulationManager>
{
    public event RefAction<float> PayTaxes;

    [SerializeField] GameObject personPrefab;
    [SerializeField] Transform populationParentObject;
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
            // Create the person
            GameObject person = Instantiate(personPrefab, populationParentObject);

            // Get the person script
            IPerson personScript = person.GetComponent<IPerson>();

            // Set the person data
            person.transform.position = personData.position;
            personScript.HomeTilePos = personData.homeTilePos;
            personScript.WorkplaceTilePos = personData.workplaceTilePos;
        }
    }
    public float CalculateTaxes()
    {
        float taxes = 0;
        PayTaxes?.Invoke(ref taxes);
        return taxes;
    }
}
