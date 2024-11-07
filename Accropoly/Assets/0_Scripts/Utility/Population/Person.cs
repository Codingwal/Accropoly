using System.Collections.Generic;
using Unity.Entities;

public struct Person
{
    public List<(IComponentData, bool)> components;
}
