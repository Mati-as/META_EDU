
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PrefabApplyGuard
{
    static PrefabApplyGuard()
    {
        PrefabStage.prefabSaving += stage =>
        {
            if (stage.name.Contains("DO NOT APPLY"))
            {
                EditorUtility.DisplayDialog("🚫 프리팹 저장 금지",
                    "이 프리팹은 Apply하면 안 됩니다.\nVariant를 사용하세요.", "확인");
                throw new System.OperationCanceledException("Apply 금지 프리팹");
            }
        };
    }
}
#endif