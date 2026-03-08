using Unity.Entities;
using UnityEngine;

public class Init : MonoBehaviour
{
    private void Awake()
    {
        SaveSystem.Initialize();

        World.DisposeAllWorlds();
    }
    private void OnDestroy()
    {
        World.DisposeAllWorlds();
    }
}