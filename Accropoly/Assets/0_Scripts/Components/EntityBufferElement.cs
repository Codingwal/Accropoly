using Unity.Entities;

public struct EntityBufferElement : IBufferElementData
{
    public Entity entity;
    public static implicit operator EntityBufferElement(Entity entity)
    {
        return new EntityBufferElement { entity = entity };
    }
    public static implicit operator Entity(EntityBufferElement element)
    {
        return element.entity;
    }
}