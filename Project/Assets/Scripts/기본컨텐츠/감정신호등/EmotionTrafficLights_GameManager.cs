using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EmotionTrafficLights_GameManager : IGameManager
{
    private enum GameObjects
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
    private Vector3[] _balloonFlyAwayPath;

    private bool _isClickable;
    private Dictionary<int, Sequence> _seqMap;
    private Dictionary<int, bool> _clickableMap;
    private Dictionary<string, int> _answerCountMap; //정답마다 터트려야할 갯수가 다르기에 이렇게 저장합니다.
    private Dictionary<int, Rigidbody> _rigidbodyMap;//미사용중 5/13/24


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
    private int _countToCollect; // 찾아야할 풍선 갯수
    private int _collectedBalloon; //찾은 풍선 갯수


    private bool _isFirstStageFinished;
    private bool _isOtherStageFinished;
    private bool _isInitializing;// 종료시점 - 씬초기화까지
  
    //---------------------------------------------- Init and Event Functions -----------------
    protected override void BindEvent()
    {
        base.BindEvent();
        FirstStageFinished -= OnFirstStageFinished;
        FirstStageFinished += OnFirstStageFinished;
        
        OtherStageFinished -= OnOtherStageFinished;
        OtherStageFinished += OnOtherStageFinished;
    }

    private void OnDestroy()
    {
        FirstStageFinished -= OnFirstStageFinished;
        OtherStageFinished -= OnOtherStageFinished;
    }



    protected override void Init()
    {
        base.Init();

        //공------------------------------
        _trafficLightMat = new Material[(int)TrafficLight.Count];
        _tlMeshRenderer = new MeshRenderer[(int)TrafficLight.Count];
        _seqMap = new Dictionary<int, Sequence>();
        _clickableMap = new Dictionary<int, bool>();
        _rigidbodyMap = new Dictionary<int, Rigidbody>();
        _balloonAwayPosition = new Vector3[2];
        _answerCountMap = new Dictionary<string, int>();
        
        _answerCountMap.Add(TrafficLight.Cry.ToString(),2);
        _answerCountMap.Add(TrafficLight.Happy.ToString(),3);
        _answerCountMap.Add(TrafficLight.Sleepy.ToString(),3);
        //_scaleSeqMap = new Dictionary<int, Sequence>();


        var ballons = transform.GetChild((int)GameObjects.EmotionalBalloons);
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
        _trafficLight = transform.GetChild((int)GameObjects.TrafficLight);

        for (var i = 0; i < (int)TrafficLight.Count; i++)
        {
            _tlMeshRenderer[i] = _trafficLight.GetChild(i).GetComponent<MeshRenderer>();
            _trafficLightMat[i] = _tlMeshRenderer[i].material;
        }

        _tlDefalutRotation = _trafficLight.rotation;
        _tlDefaultSize = _trafficLight.localScale;
        _trafficLight.localScale = Vector3.zero;
        
      
        var ballAwayPosParent = GameObject.Find("BalloonAwayPosition");
        
        var _balloonFlyAwayPathParent = GameObject.Find("BalloonFlyAwayPath").transform;

        _balloonFlyAwayPath = new Vector3[_balloonFlyAwayPathParent.childCount];
        for (int i = 0; i < _balloonFlyAwayPathParent.childCount; i++)
        {
            _balloonFlyAwayPath[i] = _balloonFlyAwayPathParent.GetChild(i).position;
        }

        
      
        for (var i = 0; i < 2; i++)
        {
            _balloonAwayPosition[i] = ballAwayPosParent.transform.GetChild(i).position;
        }

        _tlOnOffColors = new Color[2];

        _tlOnOffColors[OFF] = new Color((float)20/255, (float)20/255, (float)20/255, (float)20/255); //Dark Gray
        _tlOnOffColors[ON] = Color.white;
    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!_isClickable) return;

        
        foreach (var hit in GameManager_Hits)
        {
            var hitBalloonID = hit.transform.GetInstanceID();
            if (_clickableMap.ContainsKey(hitBalloonID) && !_clickableMap[hitBalloonID]) continue;

            if (hit.transform.gameObject.name.Contains("Balloon"))
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Hopscotch/Click_B", 0.2f);
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
        PlayBalloonAppearAnim();
    }

    private void OnOtherStageFinished()
    {
#if UNITY_EDITOR
        Debug.Log("Second Stage ReInit");
#endif
        if (_isInitializing) return;
        _isInitializing = true;
        _collectedBalloon = 0;
        _currentBalloonPopCount = 0;
        StartCoroutine(FlyAwayCo(delay:0.5f));
       
    }

    private void OnFirstStageFinished()
    {
            _trafficLight
                .DOScale(_tlDefaultSize, 1.0f)
                .SetEase(Ease.InOutBack)
                .OnComplete(() => { PlayChangeTrafficLightAnim(); })
                .SetDelay(1f);

            DOVirtual.Float(0, 0, 6.5f, _ => { })
                .OnComplete(() =>
                {
                    _collectedBalloon = 0;
                    _currentBalloonPopCount = 0;
                    RandomSetPosition();
                    PlayBalloonAppearAnim(3f);
                });
        
    }

    private IEnumerator FlyAwayCo(float delay)
    {
        yield return DOVirtual.Float(0, 0, delay, _ => { }).WaitForCompletion();
        foreach (var balloon in _balloons)
        {
            var ballId = balloon.GetInstanceID();

            //애니메이션 할당전 초기화-------
            _seqMap[ballId].Kill();
            _seqMap[ballId] = null;
        }

        foreach (var balloon in _balloons)
            if (balloon.name.Contains(_currentAnswer))
            {
#if UNITY_EDITOR
                Debug.Log("----------is Flying Away-----------");
#endif
                yield return balloon.DOPath(_balloonFlyAwayPath, 2.8f, PathType.CatmullRom).WaitForCompletion();


                var randomVec1 = Random.Range(-3f, 3f);
                randomVec1 = randomVec1 > 0 ? Mathf.Clamp(randomVec1, -3, -2) : Mathf.Clamp(randomVec1, 2, 3);

                var randomVec2 = Random.Range(-3f, 3f);
                randomVec2 = randomVec2 > 0 ? Mathf.Clamp(randomVec2, -3, -2) : Mathf.Clamp(randomVec2, 2, 3);

                yield return balloon.DOMove(balloon.position +
                                           new Vector3(randomVec1, 0, randomVec2), 0.6f).WaitForCompletion();
            }

        PopAll(); 
        yield return  DOVirtual.Float(0, 0, 2.0f, _ => { }).WaitForCompletion();
        PlayChangeTrafficLightAnim();
        
        yield return  DOVirtual.Float(0, 0, 4.5f, _ => { }).WaitForCompletion();
        RandomSetPosition();
        PlayBalloonAppearAnim(3f);
            
        
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

    private Coroutine _balloonAppearCo;

    private void PlayBalloonAppearAnim(float delay = 0.8f)
    {
        _balloonAppearCo = StartCoroutine(PlayBallAnimCo());
    }

    private IEnumerator PlayBallAnimCo(float delay = 0.05f)
    {
        _isClickable = false;
        

        foreach (var balloon in _balloons)
        {
            var ballId = balloon.GetInstanceID();

            //시퀀스 할당전 초기화-------
            if (_seqMap.ContainsKey(ballId) && _seqMap[ballId].IsActive())
            {
                _seqMap[ballId].Kill();
                _seqMap[ballId] = null;
            }


            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Hopscotch/Click_B", 0.2f);
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

        DOVirtual.Float(0, 0, 1.5f, _ => { }).OnComplete(() =>
        {

            _isInitializing = false;
            _isClickable = true;
        });
    }
    


    private void PlayChangeTrafficLightAnim()
    {
        _trafficLight.DORotateQuaternion(_tlDefalutRotation * Quaternion.Euler(-120, 0, 0), 2.0f)
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
                _countToCollect = _answerCountMap[_currentAnswer];
#if UNITY_EDITOR
                Debug.Log($" currentAnswer {_currentAnswer}");
#endif
            }
            else
            {
                _tlOnOffColors[OFF] = new Color(0.15f, 0.15f, 0.15f, 0.15f); //Dark Gray
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

        if (popCount >= BALLOON_TOTAL_COUNT && !_isFirstStageFinished)
        {
            _isFirstStageFinished = true;
            FirstStageFinished?.Invoke();
            
            _currentBalloonPopCount = 0;
            _isClickable = false;
        }
    }


    private void OnCorrectBallon(Transform ball, int hitBalloonID)
    {
        _collectedBalloon++;
        StartCoroutine(OnCorrectBallonClickedCo(ball, hitBalloonID));
    }


    /// <summary>
    ///     정답에 해당되는 풍선의 경우, 터지지않고, 위쪽으로 힘을받으며 올라갈 수 있도록 합니다.
    /// </summary>
    /// <returns></returns>
    private int _posIndex;

    private IEnumerator OnCorrectBallonClickedCo(Transform ball, int hitBalloonID)
    {
        _currentBalloonPopCount++;
        _clickableMap[hitBalloonID] = false;

        _seqMap[hitBalloonID].Kill();
        _seqMap[hitBalloonID] = null;

        var moveUpSeq = DOTween.Sequence();

        moveUpSeq.Append(ball
                .DOMove(_balloonAwayPosition
                [_posIndex++ % (int)BalloonAwayPosition.Count] +
                new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)),
                1.3f)
                .SetEase((Ease)Random.Range((int)Ease.InSine, (int)Ease.InOutSine)))
            .WaitForCompletion();

        _seqMap[hitBalloonID] = moveUpSeq;
        // 비동기 처리로 인한 PopCount 중복Writing 방지를 위한 변수선언 입니다.
        // 예를들어 빠르게 클릭하는 경우 popCount가 아래로직에서 더 높은 숫자로 비교되는 것을 방지하기 위함입니다.
        var popCount = _currentBalloonPopCount;
        if (_collectedBalloon >= _countToCollect && _isFirstStageFinished)
        {
            Managers.Sound.Play(SoundManager.Sound.Effect,
                "Audio/Gamemaster Audio - Fun Casual Sounds/Collectibles_Items_Powerup/collect_item_jingle_04", 0.3f);
        }
        yield return DOVirtual.Float(0, 0, 2.5f, _ => { }).WaitForCompletion();

        if (popCount >= BALLOON_TOTAL_COUNT || _collectedBalloon > _countToCollect && !_isFirstStageFinished)
        {
            FirstStageFinished?.Invoke();
        }
        else
        {
            OtherStageFinished?.Invoke();
           
        }

        _isClickable = false;
        yield return null;
    }

    private void PopAll()
    {
        foreach (var balloon in _balloons)
        {
            var ballId = balloon.GetInstanceID();

            //시퀀스 할당전 초기화-------
            _seqMap[ballId].Kill();
            _seqMap[ballId] = null;
            


            balloon.DOScale(Vector3.zero, Random.Range(0.12f, 0.35f))
                .SetDelay(Random.Range(0.2f, 0.3f)).SetDelay(Random.Range(1.1f, 1.4f));
        }
    }
}