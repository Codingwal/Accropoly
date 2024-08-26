using System.Collections.Generic;

public static class MapTemplates
{
    private static readonly Tile p = new(TileType.Plains, 0);
    private static readonly Tile f = new(TileType.Forest, 0);
    public static Serializable2DArray<Tile> defaultMap = new(5, 5)
    {
        array2D = new ArrayWrapper<Tile>[]
            {
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, f, p, p, p, f, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, f, f, p, p, f, p, f, f, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, f, p, f, f, f, f, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, f, p, p, p, f, f, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, f, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, f, p, p, p, f, p, p, f, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, f, p, f, f, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, f, p, p, f, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, f, p, p, p, p, p} },
            }
    };
    public static Serializable2DArray<Tile> plainsMap = new(5, 5)
    {
        array2D = new ArrayWrapper<Tile>[]
        {
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { p, p, p, p, p, p, p, p, p, p} }
        }
    };
    public static Serializable2DArray<Tile> forestMap = new(5, 5)
    {
        array2D = new ArrayWrapper<Tile>[]
        {
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
                new ArrayWrapper<Tile>(10){ array = new Tile[] { f, f, f, f, f, f, f, f, f, f} },
        }
    };
    public static Dictionary<string, Serializable2DArray<Tile>> mapTemplates = new()
    {
        {"DefaultMap", defaultMap},
        {"PlainsMap", plainsMap},
        {"ForestMap", forestMap}
    };
}
