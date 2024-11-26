using System.Collections.Generic;
using Unity.Entities;

public struct PersonData
{
    public List<(IComponentData, bool)> components;
}
// Explicit values for compatability with other versions (new PersonComponents just get a higher value)
public enum PersonComponents : int
{
    // Components
    PosComponent = 0,
    Person = 1,
    Worker = 2,
    Traveller = 3,

    // Tags
    Travelling = 100,
}
