using Unity.Entities;
using UnityEngine;

public unsafe struct ConnectingTile : IComponentData
{
    private fixed bool connectableSides[4];
    public ConnectingTile(params Direction[] connectableDirections)
    {
        foreach (var direction in connectableDirections)
        {
            AddDirection(direction);
        }
    }
    public void AddDirection(Direction direction)
    {
        connectableSides[(uint)direction] = true;
    }
    public void RemoveDirection(Direction direction)
    {
        connectableSides[(uint)direction] = false;
    }
    public bool CanConnect(Direction direction, Direction rotation)
    {
        return connectableSides[(uint)direction.Rotate(-(int)(uint)rotation)];
    }
    ///  <summary>This doesn't contain the full component information! This is only used to select a MeshMaterialPair</summary>
    public int GetIndex()
    {
        int firstTrueIndex = NextTrueIndex(0);
        if (firstTrueIndex == -1) return 0; // No connections

        int secondTrueIndex = NextTrueIndex(firstTrueIndex + 1);
        if (secondTrueIndex == -1) return 1; // 1 connection

        int thirdTrueIndex = NextTrueIndex(secondTrueIndex + 1);
        if (thirdTrueIndex == -1) // 2 connections
            return (secondTrueIndex % 2 == firstTrueIndex) ? 2 : 3; // Check if they're adjacent (3 == adjacent)

        int fourthTrueIndex = NextTrueIndex(thirdTrueIndex + 1);
        if (fourthTrueIndex == -1) return 4; // 3 connections
        return 5; // 4 connections
    }
    public Direction GetRotation()
    {
        int index = NextTrueIndex(0);
        if (index == -1) return Directions.North;
        else return (Direction)(uint)index;
    }
    private int NextTrueIndex(int start)
    {
        for (int i = start; i < 4; i++)
            if (connectableSides[i])
            {
                return i;
            }
        return -1;
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
    public override string ToString()
    {
        return $"{connectableSides[0]}, {connectableSides[1]}, {connectableSides[2]}, {connectableSides[3]}";
    }
    public static ConnectingTile Deserialize(int serializedData)
    {
        Debug.Assert(sizeof(bool) * 4 == sizeof(int)); // Assert that bool[4] and int are of the same size

        ConnectingTile data = new();

        // Convert int to bool[4], this type conversion is only possible with pointers
        int* ptr = (int*)data.connectableSides;
        *ptr = serializedData;

        return data;
    }
}
public struct IsConnectedTag : IComponentData, IEnableableComponent { }