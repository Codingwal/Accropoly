using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameLoopManager : Singleton<GameLoopManager>
{
    

    [Header("Temporary")]
    [SerializeField] private bool generateTileMap;

    private MapHandler mapHandler;

    protected override void SingletonAwake()
    {
        mapHandler = MapHandler.Instance;

        if (generateTileMap)
        {
            mapHandler.GenerateTileMap();
        }
    }
    
}