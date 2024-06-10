using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using DG.Tweening;
using Microsoft.SqlServer.Server;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class U_FishOnWater_GameManager : IGameManager
{
    private TextAsset xmlAsset;
    private XmlNode soundNode;
    public XmlDocument xmlDoc; // GameManager에서만 문서 수정, UIMAnager에서는 읽기만 수행

    private  string _xmlPath;
    private string _xmlSavePath;


    //ResourceManage
    public Dictionary<char, Sprite> _characterImageMap;
    public string currentUserName { get; private set; }
    public string currentUserScore { get; private set; }
    public char currentImageChar { get; private set; }


    private readonly string[] adjectives =
    {
        "귀여운", "활기찬", "우아한", "시원한", "따뜻한",
        "편안한", "멋진", "화려한", "아름다운", "소박한",
        "현대적인", "고풍스런", "차분한", "대담한", "풍부한",
        "산뜻한", "깔끔한", "화사한", "기발한", "재치있는", "발랄한", "예쁜"
    };

    private readonly string[] clothes =
    {
        "청바지", "스웨터", "셔츠", "티셔츠", "재킷",
        "코트", "드레스", "치마", "바지", "반바지",
        "운동복", "잠옷", "점퍼", "장갑", "패딩", 
        "가디건", "슬리퍼", "양복", "조끼", "한복", "목도리"
    };
    /*
     4x4구성의 애니메이션 경로 관련 enum 선언입니다.
   경료지점 변경 관련 유지보수가 용이하도록 설계하였습니다.
     예를들어, 5x6으로 이동경로 목록을 수정하는 경우, 경로,enum 목록추가 등 간단한 작업만으로
   경로 애니메이션 재생이 동작하도록 구성했습니다.
   */


    private enum Path
    {
        Start,
        Arrival,
        Max
    }

    private enum Anim
    {
        Idle,
        Rotate,
        Move,
        Max
    }

    // 물고기 관련 세팅 ---------------------------------------------

    private float _fishSpeed =1;
    float minDuration = 0.2f;
    float maxDuration = 2.0f;
    public float fishSpeed { get=>_fishSpeed;
                            set => _fishSpeed = Mathf.Clamp(value,0,2f); }
    private Transform[] _fishesTransforms;
    public readonly int FISH_POOL_COUNT = 60;
   
    
    private int _fishCountGoal = 30;
    public int fishCountGoal { get=>_fishCountGoal; set=> _fishCountGoal = value; }
   
    private readonly float ANIM_INTERVAL = 5.5f;
    private readonly float SIZE_IN_BUCKET = 0.4f;
    // private readonly float DURATION = 3.0f;

    private float _elapsedForInterval;
    private Vector3[] _pathStartPoints;
    private Vector3[] _pathArrivalPoints;

    private Vector3[] _pathInBucketA;
    private Vector3[] _pathInBucketB;
    private Vector3 _defaultSize;
    private Quaternion _defaultQuaternion;

    private Stack<ParticleSystem> _psPool;

    // 애니메이션 컨트롤을 위한 시퀀스 딕셔너리 
    private Dictionary<int, Sequence> _animSeq;
    private Dictionary<int, Animator> _animatorSeq;

    // 물고기가 버킷에 있는지 체크하기 위한 딕셔너리
    private Dictionary<int, bool> _isOnBucket;
    private Dictionary<int, Collider> _colliderMap;
    private Dictionary<int, ParticleSystem> _psMap;
    private Stack<ParticleSystem> _onFishCatchPsPool;


    public float timeLimit;
    public float remainTime { get; private set; }

    private int _fishCaughtCount;

    public int FishCaughtCount
    {
        get => _fishCaughtCount;
        private set
        {
            _fishCaughtCount = value;
            if (isOnReInit) return;
            if (_fishCaughtCount >= _fishCountGoal)
            {
                _fishCaughtCount = _fishCountGoal;
                InvokeFinishAndReInit();
            }
        }
    }

    public bool isOnReInit { get; private set; }
    private Transform _bucket;
    private float _elapsedForReInit;
    private bool _isGameStart;
    private Quaternion _bucketDefaultQuat;
    private Quaternion _bucketDefaultRotation;

    private int _currentFishIndex;

    public static event Action OnReady;
    public static event Action OnRoundFinished;
    public static event Action OnFishCaught;
    private bool _isCountNarrationPlaying;
    private float _elapsedToCount;

    private void Update()
    {
        if (isOnReInit || !_isGameStart) return;


        _elapsedForInterval += Time.deltaTime;
        _elapsedForReInit += Time.deltaTime;
        remainTime = timeLimit - _elapsedForReInit;
        
        if (!_isCountNarrationPlaying)
        {
            Managers.Sound.Play
                (SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Count"+$"{(int)remainTime}",0.8f);
            _isCountNarrationPlaying = true;
            _elapsedToCount = 0;
        }
        
        if (_elapsedToCount > 1f) _isCountNarrationPlaying = false;
        _elapsedToCount += Time.deltaTime *0.9f;
        
        
        
        if (_elapsedForReInit > timeLimit && !isOnReInit) InvokeFinishAndReInit();
    }

    public void SetUserInfo()
    {
        // 이름 설정
        currentUserName = adjectives[Random.Range(0, adjectives.Length)]
                          + clothes[Random.Range(0, clothes.Length)]
                          + Random.Range(1, 10);
        
        
        
        int randomInt = Random.Range('A', 'F' + 1); //Random.Range().ToString()으로 변환하면 아스키숫자로 변환되니 주의.
        char randomChar = (char)randomInt;
        currentImageChar = randomChar;
        currentImageChar = (char)Random.Range('A', 'F' + 1);
        Debug.Log($"currentIconNumber : {currentImageChar}");


        //스프라이트 설정 (아이콘) 로직 필요 --------------------------
    }

    private void InvokeFinishAndReInit()
    {
        isOnReInit = true;
        _elapsedForReInit = 0;

    

        var currentremainTime = Mathf.Clamp(remainTime, 0f, 60f);
        var remainTimeToString = currentremainTime.ToString("F1");
        currentUserScore = $"{_fishCaughtCount} / " + remainTimeToString;

#if UNITY_EDITOR
        Debug.Log($"remainTime : {currentremainTime}" + $"fishCount : {_fishCaughtCount}");
#endif

        Utils.AddUser(ref xmlDoc, currentUserName, currentUserScore,currentImageChar.ToString());
        WriteXML(xmlDoc,_xmlPath);
     
#if UNITY_EDITOR
        Debug.Log($"Saved :  {currentUserName}/{_fishCaughtCount}");
#endif

        OnRoundFinished?.Invoke();
        ReInit();
    }

    protected override void BindEvent()
    {
        base.BindEvent();

        OnFishCaught -= PlayPathAnimOneTime;
        OnFishCaught += PlayPathAnimOneTime;

        U_FishOnWater_UIManager.OnStartUIAppear -= OnRoundStart;
        U_FishOnWater_UIManager.OnStartUIAppear += OnRoundStart;
        U_FishOnWater_UIManager.OnReadyUIAppear += OnReadyUIAppear;

        U_FishOnWater_UIManager.OnRestartBtnClicked -= OnRestartBtnClicked;
        U_FishOnWater_UIManager.OnRestartBtnClicked += OnRestartBtnClicked;
    }

    private void OnReadyUIAppear()
    {
    }

    protected void OnDestroy()
    {
        U_FishOnWater_UIManager.OnRestartBtnClicked -= OnRestartBtnClicked;
        U_FishOnWater_UIManager.OnStartUIAppear -= OnRoundStart;
        OnFishCaught -= PlayPathAnimOneTime;
    }


    protected override void Init()
    {
        DOTween.Init().SetCapacity(300, 300);
        ManageProjectSettings(90, 0.15f);
        
#if UNITY_EDITOR
        // .System 파일입출력과 유니티 파일 입출력시 주소체계 혼동주의바랍니다.



        _xmlPath = "Assets/Resources/Common/Data/BB008_UserRankingData.xml";
        Read(_xmlPath);


#else
        Check_XmlFile("BB008_UserRankingData");
        _xmlPath = System.IO.Path.Combine(Application.persistentDataPath, "BB008_UserRankingData.xml");
        Read(_xmlPath);
     
        


#endif
     
        //Utils.LoadXML(ref xmlAsset, ref xmlDoc, _xmlSavePath, ref _xmlSavePath);
        _onFishCatchPsPool = new Stack<ParticleSystem>();
        var prefab = Resources.Load("게임별분류/BB008_U/CFX_OnFishCatch");
        
        for (int i = 0; i < 10; i++)
        {
            var cfx = Instantiate(prefab).GetComponent<ParticleSystem>();
            _onFishCatchPsPool.Push(cfx);
        }
        
        
        
        _animatorSeq = new Dictionary<int, Animator>();
        _fishesTransforms = new Transform[FISH_POOL_COUNT]; //전체 물고기 컨트롤용입니다. (초기화로직 수행 등)
        _animSeq = new Dictionary<int, Sequence>();
        _isOnBucket = new Dictionary<int, bool>();
        _psPool = new Stack<ParticleSystem>();
        _colliderMap = new Dictionary<int, Collider>();
        _psMap = new Dictionary<int, ParticleSystem>();
        _characterImageMap = new Dictionary<char, Sprite>();
        
        for (int i = 'A'; i < 'G'; i++)
        {
           
            char character = (char)i;
            string characterString = character.ToString();

            
            var sprite = Resources.Load<Sprite>("게임별분류/BB008_U/Character" + characterString);
            if (sprite != null)
            {
                Debug.Log($"{"게임별분류/BB008_U/Character" + characterString} : image loaded");
                // var newGameObject = new GameObject("Character" + characterString);
                // var image = newGameObject.AddComponent<Image>();
                // image.sprite = sprite;
                _characterImageMap.TryAdd((char)i, sprite);
            }
            else
            {
                Debug.LogError("Failed to load Sprite for character " + characterString);
            }
        }

        
        base.Init();
        
        remainTime = timeLimit;
        LoadAsset();
        InitializePath();
        
        // 유저 정보 --------------------------------------
        SetUserInfo();
    }
    

    private void InitializePath()
    {
        var pathStartParent = transform.GetChild((int)Path.Start);
        _pathStartPoints = new Vector3[pathStartParent.childCount];

        for (var i = 0; i < _pathStartPoints.Length; i++) _pathStartPoints[i] = pathStartParent.GetChild(i).position;

        var pathArrivalParent = transform.GetChild((int)Path.Arrival);
        _pathArrivalPoints = new Vector3[pathArrivalParent.childCount];
        for (var i = 0; i < _pathArrivalPoints.Length; i++)
            _pathArrivalPoints[i] = pathArrivalParent.GetChild(i).position;
  

        _bucket = GameObject.Find("Bucket").transform;
        _bucketDefaultQuat = _bucket.rotation;
    }

    private void LoadAsset()
    {
        var prefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/FishOnWater/Prefabs/FishOnWater_FishA");
        for (var i = 0; i < FISH_POOL_COUNT; i++)
        {
            var fish = Instantiate(prefab, transform).GetComponent<Transform>();
            var fishId = fish.GetInstanceID();
            _defaultSize = fish.localScale;
            var randomChar = Random.Range('A', 'E' + 1);
            var path = "게임별분류/기본컨텐츠/FishOnWater/Fishes/M_Fish" + (char)randomChar;
            var mat = Resources.Load<Material>(path);
            if (mat == null) Debug.LogError($"Mat is Null{path}");
            fish.GetChild(1).GetComponent<SkinnedMeshRenderer>().material =mat; // prefab상 2번째 자식에 SkinnedMeshRenderer 할당되어 있어 하드코딩 4/8/24

            //_fishesQueue.Enqueue(fish);
            _psMap.TryAdd(fishId,fish.GetComponentInChildren<ParticleSystem>());
            _psMap[fishId].Stop();
            
            _isOnBucket.TryAdd(fishId, false);
            _animatorSeq.TryAdd(fishId, fish.GetComponent<Animator>());
           
            var fishCollider = fish.GetComponent<Collider>();
            fishCollider.enabled = false;
            _colliderMap.TryAdd(fishId,fishCollider);
            _defaultQuaternion = fish.rotation;
            
            _fishesTransforms[i] = fish;
        }


        var pathInBucketParent = GameObject.Find("PathInBucketA").transform;
        _pathInBucketA = new Vector3[pathInBucketParent.childCount + 1];
        for (var i = 0; i < pathInBucketParent.childCount; i++)
            _pathInBucketA[i] = pathInBucketParent.GetChild(i).position;
        // 제자리로 돌아오게끔 (경로를 원형으로) 만들기 위한 추가 로직 
        _pathInBucketA[pathInBucketParent.childCount] = pathInBucketParent.GetChild(0).position;

        var pathInBucketBParent = GameObject.Find("PathInBucketB").transform;
        _pathInBucketB = new Vector3[pathInBucketBParent.childCount];
        for (var i = 0; i < pathInBucketBParent.childCount; i++)
            _pathInBucketB[i] = pathInBucketBParent.GetChild(i).position;

        var vfxPrefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/FishOnWater/Prefabs/CFX_WaterSplash");

        var PARTICLE_COUNT = 15;
        for (var i = 0; i < PARTICLE_COUNT; i++)
        {
            var psObj = Instantiate(vfxPrefab, GameObject.Find("CFX").transform);
            var ps = psObj.GetComponent<ParticleSystem>();
            _psPool.Push(ps);
        }
    }

    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();
        OnReady?.Invoke();
    }


    private void OnRoundStart()
    {
        isOnReInit = false;
        _isGameStart = true;
        _currentFishIndex = 0; 
        
        foreach (var key in _animSeq.Keys.ToList())
        {
            _animSeq[key].Kill();
            _animSeq[key] = null;
        }
        


        PlayPathAnim();
    }


    private void PlayPathAnimOneTime()
    {
        PlayPathAnim(1);
    }


    private void PlayPathAnim(int fishCount = 2)
    {
        if (isOnReInit) return;


        for (var i = 0; i < fishCount; i++)
        {   
            var currentFish = _fishesTransforms[_currentFishIndex++ % FISH_POOL_COUNT];
           
            var id = currentFish.GetInstanceID();
            
            var currentPath = SetPath();
            var moveAnimSeq = DOTween.Sequence();
            
     
            _colliderMap[id].enabled = false;
            _animatorSeq[id].speed = 1f;
            _psMap[id].Play();
            if (_isOnBucket[id])
                //_fishesQueue.Enqueue(currentFish);
                return;

            currentFish.position = currentPath[(int)Path.Start];
            // 애니메이션 Duration을 최소0.2초, 최대 2초로 가져가기 위해 아래와 같은식을 이용합니다.
            var fishPathDuration = 2 - fishSpeed * 0.95f;
            
            //추후 계산수식에 따른 에러 방지를 위한 방어적 클랭핑입니다. 
            fishPathDuration = Mathf.Clamp(fishPathDuration, minDuration, maxDuration);
            var randomDuration = Random.Range(fishPathDuration, fishPathDuration + 0.5f);
            _psMap[id].Play();
            moveAnimSeq
                .Append(currentFish
                    .DOPath(currentPath, randomDuration, PathType.CatmullRom)
                    .OnStart(()=>{ _psMap[id].Play();})
                    .SetLookAt(-0.01f)
                    .SetEase((Ease)Random.Range((int)Ease.InSine, (int)Ease.InOutCubic)));
            moveAnimSeq.OnStart(() =>
            {
                _psMap[id].Play();
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB008/OnFishAppear", 0.10f);
            });
            moveAnimSeq.OnComplete(() =>
            {
                _colliderMap[id].enabled = true;
                _psMap[id].Stop();
                DoAnimAfterArrival(currentFish, currentFish.position);
            });
            //순서만을 관리하므로, 동작컨트롤 후 바로 Enqueue 해줍니다. 
            // _fishesQueue.Enqueue(currentFish);
            _animSeq[id] = moveAnimSeq;
            moveAnimSeq.Play();
           
        }
    }


    /// <summary>
    ///     도착위치로 이동 후 재생할 애니메이션
    ///     유니티에서 닷트윈을 활용중인데, 각각 3분의 1확률로 하나는 DoPath를 사용해서 arrivalPos를 기준으로 작은 원(반지름0.5)을 그리면서 물고기가 계속해서 궤적을 이동하도록 애니메이션을
    ///     구성할수있게 Dotween코드를 짜줘.
    ///     두번째로는 아무것도 하지않게 하고, 세번째로는 DoMove를 사용해서 다른영역으로 이동할 수 있게 로직을 만들어줘
    /// </summary>
    private void DoAnimAfterArrival(Transform currentFish, Vector3 arrivalPos)
    {
        var randomValue = Random.Range(0f, 1f);
        var id = currentFish.GetInstanceID();
        _animSeq[currentFish.GetInstanceID()]?.Kill();
        _animSeq[currentFish.GetInstanceID()] = null;

        var randomAnimSeq = DOTween.Sequence();

        if (randomValue <= 0.18f)
        {
            var animator = currentFish.GetComponent<Animator>();
            _animatorSeq[id].speed = 0.3f;
            //
            // Vector3[] path = new Vector3[4];
            // float radius = 0.6f;
            //
            //
            //
            // path[0] = arrivalPos;
            // path[1] = arrivalPos + new Vector3(0, 0, radius);
            // path[2] = arrivalPos + new Vector3(-radius, 0, 0);
            // path[3] = arrivalPos + new Vector3(0, 0, -radius);
            //
            // randomAnimSeq.Append(currentFish.DOPath(path, 1.5f, PathType.CatmullRom).SetLookAt(-0.01f).SetOptions(true).SetLoops(-1));
        }
        else
        {
            var nextLocation = _pathArrivalPoints[Random.Range(0, _pathArrivalPoints.Length)];
            var direction = (-currentFish.position + nextLocation).normalized;
            var lookRotation = Quaternion.LookRotation(direction);

            randomAnimSeq.Append(currentFish.DORotateQuaternion(lookRotation, 0.52f));
            randomAnimSeq.AppendInterval(0.13f);
            randomAnimSeq.Append(currentFish.DOMove(_pathStartPoints[Random.Range(0, _pathArrivalPoints.Length)],
                0.6f));
            randomAnimSeq.OnComplete(() => { PlayPathAnimOneTime(); });
        }

        _animSeq[id] = randomAnimSeq;
        randomAnimSeq.Play();
    }


    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;
        // 초기화 등, 기타 로직에서 클릭을 무시해야할 경우
        if (isOnReInit) return;

        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name.Contains("Fish"))
            {
                var fish = hit.transform;
                var id = fish.GetInstanceID();

                if (_animSeq.ContainsKey(id) && _isOnBucket.ContainsKey(id))
                    if (!_isOnBucket[id])
                    {
                        PlayParticleAndReturn(hit.transform.position);
                        
                         _isOnBucket[id] = true;
                        _colliderMap[id].enabled = false;
                        _animSeq[fish.GetInstanceID()].Kill();
                        _animSeq[fish.GetInstanceID()] = null;
                        SendToBucket(fish);
                    }
            }


        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name.Contains("Screen"))
            {
                if (_psPool.Count <= 0) return;

                var ps = _psPool.Pop();
                ps.transform.position = hit.point;
                ps.Play();


                Managers.Sound.Play(SoundManager.Sound.Effect,
                    "Audio/BB008/OnWaterClick" + (char)Random.Range('A', 'D' + 1), 0.5f);
                DOVirtual.Float(0, 1, ps.main.startLifetime.constant + 0.5f, _ => { }).OnComplete(() =>
                {
                    _psPool.Push(ps);
                });
            }
    }

    private void PlayParticleAndReturn(Vector3 position)
    {
        var ps = _onFishCatchPsPool.Pop();
        ps.Stop();
        ps.transform.position = position;
        ps.Play();
        DOVirtual.Float(0, 0, 1.2f, _ => { }).OnComplete(() =>
        {
            _onFishCatchPsPool.Push(ps);
        });
    
    }

    private void ReInit()
    {
       
        _elapsedForInterval = 0;
        _elapsedForReInit = 0;
        _currentFishIndex = 0;
        remainTime = timeLimit;
        
        
        isOnReInit = true;

        foreach (var key in _animSeq.Keys.ToList())
        {
            _animSeq[key].Kill();
            _animSeq[key] = null;
        }

        foreach (var key in _isOnBucket.Keys.ToList()) _isOnBucket[key] = false;


        var escapePath = new Vector3[2];

        var count = 0;
      

        _bucket
            .DORotateQuaternion(_bucketDefaultQuat * Quaternion.Euler(0, 0, -90), 0.5f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                _bucket.DORotateQuaternion(_bucketDefaultQuat, 1f).SetEase(Ease.InSine).SetDelay(0.15f);
            });

        foreach (var fish in _fishesTransforms)
        {

            var escapeSeq = DOTween.Sequence();
            escapePath[0] = fish.position;
            escapePath[1] = _pathStartPoints[Random.Range(0, _pathStartPoints.Length)];
            escapeSeq.Append(fish.DOScale(_defaultSize, 0.5f).SetEase(Ease.InOutBounce));

            escapeSeq.Append(fish.DOPath(escapePath, 1.5f, PathType.CatmullRom)
                .SetLookAt(-0.01f)
                .SetEase((Ease)Random.Range(3, 6))
                .SetAutoKill(true));

            escapeSeq.Append(fish.DORotateQuaternion(_defaultQuaternion, 0.8f));
#if UNITY_EDITOR
            //  Debug.Log("ReInit, Fish Escape Anim");
            
#endif
        }
    }

    private Vector3[] SetPath()
    {
        var path = new Vector3[(int)Path.Max];

        path[0] = _pathStartPoints[Random.Range(0, _pathStartPoints.Length)];
        path[1] = _pathArrivalPoints[Random.Range(0, _pathArrivalPoints.Length)]
                  + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));

        return path;
    }

    private bool _isRestartBtnClicked; // 중복클릭방지

    private void OnRestartBtnClicked()
    {
        if (_isRestartBtnClicked) return;
      
        _isRestartBtnClicked = true;
        DOVirtual.Float(0, 0, 0.55f, _ => { }).OnComplete(() =>
        {
            _isRestartBtnClicked = false;
            FishCaughtCount = 0;
            OnReady?.Invoke();
        });
    }


    private void SendToBucket(Transform fish)
    {
        
         OnFishCaught?.Invoke();

         Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB008_U/Click_" + (char)Random.Range('A', 'F' + 1),
             0.8f);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB008/OnFishCaught" + (char)Random.Range('A', 'F' + 1),
            0.4f);

        var id = fish.GetInstanceID();
        _animSeq[id]?.Kill();

        var bucketSeq = DOTween.Sequence();

        var pathToBucket = new Vector3[3]; // 물고기 클릭위치 에서 버킷까지의 경로 
        //물고기가 버킷에 도착한 다음 움직이는 경로
        
        var startIndex = 0;
        var middleIndex = 1;
        var bucketIndex = 2;
        var archHegiht = fish.up * 20;

        
        //경로설정
        var inBucketPath = Random.Range(0, 100) > 50
            ? _pathInBucketA
            : _pathInBucketB; //버킷에 도착 후 경로
        
        
        pathToBucket[startIndex] = fish.position;
        pathToBucket[middleIndex] = (inBucketPath[0] + fish.position) / 2 + archHegiht;
        pathToBucket[bucketIndex] = inBucketPath[0] + new Vector3(1, 0, 1) * Random.Range(-0.2f, 0.2f);


        bucketSeq.Append
        (fish.DOPath(pathToBucket, Random.Range(0.5f, 0.8f))
            .SetLookAt(-0.005f)
            .OnStart(() => { fish.DORotateQuaternion(fish.rotation * Quaternion.Euler(0, 1080, 0), 0.1f); })
        );
        
      
        bucketSeq.Append(
            fish.DOScale(SIZE_IN_BUCKET * _defaultSize, 0.35f));
        // .OnStart(() => { fish.DORotate(Vector3.zero, 0.1f); });
        bucketSeq.OnComplete(() => { fish.position = inBucketPath[0];});

        // 양동이 안의 경로 설정 부분
        var loopType = LoopType.Restart;
        var pathInBucketWithRandomOffset = inBucketPath;
        var OFFSET_AMOUNT = 0.7f;


        for (var i = 1; i < pathInBucketWithRandomOffset.Length; i++)
            pathInBucketWithRandomOffset[i] +=
                Random.Range(-0.25f, 0.25f) * Vector3.forward + Random.Range(-0.25f, 0.25f) * Vector3.left;


        var randomChance = Random.Range(0, 100);
        if (randomChance >= 50)
        {
            var temp = pathInBucketWithRandomOffset[2];
            pathInBucketWithRandomOffset[1] = pathInBucketWithRandomOffset[2];
            pathInBucketWithRandomOffset[2] = temp;
        }
        
        
        bucketSeq.Append(fish.DOPath(pathInBucketWithRandomOffset, Random.Range(2.5f, 6.5f), PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .SetLookAt(-0.005f)
            .SetDelay(Random.Range(0.1f,0.3f))
            .SetLoops(10, LoopType.Restart));


        bucketSeq.Play();
        _animSeq[id] = bucketSeq;
        if (_isOnBucket.ContainsKey(id)) _isOnBucket[id] = true;
        
        FishCaughtCount++;
        _animatorSeq[id].speed = 0.6f;
       
    }

    public void Read(string path)
    {
        XmlDocument Document = new XmlDocument();
        Document.Load(path);
        xmlDoc = Document;
        
    }
    
    public void Check_XmlFile(string fileName)
    {
        //string filePath = Path.Combine(Application.persistentDataPath, "LOGININFO.xml");
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName+".xml");

        if (File.Exists(filePath))
        {
            Debug.Log(fileName+"XML FILE EXIST");
        }
        else
        {
            //TextAsset XmlFilepath = Resources.Load<TextAsset>("LOGININFO");
            TextAsset XmlFilepath = Resources.Load<TextAsset>(fileName);
            xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(XmlFilepath.ToString());
            xmlDoc.Save(filePath);

            Debug.Log(fileName+".xml FILE NOT EXIST");
        }
    }
    
    public void WriteXML(XmlDocument document,string path)
    {
        
        document.Save(path);
        Debug.Log($"{path}");
        //Debug.Log("SAVED DATA WRITE");
    }

 
}