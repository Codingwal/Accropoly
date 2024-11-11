using Unity.Entities;

// Singletons
public struct RunGameTag : IComponentData { }
public struct SaveGameTag : IComponentData { }
public struct LoadGameTag : IComponentData { }
public struct EntityGridHolder : IComponentData { }

// Tiles
public struct NewTileTag : IComponentData { }
public struct ActiveTileTag : IComponentData, IEnableableComponent { }
public struct HasSpaceTag : IComponentData { }

// Population
public struct NewPersonTag : IComponentData { }
// public struct SearchesSpaceTag : IComponentData { }