using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapHandler))]
[CanEditMultipleObjects]
public class MapHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate map template"))
        {
            FileHandler.SaveObject("", "template", CreateMapTemplate.CreateMap(64));
        }
    }
}
