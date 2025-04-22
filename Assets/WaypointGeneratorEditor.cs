#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaypointGenerator))]
public class WaypointGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(8);

        if (GUILayout.Button("Generate Waypoints"))
        {
            // Call the public method in edit mode
            ((WaypointGenerator)target).GenerateWaypoints();
        }
    }
}
#endif
