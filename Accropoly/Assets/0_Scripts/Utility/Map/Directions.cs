using Unity.Mathematics;

public struct Direction
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
        if (directionVec.Equals(new(1, 0))) direction = Directions.North;
        else if (directionVec.Equals(new(0, 1))) direction = Directions.East;
        else if (directionVec.Equals(new(-1, 0))) direction = Directions.South;
        else if (directionVec.Equals(new(0, -1))) direction = Directions.West;
        else throw new($"Invalid direction vector {directionVec}");
    }

    public readonly Direction Rotate(uint rotation) { return (Directions)(((uint)direction + rotation) % 4); }
    public readonly Direction Flip() { return (Directions)(((uint)direction + 2) % 4); }

    public static implicit operator Directions(Direction direction) { return direction.direction; }
    public static implicit operator Direction(Directions direction) { return new(direction); }
    public static explicit operator uint(Direction direction)
    {
        uint val = (uint)direction;
        if (val > 3) throw new($"Invalid direction {direction}");
        return val;
    }
    public static explicit operator Direction(uint direction)
    {
        if (direction > 3) throw new($"Invalid direction {direction}");
        return (Direction)direction;
    }

    public static Direction[] GetDirections()
    {
        return new Direction[] { Directions.North, Directions.East, Directions.South, Directions.West, };
    }
}
public enum Directions
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
}