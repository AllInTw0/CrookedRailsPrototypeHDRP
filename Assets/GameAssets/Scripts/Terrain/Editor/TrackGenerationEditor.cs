using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackGeneration))]
public class TrackGenerationEditor : Editor
{
    private TrackGeneration trackGeneration;

    void OnEnable()
    {
        trackGeneration = (TrackGeneration)target;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Test Pathfinding"))
        {
            trackGeneration.FindPath(Vector3.zero, new Vector3(50f, 0f, 500f));
        }
    }
}
