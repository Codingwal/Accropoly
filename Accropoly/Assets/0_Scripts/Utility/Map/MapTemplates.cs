using System.Collections.Generic;

public static class MapTemplates
{
    public static Tile[,] PlainsMap
    {
        get
        {
            Tile[,] tiles = new Tile[20, 20];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].components = TilePlacingUtility.GetComponents(TileType.Plains, new(x, y), Directions.North);
                }
            }
            return tiles;
        }
    }
    public static Tile[,] ForestMap
    {
        get
        {
            Tile[,] tiles = new Tile[20, 20];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].components = TilePlacingUtility.GetComponents(TileType.Sapling, new(x, y), Directions.North);
                }
            }
            return tiles;
        }
    }
    public static Tile[,] BigPlainsMap
    {
        get
        {
            Tile[,] tiles = new Tile[100, 100];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].components = TilePlacingUtility.GetComponents(TileType.Plains, new(x, y), Directions.North);
                }
            }
            return tiles;
        }
    }
    public static Tile[,] BigForestMap
    {
        get
        {
            Tile[,] tiles = new Tile[100, 100];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y].components = TilePlacingUtility.GetComponents(TileType.Sapling, new(x, y), Directions.North);
                }
            }
            return tiles;
        }
    }
    public static Dictionary<string, MapData> mapTemplates = new()
    {
        {"PlainsMap", new MapData(){tiles = PlainsMap}},
        {"ForestMap", new MapData(){tiles = ForestMap}},
        {"BigPlainsMap", new MapData(){tiles = BigPlainsMap}},
        {"BigForestMap", new MapData(){tiles = BigForestMap}},
    };
}
