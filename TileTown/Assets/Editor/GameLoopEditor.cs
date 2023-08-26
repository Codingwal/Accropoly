using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLoopManager))]
public class GameLoopManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

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
