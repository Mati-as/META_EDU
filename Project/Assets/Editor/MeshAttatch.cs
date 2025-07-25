using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkinnedMeshBinderEditor : EditorWindow
{
    private GameObject skinnedObject;
    private Transform characterRoot;

    private string resolvedRootBoneName = "";

    [MenuItem("Tools/Bone Binder/Bind SkinnedMesh to Rig")]
    public static void ShowWindow()
    {
        GetWindow<SkinnedMeshBinderEditor>("Bind SkinnedMesh");
    }

    private void OnGUI()
    {
        GUILayout.Label("🔗 SkinnedMesh 바인딩 툴", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        skinnedObject = (GameObject)EditorGUILayout.ObjectField("🎯 장비 오브젝트", skinnedObject, typeof(GameObject), true);
        characterRoot = (Transform)EditorGUILayout.ObjectField("🧍 캐릭터 본 루트", characterRoot, typeof(Transform), true);

        if (!string.IsNullOrEmpty(resolvedRootBoneName))
            EditorGUILayout.HelpBox($"📌 선택된 Root Bone: {resolvedRootBoneName}", MessageType.Info);

        if (GUILayout.Button("✅ 자동 바인딩 실행"))
        {
            BindSkinnedMesh();
        }
    }

    private void BindSkinnedMesh()
    {
        if (skinnedObject == null || characterRoot == null)
        {
            Debug.LogError("🚫 스킨드 오브젝트와 캐릭터 본 루트를 모두 설정해주세요.");
            return;
        }

        var smr = skinnedObject.GetComponent<SkinnedMeshRenderer>();
        if (smr == null)
        {
            Debug.LogError("🚫 선택된 오브젝트에 SkinnedMeshRenderer가 없습니다.");
            return;
        }

        // 기존 본 리스트에서 이름만 추출
        var originalBones = smr.bones;
        List<Transform> newBones = new List<Transform>();

        foreach (var bone in originalBones)
        {
            if (bone == null) continue;

            var newBone = FindChildByName(characterRoot, bone.name);
            if (newBone != null)
                newBones.Add(newBone);
            else
            {
                Debug.LogWarning($"⚠️ 본 '{bone.name}' 을 캐릭터에서 찾지 못함.");
                newBones.Add(null);
            }
        }

        // rootBone 이름 추론 (1순위: 기존 이름 / 2순위: 장비 이름 유추)
        string rootBoneName = smr.rootBone?.name;
        if (string.IsNullOrEmpty(rootBoneName))
            rootBoneName = GuessRootBoneByObjectName(skinnedObject.name.ToLower());

        resolvedRootBoneName = rootBoneName;

        var newRoot = FindChildByName(characterRoot, rootBoneName);
        if (newRoot == null)
        {
            Debug.LogError($"🚫 rootBone '{rootBoneName}' 을 캐릭터에서 찾을 수 없습니다.");
            return;
        }

        smr.rootBone = newRoot;
        smr.bones = newBones.ToArray();

        Debug.Log($"✅ 바인딩 완료: RootBone → {rootBoneName}, {newRoot.name}, 본 {newBones.Count}개 연결됨.");
    }

    private string GuessRootBoneByObjectName(string name)
    {
        name = name.ToLower();
        if (name.Contains("glove") || name.Contains("hand")) return "LeftHand";
        if (name.Contains("boot") || name.Contains("foot") || name.Contains("shoe")) return "LeftFoot";
        if (name.Contains("pant") || name.Contains("leg")) return "Hips";
        if (name.Contains("helmet") || name.Contains("head")) return "Head";
        if (name.Contains("shirt") || name.Contains("torso") || name.Contains("top") || name.Contains("outer") ||
            name.Contains("upper") || name.Contains("upperwear") || name.Contains("suit"))
            return "Spine";
        return "Hips"; // default fallback

    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            if (child.name == name)
                return child;
        return null;
    }
}