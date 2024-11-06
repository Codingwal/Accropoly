using Unity.Entities;

public unsafe struct BuildingConnector : IComponentData
{
    private fixed bool connectableSides[4];

    public bool CanConnect(Direction direction, int rotation)
    {
        return connectableSides[(uint)direction.Rotate(-rotation)];
    }
    public BuildingConnector(params Direction[] connectableDirections)
    {
        foreach (var direction in connectableDirections)
        {
            connectableSides[(uint)direction] = true;
        }
    }
}
public struct IsConnectedTag : IComponentData, IEnableableComponent { }