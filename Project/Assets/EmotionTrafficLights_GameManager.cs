using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EmotionTrafficLights_GameManager : IGameManager
{
    private enum GameObject
    {
        EmotionalBalloons,
        TrafficLight
    }

    private enum TrafficLight
    {
        Happy, //Red
        Sleepy, //Yellow
        Cry, //Blue 
        Count
    }

    private enum BalloonAwayPosition
    {
        Left,
        Right,
        Count
    }


    private Vector3[][] _ballonDefaultPositions; //2행 4열로 구성합니다.
    private readonly int COLUMN_COUNT = 4;
    private readonly int ROW_COUNT = 2;
    private Transform[] _balloons;
    private Vector3 _balloonDefalutScale;
    private Vector3[] _balloonAwayPosition;

    private bool _isClickable;
    private Dictionary<int, Sequence> _seqMap;
    private Dictionary<int, bool> _clickableMap;
    private Dictionary<int, Rigidbody> _rigidbodyMap;


    //TrafficLight----
    private Transform _trafficLight;
    private Vector3 _tlDefaultSize;
    private Quaternion _tlDefalutRotation;
    private Material[] _trafficLightMat;
    private MeshRenderer[] _tlMeshRenderer;

    private Color[] _tlOnOffColors;
    private readonly int OFF = 0;
    private readonly int ON = 1;

    public static event Action FirstStageFinished;
    public static event Action OtherStageFinished;
    private int _currentBalloonPopCount;
    private readonly int BALLOON_TOTAL_COUNT = 8;

    private string _currentAnswer = "Balloon"; //


    private bool _isFirstStageFinished;
    private bool _isOtherStageFinished;

    //---------------------------------------------- Init and Event Functions -----------------
    protected override void BindEvent()
    {
        base.BindEvent();
        FirstStageFinished -= OnStageFinished;
        FirstStageFinished += OnStageFinished;
    }

    private void OnDestroy()
    {
        FirstStageFinished -= OnStageFinished;
    }

    protected override void Init()
    {
        base.Init();

        _balloonAwayPosition = new Vector3[(int)BalloonAwayPosition.Count];
        var ballAwayPosParent = UnityEngine.GameObject.Find("BalloonAwayPosition").transform;

        for (var i = (int)BalloonAwayPosition.Left; i < (int)BalloonAwayPosition.Count; i++)
            _balloonAwayPosition[i] = ballAwayPosParent.GetChild(i).position;


        _tlOnOffColors = new Color[2];

        _tlOnOffColors[OFF] = new Color(60, 60, 60, 30); //Dark Gray
        _tlOnOffColors[ON] = Color.white;

        //공------------------------------
        _trafficLightMat = new Material[(int)TrafficLight.Count];
        _tlMeshRenderer = new MeshRenderer[(int)TrafficLight.Count];
        _seqMap = new Dictionary<int, Sequence>();
        _clickableMap = new Dictionary<int, bool>();
        _rigidbodyMap = new Dictionary<int, Rigidbody>();
        //_scaleSeqMap = new Dictionary<int, Sequence>();


        var ballons = transform.GetChild((int)GameObject.EmotionalBalloons);
        _balloons = new Transform[ballons.childCount];


        for (var i = 0; i < ballons.childCount; i++)
        {
            _balloons[i] = ballons.GetChild(i);
            var ballonID = _balloons[i].GetInstanceID();
            _clickableMap.Add(ballonID, false);
            _rigidbodyMap.Add(ballonID, _balloons[i].GetComponent<Rigidbody>());
        }


        _ballonDefaultPositions = new Vector3[ROW_COUNT][];
        _ballonDefaultPositions[0] = new Vector3[COLUMN_COUNT];
        _ballonDefaultPositions[1] = new Vector3[COLUMN_COUNT];


        var count = 0;
        _balloonDefalutScale = _balloons[0].localScale;
        for (var i = 0; i < ROW_COUNT; i++)
        for (var k = 0; k < COLUMN_COUNT; k++)
        {
            _ballonDefaultPositions[i][k] = _balloons[count].position;

            count++;
        }


        foreach (var balloon in _balloons) balloon.localScale = Vector3.zero;


        //신호등------------------------------
        _trafficLight = transform.GetChild((int)GameObject.TrafficLight);

        for (var i = 0; i < (int)TrafficLight.Count; i++)
        {
            _tlMeshRenderer[i] = _trafficLight.GetChild(i).GetComponent<MeshRenderer>();
            _trafficLightMat[i] = _tlMeshRenderer[i].material;
        }

        _tlDefalutRotation = _trafficLight.rotation;
        _tlDefaultSize = _trafficLight.localScale;
        _trafficLight.localScale = Vector3.zero;
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();

        if (!_isClickable) return;

        foreach (var hit in GameManager_Hits)
        {
            var hitBalloonID = hit.transform.GetInstanceID();
            if (_clickableMap.ContainsKey(hitBalloonID) && !_clickableMap[hitBalloonID]) continue;

            if (hit.transform.gameObject.name.Contains("Balloon"))
            {
                // fistStage 제외 나머지  플레이상황인경우
                if (_isFirstStageFinished)
                {
                    if (hit.transform.gameObject.name.Contains(_currentAnswer))
                        OnCorrectBallon(hit.transform, hitBalloonID);
                    else
                        OnWrongBalloonClicked(hit.transform, hitBalloonID);
                }
                // fistStage 인경우
                else
                {
                    OnWrongBalloonClicked(hit.transform, hitBalloonID);
                }
            }
        }
    }

    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();
        PlayBalloonAnim();
    }


    private void OnStageFinished()
    {
#if UNITY_EDITOR
        Debug.Log("ReInit");
#endif
        PopAll();
        _trafficLight
            .DOScale(_tlDefaultSize, 1.0f)
            .SetEase(Ease.InOutBack)
            .OnComplete(() => { PlayChangeTrafficLightAnim(); })
            .SetDelay(1f);

        DOVirtual.Float(0, 0, 6.5f, _ => { })
            .OnComplete(() =>
            {
                RandomSetPosition();
                PlayBalloonAnim(3f);
            });
    }

    //---------------------------------------------- other methods  -------------------------------------
    private void RandomSetPosition()
    {
        Utils.Shuffle(_balloons);

        var count = 0;
        for (var i = 0; i < ROW_COUNT; i++)
        for (var k = 0; k < COLUMN_COUNT; k++)
        {
            _balloons[count].position = _ballonDefaultPositions[i][k];
            count++;
        }
    }

    private void PlayBalloonAnim(float delay = 0.8f)
    {
        StartCoroutine(PlayBallAnimCo());
    }

    private IEnumerator PlayBallAnimCo(float delay = 0.15f)
    {
        _isClickable = false;
        if (_isFirstStageFinished) _isOtherStageFinished = false;

        foreach (var balloon in _balloons)
        {
            var ballId = balloon.GetInstanceID();

            //시퀀스 할당전 초기화-------
            if (_seqMap.ContainsKey(ballId) && _seqMap[ballId].IsActive())
            {
#if UNITY_EDITOR
                Debug.Log("Kill");
#endif
                _seqMap[ballId].Kill();
                _seqMap[ballId] = null;
            }


            yield return balloon.DOScale(_balloonDefalutScale, Random.Range(0.02f, 0.15f))
                .SetDelay(Random.Range(delay, delay + 0.3f)).WaitForCompletion();
        }

        foreach (var balloon in _balloons)
        {
            var ballId = balloon.GetInstanceID();


            //클릭가능 위한 콜라이더 설정-------
            _clickableMap[ballId] = true;

            //랜덤이동 경로설정
            var randomPath = new Vector3[3];
            randomPath[0] = balloon.position;
            randomPath[1] = balloon.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f));
            randomPath[2] = balloon.position;

            //시퀀스 구성-------
            var balloonSeq = DOTween.Sequence();

            balloonSeq.Append(balloon.DOPath(randomPath, 3f));
            balloonSeq.Append(balloon.DOShakeRotation(0.5f, 10, 3));
            balloonSeq.SetLoops(100, LoopType.Yoyo);

            _seqMap[ballId] = balloonSeq;
        }

        DOVirtual.Float(0, 0, 0.5f, _ => { }).OnComplete(() =>
        {
#if UNITY_EDITOR
            Debug.Log("----------is Clickable-----------");
#endif
            _isClickable = true;
        });
    }

    private void PlayChangeTrafficLightAnim()
    {
        _trafficLight.DORotateQuaternion(_tlDefalutRotation * Quaternion.Euler(120, 0, 0), 2.8f)
            .SetEase(Ease.InOutBounce)
            .OnComplete(() =>
            {
                ChangeTrafficLight();
                _trafficLight.DORotateQuaternion(_tlDefalutRotation, 1.5f).SetEase(Ease.OutBounce).SetDelay(1f);
            })
            .SetDelay(1f);
    }

    private void ChangeTrafficLight()
    {
        Utils.Shuffle(_tlMeshRenderer);
        _isFirstStageFinished = true;
        _posIndex = Random.Range((int)BalloonAwayPosition.Left, (int)BalloonAwayPosition.Count);
        
        for (var i = 0; i < _tlMeshRenderer.Length; i++)
            if (i == 0)
            {
                _trafficLightMat[0].color = _tlOnOffColors[ON];
                _tlMeshRenderer[0].material.color = _trafficLightMat[i].color;
                _currentAnswer = _tlMeshRenderer[0].transform.gameObject.name;
#if UNITY_EDITOR
                Debug.Log($" currentAnswer {_currentAnswer}");
#endif
            }
            else
            {
                _tlOnOffColors[OFF] = new Color(0.45f, 0.45f, 0.45f, 0.45f); //Dark Gray
                _tlMeshRenderer[i].material.color = _tlOnOffColors[OFF];
            }
    }


    private void OnWrongBalloonClicked(Transform ballTransform, int hitBalloonID)
    {
        StartCoroutine(OnWrongBallClickedCo(ballTransform, hitBalloonID));
    }

    private IEnumerator OnWrongBallClickedCo(Transform ballTransform, int hitBalloonID)
    {
        _currentBalloonPopCount++;
        _clickableMap[hitBalloonID] = false;
        _seqMap[hitBalloonID].Kill(this);
        ballTransform.DOScale(Vector3.zero, 0.057f).SetEase(Ease.InBack);


        var popCount = _currentBalloonPopCount;
        yield return DOVirtual.Float(0, 0, 2f, _ => { }).WaitForCompletion();

        if (popCount >= BALLOON_TOTAL_COUNT)
        {
            FirstStageFinished?.Invoke();
            _currentBalloonPopCount = 0;
            _isClickable = false;
          
        }
    }


    private void OnCorrectBallon(Transform ball, int hitBalloonID)
    {
        StartCoroutine(KeepBalloonMovingCo(ball, hitBalloonID));
    }


    /// <summary>
    ///     정답에 해당되는 풍선의 경우, 터지지않고, 위쪽으로 힘을받으며 올라갈 수 있도록 합니다.
    /// </summary>
    /// <returns></returns>
    private int _posIndex;
    private IEnumerator KeepBalloonMovingCo(Transform ball, int hitBalloonID)
    {
        _currentBalloonPopCount++;
        _clickableMap[hitBalloonID] = false;

        _seqMap[hitBalloonID].Kill();
        _seqMap[hitBalloonID] = null;

        var moveUpSeq = DOTween.Sequence();
      
        moveUpSeq.Append(ball
                .DOMove(_balloonAwayPosition
                             [(_posIndex++) % (int)BalloonAwayPosition.Count]+
                        new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)),
                    1.3f)
                .SetEase((Ease)Random.Range((int)Ease.InSine, (int)Ease.InOutSine)))
            .WaitForCompletion();

        _seqMap[hitBalloonID] = moveUpSeq;
        // 비동기 처리로 인한 PopCount 중복Writing 방지를 위한 변수선언 입니다.
        // 예를들어 빠르게 클릭하는 경우 popCount가 아래로직에서 더 높은 숫자로 비교되는 것을 방지하기 위함입니다.
        var popCount = _currentBalloonPopCount;
        yield return DOVirtual.Float(0, 0, 2f, _ => { }).WaitForCompletion();

        if (popCount >= BALLOON_TOTAL_COUNT)
        {
            FirstStageFinished?.Invoke();
            _currentBalloonPopCount = 0;
            _isOtherStageFinished = true;
            _isClickable = false;
        }

        yield return null;
        // while (!_isOtherStageFinished)
        // {
        //     _rigidbodyMap[hitBalloonID].AddForce(Vector3.forward * 2f,ForceMode.Acceleration);
        //     yield return null;
        //     yield return DOVirtual.Float(0, 0, 0.5f,_ => { }).WaitForCompletion();
        // }
    }

    private void PopAll()
    {
        foreach (var balloon in _balloons)
        {
            var ballId = balloon.GetInstanceID();

            //시퀀스 할당전 초기화-------
            if (_seqMap.ContainsKey(ballId) && _seqMap[ballId].IsActive())
            {
#if UNITY_EDITOR
                Debug.Log("Kill");
#endif
                _seqMap[ballId].Kill();
                _seqMap[ballId] = null;
            }


            balloon.DOScale(Vector3.zero, Random.Range(0.12f, 0.35f))
                .SetDelay(Random.Range(0.2f, 0.3f)).SetDelay(Random.Range(1.1f, 1.4f));
        }
    }
}