using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PreCreationSystemGroup : ComponentSystemGroup { } // Delete NewTileTag & NewPersonTag which are created in the CreationSystemGroup

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(PreCreationSystemGroup))]
public partial class CreationSystemGroup : ComponentSystemGroup { } // Place tiles, create people, load & save the game

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(CreationSystemGroup))]
public partial class ComponentInitializationSystemGroup : ComponentSystemGroup { } // Create tag components, set default values

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial class LateInitializationSystemGroup : ComponentSystemGroup { } // prepare for input -> reset inputData, delete tags

// most systems are executed in SimulationSystemGroup
