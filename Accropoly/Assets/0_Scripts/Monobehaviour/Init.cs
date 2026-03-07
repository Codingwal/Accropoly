using Unity.Entities;
using UnityEngine;

public class Init : MonoBehaviour
{
    private void Awake()
    {
        SaveSystem.Initialize();

        // If there is still an active world, destroy it
        // This happens if the editor game is stopped without exiting the world
        World.DefaultGameObjectInjectionWorld?.Dispose();
    }
}