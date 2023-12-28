#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 작업편의를 위한 툴 입니다.
/// 에디터상에서 RendermMesh에 접근하여 light를 casting mode로 바꿉니다. (빛 연산 관련 최적화)
/// </summary>
public class EditorOnly_StaticWindowToggle : EditorWindow
{
  
    [MenuItem("Tools/Static Shadow Caster Editor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(EditorOnly_StaticWindowToggle), false, "Static Shadow Caster Editor");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Enable Static Shadow Caster"))
        {
            EnableStaticShadowCastingForSelected();
        }
    }

    private static void EnableStaticShadowCastingForSelected()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            // GameObject를 Static으로 설정하고, GI에 기여하도록 설정합니다.
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);
            flags |= StaticEditorFlags.ContributeGI;
            GameObjectUtility.SetStaticEditorFlags(obj, flags);

            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // Scene이 변경되었다고 에디터에 알립니다.
            EditorUtility.SetDirty(obj);
        }
    }
}

#endif
