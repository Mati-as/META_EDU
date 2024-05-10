using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

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


    private Vector3[][] _ballonPositions; //2행 4열로 구성합니다.
    private Transform[] _balloons;
    private Vector3 _balloonDefalutScale;

    private bool _isClickable;
    private Dictionary<int, Sequence> _seqMap;
    private Dictionary<int, Collider> _colliderMap;
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
    
    private string _currentAnswer = "Balloon"; // 1차플레이 이후 각 풍선명으로 동적으로 변경합니다.
    private static readonly int BASE_COLOR = Shader.PropertyToID("_BaseColor");


    private bool _isFirstStageFinished;
    private bool _isOtherStageFinished;

    //---------------------------------------------- Init and Event Functions -----------------
    protected override void BindEvent()
    {
        base.BindEvent();
        FirstStageFinished -= OnFirstStageFinished;
        FirstStageFinished += OnFirstStageFinished;
    }

    private void OnDestroy()
    {
        FirstStageFinished -= OnFirstStageFinished;
    }

    protected override void Init()
    {
        base.Init();
        _tlOnOffColors = new Color[2];
        
        _tlOnOffColors[OFF] = new Color(60, 60, 60, 30); //Dark Gray
        _tlOnOffColors[ON] = Color.white;

        //공------------------------------
        _trafficLightMat = new Material[(int)TrafficLight.Count];
        _tlMeshRenderer = new MeshRenderer[(int)TrafficLight.Count];
        _seqMap = new Dictionary<int, Sequence>();
        _colliderMap = new Dictionary<int, Collider>();
        _rigidbodyMap = new Dictionary<int, Rigidbody>();
        //_scaleSeqMap = new Dictionary<int, Sequence>();

        _ballonPositions = new Vector3[2][];
        _ballonPositions[0] = new Vector3[4];
        _ballonPositions[1] = new Vector3[4];


        var ballons = transform.GetChild((int)GameObject.EmotionalBalloons);
        _balloons = new Transform[ballons.childCount];


        for (var i = 0; i < ballons.childCount; i++)
        {
            _balloons[i] = ballons.GetChild(i);
            var ballonID = _balloons[i].GetInstanceID();
            _colliderMap.Add(ballonID,_balloons[i].GetComponent<Collider>());
            _rigidbodyMap.Add(ballonID,_balloons[i].GetComponent<Rigidbody>());
        }
        _balloonDefalutScale = _balloons[0].localScale;
        RandomSetPosition();

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

    private void PlayBalloonAnim()
    {
        _isClickable = false;
        foreach (var balloon in _balloons)
        {
            //시퀀스 할당전 초기화-------
            var ballId = balloon.GetInstanceID();
            //클릭가능 위한 콜라이더 설정-------
            _colliderMap[ballId].enabled = true;
            
            balloon.DOScale(_balloonDefalutScale, Random.Range(0.7f, 1f)).SetDelay(Random.Range(0f, 1.0f)).OnComplete(
                () =>
                {
                  
                    if (_seqMap.ContainsKey(ballId) && _seqMap[ballId].IsActive())
                    {
                        _seqMap[ballId].Kill();
                        _seqMap[ballId] = null;
                    }

                    //시퀀스 구성-------
                    var moveSeq = DOTween.Sequence();
                    var randomPath = new Vector3[3];
                    randomPath[0] = balloon.position;
                    randomPath[1] = balloon.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),
                        Random.Range(-0.1f, 0.1f));
                    randomPath[2] = balloon.position;

                    moveSeq.Append(balloon.DOPath(randomPath, 3f));
                    moveSeq.Append(balloon.DOShakeRotation(0.5f, 10, 3));
                    moveSeq.SetLoops(-1, LoopType.Yoyo);
                    _seqMap.Add(ballId, moveSeq);

              
                });

        }
          
        DOVirtual.Float(0, 0, 2.5f, _ => { }).OnComplete(() =>
        {
#if UNITY_EDITOR
            Debug.Log("Now Clickble-----------");
#endif
            _isClickable = true;
        });

    }


    private void OnFirstStageFinished()
    {
        _trafficLight
            .DOScale(_tlDefaultSize, 1.5f)
            .SetEase(Ease.InOutBack)
            .OnComplete(() =>
        {
            PlayChangeTrafficLightAnim();
        });

        DOVirtual.Float(0, 0, 5f, _ => { }).OnComplete(() =>
        {
            PlayBalloonAnim();
        });
      
      
    }

    //---------------------------------------------- other methods  ----------------------------


    private void RandomSetPosition()
    {
        Utils.Shuffle(_balloons);
        

        for (var i = 0; _ballonPositions.Length < 2; i++)
        for (var k = 0; k < _ballonPositions[0].Length; k++)
            _balloons[i].position = _ballonPositions[i][k];
    }

    private void PlayChangeTrafficLightAnim()
    {
        _trafficLight.DORotateQuaternion(_tlDefalutRotation * Quaternion.Euler(120, 0, 0), 2.8f)
            .SetEase(Ease.InOutBounce)
            .OnComplete(() =>
            {
                ChangeTrafficLight();
                _trafficLight.DORotateQuaternion( _tlDefalutRotation , 1.5f).SetEase(Ease.OutBounce).SetDelay(1f);
            })
            .SetDelay(1f);
    }

    private void ChangeTrafficLight()
    {
        Utils.Shuffle(_tlMeshRenderer);
        _isFirstStageFinished = true;
        
        for (var i = 0; i < _tlMeshRenderer.Length; i++)
        {
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
    }


    private void OnWrongBalloonClicked(Transform ballTransform,int hitBalloonID)
    {
        _currentBalloonPopCount++;
        _colliderMap[hitBalloonID].enabled = false;
        _seqMap[hitBalloonID].Kill();
        ballTransform.DOScale(Vector3.zero, 0.067f).SetEase(Ease.InBack);

        if (_currentBalloonPopCount >= BALLOON_TOTAL_COUNT)
        {
            FirstStageFinished?.Invoke();
            _currentBalloonPopCount = 0;
            _isClickable = false;
        }
    }
    private void OnCorrectBallon(Transform ballTransform,int hitBalloonID)
    {
        
        StartCoroutine(KeepBalloonMovingCo(ballTransform,hitBalloonID));
    }

    
    /// <summary>
    /// 정답에 해당되는 풍선의 경우, 터지지않고, 위쪽으로 힘을받으며 올라갈 수 있도록 합니다.
    /// </summary>
    /// <returns></returns>
    IEnumerator KeepBalloonMovingCo(Transform ballTransform,int hitBalloonID)
    {

        _currentBalloonPopCount++;
        _colliderMap[hitBalloonID].enabled = false;
        _seqMap[hitBalloonID]?.Kill(true);
        _seqMap[hitBalloonID] = null;
        
        while (!_isOtherStageFinished)
        {
#if UNITY_EDITOR
            Debug.Log(" Correct Balloon Moving......");
#endif
            _rigidbodyMap[hitBalloonID].AddForce(Vector3.back * 2f,ForceMode.Acceleration);
            yield return null;
            yield return DOVirtual.Float(0, 0, 0.5f,_ => { }).WaitForCompletion();
        }
        
    }
 
}