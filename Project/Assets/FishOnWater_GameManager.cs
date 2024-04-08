using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Sequence = DG.Tweening.Sequence;

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

    private Queue<Transform> _fishes;
    private readonly int FISH_COUNT = 8;
    private readonly float DURATION = 4.5f;
    private readonly float _animInterval = 5.5f;
    private float _elapsedForInterval;
    private Vector3[,] _pathPoints;
    private Vector3[] _pathInBucket;
    private Stack<ParticleSystem> _psPool;
    
    // 애니메이션 컨트롤을 위한 시퀀스 딕셔너리 
    private Dictionary<int, Sequence> _animSeq;
    
    // 물고기가 버킷에 있는지 체크하기 위한 딕셔너리
    private Dictionary<int, bool> _isOnBucket;

    public float timeLimt;
    private float _elapsedForReInit;
    private Transform _bucket;
 


    private void Start()
    {
        Init();
        PlayAnim();
    }


    private void Update()
    {
        if (!_isOnReInit)
        {
            _elapsedForInterval += Time.deltaTime;
        }
    
        if (_elapsedForInterval > _animInterval)
        {
            PlayAnim();
            _elapsedForInterval = 0;
        }

        
        _elapsedForReInit +=Time.deltaTime;
        // if (_elapsedForReInit > timeLimt)
        // {
        //     ReInit();
        // }
       
    }

    protected override void Init()
    {
        base.Init();
        _pathPoints = new Vector3[(int)PathRow.Max, (int)PathColumn.Max];

        for (var row = 0; row < (int)PathRow.Max; row++)
        {
            var pathParent = transform.GetChild(row);

            for (var column = 0; column < (int)PathColumn.Max; column++)
                _pathPoints[row, column] = pathParent.GetChild(column).position;
        }

        _fishes = new Queue<Transform>();
        _animSeq = new Dictionary<int, Sequence>();
        _isOnBucket = new Dictionary<int, bool>();
        _psPool = new Stack<ParticleSystem>();
        
        var prefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/FishOnWater/Prefabs/FishOnWater_Fish");
        for (var i = 0; i < FISH_COUNT; i++)
        {
            var fish = Instantiate(prefab, transform).GetComponent<Transform>();

            var randomChar = Random.Range('A', 'C' + 1);
            string path = "게임별분류/기본컨텐츠/FishOnWater/Fishes/M_Fish" + (char)randomChar;
            var mat =Resources.Load<Material>(path);
            if(mat == null) Debug.LogError($"Mat is Null{path}");
            fish.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = mat; // prefab상 2번째 자식에 SkinnedMeshRenderer 할당되어 있어 하드코딩 4/8/24
                    
            _fishes.Enqueue(fish);
            _isOnBucket.TryAdd(fish.GetInstanceID(), false);
        }
        
        
        var pathInBucketParent = GameObject.Find("PathInBucket").transform;
        _pathInBucket = new Vector3[pathInBucketParent.childCount + 1];
        for (int i = 0; i < pathInBucketParent.childCount; i++)
        {
            _pathInBucket[i] = pathInBucketParent.GetChild(i).position;
        }
        // 제자리로 돌아오게끔 (경로를 원형으로) 만들기 위한 추가 로직 
        _pathInBucket[pathInBucketParent.childCount] = pathInBucketParent.GetChild(0).position;
       
        var vfxPrefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/FishOnWater/Prefabs/CFX_WaterSplash");
        
        var PARTICLE_COUNT = 15;
        for (int i = 0; i < PARTICLE_COUNT; i++)
        {
            var psObj = Instantiate(vfxPrefab, GameObject.Find("CFX").transform);
            var ps = psObj.GetComponent<ParticleSystem>();
            _psPool.Push(ps);
        }

        _bucket = GameObject.Find("Bucket").transform;
    }


    private Quaternion _bucketDefaultRotation;

    private void PlayAnim()
    {
        
        // _bucket.DORotateQuaternion(_bucketDefaultRotation * Quaternion.Euler(0,0,-40),1f).SetEase(Ease.InOutSine)
        //     .OnComplete(() =>
        //     {
        //         
        //     })
        var currentPath = SetPath();
        var seq = DOTween.Sequence();
        
        if (_fishes.Count > 0)
        {
            var currentFish = _fishes.Dequeue();
            var id = currentFish.GetInstanceID();

            if (_isOnBucket[id])
            {
                _fishes.Enqueue(currentFish);
                return;
            }
            
            currentFish.position = currentPath[(int)PathColumn.Start];
            seq.Append(currentFish.DOPath(currentPath, DURATION, PathType.CatmullRom)
                    .SetLookAt(-0.01f)
                    .SetEase((Ease)Random.Range((int)Ease.InSine, (int)Ease.InOutCubic)))
                .OnComplete(() =>
                {
                    _animSeq[id].Kill();
                    _animSeq[id] = null;
                    _fishes.Enqueue(currentFish);
                });

            _animSeq.TryAdd(id, seq);
            seq.Play();
        }
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        foreach (var hit in GameManager_Hits)
            if (hit.transform.gameObject.name.Contains("Fish"))
            {
                var fish = hit.transform;
                var id = fish.GetInstanceID();

                if (_animSeq.ContainsKey(id) && _isOnBucket.ContainsKey(id))
                {
                    if (!_isOnBucket[id])
                    {
                        _animSeq[fish.GetInstanceID()].Kill();
                        _animSeq[fish.GetInstanceID()] = null;
                        SendToBucket(fish);
                    }
                   
                }
            }

        foreach (var hit in GameManager_Hits)
        {
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
            else
            {
                return;
            }
        }
    }

    private bool _isOnReInit;
    private void ReInit()
    {
        _isOnReInit = true;
        foreach (var key in _animSeq.Keys.ToList())
        {
            _animSeq[key].Kill();
            _animSeq[key] = null;
        }

        var escapePath = new Vector3[2];

        foreach (var fish in _fishes)
        {
            escapePath[0] = fish.position;
            var randomNumToSetEscape = Random.Range(0, 100);
         
            if (randomNumToSetEscape > 50)
            {
             escapePath[1] = _pathPoints[(int)PathColumn.Start, (int)PathColumn.End];
            }
            else
            {
             escapePath[1] = _pathPoints[(int)PathColumn.End, (int)PathColumn.End];
            }

            fish.DOPath(escapePath, 3f).SetEase((Ease)Random.Range(3, 6)).OnComplete(() =>
            {
                _isOnReInit = false;
            });
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

    private void KillAnim()
    {
    }

    private void SendToBucket(Transform fish)
    {
        
        var id = fish.GetInstanceID();
        Sequence bucketSeq = DOTween.Sequence();
        var pathToBucket = new Vector3[3];
        var startIndex = 0;
        var middleIndex = 1;
        var bucketIndex = 2;
        var archHegiht =  fish.up * 20;
        
        //경로설정
        pathToBucket[startIndex] = fish.position;
        pathToBucket[middleIndex] = (_pathInBucket[0] + fish.position) / 2 + archHegiht;
        pathToBucket[bucketIndex] = _pathInBucket[0];

        bucketSeq.Append(fish.DOPath(pathToBucket,1f)
            .SetLookAt(-0.01f)
            .OnStart(() =>
            {
                fish.DORotateQuaternion(transform.rotation * Quaternion.Euler(0, 1080, 0),1.1f);
            })
            .OnComplete(() =>
            {
                fish.position = _pathInBucket[0];
                fish.DORotate(Vector3.zero,0.1f);
                fish.DOMove(_pathInBucket[0], 0.25f).OnComplete(() =>
                {
                    fish.DOPath(_pathInBucket, 6f, PathType.CatmullRom)
                        .SetLoops(-1, LoopType.Restart)
                        .SetLookAt(-0.01f)
                        .SetEase(Ease.InSine);

                });
                
            }));
        
        _animSeq.TryAdd(id, bucketSeq);
        
        if (_isOnBucket.ContainsKey(id))
        {
            _isOnBucket[id] = true;
        }
    }
}