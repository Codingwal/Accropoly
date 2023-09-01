using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : MonoBehaviour, IPerson
{
    public Vector2 HomeTilePos { get; set; }
    public Vector2? WorkplaceTilePos { get; set; }

    public float Happiness
    {
        get
        {
            return 50;
        }
    }
    private void OnEnable()
    {
        GameLoopManager.Instance.SaveWorld += SaveData;
        TownManager.Instance.PayTaxes += PayTaxes;
    }
    private void OnDisable()
    {
        GameLoopManager.Instance.SaveWorld -= SaveData;
        TownManager.Instance.PayTaxes -= PayTaxes;
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
    private void PayTaxes(ref float taxes)
    {
        taxes += Happiness;
    }
}
