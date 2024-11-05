using Unity.Mathematics;

public struct Direction
{
    public Directions direction;
    public readonly int2 DirectionVec => ToDirVec(direction);
    public Direction(int2 directionVec)
    {
        direction = FromDirVec(directionVec);
    }
    public static int2 ToDirVec(Directions direction)
    {
        return direction switch
        {
            Directions.North => new(0, 1),
            Directions.East => new(1, 0),
            Directions.South => new(0, -1),
            Directions.West => new(-1, 0),
            _ => throw new($"Invalid direction {direction}"),
        };
    }
    public static Directions FromDirVec(int2 directionVec)
    {
        if (directionVec.Equals(new(1, 0))) return Directions.North;
        else if (directionVec.Equals(new(0, 1))) return Directions.East;
        else if (directionVec.Equals(new(-1, 0))) return Directions.South;
        else if (directionVec.Equals(new(0, -1))) return Directions.West;
        else throw new($"Invalid direction vector {directionVec}");
    }
    public static uint ToUint(Directions direction)
    {
        uint val = (uint)direction;
        if (val > 3) throw new($"Invalid direction {direction}");
        return val;
    }
    public static Directions Rotate(Directions direction, uint rotation)
    {
        return (Directions)(((uint)direction + rotation) % 4);
    }
    public static Directions Flip(Directions direction)
    {
        return (Directions)(((uint)direction + 2) % 4);
    }
}
public enum Directions
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
}