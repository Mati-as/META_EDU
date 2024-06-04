#if UNITY_EDITOR
using UnityEditor;

public class ActivateToggle 
{
    [MenuItem("Tools/Toggle Active State %t")] // Ctrl + T or Cmd + T
    private static void ToggleActive()
    {
        foreach (var obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj, "Toggle Active State");
            obj.SetActive(!obj.activeSelf);
            EditorUtility.SetDirty(obj);
        }
    }
}

#endif
