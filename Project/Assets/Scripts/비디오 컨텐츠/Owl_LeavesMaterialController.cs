using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private Material[][] materialGroup;
    private Transform[] tempChildren;

    private List<LeafInfo> _leafs;
  

    private Dictionary<int, Color> _defaultDarkenColorMap;
    public Dictionary<Material, Sequence> sequenceMap;
    public Dictionary<string, Material[]> matByNames;
    public Dictionary<string, bool> isCountedMap;


    private OwlVideoBaseGameManager _baseGameManager;
    private Ray rayForShader;
    [SerializeField] private float _intensity = 4.5f;
    private Color _brightenColor;
   
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

        foreach (var mat in materialGroup[(int)LeaveLocation.BottomLeft])
            if (mat != null)
                BrightenUp(mat);

        foreach (var mat in materialGroup[(int)LeaveLocation.BottomRight])
            if (mat != null)
                BrightenUp(mat);

        foreach (var mat in materialGroup[(int)LeaveLocation.TopRight])
            if (mat != null)
                BrightenUp(mat);

        foreach (var mat in materialGroup[(int)LeaveLocation.TopLeft])
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
        Base_GameManager.On_GmRay_Synced -= OnClicked;
        Base_GameManager.On_GmRay_Synced += OnClicked;

        OwlVideoBaseGameManager.onOwlSpeechBubbleFinished -= OnOwnSpeechBubbleFinished;
        OwlVideoBaseGameManager.onOwlSpeechBubbleFinished += OnOwnSpeechBubbleFinished;

        InteractableVideoBaseGameManager.onRewind -= OnRewind;
        InteractableVideoBaseGameManager.onRewind += OnRewind;
    }

    private void OnDestroy()
    {
        Base_GameManager.On_GmRay_Synced -= OnClicked;


        InteractableVideoBaseGameManager.onRewind -= OnRewind;
        OwlVideoBaseGameManager.onOwlSpeechBubbleFinished += OnOwnSpeechBubbleFinished;
    }

    private void BrightenUp(Material mat)
    {
        
        mat.DOColor(_brightenColor, 1.35f)
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
        if (OwlVideoBaseGameManager.isJustRewind)
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
           
            
            var seq = DOTween.Sequence();
            seq.AppendInterval(5f);
            seq.AppendCallback(()=>
            {
                Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/AA010_Narration/Owl_ReClickLeaves", 0.5f);
            });
        }
    }

    private void OnClicked()
    {
     

        rayForShader = Base_GameManager.GameManager_Ray;
        RaycastHit hit;

        if (!_isGameInited) 
        {
#if UNITY_EDITOR
Debug.Log($"material is glowing yet");
#endif
return;
}
if (!OwlVideoBaseGameManager.isOwlUIFinished)
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
        var randomChar = (char)Random.Range('A', 'C' + 1);
        Managers.soundManager.Play(SoundManager.Sound.Effect, $"Audio/비디오 컨텐츠/Owl/OnLeaveClick{randomChar}");
        foreach (var mat in matByNames[objName])
            if (mat != null)
                mat.DOColor(_defaultDarkenColorMap[mat.GetInstanceID()], 2.3f);
    }

    private void Init()
    {
        GameObject.FindWithTag("GameManager").TryGetComponent(out _baseGameManager);

        _darkenCountToReplay = (int)LeaveLocation.Max;

        var CHILD_COUNT = transform.childCount;
        matByNames = new Dictionary<string, Material[]>();
        materialGroup = new Material[CHILD_COUNT][];
        _defaultDarkenColorMap = new Dictionary<int, Color>();
        _leafs = new List<LeafInfo>();
        tempChildren = new Transform[CHILD_COUNT];

        for (var i = 0; i < CHILD_COUNT; ++i) materialGroup[i] = new Material[30];

        for (var i = 0; i < CHILD_COUNT; ++i)
        {
            tempChildren[i] = transform.GetChild(i);


            //복사본 가져오는 방식.
            var childRenderer = tempChildren[i].GetComponentsInChildren<Renderer>();

           
            var count = 0;

            foreach (var renderer in childRenderer)
            {
                if (renderer is ParticleSystemRenderer) continue;

                if (renderer != null && renderer.material != null) // null 체크
                {
                    materialGroup[i][count] = renderer.material;
                    _defaultDarkenColorMap.TryAdd(materialGroup[i][count].GetInstanceID(), renderer.material.color);

                    count++;
#if UNITY_EDITOR

#endif
                }

                var leaf = new LeafInfo();


                leaf.name = tempChildren[i].gameObject.name;
                //클릭시 이름을 비교하여 쉐이더 컨트롤 

                matByNames.TryAdd(leaf.name, materialGroup[i]);
                isCountedMap.TryAdd(leaf.name, false);
                _leafs.Add(leaf);
                
                _brightenColor = renderer.material.color*_intensity;
            }
        }
    }
}