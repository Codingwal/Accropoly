using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLoopManager))]
public class GameLoopManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameLoopManager gameLoopManager = GameLoopManager.Instance;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate tile map"))
        {
            gameLoopManager.LoadWorld();
        }
        if (GUILayout.Button("Save tile map"))
        {
            gameLoopManager.SaveWorld();
        }
        if (GUILayout.Button("List all maps"))
        {
            string[] maps = FileHandler.ListFiles("Saves");

            foreach (string map in maps)
            {
                Debug.Log(map);
            }
        }
    }
}
