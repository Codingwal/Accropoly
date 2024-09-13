using Unity.Entities;

// Singletons
public struct RunGameTag : IComponentData { }
public struct SaveGameTag : IComponentData { }
public struct LoadGameTag : IComponentData { }

// Tiles
public struct EntityGridHolder : IComponentData { }
public struct NewTileTag : IComponentData { }

// Population

