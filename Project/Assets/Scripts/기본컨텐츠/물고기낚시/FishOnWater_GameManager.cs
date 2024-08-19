using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class FishOnWater_GameManager : IGameManager
{
    /*
       4x4구성의 애니메이션 경로 관련 enum 선언입니다.
     경료지점 변경 관련 유지보수가 용이하도록 설계하였습니다.
       예를들어, 5x6으로 이동경로 목록을 수정하는 경우, 경로,enum 목록추가 등 간단한 작업만으로
     경로 애니메이션 재생이 동작하도록 구성했습니다.
     */
    private enum PathRow
    {
        Start,
        MiddleA,
        MiddleB,
        End,
        Max
    }

    private enum PathColumn
    {
        Start,
        MiddleA,
        MiddleB,
        End,
        Max
    }
    
    private Transform[] _fishesTransforms;
    private readonly int FISH_COUNT = 15;
    private readonly float _animInterval = 5.5f;
    private readonly float _sizeInBucket = 0.4f;
    // private readonly float DURATION = 3.0f;
   
    private float _elapsedForInterval;
    private Vector3[,] _pathPoints;
    private Vector3[] _pathInBucketA;
    private Vector3[] _pathInBucketB;
    private Vector3 _defaultSize;

    private Stack<ParticleSystem> _psPool;
    
    // 애니메이션 컨트롤을 위한 시퀀스 딕셔너리 
    private Dictionary<int, Sequence> _animSeq;

    // 물고기가 버킷에 있는지 체크하기 위한 딕셔너리
    private Dictionary<int, bool> _isOnBucket;
    private Transform _bucket;

    public float timeLimit ;
    private float _elapsedForReInit;
    private bool _isOnReInit;
    private Quaternion _bucketDefaultQuat;
    private Quaternion _bucketDefaultRotation;
    
    private int _currentFishIndex;

    private void Start()
    {
        Init();
        
    }


    private void Update()
    {
        if (!_isOnReInit) _elapsedForInterval += Time.deltaTime;


        if (_elapsedForInterval > _animInterval && !_isOnReInit)
        {
            PlayPathAnim(Random.Range(1,4));
            _elapsedForInterval = 0;
        }

        if (!_isOnReInit) _elapsedForReInit += Time.deltaTime;
        
        if (_elapsedForReInit > timeLimit && !_isOnReInit)
        {
            _elapsedForReInit = 0;
            _isOnReInit = true;
            ReInit();
        }
    }

    protected override void Init()
    {
        base.Init();

        DOTween.Init().SetCapacity(300,300);
        
        _fishesTransforms = new Transform[FISH_COUNT]; //전체 물고기 컨트롤용입니다. (초기화로직 수행 등)
        _animSeq = new Dictionary<int, Sequence>();
        _isOnBucket = new Dictionary<int, bool>();
        _psPool = new Stack<ParticleSystem>();
        
        
        _pathPoints = new Vector3[(int)PathRow.Max, (int)PathColumn.Max];

        for (var row = 0; row < (int)PathRow.Max; row++)
        {
            var pathParent = transform.GetChild(row);

            for (var column = 0; column < (int)PathColumn.Max; column++)
                _pathPoints[row, column] = pathParent.GetChild(column).position;
        }

        var prefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/FishOnWater/Prefabs/FishOnWater_FishA");
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



    private void PlayPathAnim(int fishCount)
    {
        if (_isOnReInit) return;

        for (int i = 0; i < fishCount; i++)
        {
            var currentPath = SetPath();
            var moveAnimSeq = DOTween.Sequence();


            var currentFish = _fishesTransforms[_currentFishIndex % FISH_COUNT];
            var id = currentFish.GetInstanceID();

            if (_isOnBucket[id])
            {
                //_fishesQueue.Enqueue(currentFish);
                return;
            }

            currentFish.position = currentPath[(int)PathColumn.Start];
            var randomDuration = Random.Range(4.0f, 5.0f);
            moveAnimSeq.Append(currentFish.DOPath(currentPath, randomDuration, PathType.CatmullRom)
                    .SetLookAt(-0.01f)
                    .SetEase((Ease)Random.Range((int)Ease.InSine, (int)Ease.InOutCubic)))
                .OnComplete(() =>
                {
                    _animSeq[id].Kill();
                    _animSeq[id] = null;
                });
            
            //순서만을 관리하므로, 동작컨트롤 후 바로 Enqueue 해줍니다. 
            // _fishesQueue.Enqueue(currentFish);
            _animSeq[id] = moveAnimSeq;
            moveAnimSeq.Play();
            _currentFishIndex++;
        }
       
    }
    

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        // 초기화 등, 기타 로직에서 클릭을 무시해야할 경우
        if (_isOnReInit) return;
        
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
                DOVirtual.Float(0, 1, ps.main.startLifetime.constant + 0.5f, _ => { }).OnComplete(() =>
                {
                    _psPool.Push(ps);
                });
            }
        
    }

    private void ReInit()
    {
        foreach (var key in _animSeq.Keys.ToList())
        {
            _animSeq[key].Kill();
            _animSeq[key] = null;
        }

        foreach (var key in _isOnBucket.Keys.ToList())
        {
            _isOnBucket[key] = false;
        }


        var escapePath = new Vector3[3];
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
            var randomNumToSetEscape = Random.Range(0, 100);

            if (randomNumToSetEscape > 50)
            {
                escapePath[1] = _pathPoints[(int)PathColumn.MiddleA, (int)PathColumn.End];
                escapePath[2] = _pathPoints[(int)PathColumn.Start, (int)PathColumn.End];
            }
            else
            {
                escapePath[1] = _pathPoints[(int)PathColumn.MiddleB, (int)PathColumn.End];
                escapePath[2] = _pathPoints[(int)PathColumn.End, (int)PathColumn.End];
            }

            fish.DOScale(_defaultSize, 0.8f).SetEase(Ease.InOutBounce);
            
            fish.DOPath(escapePath, 1.5f, PathType.CatmullRom)
                .SetLookAt(-0.01f)
                .SetEase((Ease)Random.Range(3, 6))
                .SetAutoKill(true)
                .OnComplete(() =>
                {
                    _isOnReInit = false;
                });

#if UNITY_EDITOR
Debug.Log($"ReInit, Fish Escape Anim");
#endif
}

}

    private Vector3[] SetPath()
    {
        var path = new Vector3[(int)PathColumn.Max];

        // 오른쪽-왼쪽 시작점 결정을 위한 확률설계


        var chance = Random.Range(0, 100);

        if (chance < 50)
            for (var i = (int)PathColumn.Start; i < (int)PathColumn.Max; i++)
                path[i] = _pathPoints[i, Random.Range(0, (int)PathRow.Max)];
        else
            for (var k = (int)PathColumn.End; k >= 0; k--)
                path[(int)PathColumn.End - k] = _pathPoints[k, Random.Range(0, (int)PathRow.Max)];

        return path;
    }


    private void SendToBucket(Transform fish)
    {
#if UNITY_EDITOR
Debug.Log("BucktPathIsSet");
#endif

        
        var id = fish.GetInstanceID();
        _animSeq[id]?.Kill();
        
        var bucketSeq = DOTween.Sequence();

        var pathToBucket = new Vector3[3]; // 물고기 클릭위치 에서 버킷까지의 경로 
        var inBucketPath = Random.Range(0, 100) > 50 ? _pathInBucketA : _pathInBucketB; //버킷에 도착 후 경로


        var startIndex = 0;
        var middleIndex = 1;
        var bucketIndex = 2;
        var archHegiht = fish.up * 20;
        
        //경로설정
        pathToBucket[startIndex] = fish.position;
        pathToBucket[middleIndex] = (inBucketPath[0] + fish.position) / 2 + archHegiht;
        pathToBucket[bucketIndex] = inBucketPath[0];


        bucketSeq.Append
        (fish.DOPath(pathToBucket, 1f)
                .SetLookAt(-0.01f)
                .OnStart(() => { fish.DORotateQuaternion(fish.rotation * Quaternion.Euler(0, 1080, 0), 1f); })
                .OnComplete(() => { fish.position = inBucketPath[0]; })
        );

        bucketSeq.Append(
            fish.DOScale(_sizeInBucket * _defaultSize, 1f));
           // .OnStart(() => { fish.DORotate(Vector3.zero, 0.1f); });


        // 양동이 안의 경로 설정 부분
        var loopType = LoopType.Restart;
        var pathInBucketWithRandomOffset = inBucketPath;
        var OFFSET_AMOUNT = 0.7f;
        
#if UNITY_EDITOR
        Debug.Log($"path info : {pathInBucketWithRandomOffset[0]}");
#endif
        for (int i = 1; i < pathInBucketWithRandomOffset.Length; i++)
        {
            pathInBucketWithRandomOffset[i] += Random.Range(-1.0f, 1.0f) * Vector3.forward + Random.Range(-1.0f, 1.0f) * Vector3.left;
        }


        bucketSeq.Append(fish.DOPath(pathInBucketWithRandomOffset, Random.Range(3.5f, 6.5f), PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .SetLookAt(-0.01f)
            .SetLoops(10, LoopType.Restart));
      
        

        bucketSeq.Play();
        _animSeq[id] = bucketSeq;
       

        if (_isOnBucket.ContainsKey(id)) _isOnBucket[id] = true;
    }
}




