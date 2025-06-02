using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects] // <-- Allows editing multiple objects at once
[CustomEditor(typeof(TransformLayoutGroup))]
public class TransformLayoutGroupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TransformLayoutGroup script = (TransformLayoutGroup)target;
        if (GUILayout.Button("Apply Layout"))
        {
            script.ApplyLayout();
        }
    }
}
