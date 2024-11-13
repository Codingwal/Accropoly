using Unity.Entities;

// Singletons
public struct RunGameTag : IComponentData { }
public struct SaveGameTag : IComponentData { }
public struct LoadGameTag : IComponentData { }
public struct NewDayTag : IComponentData { }
public struct EntityGridHolder : IComponentData { }

// Tiles
public struct NewTileTag : IComponentData { }
public struct ActiveTileTag : IComponentData, IEnableableComponent { }
public struct HasSpaceTag : IComponentData { }
public struct BuildingConnectorTag : IComponentData { }

// Population
public struct NewPersonTag : IComponentData { }
// public struct SearchesSpaceTag : IComponentData { }