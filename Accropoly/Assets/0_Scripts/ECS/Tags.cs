using Unity.Entities;

// Singletons
public struct RunGameTag : IComponentData { }
public struct SaveGameTag : IComponentData { }
public struct LoadGameTag : IComponentData { }
public struct EntityGridHolder : IComponentData { }

// Tiles
public struct NewTileTag : IComponentData { }
public partial struct ActiveTileTag : IComponentData, IEnableableComponent { }

// Population

