using Unity.Entities;
using UnityEngine;

public class ResizecollisionGround : MonoBehaviour
{
    void Update()
    {
        float mapScale = 20; // TODO
        transform.position = new(mapScale - 1, -0.01f, mapScale - 1);
        transform.localScale = new(mapScale / 5, 1, mapScale / 5);

    }
}
