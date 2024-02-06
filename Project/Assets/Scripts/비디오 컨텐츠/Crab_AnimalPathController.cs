using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Crab_AnimalPathController : MonoBehaviour
{
    [FormerlySerializedAs("crabVideoGameManager")]
    [FormerlySerializedAs("crab_effectController")]
    [Header("Reference")]
    [SerializeField]
    private CrabEffectManager crabEffectManager;

    [FormerlySerializedAs("_videoContentPlayer")] [SerializeField]
    private CrabVideoGameManager videoGameManager;

    //animation control part.
    public static readonly int ROLL_ANIM = Animator.StringToHash("Roll");
    public static readonly int ROTATE_A_ANIM = Animator.StringToHash("Rotate_A");
    public static readonly int ROTATE_B_ANIM = Animator.StringToHash("Rotate_B");
    public static readonly int IDLE_ANIM = Animator.StringToHash("Idle");

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
        Arrival
    }

    // private enum InPath
    // {
    //     Start,
    //     Arrival
    // }

    private enum LoopPath
    {
        Start,
        Mid1,
        Mid2,
        Mid3,
        End
    }

    public Vector3[] circularPath { get; private set; }
    public Vector3[] linearPath { get; private set; }


    private Vector3 _pivotPoint;

    private readonly string _crabPrefabPath = "게임별분류/비디오컨텐츠/Crab/";


    private Stack<Crab> _inactiveCrabPool;
    private Queue<Crab> _activeCrabPool;
    private Animator _currentAnimator;


    [FormerlySerializedAs("startPoints")] [Header("Start Points")]
    public Transform[] appearablePoints;

    [Header("Offsets")] public float startOffset;
    public float midOffset;
    public float endOffset;
    [Space(10f)] public float linearPathDistance;


    private Vector3 main;
    private Vector3 start;
    private Vector3 mid1;
    private Vector3 mid2;
    private Vector3 mid3;
    private Vector3 end;

    private Vector3 start_Linear;
    private Vector3 end_Linear;

    // private Vector3[] inPath; DoMove로 대체
    private Vector3[] awayPath;

    //경로의 종류에 따라 애니메이션을 결정하기 위한 bool 연산자 선언
    //false인경우 원형루프, true인 경우 선형 루프이며, Idle 애니메이션 재생합니다. 
    private bool _isLinearPath;
    private bool _isNoPath;

    public Transform lookAtTarget;

    [FormerlySerializedAs("randomLoopRange")] [Range(0, 5)]
    public float randomDistance;


    private void Start()
    {
        if (!isInit) Init();
    }

    private bool isInit;

    private void Init()
    {
        DOTween.Init().SetCapacity(100, 200);
        _inactiveCrabPool = new Stack<Crab>();
        _activeCrabPool = new Queue<Crab>();


        linearPath = new Vector3[2];
        circularPath = new Vector3[5];
        awayPath = new Vector3[1];
        // inPath = new Vector3[2];
        _distancesFromStartPoint = new float[4];

        EffectManager.OnClickInEffectManager -= CrabOnClicked;
        EffectManager.OnClickInEffectManager += CrabOnClicked;

        CrabVideoGameManager.onCrabAppear -= OnCrabAppear;
        CrabVideoGameManager.onCrabAppear += OnCrabAppear;

        SetPool(_inactiveCrabPool, "CrabA");
        SetPool(_inactiveCrabPool, "CrabB");
        SetPool(_inactiveCrabPool, "CrabC");
        SetPool(_inactiveCrabPool, "CrabD");
        SetPool(_inactiveCrabPool, "CrabE");
        SetPool(_inactiveCrabPool, "CrabF");
        SetPool(_inactiveCrabPool, "CrabG");
        SetPool(_inactiveCrabPool, "CrabH");
        SetPool(_inactiveCrabPool, "CrabI");
        SetPool(_inactiveCrabPool, "CrabJ");
        SetPool(_inactiveCrabPool, "CrabK");
        SetPool(_inactiveCrabPool, "CrabL");
        


        isInit = true;
    }

    private void OnCrabAppear()
    {
        foreach (var crab in _activeCrabPool)
        {
            //시퀀스 중단으로 인해, AwayPath.Start를 한번더 할당해줘야 합니다. 
            //crab.awayPath[(int)AwayPath.start] = crab.gameObj.transform.position;
            UpdateDistanceFromStartPointToClickedPoint(crab);
            SendCrabHome(crab);
        }
    }

    /// <summary>
    ///     영상이 나오면 크랩의 시퀀스를 모두 멈추고 나타나는 위치로 돌려보냅니다.
    ///     경로 설정에 나오는 3rd 시퀀스와 로 동일합니다.
    /// </summary>
    /// <param name="_crabDoingPath"></param>
    private void SendCrabHome(Crab _crabDoingPath)
    {
        DeactivateAnim(_crabDoingPath.gameObj.GetComponent<Animator>());

        if (_crabDoingPath.currentSequence != null && _crabDoingPath.currentSequence.IsActive())
            _crabDoingPath.currentSequence.Kill();

        StartCoroutine(CheckSequenceKilled(_crabDoingPath.currentSequence));

        _crabDoingPath.currentSequence = DOTween.Sequence();


// 세 번째 트윈: 집으로 돌아가기
        _crabDoingPath.currentSequence.Append(_crabDoingPath.gameObj.transform
            .DOMove(_crabDoingPath.awayPath[0], 3f)
            .OnStart(() =>
            {
                DeactivateAnim(_crabDoingPath.animator);
// #if UNITY_EDITOR
//                 Debug.Log($"{_crabDoingPath.awayPath[0]}, {_crabDoingPath.awayPath[(int)AwayPath.Arrival]} 꽃게 집에보내기");
// #endif
            })
            .OnComplete(() =>
            {
                _crabDoingPath.gameObj.SetActive(false);
                _inactiveCrabPool.Push(_crabDoingPath);

                _crabDoingPath.isPathSet = false;
                _crabDoingPath.isGoingHome = false;
            }));

// 시퀀스 재생
        _crabDoingPath.currentSequence.Play();
    }

    private void OnDestroy()
    {
        EffectManager.OnClickInEffectManager -= CrabOnClicked;
    }


    private void CrabOnClicked()
    {
        if (!isInit) return;

        if (videoGameManager.isCrabAppearable) DoPathToClickPoint();


#if UNITY_EDITOR

#endif
    }


    private int _closestStartPointIndex;
    private float[] _distancesFromStartPoint;

    /// <summary>
    ///     4개의 Start 포인트에서 가장 가까운 생성지점이 어딘지 매 클릭시 업데이트 합니다.
    /// </summary>
    private void UpdateDistanceFromStartPointToClickedPoint(Crab crabBeforeMoving)
    {
        for (var i = (int)StartDirection.Upper_Left; i < appearablePoints.Length; ++i)
            _distancesFromStartPoint[i] =
                Vector2.Distance(crabEffectManager.currentHitPoint, appearablePoints[i].position);

        _closestStartPointIndex = (int)StartDirection.Upper_Left;

        for (var section = (int)StartDirection.Upper_Right; section < (int)StartDirection.Bottom_Left; section++)
            if (_distancesFromStartPoint[section] > _distancesFromStartPoint[_closestStartPointIndex])
                _closestStartPointIndex = section;
        // if (_distancesFromStartPoint[section] < _distancesFromStartPoint[_closestStartPointIndex])
        // {
        //     
        //     crabBeforeMoving.awayPath[(int)AwayPath.Arrival] = appearablePoints[Random.Range(0, 4)].position;
        //     
        //
        // }
        crabBeforeMoving.awayPath[(int)AwayPath.Arrival] = appearablePoints[Random.Range(0, 4)].position;
// #if UNITY_EDITOR
//         Debug.Log($"{crabBeforeMoving.gameObj.name}돌아갈 위치 {crabBeforeMoving.awayPath[(int)AwayPath.Arrival]}");
// #endif
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
            var crab = _activeCrabPool.Dequeue();

            if (!crab.isGoingHome && crab.isPathSet)
            {
                SetAndPlayPath(crab);

                PushIntoActivePool(crab);
            }
        }
    }

    [Header("duration Setting")] public float pathDuration;
    public float pathDurationOnSec;
    private float _duration;

    private void SetAndPlayPath(Crab _crabDoingPath)
    {
        SetInAndLoopPath(_crabDoingPath);
        CopyPathArrays(_crabDoingPath);
        _duration = _crabDoingPath.isPathSet ? pathDurationOnSec : pathDuration;

        if (_crabDoingPath.isPathSet == false)
        {
            _crabDoingPath.gameObj.transform.position = appearablePoints[_closestStartPointIndex].position;
            //inPath[(int)InPath.Start] = appearablePoints[_closestStartPointIndex].position;
            _crabDoingPath.isPathSet = true;
        }
        else
        {
#if UNITY_EDITOR

#endif
            // inPath[(int)InPath.Start] = _crabDoingPath.gameObj.transform.position;
        }

        PlayPath(_crabDoingPath);
    }

    private void DeactivateAnim(Animator animator)
    {
        animator.SetBool(ROLL_ANIM, false);
        animator.SetBool(ROTATE_B_ANIM, false);
        animator.SetBool(ROTATE_A_ANIM, false);
        animator.SetBool(IDLE_ANIM, false);
    }

    private void SetAnim(Animator animator, bool isNoPath = false)
    {
        var randomAnim = Random.Range(0, 4);


        switch (randomAnim)
        {
            case 0:
                animator.SetBool(ROLL_ANIM, true);
                break;
            case 1:
                animator.SetBool(ROTATE_A_ANIM, true);
                break;
            case 2:
                animator.SetBool(ROTATE_B_ANIM, true);
                break;
        }
    }


    private void PlayPath(Crab _crabDoingPath)
    {
        if (!videoGameManager.isCrabAppearable) return;
        
#if UNITY_EDITOR
//Debug.Log($"꽃게 생성 가능 여부 :  {videoGameManager.isCrabAppearable} ");
#endif
        
        UpdateDistanceFromStartPointToClickedPoint(_crabDoingPath);
        DeactivateAnim(_crabDoingPath.gameObj.GetComponent<Animator>());

        if (_crabDoingPath.currentSequence != null && _crabDoingPath.currentSequence.IsActive())
            _crabDoingPath.currentSequence.Kill();

        StartCoroutine(CheckSequenceKilled(_crabDoingPath.currentSequence));

        _crabDoingPath.currentSequence = DOTween.Sequence();


        // 첫 번째 트윈: 화면 밖에서 안으로 이동
        _crabDoingPath.currentSequence.Append(_crabDoingPath.gameObj.transform
            .DOMove(crabEffectManager.currentHitPoint + Vector3.up * Random.Range(0, 3), _crabDoingPath.gameObj.transform.localScale.x > 20 ? 5.2f : 1.8f) //큰 게 속도: 작은 게 속도
            .OnStart(() => { _crabDoingPath.gameObj.transform.DOLookAt(lookAtTarget.position, 0.01f); })
            .OnComplete(() =>
            {
                _crabDoingPath.linearPath[0] = _crabDoingPath.gameObj.transform.position;

                _crabDoingPath.awayPath[(int)AwayPath.Arrival] =
                    appearablePoints[Random.Range(0, 4)].position;
#if UNITY_EDITOR
                //   Debug.Log("첫 번째 트윈 완료");
#endif
                // _crabDoingPath.gameObj.transform.position = _crabDoingPath.loopPath[(int)LoopPath.Start];
            }));

        if (_crabDoingPath.isNoPath)
        {
            _crabDoingPath.currentSequence.Append(DOVirtual.Float(0, 0, 4.5f, val => val++).OnStart(() =>
            {
                _crabDoingPath.animator.SetBool(IDLE_ANIM, true);
#if UNITY_EDITOR
                Debug.Log("Idle: 경로없음");
#endif
            }).OnComplete(() =>
            {
                _crabDoingPath.awayPath[(int)AwayPath.Arrival] =
                    appearablePoints[Random.Range(0, 4)].position;
            }));
        }
        else
        {
            if (_crabDoingPath.isLinearPath)
                // 두 번째 트윈: 경로 따라 이동
                _crabDoingPath.currentSequence.Append(_crabDoingPath.gameObj.transform
                    .DOPath(_crabDoingPath.linearPath, 2.5f)
                    .SetLoops(8, LoopType.Yoyo)
                    .SetEase(Ease.Linear)
                    .OnStart(() =>
                    {
                        SetAnim(_crabDoingPath.animator);
#if UNITY_EDITOR
                        Debug.Log(
                            $"꽃게선형움직임 중, bool : {_crabDoingPath.isLinearPath}, (좌우)선형 경로{linearPath[0]},{linearPath[1]} ");
#endif
                    })
                    .OnComplete(() =>
                    {
                        _crabDoingPath.isGoingHome = true;
                        _crabDoingPath.awayPath[(int)AwayPath.Arrival] =
                            appearablePoints[Random.Range(0, 4)].position;
                    }));
            else
                // 두 번째 트윈: 경로 따라 이동
                _crabDoingPath.currentSequence.Append(_crabDoingPath.gameObj.transform
                    .DOPath(_crabDoingPath.circularPath, 2.5f, PathType.CatmullRom)
                    .SetLoops(8, LoopType.Yoyo)
                    .SetEase(Ease.Linear)
                    .OnStart(() =>
                    {
                        SetAnim(_crabDoingPath.animator);
#if UNITY_EDITOR
                        Debug.Log("원형 경로");
#endif
                    })
                    .OnComplete(() =>
                    {
                        _crabDoingPath.isGoingHome = true;
                        _crabDoingPath.awayPath[(int)AwayPath.Arrival] =
                            appearablePoints[Random.Range(0, 4)].position;
                    }));
        }


// 세 번째 트윈: 집으로 돌아가기
        _crabDoingPath.currentSequence.Append(_crabDoingPath.gameObj.transform
            .DOMove(_crabDoingPath.awayPath[(int)AwayPath.Arrival], 3f)
            .OnStart(() =>
            {
                DeactivateAnim(_crabDoingPath.animator);
#if UNITY_EDITOR
                Debug.Log($"{_crabDoingPath.awayPath[0]}, {_crabDoingPath.awayPath[1]} 꽃게 집에보내기");
#endif
            })
            .OnComplete(() =>
            {
                _crabDoingPath.gameObj.SetActive(false);
                _inactiveCrabPool.Push(_crabDoingPath);

                _crabDoingPath.isPathSet = false;
                _crabDoingPath.isGoingHome = false;
            }));

// 시퀀스 재생
        _crabDoingPath.currentSequence.Play();
    }


    private IEnumerator CheckSequenceKilled(Sequence sequence)
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
Debug.Log("Kill 실패: Sequence가 여전히 활성화되어 있습니다.");
#endif
        }
    }

    private void InitializeAndSetSequence(Crab crab)
    {
        if (crab.currentSequence != null && crab.currentSequence.IsActive()) DOTween.Kill(crab);

        crab.currentSequence = DOTween.Sequence();
    }

    private void SetBool(Crab crab)
    {
        crab.isLinearPath = _isLinearPath;
        crab.isNoPath = _isNoPath;
    }

    private void CopyPathArrays(Crab crab)
    {
        // crab.inPath = new Vector3[inPath.Length];
        // Array.Copy(inPath, crab.inPath, inPath.Length);

        if (crab.isLinearPath)
        {
            crab.linearPath = new Vector3[1];
            Array.Copy(linearPath, crab.linearPath, 1);
        }
        else
        {
            crab.circularPath = new Vector3[circularPath.Length];
            Array.Copy(circularPath, crab.circularPath, circularPath.Length);
        }

        crab.awayPath = new Vector3[1];
        Array.Copy(awayPath, crab.awayPath, 1);
    }

    /// <summary>
    ///     클릭시 게가 이동할 경로를 클릭하는 곳으로 이동시키는 함수 입니다.
    /// </summary>
    private enum PathName
    {
        Circular,
        Linear,
        NoPath
    }

    private void SetInAndLoopPath(Crab crab)
    {
        main = crabEffectManager.currentHitPoint;

        _isNoPath = false;
        _isLinearPath = false;

        var currentPath = Random.Range(0, 3);

        if (currentPath == (int)PathName.Circular)
        {
            start = main + Vector3.up * startOffset * Random.Range(1, randomDistance);
            mid1 = main + Vector3.forward * midOffset * Random.Range(1, randomDistance);
            mid2 = main + Vector3.down * midOffset * Random.Range(1, randomDistance);
            mid3 = main + Vector3.back * midOffset * Random.Range(1, randomDistance);

            /*           mid
             *    start       mid2
             *           end
             */

            circularPath[(int)LoopPath.Start] = start;
            circularPath[(int)LoopPath.Mid1] = mid1;
            circularPath[(int)LoopPath.Mid2] = mid2;
            circularPath[(int)LoopPath.Mid3] = mid3;
            circularPath[(int)LoopPath.End] = start;


            _isLinearPath = false;
        }
        else if (currentPath == (int)PathName.Linear)
        {
            end_Linear = main + transform.forward * linearPathDistance * Random.Range(1, randomDistance);

            linearPath[0] = end_Linear;

#if UNITY_EDITOR

#endif


            _isLinearPath = true;
        }
        else
        {
            _isNoPath = true;
        }

        SetBool(crab);
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

            crab.animator = crab.gameObj.GetComponent<Animator>();

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
        if (_inactiveCrabPool.Count > 0)
        {
            var crab = new Crab();
            crab = _inactiveCrabPool.Pop();
            crab.gameObj.SetActive(true);


            return crab;
        }

        return null;
    }

    private void PushIntoActivePool(Crab _crab)
    {
#if UNITY_EDITOR

#endif
        _activeCrabPool.Enqueue(_crab);
    }
}


public class Crab
{
    public GameObject gameObj { get; set; }

    // public Vector3[] inPath { get; set; }
    public Vector3[] circularPath { get; set; }
    public Vector3[] linearPath { get; set; }
    public Vector3[] awayPath { get; set; }

    //isPathSet은 경로 재설정 위한 bool값.
    //isInitiallySetPath는 중복여부 확인을 위한값
    public bool isPathSet { get; set; }

    public Sequence currentSequence { get; set; }

    public bool isGoingHome { get; set; }
    public bool isLinearPath { get; set; }
    public bool isNoPath { get; set; }
    public Animator animator { get; set; }
}