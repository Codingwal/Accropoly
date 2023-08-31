using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CreateMapTemplate
{
    public static Serializable2DArray<Tile> CreateMap(int size)
    {
        Serializable2DArray<Tile> map = new(size, size);

        return map;
    }
}
