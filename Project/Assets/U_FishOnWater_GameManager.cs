using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class U_FishOnWater_GameManager : IGameManager
{
    private TextAsset xmlAsset;
    private XmlNode soundNode;
    public XmlDocument xmlDoc; // GameManager에서만 문서 수정, UIMAnager에서는 읽기만 수행

    private readonly string xmlPath = "Common/Data/BB008_UserRankingData";
    private string _xmlSavePath = "Resources/Common/Data/BB008_UserRankingData.xml";


    //ResourceManage
    private Dictionary<string, Sprite> _userIconMap;
    public string currentUserName { get; private set; }
    public string currentUserScore { get; private set; }

    private readonly string[] adjectives =
    {
        "귀여운", "활기넘치는", "우아한", "시원한", "따뜻한",
        "편안한", "멋진", "화려한", "아름다운", "소박한",
        "현대적인", "고전적인", "차분한", "대담한", "풍부한",
        "산뜻한", "깔끔한", "화사한", "기발한", "재치넘치는"
    };

    private readonly string[] clothes =
    {
        "청바지", "스웨터", "셔츠", "티셔츠", "재킷",
        "코트", "드레스", "치마", "바지", "반바지",
        "운동복", "잠옷", "점퍼", "블라우스", "베스트",
        "가디건", "레깅스", "양복", "조끼", "한복", "목도리"
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

    private Transform[] _fishesTransforms;
    public readonly int FISH_COUNT = 50;
    public readonly int FISH_COUNT_GOAL = 20;
    private readonly float ANIM_INTERVAL = 5.5f;
    private readonly float SIZE_IN_BUCKET = 0.4f;
    // private readonly float DURATION = 3.0f;

    private float _elapsedForInterval;
    private Vector3[] _pathStartPoints;
    private Vector3[] _pathArrivalPoints;

    private Vector3[] _pathInBucketA;
    private Vector3[] _pathInBucketB;
    private Vector3 _defaultSize;

    private Stack<ParticleSystem> _psPool;

    // 애니메이션 컨트롤을 위한 시퀀스 딕셔너리 
    private Dictionary<int, Sequence> _animSeq;
    private Dictionary<int, Animator> _animatorSeq;

    // 물고기가 버킷에 있는지 체크하기 위한 딕셔너리
    private Dictionary<int, bool> _isOnBucket;

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
            if (_fishCaughtCount >= FISH_COUNT_GOAL)
            {
                _fishCaughtCount = FISH_COUNT_GOAL;
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

    private void Update()
    {
        if (isOnReInit || !_isGameStart) return;

        _elapsedForInterval += Time.deltaTime;
        _elapsedForReInit += Time.deltaTime;
        remainTime = timeLimit - _elapsedForReInit;

        if (_elapsedForReInit > timeLimit && !isOnReInit) InvokeFinishAndReInit();
    }

    private void InvokeFinishAndReInit()
    {
        isOnReInit = true;
        _elapsedForReInit = 0;

        currentUserName = adjectives[Random.Range(0, adjectives.Length)]
                           + clothes[Random.Range(0, clothes.Length)]
                           + Random.Range(0, 1000);
        currentUserScore = ((_fishCaughtCount + remainTime) * 100f).ToString("F2");
#if UNITY_EDITOR
        Debug.Log($"remainTime : {remainTime}" + $"fishCount : {_fishCaughtCount}");
#endif

        Utils.AddUser(ref xmlDoc, currentUserName, currentUserScore);
        Utils.SaveXML(ref xmlDoc, _xmlSavePath);

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

        U_FishOnWater_UIManager.OnRestartBtnClicked -= OnRestartBtnClicked;
        U_FishOnWater_UIManager.OnRestartBtnClicked += OnRestartBtnClicked;
    }

    protected void OnDestroy()
    {
        U_FishOnWater_UIManager.OnRestartBtnClicked -= OnRestartBtnClicked;
        U_FishOnWater_UIManager.OnStartUIAppear -= OnRoundStart;
        OnFishCaught -= PlayPathAnimOneTime;
    }


    protected override void Init()
    {
        base.Init();
        Utils.LoadXML(ref xmlAsset, ref xmlDoc, xmlPath, ref _xmlSavePath);

        DOTween.Init().SetCapacity(300, 300);
        ManageProjectSettings(90, 0.15f);
        remainTime = timeLimit;


        var pathStartParent = transform.GetChild((int)Path.Start);
        _pathStartPoints = new Vector3[pathStartParent.childCount];

        for (var i = 0; i < _pathStartPoints.Length; i++) _pathStartPoints[i] = pathStartParent.GetChild(i).position;

        var pathArrivalParent = transform.GetChild((int)Path.Arrival);
        _pathArrivalPoints = new Vector3[pathArrivalParent.childCount];
        for (var i = 0; i < _pathArrivalPoints.Length; i++)
            _pathArrivalPoints[i] = pathArrivalParent.GetChild(i).position;


        _animatorSeq = new Dictionary<int, Animator>();
        _fishesTransforms = new Transform[FISH_COUNT]; //전체 물고기 컨트롤용입니다. (초기화로직 수행 등)
        _animSeq = new Dictionary<int, Sequence>();
        _isOnBucket = new Dictionary<int, bool>();
        _psPool = new Stack<ParticleSystem>();


        var prefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/FishOnWater/Prefabs/FishOnWater_Fish");
        for (var i = 0; i < FISH_COUNT; i++)
        {
            var fish = Instantiate(prefab, transform).GetComponent<Transform>();

            _defaultSize = fish.localScale;
            var randomChar = Random.Range('A', 'E' + 1);
            var path = "게임별분류/기본컨텐츠/FishOnWater/Fishes/M_Fish" + (char)randomChar;
            var mat = Resources.Load<Material>(path);
            if (mat == null) Debug.LogError($"Mat is Null{path}");
            fish.GetChild(1).GetComponent<SkinnedMeshRenderer>().material =
                mat; // prefab상 2번째 자식에 SkinnedMeshRenderer 할당되어 있어 하드코딩 4/8/24

            //_fishesQueue.Enqueue(fish);
            _fishesTransforms[i] = fish;
            _isOnBucket.TryAdd(fish.GetInstanceID(), false);
            _animatorSeq.TryAdd(fish.GetInstanceID(), fish.GetComponent<Animator>());
        }


        var pathInBucketParent = GameObject.Find("PathInBucket").transform;
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

        _bucket = GameObject.Find("Bucket").transform;
        _bucketDefaultQuat = _bucket.rotation;
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
            var currentPath = SetPath();
            var moveAnimSeq = DOTween.Sequence();


            var currentFish = _fishesTransforms[_currentFishIndex % FISH_COUNT];
            var id = currentFish.GetInstanceID();

            _animatorSeq[id].speed = 1f;

            if (_isOnBucket[id])
                //_fishesQueue.Enqueue(currentFish);
                return;
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB008/OnFishAppear", 0.15f);
            currentFish.position = currentPath[(int)Path.Start];
            var randomDuration = Random.Range(1.0f, 2.5f);
            moveAnimSeq
                .Append(currentFish
                    .DOPath(currentPath, randomDuration, PathType.CatmullRom)
                    .OnStart(() =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB008/OnFishAppear", 0.15f);
                    })
                    .SetLookAt(-0.01f)
                    .SetEase((Ease)Random.Range((int)Ease.InSine, (int)Ease.InOutCubic)));
            moveAnimSeq.OnComplete(() => { DoAnimAfterArrival(currentFish, currentFish.position); });
            //순서만을 관리하므로, 동작컨트롤 후 바로 Enqueue 해줍니다. 
            // _fishesQueue.Enqueue(currentFish);
            _animSeq[id] = moveAnimSeq;
            moveAnimSeq.Play();
            _currentFishIndex++;
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

        if (randomValue <= 0.33f)
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

    private void ReInit()
    {
        _elapsedForInterval = 0;
        _elapsedForReInit = 0;
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
        _currentFishIndex = 0;

        _bucket
            .DORotateQuaternion(_bucketDefaultQuat * Quaternion.Euler(0, 0, -90), 0.5f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                _bucket.DORotateQuaternion(_bucketDefaultQuat, 1f).SetEase(Ease.InSine).SetDelay(0.15f);
            });

        foreach (var fish in _fishesTransforms)
        {
            escapePath[0] = fish.position;
            escapePath[1] = _pathStartPoints[Random.Range(0, _pathStartPoints.Length)];
            fish.DOScale(_defaultSize, 0.8f).SetEase(Ease.InOutBounce);

            fish.DOPath(escapePath, 1.5f, PathType.CatmullRom)
                .SetLookAt(-0.01f)
                .SetEase((Ease)Random.Range(3, 6))
                .SetAutoKill(true);

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

    private void OnRestartBtnClicked()
    {
        DOVirtual.Float(0, 0, 2f, _ => { }).OnComplete(() =>
        {
            FishCaughtCount = 0;
            OnReady?.Invoke();
        });
    }


    private void SendToBucket(Transform fish)
    {
#if UNITY_EDITOR
        Debug.Log("BucktPathIsSet");
#endif
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BB008/OnFishCaught" + (char)Random.Range('A', 'C' + 1),
            0.5f);

        var id = fish.GetInstanceID();
        _animSeq[id]?.Kill();

        var bucketSeq = DOTween.Sequence();

        var pathToBucket = new Vector3[3]; // 물고기 클릭위치 에서 버킷까지의 경로 
        var inBucketPath = Random.Range(0, 100) > 50
            ? _pathInBucketA
            : _pathInBucketB; //버킷에 도착 후 경로
        inBucketPath[1] +=
            new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
        inBucketPath[2] +=
            new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));

        var startIndex = 0;
        var middleIndex = 1;
        var bucketIndex = 2;
        var archHegiht = fish.up * 20;

        //경로설정
        pathToBucket[startIndex] = fish.position;
        pathToBucket[middleIndex] = (inBucketPath[0] + fish.position) / 2 + archHegiht;
        pathToBucket[bucketIndex] = inBucketPath[0];


        bucketSeq.Append
        (fish.DOPath(pathToBucket, Random.Range(1.0f, 2.0f))
            .SetLookAt(-0.01f)
            .OnStart(() => { fish.DORotateQuaternion(fish.rotation * Quaternion.Euler(0, 1080, 0), 1f); })
            .OnComplete(() => { fish.position = inBucketPath[0]; })
        );

        bucketSeq.Append(
            fish.DOScale(SIZE_IN_BUCKET * _defaultSize, 1f));
        // .OnStart(() => { fish.DORotate(Vector3.zero, 0.1f); });


        // 양동이 안의 경로 설정 부분
        var loopType = LoopType.Restart;
        var pathInBucketWithRandomOffset = inBucketPath;
        var OFFSET_AMOUNT = 0.7f;


        for (var i = 1; i < pathInBucketWithRandomOffset.Length; i++)
            pathInBucketWithRandomOffset[i] +=
                Random.Range(-1.0f, 1.0f) * Vector3.forward + Random.Range(-1.0f, 1.0f) * Vector3.left;


        bucketSeq.Append(fish.DOPath(pathInBucketWithRandomOffset, Random.Range(3.5f, 6.5f), PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .SetLookAt(-0.01f)
            .SetLoops(10, LoopType.Restart));


        bucketSeq.Play();
        _animSeq[id] = bucketSeq;


        if (_isOnBucket.ContainsKey(id)) _isOnBucket[id] = true;
        FishCaughtCount++;
        _animatorSeq[id].speed = 0.6f;
        OnFishCaught?.Invoke();
    }


    //xml 파일 핸들링.. 테스트 후 Util 클래스로 이동여부 검토 해야합니다. 5/30/24 민석
    // Load the XML file
    private void LoadXML()
    {
        xmlAsset = Resources.Load<TextAsset>("Common/Data/BB008_UserRankingData");
        xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlAsset.text);

        // Get the path to save the file later
        _xmlSavePath = System.IO.Path.Combine(Application.dataPath, "Common/Data/BB008_UserRankingData");
    }

    // Add a new user to the XML
    private void AddUser(string score, string username)
    {
        XmlNode root = xmlDoc.DocumentElement;

        // Find the highest userID
        var highestUserID = -1;
        foreach (XmlNode node in root.SelectNodes("StringData"))
        {
            var userID = int.Parse(node.Attributes["userID"].Value);
            if (userID > highestUserID) highestUserID = userID;
        }

        // Create a new user with the next available userID
        var newUserID = highestUserID + 1;

        var newUser = xmlDoc.CreateElement("StringData");
        newUser.SetAttribute("userID", newUserID.ToString());
        newUser.SetAttribute("username", username);
        newUser.SetAttribute("score", score);

        root.AppendChild(newUser);
    }

    // Save the XML file
    private void SaveXML()
    {
        xmlDoc.Save(_xmlSavePath);
        Debug.Log("XML file saved to: " + _xmlSavePath);
    }
}