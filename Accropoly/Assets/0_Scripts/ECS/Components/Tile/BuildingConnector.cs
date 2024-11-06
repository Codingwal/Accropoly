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
        Debug.Assert(sizeof(bool) * 4 == sizeof(int));

        Print();

        fixed (bool* ptr = connectableSides)
        {
            int val = *(int*)ptr;
            Debug.Log($"-> {val}");
            return val;
        }
    }
    public void Print()
    {
        Debug.LogWarning($"{connectableSides[0]}, {connectableSides[1]}, {connectableSides[2]}, {connectableSides[3]}");
    }
    public static BuildingConnector Deserialize(int serializedData)
    {
        Debug.Assert(sizeof(bool) * 4 == sizeof(int));

        BuildingConnector buildingConnector = new();
        int* ptr = (int*)buildingConnector.connectableSides;
        *ptr = serializedData;

        Debug.Log($"{serializedData} ->");
        buildingConnector.Print();

        return buildingConnector;
    }
}
public struct IsConnectedTag : IComponentData, IEnableableComponent { }