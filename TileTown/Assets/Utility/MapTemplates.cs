using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapTemplates
{
    private static readonly TileType p = TileType.Plains;
    private static readonly TileType f = TileType.Forest;
    public static Serializable2DArray<TileType> defaultMap = new(5, 5)
    {
        array2D = new ArrayWrapper<TileType>[]
            {
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, p, f, p} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, f, f, p, p} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, f, p, f} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, f, p, p} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, p, p, p} },
            }
    };
    public static Serializable2DArray<TileType> plainsMap = new(5, 5)
    {
        array2D = new ArrayWrapper<TileType>[]
        {
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, p, p, p} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, p, p, p} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, p, p, p} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, p, p, p} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { p, p, p, p, p} },
        }
    };
    public static Serializable2DArray<TileType> forestMap = new(5, 5)
    {
        array2D = new ArrayWrapper<TileType>[]
        {
                new ArrayWrapper<TileType>(5){ array = new TileType[] { f, f, f, f, f} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { f, f, f, f, f} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { f, f, f, f, f} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { f, f, f, f, f} },
                new ArrayWrapper<TileType>(5){ array = new TileType[] { f, f, f, f, f} },
        }
    };
}
