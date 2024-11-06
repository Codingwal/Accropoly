using Unity.Entities;
using UnityEngine;

public partial class HabitatInitializationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<NewTileTag>().ForEach((Entity entity, ref Habitat habitat) =>
        {
            habitat.freeSpace = habitat.totalSpace;
            EntityManager.AddComponent<HasSpaceTag>(entity);
        }).WithoutBurst().WithStructuralChanges().Run();
    }
}
