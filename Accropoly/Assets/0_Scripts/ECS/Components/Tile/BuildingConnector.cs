using Unity.Entities;
using UnityEngine;

public unsafe struct BuildingConnector : IComponentData
{
    private fixed bool connectableSides[4];
    public BuildingConnector(params Direction[] connectableDirections)
    {
        foreach (var direction in connectableDirections)
        {
            connectableSides[(uint)direction] = true;
        }
    }
    public bool CanConnect(Direction direction, int rotation)
    {
        return connectableSides[(uint)direction.Rotate(-rotation)];
    }
    public int Serialize()
    {
        Debug.Assert(sizeof(bool) * 4 == sizeof(int)); // Assert that bool[4] and int are of the same size

        // Convert bool[4] to int, this type conversion is only possible with pointers
        fixed (bool* ptr = connectableSides)
        {
            return *(int*)ptr;
        }
    }
    public void Print()
    {
        Debug.LogWarning($"{connectableSides[0]}, {connectableSides[1]}, {connectableSides[2]}, {connectableSides[3]}");
    }
    public static BuildingConnector Deserialize(int serializedData)
    {
        Debug.Assert(sizeof(bool) * 4 == sizeof(int)); // Assert that bool[4] and int are of the same size

        BuildingConnector buildingConnector = new();

        // Convert int to bool[4], this type conversion is only possible with pointers
        int* ptr = (int*)buildingConnector.connectableSides;
        *ptr = serializedData;

        return buildingConnector;
    }
}
public struct IsConnectedTag : IComponentData, IEnableableComponent { }