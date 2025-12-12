#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapManager mapManager = (MapManager)target;
        if (GUILayout.Button("Generate Map Prefab"))
        {
            //mapManager.GenerateMapPrefab();
        }
    }
}
#endif