using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class TA_FishOnWater_GameManager : IGameManager
{
    public bool easyVersion;


    public XmlDocument xmlDoc_Temp; //temp는 xml리셋시 초기화 및 permanant로 정보이동 GameManager에서만 문서 수정, UIMAnager에서는 읽기만 수행
    public XmlDocument xmlDoc_Permanant;
    public XmlDocument xmlDoc_Setting;

    private string
        _xmlPathTemp; //MonoBehavior에서 호출불가//= System.IO.Path.Combine(Application.persistentDataPath, "tempData.xml");

    private string _xmlPathPermanant; // = System.IO.Path.Combine(Application.persistentDataPath, "PermanantData.xml");
    private string _xmlPathSetting; // = System.IO.Path.Combine(Application.persistentDataPath, "PermanantData.xml")
    private string _xmlSavePath;

    private int _currentMode;
    public readonly string SINGLE_PLAY_IN_KOREAN = "1인 플레이";
    public readonly string MULTI_PLAY_IN_KOREAN = "2인 플레이";

    public int currentMode
    {
        get => _currentMode;
        set
        {
#if UNITY_EDITOR12
            Debug.Log($"current Mode{(PlayMode)_currentMode}");
#endif
            _currentMode = Math.Clamp(value, (int)PlayMode.SinglePlay, (int)PlayMode.MultiPlay);
        }
    }

    public enum PlayMode
    {
        SinglePlay,
        MultiPlay,
        Max
    }

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
        "가디건", "슬리퍼", "양복", "조끼", "한복", "목도리", "블라우스"
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

    private float _fishSpeed = 1;
    private readonly float minDuration = 0.2f;
    private readonly float maxDuration = 2.0f;

    public float fishSpeed
    {
        get => _fishSpeed;
        set => _fishSpeed = Mathf.Clamp(value, 0, 2f);
    }

    private Transform[] _fishesTransforms;
    public readonly int FISH_POOL_COUNT = 100;


    public int fishCountGoal { get; set; } = 30;

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


    private readonly int MAX_FISH_COUNT_ON_SCREEN = 2; // 최대활성화 가능 물고기 수
    private int _fishOnScreen; // 현재 클릭가능하도록 활성화되어있는 물고기 수.

    public int fishOnScreen
    {
        get => _fishOnScreen;
        private set
        {
            if (value >= 0) _fishOnScreen = value;
        }
    }

    public float timeLimit;
    public float remainTime { get; private set; }

    public int fishCaughtCount { get; private set; }

    public int FishCaughtCount
    {
        get => fishCaughtCount;
        private set
        {
            fishCaughtCount = value;
            if (isOnReInit) return;
            if (fishCaughtCount >= fishCountGoal && (PlayMode)currentMode == PlayMode.SinglePlay)
            {
                fishCaughtCount = fishCountGoal;
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
            Managers.soundManager.Play
                (SoundManager.Sound.Effect, "Audio/기본컨텐츠/HandFlip2/Count" + $"{(int)remainTime}", 0.8f);
            _isCountNarrationPlaying = true;
            _elapsedToCount = 0;
        }

        if (_elapsedToCount > 1f) _isCountNarrationPlaying = false;
        _elapsedToCount += Time.deltaTime * 0.9f;


        if (_elapsedForReInit > timeLimit && !isOnReInit) InvokeFinishAndReInit();
    }

    public void SetUserInfo()
    {
        // 이름 설정
        currentUserName = adjectives[Random.Range(0, adjectives.Length)]
                          + clothes[Random.Range(0, clothes.Length)]
                          + Random.Range(1, 10);


        var randomInt = Random.Range('A', 'F' + 1); //Random.Range().ToString()으로 변환하면 아스키숫자로 변환되니 주의.
        var randomChar = (char)randomInt;
        currentImageChar = randomChar;
        currentImageChar = (char)Random.Range('A', 'F' + 1);


        //스프라이트 설정 (아이콘) 로직 필요 --------------------------
    }

    private void InvokeFinishAndReInit()
    {
        isOnReInit = true;
        _elapsedForReInit = 0;


        var currentremainTime = Mathf.Clamp(remainTime, 0f, 60f);
        var remainTimeToString = currentremainTime.ToString("F1");
        currentUserScore = $"{fishCaughtCount} / " + remainTimeToString;

#if UNITY_EDITOR
        Debug.Log($"remainTime : {currentremainTime}" + $"fishCount : {fishCaughtCount}");
#endif

        if (currentMode == (int)PlayMode.MultiPlay)
        {
            Utils.AddUser(ref xmlDoc_Temp, _currentMode.ToString(), currentUserName, fishCaughtCount.ToString(),
                currentImageChar.ToString());
            WriteXML(xmlDoc_Temp, _xmlPathTemp);
        }
        else if (currentMode == (int)PlayMode.SinglePlay)
        {
            Utils.AddUser(ref xmlDoc_Temp, _currentMode.ToString(), currentUserName, currentUserScore,
                currentImageChar.ToString());
            WriteXML(xmlDoc_Temp, _xmlPathTemp);
        }


#if UNITY_EDITOR
        Debug.Log($"Saved :  {currentUserName}/{fishCaughtCount}");
#endif

        OnRoundFinished?.Invoke();
        ReInit();
    }

    protected override void BindEvent()
    {
        base.BindEvent();

        OnFishCaught -= PlayPathOnCaught;
        OnFishCaught += PlayPathOnCaught;

        OnFishCaught -= DecreaseFishCount;
        OnFishCaught += DecreaseFishCount;

        TA_FishOnWater_UIManager.OnStartUIAppear -= OnRoundStart;
        TA_FishOnWater_UIManager.OnStartUIAppear += OnRoundStart;
        TA_FishOnWater_UIManager.OnReadyUIAppear += OnReadyUIAppear;

        TA_FishOnWater_UIManager.OnRestartBtnClicked -= OnRestartBtnClicked;
        TA_FishOnWater_UIManager.OnRestartBtnClicked += OnRestartBtnClicked;

        TA_FishOnWater_UIManager.OnResetXML -= OnXmlReset;
        TA_FishOnWater_UIManager.OnResetXML += OnXmlReset;

        TA_FishOnWater_UIManager.OnResetSettingBtnClicked -= OnResetSetting;
        TA_FishOnWater_UIManager.OnResetSettingBtnClicked += OnResetSetting;

        TA_FishOnWater_UIManager.OnSaveCurrentSettingClicked -= OnSaveCurrentSettings;
        TA_FishOnWater_UIManager.OnSaveCurrentSettingClicked += OnSaveCurrentSettings;
    }

    private void OnReadyUIAppear()
    {
    }

    protected void OnDestroy()
    {
        TA_FishOnWater_UIManager.OnResetXML -= OnXmlReset;
        TA_FishOnWater_UIManager.OnRestartBtnClicked -= OnRestartBtnClicked;
        TA_FishOnWater_UIManager.OnStartUIAppear -= OnRoundStart;
        TA_FishOnWater_UIManager.OnResetSettingBtnClicked -= OnResetSetting;
        OnFishCaught -= PlayPathOnCaught;
        OnFishCaught -= DecreaseFishCount;
        TA_FishOnWater_UIManager.OnSaveCurrentSettingClicked -= OnSaveCurrentSettings;
    }


    protected override void Init()
    {
        _xmlPathTemp = System.IO.Path.Combine(Application.persistentDataPath, "tempData.xml");
        _xmlPathPermanant = System.IO.Path.Combine(Application.persistentDataPath, "PermanantData.xml");
        _xmlPathSetting = System.IO.Path.Combine(Application.persistentDataPath, "SettingData.xml");
        isStartButtonClicked = false;
        DOTween.Init().SetCapacity(500, 500);
        ManageProjectSettings(90, 0.15f);

//#if UNITY_EDITOR
        // .System 파일입출력과 유니티 파일 입출력시 주소체계 혼동주의바랍니다.
        Check_XmlFile("TempData");
        Read(ref xmlDoc_Temp, _xmlPathTemp);

        Check_XmlFile("PermanantData");
        Read(ref xmlDoc_Permanant, _xmlPathPermanant);

        Check_XmlFile("SettingData");
        Read(ref xmlDoc_Setting, _xmlPathSetting);
// #else
//         Check_XmlFile("BB008_UserRankingData");
//         _xmlPath = System.IO.Path.Combine(Application.persistentDataPath, "BB008_UserRankingData.xml");
//         Read(_xmlPath);
//
//
// #endif

        _onFishCatchPsPool = new Stack<ParticleSystem>();
        var prefab = Resources.Load("게임별분류/BB008_U/CFX_OnFishCatch");

        for (var i = 0; i < 10; i++)
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
            var character = (char)i;
            var characterString = character.ToString();


            var sprite = Resources.Load<Sprite>("게임별분류/BB008_U/Character" + characterString);
            if (sprite != null)
                //                Debug.Log($"{"게임별분류/BB008_U/Character" + characterString} : image loaded");
                // var newGameObject = new GameObject("Character" + characterString);
                // var image = newGameObject.AddComponent<Image>();
                // image.sprite = sprite;
                _characterImageMap.TryAdd((char)i, sprite);
            else
                Debug.LogError("Failed to load Sprite for character " + characterString);
        }


        base.Init();

        remainTime = timeLimit;
        LoadAsset();
        InitializePath();

        // 유저 정보 --------------------------------------
        SetUserInfo();
        fishSpeed = easyVersion ? 0.3f : fishSpeed;
        Debug.Log($"easyVersion : {easyVersion}, fishSpeed : {fishSpeed}");
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
            fish.GetChild(1).GetComponent<SkinnedMeshRenderer>().material =
                mat; // prefab상 2번째 자식에 SkinnedMeshRenderer 할당되어 있어 하드코딩 4/8/24

            //_fishesQueue.Enqueue(fish);
            _psMap.TryAdd(fishId, fish.GetComponentInChildren<ParticleSystem>());
            _psMap[fishId].Stop();

            _isOnBucket.TryAdd(fishId, false);
            _animatorSeq.TryAdd(fishId, fish.GetComponent<Animator>());

            var fishCollider = fish.GetComponent<Collider>();
            fishCollider.enabled = false;
            _colliderMap.TryAdd(fishId, fishCollider);
            _defaultQuaternion = fish.rotation;

            _fishesTransforms[i] = fish;
        }


        var pathInBucketParent = GameObject.Find("PathInBucketA").transform;
        _pathInBucketA = new Vector3[pathInBucketParent.childCount];
        for (var i = 0; i < pathInBucketParent.childCount; i++)
            _pathInBucketA[i] = pathInBucketParent.GetChild(i).position;

        var pathInBucketBParent = GameObject.Find("PathInBucketB").transform;
        _pathInBucketB = new Vector3[pathInBucketBParent.childCount];
        for (var i = 0; i < pathInBucketBParent.childCount; i++)
            _pathInBucketB[i] = pathInBucketBParent.GetChild(i).position;

        var vfxPrefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/FishOnWater/Prefabs/CFX_WaterSplash");

        var PARTICLE_COUNT = 50;
        for (var i = 0; i < PARTICLE_COUNT; i++)
        {
            var psObj = Instantiate(vfxPrefab, GameObject.Find("CFX").transform);
            var ps = psObj.GetComponent<ParticleSystem>();
            _psPool.Push(ps);
        }
    }


    private void OnRoundStart()
    {
        isOnReInit = false;
        _isGameStart = true;
        _currentFishIndex = 0;

        foreach (var key in _animSeq.Keys.ToList())
            if (_animSeq[key] != null)
            {
                _animSeq[key].Kill();
                _animSeq[key] = null;
            }


        PlayFishPathAnim();
    }


    private void PlayPathOnCaught()
    {
        fishOnScreen--;
        //화면에 두마리 이상 보이지 않도록
        if (fishOnScreen > MAX_FISH_COUNT_ON_SCREEN) return;

        if (fishOnScreen <= 0)
            PlayFishPathAnim();
        else if (fishOnScreen == 1)
            PlayFishPathAnim(1);
    }

