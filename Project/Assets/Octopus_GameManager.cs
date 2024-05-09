using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Octopus_GameManager : IGameManager
{
    // 신발색은 고정, 왼쪽부터 오른쪽순서입니다. 
    private enum Shoe
    {
        Navy,
        Purple,
        Orange,
        Yellow,
        Green,
        Count
    }

    private string _currentAnswer = "NONE!@#"; // 초기정답은 없는것으로 처리, string.Contains사용하므로 string.empty사용하지 않습니다.
    private int _currentAnserIndex = -1;

    private Queue<Transform> _ballsQue;
    private Queue<Transform> _ballsDequeued; // dequeue된 ball  별도 참조 저장용
    private Dictionary<int, Collider> _ballColliderMap;
    private Dictionary<int, Rigidbody> _rigidbodies;
    private Vector3[] _enterPath;
    private Transform _octopusBody;
    private Transform[] _shoes; // 플레이로직에 활용되지 않으며, Shake애니메이션용입니다. 
    private Transform[] _shoeMuzzles; // 신발에서 공이나올때 뚜껑처럼 열리는 부분입니다.
    private Dictionary<int, Quaternion> _muzzleDefaultRotationMap;
    private Vector3[] _defaultLocations;// 공을 한칸씩 움직이게끔 하기 위한 초기위치 참조 
    private Dictionary<int, Vector3> _defaultLocationMap; // 게임 플레이 재초기화시 참조용
    private readonly Vector3 _ballPopPositionOffset = new(0, -0f, 0); // 공 찾은경우 나오는 위치 보정값 입니다. 
    private Dictionary<int, int> _currentOrderMap; //공을 한칸씩 움직일때, 현재 인덱스 참조용

    private Transform _currentBall;
    private int _currentBallCount;

    private Vector3 _ballDefaultSize;
    
    private readonly float _sizeDecreaseAmount = 0.3f;
    private readonly int IS_VANISHED = -1;
    
    private bool _isDoingAnimation;
    private bool _isBallClickable = true; // 공이 관을타고 문어로 들어갈 수 있는 애니메이션이 발동할 수 있는경우
    private bool _isInit; //중복초기화 방지..
    
    public static event Action OnReinit;
    
    protected override void BindEvent()
    {
        base.BindEvent();
        
        OnReinit -= OnReInit;
        OnReinit += OnReInit;
    }

    protected void OnDestroy()
    {
        OnReinit -= OnReInit;
    }

    private void OnCorrect()
    {
        
        _isDoingAnimation = true;

        var CurrentBallId = _currentBall.GetInstanceID();
        _currentBall.localScale = Vector3.zero;
        Shoe shoeType = (Shoe)Enum.Parse(typeof(Shoe), _currentAnswer, true);
        _currentAnserIndex = (int)shoeType;
        
        _currentBall.position = _shoeMuzzles[_currentAnserIndex].position + _ballPopPositionOffset;
        _currentBall.localScale = Vector3.zero;
        _currentBall.DOScale(_ballDefaultSize, 1.5f).SetEase(Ease.OutBounce);
        _shoeMuzzles[_currentAnserIndex]
            .DORotateQuaternion(_muzzleDefaultRotationMap[_shoeMuzzles[_currentAnserIndex].GetInstanceID()]
                                * Quaternion.Euler(-90f, 0f, 0f), 0.8f).OnComplete(() =>
            {
                _shoeMuzzles[_currentAnserIndex]
                    .DORotateQuaternion(
                        _muzzleDefaultRotationMap[_shoeMuzzles[_currentAnserIndex].GetInstanceID()], 0.8f).SetEase(Ease.OutBounce);;
            });
#if UNITY_EDITOR
        Debug.Log($"정답! : {_currentAnserIndex } string{_currentAnswer}");
#endif
        if (_ballColliderMap.TryGetValue(CurrentBallId, out var col)) col.enabled = true;
        if (_rigidbodies.TryGetValue(CurrentBallId, out var rigid)) rigid.constraints = RigidbodyConstraints.None;
       
        MoveBallsOneSpace();
    }



    protected override void Init()
    {
        base.Init();
       
        
        
        _ballsQue = new Queue<Transform>();
        _ballsDequeued = new Queue<Transform>();
        _ballColliderMap = new Dictionary<int, Collider>();
        _rigidbodies = new Dictionary<int, Rigidbody>();
        _shoes = new Transform[(int)Shoe.Count];
        _shoeMuzzles = new Transform[(int)Shoe.Count];
        _currentOrderMap = new Dictionary<int, int>();
        _muzzleDefaultRotationMap = new Dictionary<int, Quaternion>();
        _defaultLocationMap = new Dictionary<int, Vector3>();
        
        _octopusBody = GameObject.Find("OctopusBody").transform;
        Debug.Log($"{_octopusBody.transform.name}");
        
        
        //ball초기화
        var ballsParent = GameObject.Find("Balls").transform;
        var childCount = ballsParent.childCount;
        _defaultLocations = new Vector3[childCount];
        _currentBallCount = childCount;
        for (var i = 0; i < _currentBallCount; i++)
        {
            var ball = ballsParent.GetChild(i);

            _ballDefaultSize = ball.localScale;
            _ballsQue.Enqueue(ball);

            var collider = ball.GetComponent<Collider>();
            var id = ball.GetInstanceID();
            var ballDefaultPos = ball.position;
            
            collider.enabled = false;
            _ballColliderMap.Add(id, collider);
            _currentOrderMap.Add(id, i);
            _rigidbodies.Add(id, ball.GetComponent<Rigidbody>());
            _defaultLocations[i] = ballDefaultPos;
            _defaultLocationMap.Add(id,ballDefaultPos);
        }

        var enterPathParent = transform.Find("BallEnterPath").transform;
        _enterPath = new Vector3[enterPathParent.childCount];

        for (var i = 0; i < enterPathParent.childCount; i++) _enterPath[i] = enterPathParent.GetChild(i).position;

        var shoeParent = GameObject.Find("Shoes").transform;

        for (var i = 0; i < (int)Shoe.Count; i++)
        {
            _shoes[i] = shoeParent.GetChild(i);
            _shoeMuzzles[i] = shoeParent.GetChild(i).GetChild(0);
            _muzzleDefaultRotationMap.Add(_shoeMuzzles[i].GetInstanceID(), _shoeMuzzles[i].rotation);
        }

        EnableClickableCollider();
    }
    
    
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!isStartButtonClicked) return;
        if (_isDoingAnimation) return;

        foreach (var hit in GameManager_Hits)
        {
            var clickedId = hit.transform.GetInstanceID();


            if (hit.transform.gameObject.name.Contains("ball"))
            {
                DoEnterPath(hit.transform, _enterPath);
                _ballColliderMap[clickedId].enabled = false;
                return;
            }


            if (hit.transform.gameObject.name.Contains(_currentAnswer))
            {
                if (!_isBallClickable) return;
                _isBallClickable = false;
                OnCorrect();


              
            }
        }
    }

    private bool RoundInitCheckAndInvoke()
    {
        var isInited = false;
        
        DOVirtual.Float(0, 0, 2f, _ => { }).OnComplete(() =>
        {
    

           
        });
        
        return isInited;
    }

    private void EnableClickableCollider()
    {
        if (_ballsQue.Count >= 1)
        {

            _currentBall = _ballsQue.Dequeue();

            _ballsDequeued.Enqueue(_currentBall);
        }
       
        
        _ballColliderMap[_currentBall.GetInstanceID()].enabled = true;
    }

    private void DoEnterPath(Transform ball, Vector3[] path)
    {
        ball.DOPath(path, 1.3f, PathType.CatmullRom).SetEase(Ease.OutSine)
            .OnComplete(() => { ShakeAnim(_octopusBody); });

        OnEnterPath(ball);


        DOVirtual.Float(0, 0, 3f, _ => { }).OnComplete(() =>
        {
            var answerEnum = (Shoe)Random.Range(0, (int)Shoe.Count);
            _currentAnswer = answerEnum.ToString();

#if UNITY_EDITOR
            Debug.Log($"정답: {_currentAnswer}");
#endif
        });
    }

    private void OnReInit()
    {
        
#if UNITY_EDITOR
        Debug.Log("ReInit-----------------");
#endif

        while (_ballsDequeued.Count > 0)
        {
            var ball = _ballsDequeued.Dequeue();
            _ballsQue.Enqueue(ball);
        }
        
        foreach (var ball in _ballsQue)
        {
            var id = ball.GetInstanceID();
            _rigidbodies[id].constraints = RigidbodyConstraints.FreezeAll;
            ball.localScale = Vector3.zero;
            ball.position = _defaultLocationMap[id];
            ball.DOScale(_ballDefaultSize, 1f).SetEase(Ease.OutBounce).SetDelay(Random.Range(0.5f, 0.8f)).OnComplete(
                () =>
                {
                    _isInit = false;
                    
                 }
                );
        }
        
        foreach(var ball in _ballColliderMap.Values.ToList())
        {
            ball.enabled = false;
        }
        
        
#if UNITY_EDITOR
        Debug.Log($"볼개수(After dequeue): {_ballsQue.Count}");
#endif
        _isDoingAnimation = false;
        _isBallClickable = true;
        _currentAnswer = "NONE";
        _isInit = false;
        EnableClickableCollider();



        
    }

    

    /// <summary>
    ///     공을 현재위치에서 앞으로 한칸(각 공사이를 한칸으로 정의) 이동합니다.
    /// </summary>
    private void MoveBallsOneSpace()
    {

        //if (RoundInitCheckAndInvoke()) return;
        if (_ballsQue.Count <= 0)
        {
            DOVirtual.Float(0, 0, 3f, _ => { }).OnComplete(() =>
            {
                OnReinit?.Invoke();
            });
        
            return;
        }
        
        var count = 0;
        foreach (var ball in _ballsQue)
        {
            var id = ball.GetInstanceID();
            var currentOrder = _currentOrderMap[id];
            if (currentOrder != 0 && currentOrder != IS_VANISHED)
            {
                var path = new Vector3[2];
                path[0] = ball.position;
                path[1] = _defaultLocations[currentOrder - 1];
                ball.DORotate(new Vector3(100, 100, 100), 1.2f);
                ball.DOPath(path, 1f).OnComplete(() =>
                {
                    ReInitPerClick();
                    
                    _currentOrderMap[id] = currentOrder - 1;
                    if (_currentOrderMap[id] == 0) _ballColliderMap[id].enabled = true;
                });
            }
        }
    }

    /// <summary>
    /// 라운드 초기화와 달리 공움직에 따른 구분
    /// </summary>
    ///

    private void ReInitPerClick()
    {
        if (_isInit) return;
        _isInit = true;
        
        
        var delay = 2f;
        
        DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
        {

            EnableClickableCollider();
#if UNITY_EDITOR
            Debug.Log($"볼개수(After dequeue): {_ballsQue.Count}");
#endif
            _isDoingAnimation = false;
            _isBallClickable = true;
            _currentAnswer = "NONE";
            _isInit = false;

        });

        
    }

    private void ShakeAnim(Transform body)
    {
        body.DOShakePosition(1.2f, 0.025f);
        body.DOShakeRotation(1.2f, 1f).OnComplete(() =>
        {
            foreach (var shoe in _shoeMuzzles) shoe
                .DOShakePosition(0.7f, 0.010f).SetDelay(Random.Range(0.3f,0.8f));
        });
    }


    private void OnEnterPath(Transform ball)
    {
        _currentBallCount--;
        ball.transform.DOScale(_ballDefaultSize * _sizeDecreaseAmount, 1.1f).SetEase(Ease.OutBounce);
        _currentOrderMap[ball.GetInstanceID()] = IS_VANISHED;
    }
}