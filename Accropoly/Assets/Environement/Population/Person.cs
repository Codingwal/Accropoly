using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour, IPerson
{
    public GameObject PersonObject { get { return gameObject; } }
    public Vector2 HomeTilePos { get; set; }
    public Vector2? WorkplaceTilePos { get; set; }

    public float Happiness
    {
        get
        {
            float happiness = 50;

            if (TownManager.Instance.HasElectricity()) { happiness += 10; } else { happiness -= 20; };

            return happiness;
        }
    }
    private void OnEnable()
    {
        GameLoopManager.Instance.SaveWorld += SaveData;
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.SaveWorld -= SaveData;
    }
    private void SaveData(ref World world)
    {
        PersonData personData = new()
        {
            position = transform.position,
            homeTilePos = HomeTilePos,
            workplaceTilePos = WorkplaceTilePos
        };
        world.population.Add(personData);
    }
}
