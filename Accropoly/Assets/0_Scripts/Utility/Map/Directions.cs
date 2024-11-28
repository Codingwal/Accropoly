using System;
using Unity.Mathematics;
using UnityEngine;

public struct Direction : IEquatable<Direction>
{
    public Directions direction;
    public readonly int2 DirectionVec => direction switch
    {
        Directions.North => new(0, 1),
        Directions.East => new(1, 0),
        Directions.South => new(0, -1),
        Directions.West => new(-1, 0),
        _ => throw new($"Invalid direction {direction}"),
    };

    public Direction(Directions direction) { this.direction = direction; }
    public Direction(int2 directionVec)
    {
        if (directionVec.Equals(new(0, 1))) direction = Directions.North;
        else if (directionVec.Equals(new(1, 0))) direction = Directions.East;
        else if (directionVec.Equals(new(0, -1))) direction = Directions.South;
        else if (directionVec.Equals(new(-1, 0))) direction = Directions.West;
        else throw new($"Invalid direction vector {directionVec}");
    }

    public readonly Direction Rotate(int rotation) { return Rotate(direction, rotation); }
    public readonly Direction Flip() { return (Direction)Normalize((int)direction + 2); }
    public readonly float ToRadians() { return math.radians((uint)direction * 90); }
    public readonly bool IsOpposite(Direction other)
    {
        return other == Flip();
    }
    public readonly bool IsAdjacent(Direction other) { return !IsOpposite(other); }
    public readonly override string ToString()
    {
        return direction.ToString();
    }
    public readonly bool Equals(Direction other)
    {
        return direction == other.direction;
    }
    public static bool operator ==(Direction left, Direction right) { return left.Equals(right); }
    public static bool operator !=(Direction left, Direction right) { return !(left == right); }
    public static bool operator ==(Direction left, Directions right) { return left.direction == right; }
    public static bool operator !=(Direction left, Directions right) { return !(left == right); }
    public static implicit operator Directions(Direction direction) { return direction.direction; }
    public static implicit operator Direction(Directions direction) { return new(direction); }
    public static explicit operator int(Direction direction) { return (int)(uint)direction; }
    public static explicit operator Direction(int direction)
    {
        if (direction < 0) throw new($"Invalid direction {direction}");
        return (Direction)(uint)direction;
    }
    public static explicit operator uint(Direction direction)
    {
        uint val = (uint)direction.direction;
        if (val > 3) throw new($"Invalid direction {direction.direction}");
        return val;
    }
    public static explicit operator Direction(uint direction)
    {
        if (direction > 3) throw new($"Invalid direction {direction}");
        return (Direction)(Directions)direction;
    }

    private static int Normalize(int value) { return (value + 100) % 4; }
    public static Direction Rotate(Direction direction, int rotation) { return (Direction)Normalize((int)direction + rotation); }
    public static Direction[] GetDirections()
    {
        return new Direction[] { Directions.North, Directions.East, Directions.South, Directions.West };
    }
    ///<summary>
    /// Calculates how you need to rotate direction to get resultingDirection.<br/>
    /// resultingDirection = direction.Rotate(returnValue)
    ///</summary>
    public static int GetRotation(Direction direction, Direction resultingDirection)
    {
        return Normalize((int)resultingDirection - (int)direction);
    }
}
public enum Directions
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
}