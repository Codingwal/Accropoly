using UnityEngine;

public class ResizecollisionGround : MonoBehaviour
{
    void Update()
    {
        if (!WorldManager.ActiveWorld)
            return;

        float mapSize = WorldManager.WorldInfo.mapSize;
        transform.position = new(mapSize - 1, -0.01f, mapSize - 1);
        transform.localScale = new(mapSize / 5, 1, mapSize / 5);
    }
}
