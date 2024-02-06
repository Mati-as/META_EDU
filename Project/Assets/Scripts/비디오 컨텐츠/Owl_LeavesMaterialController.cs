using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class LeafInfo
{
    public string name;
    public bool isDarkendAndNonClickable;
}

internal enum LeaveLocation
{
    BottomLeft,
    BottomRight,
    TopRight,
    TopLeft,
    Max
}

public class Owl_LeavesMaterialController : MonoBehaviour
{
    private Material[][] materalGroup;
    private Transform[] tempChildren;

    private List<LeafInfo> _leafs;
    public float intensity = 1.15f;

    public Dictionary<Material, Color> defaultColorMap;
    public Dictionary<Material, Sequence> sequenceMap;
    public Dictionary<string, Material[]> matByNames;
    public Dictionary<string, bool> isCountedMap;


    private Owl_VideoGameManager _gameManager;
    private Ray rayForShader;

   
    public static event Action OnAllLeavesDarkend;


    //게임 실행 후 일정 시간 뒤, 게임진행 가능..
    private bool _isGameInited;

    private int _darkendCount;
    private int _darkenCountToReplay;

    private void Start()
    {
        sequenceMap = new Dictionary<Material, Sequence>();
        isCountedMap = new Dictionary<string, bool>();
        Init();
        BrightenLeaves();
        BindEvent();
    }


    /// <summary>
    ///     배열전체를 할당받는 방식이기에, material 에 null값이 포함되어 받아집니다.
    ///     따라서 BlinkLeaves 함수 동작 시, 반드시 널 체크 해야합니다.
    /// </summary>
    private void BrightenLeaves()
    {
#if UNITY_EDITOR
        Debug.Log("Material Brightness Changing");
#endif

        foreach (var mat in materalGroup[(int)LeaveLocation.BottomLeft])
            if (mat != null)
                BrightenUp(mat);

        foreach (var mat in materalGroup[(int)LeaveLocation.BottomRight])
            if (mat != null)
                BrightenUp(mat);

        foreach (var mat in materalGroup[(int)LeaveLocation.TopRight])
            if (mat != null)
                BrightenUp(mat);

        foreach (var mat in materalGroup[(int)LeaveLocation.TopLeft])
            if (mat != null)
                BrightenUp(mat);
    }

    private bool CheckAllLeavesDarkend()
    {
        if (_darkendCount >= _darkenCountToReplay) return true;
        return false;
    }

    private void BindEvent()
    {
        IGameManager.On_GmRay_Synced -= OnClicked;
        IGameManager.On_GmRay_Synced += OnClicked;

        Owl_VideoGameManager.onOwlSpeechBubbleFinished -= OnOwnSpeechBubbleFinished;
        Owl_VideoGameManager.onOwlSpeechBubbleFinished += OnOwnSpeechBubbleFinished;

        InteractableVideoGameManager.onRewind -= OnRewind;
        InteractableVideoGameManager.onRewind += OnRewind;
    }

    private void OnDestroy()
    {
        IGameManager.On_GmRay_Synced -= OnClicked;


        InteractableVideoGameManager.onRewind -= OnRewind;
        Owl_VideoGameManager.onOwlSpeechBubbleFinished += OnOwnSpeechBubbleFinished;
    }

    private void BrightenUp(Material mat)
    {
        mat.DOColor(mat.color * intensity, 1.35f)
            .SetDelay(4.5f)
            .OnStart(() => { _isGameInited = false; })
            .OnComplete(() => { DOVirtual.Float(0, 1, 1f, _ => { })
                .OnComplete(() => { _isGameInited = true; }); });
    }

    private void OnRewind()
    {
        _darkendCount = 0;

        BrightenLeaves();
        foreach (var leaves in isCountedMap.Keys.ToList()) isCountedMap[leaves] = false;
        foreach (var leaf in _leafs) leaf.isDarkendAndNonClickable = false;
    }

    private void OnOwnSpeechBubbleFinished()
    {
        _darkendCount = 0;
        if (Owl_VideoGameManager.isJustRewind)
        {
            BrightenLeaves();
            foreach (var leaves in isCountedMap.Keys.ToList()) isCountedMap[leaves] = false;

            //나뭇잎이 다시 밝아질 시간을 충분히 준 후, isDarkend를 적용
            DOVirtual.Float(0, 1, 7f, _ => { })
            .OnComplete(() =>
            {
                
#if UNITY_EDITOR
                Debug.Log($"나뭇잎 클릭가능");
#endif
                foreach (var leaf in _leafs) leaf.isDarkendAndNonClickable = false;
            });
           
        }
    }

    private void OnClicked()
    {
     

        rayForShader = IGameManager.GameManager_Ray;
        RaycastHit hit;

        if (!_isGameInited)
        {
#if UNITY_EDITOR
Debug.Log($"material is glowing yet");
#endif
return;
}
if (!Owl_VideoGameManager.isOwlUIFinished)
{
#if UNITY_EDITOR
Debug.Log($"owl ui isn't finished yet.");
#endif
            return;
        }
        if (Physics.Raycast(rayForShader, out hit))
            foreach (var leaf in _leafs)
                if (hit.transform.gameObject.name == leaf.name)
                    // 각 Leaf Group당 한 번만 카운트 되도록
                    if (!isCountedMap[leaf.name])
                    {
                        isCountedMap[leaf.name] = true;
                        _darkendCount++;

                        if (!leaf.isDarkendAndNonClickable)
                        {
                            DarkenLeaf(leaf.name);
                            leaf.isDarkendAndNonClickable = true;


                            if (CheckAllLeavesDarkend()) OnAllLeavesDarkend?.Invoke();
                        }
                    }
    }

    /// <summary>
    ///     널체크 필수 입니다.
    /// </summary>
    /// <param name="objName"></param>
    private void DarkenLeaf(string objName)
    {
        foreach (var mat in matByNames[objName])
            if (mat != null)
                mat.DOColor(mat.color / 4, 2.3f);
    }

    private void Init()
    {
        GameObject.FindWithTag("GameManager").TryGetComponent(out _gameManager);

        _darkenCountToReplay = (int)LeaveLocation.Max;

        var CHILD_COUNT = transform.childCount;
        matByNames = new Dictionary<string, Material[]>();
        materalGroup = new Material[CHILD_COUNT][];

        _leafs = new List<LeafInfo>();
        tempChildren = new Transform[CHILD_COUNT];

        for (var i = 0; i < CHILD_COUNT; ++i) materalGroup[i] = new Material[30];

        for (var i = 0; i < CHILD_COUNT; ++i)
        {
            tempChildren[i] = transform.GetChild(i);


            //복사본 가져오는 방식.
            var childRenderer = tempChildren[i].GetComponentsInChildren<Renderer>();

            defaultColorMap = new Dictionary<Material, Color>();
            var count = 0;

            foreach (var renderer in childRenderer)
            {
                if (renderer is ParticleSystemRenderer) continue;

                if (renderer != null && renderer.material != null) // null 체크
                {
                    materalGroup[i][count] = renderer.material;
                    defaultColorMap.TryAdd(materalGroup[i][count], renderer.material.color);

                    count++;
#if UNITY_EDITOR

#endif
                }

                var leaf = new LeafInfo();


                leaf.name = tempChildren[i].gameObject.name;
                //클릭시 이름을 비교하여 쉐이더 컨트롤 

                matByNames.TryAdd(leaf.name, materalGroup[i]);
                isCountedMap.TryAdd(leaf.name, false);
                _leafs.Add(leaf);
            }
        }
    }
}