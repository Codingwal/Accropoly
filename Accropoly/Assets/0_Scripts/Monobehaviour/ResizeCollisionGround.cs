using Unity.Entities;
using UnityEngine;

public class ResizecollisionGround : MonoBehaviour
{
    EntityQuery query;
    // Start is called before the first frame update
    void Start()
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        query = em.CreateEntityQuery(typeof(Tags.LoadGame));
    }

    // Update is called once per frame
    void Update()
    {
        if (query.CalculateEntityCount() != 0)
        {
            float mapScale = Systems.WorldDataSystem.worldData.map.tiles.GetLength(0);
            transform.position = new(mapScale - 1, -0.01f, mapScale - 1);
            transform.localScale = new(mapScale / 5, 1, mapScale / 5);
        }
    }
}
