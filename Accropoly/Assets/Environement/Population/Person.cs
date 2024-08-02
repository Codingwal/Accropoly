using System;
using UnityEngine;

public class Person : MonoBehaviour, IPerson
{
    public GameObject PersonObject { get { return gameObject; } }
    public Vector2 HomeTilePos { get; set; }
    public bool HasWorkplace { get; set; } = false;
    public Vector2 WorkplaceTilePos { get; set; }

    public float Happiness
    {
        get
        {
            // This is not related, it should just be called sometimes, which applies for this field
            if (!HasWorkplace) SearchWorkplace();

            // Calculate happiness
            float happiness = 50;

            if (HasWorkplace) { happiness += 20; } else { happiness -= 20; }

            if (TownManager.Instance.HasElectricity()) { happiness += 10; } else { happiness -= 20; };

            happiness = Math.Clamp(happiness, 1, 100);

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
    private void SaveData(ref WorldData world)
    {
        PersonData personData = new()
        {
            position = transform.position,
            homeTilePos = HomeTilePos,
            hasWorkplace = HasWorkplace,
            workplaceTilePos = WorkplaceTilePos
        };
        world.population.Add(personData);
    }
    private void SearchWorkplace()
    {
        foreach (IWorkplace workplace in TownManager.Instance.workplaces)
        {
            if (workplace.AddWorker(this))
            {
                WorkplaceTilePos = workplace.TilePos;
                HasWorkplace = true;
                return;
            };
        }
    }
    public void LeaveWorkplace()
    {
        HasWorkplace = false;

        SearchWorkplace();
    }

    public void Init()
    {
        if (!HasWorkplace) SearchWorkplace();
    }

    public void OnRemove()
    {
        if (HasWorkplace)
        {
            MapHandler.Instance.map.GetValue(WorkplaceTilePos).GetComponent<IWorkplace>().Workers.Remove(this);
        }
    }
}
