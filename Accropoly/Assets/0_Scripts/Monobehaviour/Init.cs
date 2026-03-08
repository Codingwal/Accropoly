using Unity.Entities;
using UnityEngine;

public class Init : MonoBehaviour
{
    private void Awake()
    {
        SaveSystem.Initialize();

        if (WorldManager.ActiveWorld)
            WorldManager.DestroyWorld();

        World.DefaultGameObjectInjectionWorld?.Dispose();
    }
    private void OnDestroy()
    {
        if (WorldManager.ActiveWorld)
        {
            WorldManager.DestroyWorld();
        }
    }
}