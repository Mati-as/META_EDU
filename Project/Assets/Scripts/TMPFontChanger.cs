#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
public class TMPFontChanger : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Change TMP Font")]
    public static void ShowWindow()
    {
        GetWindow<TMPFontChanger>("Change TMP Font");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace TMP Font", EditorStyles.boldLabel);

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("Change Font"))
        {
            ChangeFontInSelectedObjects();
        }
    }

    private void ChangeFontInSelectedObjects()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            TextMeshProUGUI[] tmpComponents = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI tmp in tmpComponents)
            {
                tmp.font = newFont;
            }
        }
    }
}
#endif