#if UNITY_EDITOR
    private void FixedUpdate()
    {
//        Debug.Log($"fish on screen Count {fishOnScreen}");
    }

#endif

    private void DecreaseFishCount()
    {
    }


    private void PlayFishPathAnim(int fishCount = 2)
    {
        if (isOnReInit) return;

        if (fishOnScreen > MAX_FISH_COUNT_ON_SCREEN)
        {
#if UNITY_EDITOR
            Debug.Log("fish on screen is over than max count");
#endif
            return;
        }

        fishOnScreen += fishCount;
        var fishPathDuration = 2 - fishSpeed * 0.90f;
        fishPathDuration = Mathf.Clamp(fishPathDuration, minDuration, maxDuration);

        for (var i = 0; i < fishCount; i++)
        {
            var currentFish = _fishesTransforms[_currentFishIndex++ % FISH_POOL_COUNT];

            var id = currentFish.GetInstanceID();
            if (_isOnBucket[id]) return;

            var currentPath = SetPath();
            var moveAnimSeq = DOTween.Sequence();

            _colliderMap[id].enabled = false;
            _animatorSeq[id].speed = 1f;
            _psMap[id].Play();


            currentFish.position = currentPath[(int)Path.Start];
            // 애니메이션 Duration을 최소0.2초, 최대 2초로 가져가기 위해 아래와 같은식을 이용합니다.


            //추후 계산수식에 따른 에러 방지를 위한 방어적 클랭핑입니다. 

            var randomDuration = Random.Range(fishPathDuration, fishPathDuration + 0.5f);
            _psMap[id].Play();
            moveAnimSeq
                .Append(currentFish
                    .DOPath(currentPath, randomDuration, PathType.CatmullRom)
                    .OnStart(() => { _psMap[id].Play(); })
                    .SetLookAt(-0.01f)
                    .SetEase((Ease)Random.Range((int)Ease.InSine, (int)Ease.InOutCubic)));
            moveAnimSeq.OnStart(() =>
            {
                _psMap[id].Play();
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/BB008/OnFishAppear", 0.10f);
            });
            moveAnimSeq.InsertCallback(randomDuration * 0.68f, () => { _colliderMap[id].enabled = true; });
            moveAnimSeq.OnComplete(() =>
            {
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
        _colliderMap[id].enabled = true;

        var randomAnimSeq = DOTween.Sequence();

        if (randomValue <= 0.22f)
        {
            var animator = currentFish.GetComponent<Animator>();
            _animatorSeq[id].speed = 0.3f;
        }
        else
        {
            var nextLocation = _pathArrivalPoints[Random.Range(0, _pathArrivalPoints.Length)];
            var direction = (-currentFish.position + nextLocation).normalized;
            var lookRotation = Quaternion.LookRotation(direction);

            randomAnimSeq.Append(currentFish.DORotateQuaternion(lookRotation, 0.52f));
            randomAnimSeq.AppendInterval(0.23f);
            randomAnimSeq.Append(currentFish.DOMove(_pathStartPoints[Random.Range(0, _pathStartPoints.Length)],
                0.7f));
            randomAnimSeq.OnComplete(() =>
            {
                fishOnScreen--;
                PlayFishPathAnim(1);
            });
        }

        _animSeq[id] = randomAnimSeq;
        randomAnimSeq.Play();
    }


    public override void OnRaySynced()
    {
        if (!PreCheck()) return;

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


                Managers.soundManager.Play(SoundManager.Sound.Effect,
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
        DOVirtual.Float(0, 0, 1.2f, _ => { }).OnComplete(() => { _onFishCatchPsPool.Push(ps); });
    }

    private void ReInit()
    {
        _elapsedForInterval = 0;
        _elapsedForReInit = 0;
        _currentFishIndex = 0;
        fishOnScreen = 0;

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
            FishCaughtCount = 0;
            OnReady?.Invoke();
        });
        DOVirtual.Float(0, 0, 2f, _ => { }).OnComplete(() => { _isRestartBtnClicked = false; });
    }


    private void SendToBucket(Transform fish)
    {
        OnFishCaught?.Invoke();


        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/BB008_U/Click_" + (char)Random.Range('A', 'F' + 1),
            0.8f);
        Managers.soundManager.Play(SoundManager.Sound.Effect,
            "Audio/BB008/OnFishCaught" + (char)Random.Range('A', 'F' + 1),
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
        bucketSeq.OnComplete(() => { fish.position = inBucketPath[0]; });

        // 양동이 안의 경로 설정 부분
        var loopType = LoopType.Restart;

        var OFFSET_AMOUNT = 0.7f;


        bucketSeq.Append(fish.DOPath(inBucketPath, Random.Range(3.5f, 11.5f), PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .SetLookAt(-0.005f)
            .SetDelay(Random.Range(0.1f, 0.45f))
            .SetLoops(10, LoopType.Restart));


        bucketSeq.Play();
        _animSeq[id] = bucketSeq;
        if (_isOnBucket.ContainsKey(id)) _isOnBucket[id] = true;

        FishCaughtCount++;
        _animatorSeq[id].speed = 0.5f;
    }

    public void Read(ref XmlDocument doc, string path)
    {
        var Document = new XmlDocument();
        Document.Load(path);
        doc = Document;
    }

    public void Check_XmlFile(string fileName)
    {
        //string filePath = Path.Combine(Application.persistentDataPath, "LOGININFO.xml");
        var filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + ".xml");

        if (File.Exists(filePath))
        {
            Debug.Log(fileName + "XML FILE EXIST");
        }
        else
        {
            //TextAsset XmlFilepath = Resources.Load<TextAsset>("LOGININFO");
            var XmlFilepath = Resources.Load<TextAsset>(fileName);
            xmlDoc_Temp = new XmlDocument();
//            xmlDoc.LoadXml(XmlFilepath.ToString());
            //           xmlDoc.Save(filePath);
            Debug.Log(fileName + ".xml FILE NOT EXIST");
        }
    }

    public void WriteXML(XmlDocument document, string path)
    {
        document.Save(path);
        Debug.Log($"{path}");
        //Debug.Log("SAVED DATA WRITE");
    }


    private void OnXmlReset()
    {
        // Load the permanent document from file
        if (File.Exists(_xmlPathPermanant))
        {
            xmlDoc_Permanant.Load(_xmlPathPermanant);
        }
        else
        {
            // Create a new root element if the permanent file does not exist
            var root = xmlDoc_Permanant.CreateElement("Root");
            xmlDoc_Permanant.AppendChild(root);
        }

        // Load the temporary document from file
        if (File.Exists(_xmlPathTemp))
        {
            xmlDoc_Temp.Load(_xmlPathTemp);
        }
        else
        {
            Debug.LogError($"Temp file {_xmlPathTemp} does not exist.");
            return;
        }

        // Get the root element of both documents
        var permanantRoot = xmlDoc_Permanant.DocumentElement;
        var tempRoot = xmlDoc_Temp.DocumentElement;

        foreach (XmlNode tempNode in tempRoot.ChildNodes)
        {
            // Check if the node already exists in the permanent document
            var nodeExists = false;
            foreach (XmlNode permNode in permanantRoot.ChildNodes)
                if (NodesAreEqual(tempNode, permNode))
                {
                    nodeExists = true;
                    break;
                }

            // If the node does not exist, import and append it
            if (!nodeExists)
            {
                var importedNode = xmlDoc_Permanant.ImportNode(tempNode, true);
                permanantRoot.AppendChild(importedNode);
#if UNITY_EDITOR
                Debug.Log($"append succeeded: {tempNode.Attributes} {tempNode.ChildNodes}");
#endif
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"node already exists: {tempNode.Attributes} {tempNode.ChildNodes}");
            }
#endif
        }


        WriteXML(xmlDoc_Permanant, _xmlPathPermanant);


        tempRoot.RemoveAll();
        // Save the cleared temporary document
        WriteXML(xmlDoc_Temp, _xmlPathTemp);
    }

    private bool NodesAreEqual(XmlNode node1, XmlNode node2)
    {
        // Compare the nodes based on their name and attributes (can be customized)
        if (node1.Name != node2.Name)
            return false;

        if (node1.Attributes.Count != node2.Attributes.Count)
            return false;

        for (var i = 0; i < node1.Attributes.Count; i++)
            if (node1.Attributes[i].Name != node2.Attributes[i].Name ||
                node1.Attributes[i].Value != node2.Attributes[i].Value)
                return false;

        // Additional checks can be added as needed (e.g., comparing child nodes)
        return true;
    }


    private void OnResetSetting()
    {
        var tempRootSetting = xmlDoc_Setting.DocumentElement;
        tempRootSetting.RemoveAll();

        var setting = xmlDoc_Setting.CreateElement("UI_Fish_SettingData");
        setting.SetAttribute("mainvolume", "0.5");
        setting.SetAttribute("bgmvol", "0.5");
        setting.SetAttribute("effectvol", "0.5");

        setting.SetAttribute("timelimit", "30");
        setting.SetAttribute("fishspeed", "1");
        setting.SetAttribute("fishgoalcount", "30");

        tempRootSetting.AppendChild(setting);

        WriteXML(xmlDoc_Setting, _xmlPathSetting);
    }

    private void OnSaveCurrentSettings(float mainVolume, float bgmVol, float effectVol, int timeLimit, float fishSpeed,
        int fishGoalCount)
    {
        var tempRootSetting = xmlDoc_Setting.DocumentElement;
        tempRootSetting.RemoveAllAttributes();

        var setting = xmlDoc_Setting.CreateElement("UI_Fish_SettingData");
        setting.SetAttribute("mainvolume", mainVolume.ToString("F2"));
        setting.SetAttribute("bgmvol", bgmVol.ToString("F2"));
        setting.SetAttribute("effectvol", effectVol.ToString("F2"));
        setting.SetAttribute("timelimit", timeLimit.ToString("F2"));
        setting.SetAttribute("fishspeed", fishSpeed.ToString("F2"));
        setting.SetAttribute("fishgoalcount", fishGoalCount.ToString("F2"));

        tempRootSetting.AppendChild(setting);

        WriteXML(xmlDoc_Setting, _xmlPathSetting);
    }
}