using System.Collections.Generic;

public static class MapTemplates
{
    public static Tile[,] PlainsMap
    {
        get
        {
            Tile[,] tiles = new Tile[10, 10];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].components = TilePlacingUtility.GetComponents(TileType.Plains, new(x, y), 0);
                }
            }
            return tiles;
        }
    }
    public static Tile[,] ForestMap
    {
        get
        {
            Tile[,] tiles = new Tile[10, 10];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].components = TilePlacingUtility.GetComponents(TileType.Sapling, new(x, y), 0);
                }
            }
            return tiles;
        }
    }
    public static Dictionary<string, MapData> mapTemplates = new()
    {
        {"PlainsMap", new MapData(){tiles = PlainsMap}},
        {"ForestMap", new MapData(){tiles = ForestMap}}
    };
}
