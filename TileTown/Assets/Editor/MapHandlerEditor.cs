using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapHandler))]
public class MapHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapHandler mapHandler = MapHandler.Instance;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate tile map"))    
        {
            mapHandler.GenerateTileMap();
        }
        if (GUILayout.Button("Save tile map"))
        {
            mapHandler.SaveTileMap();
        }
    }
}
