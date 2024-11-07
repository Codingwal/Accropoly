using System.Collections.Generic;
using Unity.Entities;

public struct Person
{
    public List<(IComponentData, bool)> components;
}
// Explicit values for compatability with other versions (new PersonComponents just get a higher value)
public enum PersonComponents : int
{
    // Components
    PosComponent = 0,
    PersonComponent = 1,

    // Tags
}
