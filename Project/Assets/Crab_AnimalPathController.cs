using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Crab_AnimalPathController : MonoBehaviour
{
    private enum StartDirection // 1     2
        // 4     3 
    {
        Upper_Left,
        Upper_Right,
        Bottom_Right,
        Bottom_Left
    }

    private enum AwayPath
    {
        start,
        arrival
    }

    private enum InPath
    {
        Start,
        Arrival
    }

    private enum LoopPath
    {
        Start,
        Mid1,
        Mid2,
        Mid3,
        End
    }

    public Vector3[] loopPath { get; set; }

    [FormerlySerializedAs("c_efc")] [SerializeField]
    private Crab_EffectController crab_effectController;

    private Vector3 _pivotPoint;

    private readonly string _crabPrefabPath = "게임별분류/비디오컨텐츠/Crab/";


    private Stack<Crab> inactiveCrabPool;
    private Queue<Crab> activeCrabPool;

    [FormerlySerializedAs("startPoints")] [Header("Start Points")]
    public Transform[] appearablePoints;

    [Header("Offsets")] public float startOffset;
    public float midOffset;
    public float endOffset;


    private Vector3 main;
    private Vector3 start;
    private Vector3 mid1;
    private Vector3 mid2;
    private Vector3 mid3;
    private Vector3 end;

    private Vector3[] inPath;
    private Vector3[] awayPath;

    public Transform lookAtTarget;

    [Range(0, 5)] public float randomLoopRange;


    private void Start()
    {
        if (!isInit) Init();
    }

    private bool isInit;

    private void Init()
    {
        inactiveCrabPool = new Stack<Crab>();
        activeCrabPool = new Queue<Crab>();

        loopPath = new Vector3[5];
        awayPath = new Vector3[2];
        inPath = new Vector3[2];
        _distancesFromStartPoint = new float[4];

        Crab_EffectController.OnClickForEachClick -= OnClicked;
        Crab_EffectController.OnClickForEachClick += OnClicked;

        SetPool(inactiveCrabPool, "CrabA");
        SetPool(inactiveCrabPool, "CrabB");
        SetPool(inactiveCrabPool, "CrabC");
        SetPool(inactiveCrabPool, "CrabD");


        isInit = true;
    }


    private void OnDestroy()
    {
        Crab_EffectController.OnClickForEachClick -= OnClicked;
    }


    private void OnClicked()
    {
        if (!isInit) return;

        SetInAndLoopPath();
        UpdateDistanceFromStartPointToClickedPoint();
        DoPathToClickPoint();

#if UNITY_EDITOR

#endif
    }


    private int _closestStartPointIndex;
    private float[] _distancesFromStartPoint;

    /// <summary>
    ///     4개의 Start 포인트에서 가장 가까운 생성지점이 어딘지 매 클릭시 업데이트 합니다.
    /// </summary>
    private void UpdateDistanceFromStartPointToClickedPoint()
    {
        for (var i = (int)StartDirection.Upper_Left; i < appearablePoints.Length; ++i)
            _distancesFromStartPoint[i] =
                Vector2.Distance(crab_effectController.hitPoint, appearablePoints[i].position);

        _closestStartPointIndex = (int)StartDirection.Upper_Left;

        for (var section = (int)StartDirection.Upper_Right; section < (int)StartDirection.Bottom_Left; section++)
        {
            if (_distancesFromStartPoint[section] > _distancesFromStartPoint[_closestStartPointIndex])
                _closestStartPointIndex = section;

            if (_distancesFromStartPoint[section] < _distancesFromStartPoint[_closestStartPointIndex])
                awayPath[(int)AwayPath.arrival] = appearablePoints[Random.Range(0, 4)].position;
        }
    }

    private void DoPathToClickPoint()
    {
        var crabFromPool = GetFromPool();

        if (crabFromPool != null)
        {
            SetAndPlayPath(crabFromPool);
          
            PushIntoActivePool(crabFromPool);
        }
        else
        {
            var crab = activeCrabPool.Dequeue();

            if (!crab.isGoingHome && crab.isPathSet)
            {
                SetAndPlayPath(crab);
                
                PushIntoActivePool(crab);
            }
        }
    }

    [Header("duration Setting")] public float _pathDurationOnSec;
    public float _pathDuration;
    public float _duration;

    private void SetAndPlayPath(Crab crabDoingPath)
    {
      
        _duration = crabDoingPath.isPathSet ? _pathDurationOnSec : _pathDuration;

        if (crabDoingPath.isPathSet == false)
        {
            crabDoingPath.gameObj.transform.position = appearablePoints[_closestStartPointIndex].position;
            inPath[(int)InPath.Start] = appearablePoints[_closestStartPointIndex].position;
            crabDoingPath.isPathSet = true;
        }
        else
        {

#if UNITY_EDITOR
          
#endif
           // inPath[(int)InPath.Start] = _crabDoingPath.gameObj.transform.position;
        }
        
        PlayPath(crabDoingPath);
      
    }
    

    private void PlayPath(Crab _crabDoingPath)
    {
        if (_crabDoingPath.currentSequence != null && _crabDoingPath.currentSequence.IsActive())
        {
            _crabDoingPath.currentSequence.Kill();
        }
        
        StartCoroutine(CheckSequenceKilled(_crabDoingPath.currentSequence));
        
        _crabDoingPath.currentSequence = DOTween.Sequence();
        
        _crabDoingPath.currentSequence
                .Append(_crabDoingPath.gameObj.transform
                .DOMove(crab_effectController.hitPoint + Vector3.up *Random.Range(0,3), _duration)
                .OnStart(() =>
                {
                    _crabDoingPath.gameObj.transform.DOLookAt(lookAtTarget.position, 0.01f);
                })
                .OnComplete(() =>
                {
#if UNITY_EDITOR
                       Debug.Log("시퀀스시작");
#endif
                    _crabDoingPath.gameObj.transform.position = _crabDoingPath.loopPath[(int)LoopPath.Start];
                    
                   // SetInAndLoopPath();
                   // CopyPathArrays(_crabDoingPath);
                   
                    _crabDoingPath.gameObj.transform.DOPath(_crabDoingPath.loopPath, 2.5f, PathType.CatmullRom)
                        .SetLoops(8, LoopType.Yoyo)
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            _crabDoingPath.isGoingHome = true;
                            awayPath[(int)AwayPath.start] = _crabDoingPath.gameObj.transform.position;

                            _crabDoingPath.gameObj.transform
                                .DOPath(_crabDoingPath.awayPath, 3f, PathType.Linear)
                                .OnComplete(() =>
                                {
                                    _crabDoingPath.gameObj.SetActive(false);
                                    inactiveCrabPool.Push(_crabDoingPath);

                                    _crabDoingPath.isPathSet = false;
                                    _crabDoingPath.isGoingHome = false;
                                });
                        });
                    
                }));
        _crabDoingPath.currentSequence.Play();
    }

   

    IEnumerator CheckSequenceKilled(Sequence sequence)
    {
        yield return new WaitForSeconds(0.1f); // 짧은 지연

        if (sequence == null || !sequence.IsActive())
        {
#if UNITY_EDITOR
            //     Debug.Log("Kill 성공: Sequence가 중단되었습니다.");
#endif
        }
        else
        {
#if UNITY_EDITOR
            //    Debug.Log("Kill 실패: Sequence가 여전히 활성화되어 있습니다.");
#endif
        }
    }
    private void InitializeAndSetSequence(Crab crab)
    {
        if (crab.currentSequence != null && crab.currentSequence.IsActive())
        {
            DOTween.Kill(crab);
        }

        crab.currentSequence = DOTween.Sequence();
    }

    private void CopyPathArrays(Crab crab)
    {
        crab.inPath = new Vector3[inPath.Length];
        Array.Copy(inPath, crab.inPath, inPath.Length);

        crab.loopPath = new Vector3[loopPath.Length];
        Array.Copy(loopPath, crab.loopPath, loopPath.Length);

        crab.awayPath = new Vector3[awayPath.Length];
        Array.Copy(awayPath, crab.awayPath, awayPath.Length);
    }

    /// <summary>
    ///     클릭시 게가 이동할 경로를 클릭하는 곳으로 이동시키는 함수 입니다.
    /// </summary>
    private void SetInAndLoopPath()
    {
        main = crab_effectController.hitPoint;

        start = main + Vector3.up * startOffset * Random.Range(1, randomLoopRange);
        mid1 = main + Vector3.forward * midOffset * Random.Range(1, randomLoopRange);
        mid2 = main + Vector3.down * midOffset * Random.Range(1, randomLoopRange);
        mid3 = main + Vector3.back * midOffset * Random.Range(1, randomLoopRange);
        
        /*           mid
         *    start       mid2
         *           end
         */

        loopPath[(int)LoopPath.Start] = start;
        loopPath[(int)LoopPath.Mid1] = mid1;
        loopPath[(int)LoopPath.Mid2] = mid2;
        loopPath[(int)LoopPath.Mid3] = mid3;
        loopPath[(int)LoopPath.End] = start;
    }

    private void SetPool(Stack<Crab> pool, string name)
    {
        /*
         * 프리팹은 로컬에서 Active/Inactive 대상이 아니므로 반드시 가져오기/비활성화 하기 분리해서
         * 로직을 실행해야 함
         */
        var prefab = Resources.Load<GameObject>(_crabPrefabPath + name);

        if (prefab != null)
        {
            var crab = new Crab();
            crab.gameObj = Instantiate(prefab, transform);

            // instance.transform.position = startPoints[0].position;

            crab.gameObj.SetActive(false);
#if UNITY_EDITOR

#endif
            pool.Push(crab);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError($"this gameObj is null.. pathinfo: {_crabPrefabPath + name}");
#endif
        }
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    private Crab GetFromPool()
    {
        if (inactiveCrabPool.Count > 0)
        {
            var crab = new Crab();
            crab = inactiveCrabPool.Pop();
            crab.gameObj.SetActive(true);
            

            return crab;
        }

        return null;
    }

    private void PushIntoActivePool(Crab _crab)
    {
#if UNITY_EDITOR

#endif
        activeCrabPool.Enqueue(_crab);
    }
}


public class Crab
{
    public GameObject gameObj { get; set; }
    public Vector3[] inPath { get; set; }
    public Vector3[] loopPath { get; set; }
    public Vector3[] awayPath { get; set; }

    //isPathSet은 경로 재설정 위한 bool값.
    //isInitiallySetPath는 중복여부 확인을 위한값
    public bool isPathSet { get; set; }

    public Sequence currentSequence { get; set; }

    public bool isGoingHome { get; set; }
}