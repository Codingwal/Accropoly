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
        public void AddDirection(Direction direction)
        {
            connectableSides[(uint)direction] = true;
        }
        public void RemoveDirection(Direction direction)
        {
            connectableSides[(uint)direction] = false;
        }
        ///  <summary>This doesn't contain the full component information! This is only used to select a MeshMaterialPair</summary>
        public readonly int GetIndex()
        {
            Direction rotation = GetRotation();
            ConnectingTile endrotatedRotation = Rotate(-(int)(uint)rotation);
            if (endrotatedRotation.CountConnectableSides() == 0) return 0;
            else if (endrotatedRotation.CountConnectableSides() == 1) return 1;
            else if (endrotatedRotation.CountConnectableSides() == 2) return endrotatedRotation.connectableSides[1] ? 3 : 2;
            else if (endrotatedRotation.CountConnectableSides() == 3) return 4;
            else return 5;
        }
        public readonly Direction GetRotation()
        {
            int connectableSidesCount = CountConnectableSides();

            if (connectableSidesCount == 0 || connectableSidesCount == 4) return Directions.North;

            // Find the combination with the lowest sum. This basically finds the completely left shifted variant
            uint bestRotation = 100;
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
public struct IsConnectedTag : IComponentData, IEnableableComponent { }
public enum ConnectingTileGroup
{
    Street,
    Lake,
    River,
}