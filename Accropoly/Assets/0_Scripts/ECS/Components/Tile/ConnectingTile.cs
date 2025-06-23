using Unity.Entities;
using UnityEngine;

namespace Components
{
    public unsafe struct ConnectingTile : IComponentData
    {
        private fixed bool connectableSides[4];
        public ConnectingTileGroup group;
        public ConnectingTile(ConnectingTileGroup group, params Direction[] connectableDirections)
        {
            this.group = group;
            foreach (var direction in connectableDirections)
            {
                AddDirection(direction);
            }
        }
        public bool IsConnected(Direction direction)
        {
            return connectableSides[(uint)direction];
        }
        public void AddDirection(Direction direction)
        {
            connectableSides[(uint)direction] = true;
        }
        public void RemoveDirection(Direction direction)
        {
            connectableSides[(uint)direction] = false;
        }
        public const int notConnected = 0;
        public const int deadEnd = 1;
        public const int straight = 2;
        public const int curve = 3;
        public const int tJunction = 4;
        public const int junction = 5;
        ///  <remarks>This doesn't contain the full component information! This is only used to select a MeshMaterialPair</remarks>
        public readonly int GetIndex()
        {
            Direction rotation = GetRotation();
            ConnectingTile endrotatedRotation = Rotate(-(int)(uint)rotation);
            return endrotatedRotation.CountConnectableSides() switch
            {
                0 => notConnected,
                1 => deadEnd,
                2 => endrotatedRotation.connectableSides[1] ? curve : straight,
                3 => tJunction,
                _ => junction
            };

        }
        public readonly Direction GetRotation()
        {
            int connectableSidesCount = CountConnectableSides();

            if (connectableSidesCount == 0 || connectableSidesCount == 4) return Directions.North;

            // Find the combination with the lowest sum. This basically finds the completely left shifted variant
            uint bestRotation = int.MaxValue;
            int bestValue = int.MaxValue;
            for (uint i = 0; i < 4; i++)
            {
                ConnectingTile tmp = Rotate(-(int)i);
                int trueIndex1 = tmp.NextTrueIndex(0);
                int trueIndex2 = tmp.NextTrueIndex(trueIndex1 + 1);
                int trueIndex3 = tmp.NextTrueIndex(trueIndex2 + 1);

                int value = trueIndex1 + trueIndex2 + trueIndex3;
                if (value < bestValue)
                {
                    bestValue = value;
                    bestRotation = i;
                }
            }
            Debug.Assert(bestRotation != int.MaxValue);

            return (Direction)bestRotation;
        }
        private readonly int NextTrueIndex(int startInclusive)
        {
            for (int i = startInclusive; i < 4; i++)
                if (connectableSides[i])
                    return i;
            return -1;
        }
        private readonly ConnectingTile Rotate(int rotation)
        {
            ConnectingTile result = new();
            for (uint i = 0; i < 4; i++)
            {
                uint index = (uint)Direction.Rotate((Direction)i, rotation);
                result.connectableSides[index] = connectableSides[i];
            }
            return result;
        }
        private readonly int CountConnectableSides()
        {
            int count = 0;
            for (int i = 0; i < 4; i++)
                if (connectableSides[i]) count++;
            return count;
        }
        public readonly override string ToString()
        {
            return $"{connectableSides[0]}, {connectableSides[1]}, {connectableSides[2]}, {connectableSides[3]}";
        }
    }
}
public enum ConnectingTileGroup
{
    Street,
    Lake,
    River,
}
namespace Tags
{
    public struct IsConnected : IComponentData, IEnableableComponent { }
}