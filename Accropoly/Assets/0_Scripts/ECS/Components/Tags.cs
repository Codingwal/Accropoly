using Unity.Entities;

namespace Tags
{
    // Singletons
    public struct NewDay : IComponentData { }
    public struct EntityGridHolder : IComponentData { }

    // Tiles
    [Save("NewTile")]
    public struct NewTile : IComponentData { }

    [Save("ActiveTile")]
    public struct ActiveTile : IComponentData, IEnableableComponent { }

    [Save("DisabledTile")]
    public struct DisabledTile : IComponentData { }

    [Save("Replace")]
    public struct Replace : IComponentData { }

    [Save("HasSpace")]
    public struct HasSpace : IComponentData { }

    [Save("BuildingConnector")]
    public struct BuildingConnector : IComponentData { }

    // Population
    [Save("NewPerson")]
    public struct NewPerson : IComponentData { }

    // Other
    [Save("Billboard")]
    public struct Billboard : IComponentData { }
}