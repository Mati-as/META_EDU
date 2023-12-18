#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using TMPro; // TextMeshPro 네임스페이스를 사용합니다.

public class TMP_changer_EditorOnly : EditorWindow
{
    private TMP_FontAsset newFontAsset; // 교체할 새 TMP 폰트 에셋입니다.

    [MenuItem("Tools/TMP Replacer")] // 메뉴에 TMP Replacer를 추가합니다.
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<TMP_changer_EditorOnly>("TMP Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace TMP Fonts in Selected Objects and Children", EditorStyles.boldLabel);

        newFontAsset = EditorGUILayout.ObjectField("New Font Asset", newFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;

        if (GUILayout.Button("Replace Fonts"))
        {
            ReplaceAllFonts();
        }
    }

    private void ReplaceAllFonts()
    {
        if (newFontAsset == null)
        {
            Debug.LogError("No TMP Font Asset selected!");
            return;
        }

        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogError("No game objects selected!");
            return;
        }

        foreach (GameObject rootGameObject in Selection.gameObjects)
        {
            // 선택된 게임 오브젝트와 그 자식 오브젝트들에서 모든 TextMeshPro 컴포넌트를 찾습니다.
            TextMeshProUGUI[] textComponents = rootGameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI tmp in textComponents)
            {
                if (tmp.font != newFontAsset) // 현재 폰트가 새로운 폰트 에셋과 다를 경우에만 교체합니다.
                {
                    Undo.RecordObject(tmp, "TMP Font Changed"); // 변경 사항을 되돌릴 수 있도록 Undo 기능을 추가합니다.
                    tmp.font = newFontAsset; // 폰트를 교체합니다.
                    EditorUtility.SetDirty(tmp); // 변경 사항을 저장합니다.
                }
            }
        }

        Debug.Log("TMP fonts in selected objects and their children have been replaced with " + newFontAsset.name);
    }
}

#endif