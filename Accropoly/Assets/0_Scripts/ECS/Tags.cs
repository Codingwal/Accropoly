using Unity.Entities;

namespace Tags
{
    // Singletons
    public struct RunGame : IComponentData { }
    public struct SaveGame : IComponentData { }
    public struct LoadGame : IComponentData { }
    public struct NewDay : IComponentData { }
    public struct EntityGridHolder : IComponentData { }

    // Tiles
    public struct NewTile : IComponentData { }
    public struct ActiveTile : IComponentData, IEnableableComponent { }
    public struct Replace : IComponentData { }
    public struct HasSpace : IComponentData { }
    public struct BuildingConnector : IComponentData { }

    // Population
    public struct NewPerson : IComponentData { }

    // Other
    public struct Billboard : IComponentData { }
}