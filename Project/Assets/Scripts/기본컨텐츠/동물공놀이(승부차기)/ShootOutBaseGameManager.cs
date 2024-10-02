using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

public class ShootOutBaseGameManager : Base_GameManager
{

    enum HitPointName
    {
        Start,
        Middle,
        End,
        Max
    }
    private Vector3[] _clickedpoints;
    
    //StartPoint는 StartPoint에서별도로 지정해주기때문에 Middle부터 추가경로를 받습니다
    private int _currentPointIndexToSet =(int)HitPointName.Middle;
    
    private readonly float THIS_EPSILON = 0.01f;
    private bool _isSoccerBallClicked;
    
    [Range(0,20)]
    public float power;

    private Transform _ball;
    private Rigidbody _ballRb;
    private ParticleSystem _kickPs;
    private Vector3 _ballDefaultPos;
    private Quaternion _defaultRotation;
    
    

    public static event Action OnLaunchBall; 

    // 클릭실패(공발사 실패)시 한번만 실행되도록 합니다. 
    private bool _isBallInited;
    private bool _isOnGoalAnimPlaying;

    
    protected override void Init()
    {
        base.Init();

      
         
        _clickedpoints = new Vector3[(int)HitPointName.Max]
            {Vector3.zero,Vector3.zero,Vector3.zero};
        
        DefaultSensitivity = 0.011f;
        
        _ball = GameObject.Find("SoccerBall").transform;
        var ParticleIndex =0;
        transform.GetChild(ParticleIndex).TryGetComponent<ParticleSystem>(out _kickPs);
        _defaultRotation = _ball.rotation;
        _ballRb = _ball.GetComponent<Rigidbody>();
        _ballDefaultPos = _ball.position;
    }

    protected override void BindEvent()
    {
        base.BindEvent();
        ShootOut_GoalPostController.OnGoal -= OnGoal;
        ShootOut_GoalPostController.OnGoal += OnGoal;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        ShootOut_GoalPostController.OnGoal -= OnGoal;
    }
    private void OnGoal()
    {
        if (_isOnGoalAnimPlaying) return;
        _isOnGoalAnimPlaying = true;

        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Gamemaster Audio - Fun Casual Sounds/Collectibles_Items_Powerup/collect_item_jingle_04");
    }

    private bool _isFirstPointSet;
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        if (_isBallMovingToDefaultPos) return;
        if (_isBallLaunched) return;

        foreach (var hit in GameManager_Hits)
        {
            
            if (hit.transform.gameObject.name.Contains("StartPoint"))
            {
                if (!ClickedPointsContains(hit.point) && !_isFirstPointSet)
                {
#if UNITY_EDITOR
                    Debug.Log("FirstPoint Has Set");
#endif
                    _clickedpoints[(int)HitPointName.Start] = hit.point;
                    _isFirstPointSet = true;
                }
            }
            
            if (hit.transform.gameObject.name.Contains("SoccerBall"))
            {
                if (!ClickedPointsContains(hit.point))
                {
                    _isSoccerBallClicked = true;
                    SetHitPoints(_currentPointIndexToSet, hit.point);
                    
                    _currentPointIndexToSet++;
                    
                   
                    return;
                }
            }
            
            DOVirtual.Float(0, 0, 0.45f, _ => { }).OnComplete(() =>
            {

                if (!_isBallLaunched && !_isBallInited)
                {
                    _isBallInited = true;
                    InitBallAgain();
                    DOVirtual.Float(0, 0, 0.40f, _ => { }).OnComplete(() =>
                    {
                        _isBallInited = false;
                    });
                }
            });
        }
    }
    
    /// <summary>
    /// 공을 제대로 차지 못한경우 다시 할당하기 위해 사용.
    /// </summary>
    private void InitBallAgain()
    {
        if (!_isBallLaunched && !_isBallInited)
        {
            _clickedpoints = new Vector3[(int)HitPointName.Max]
                {Vector3.zero,Vector3.zero,Vector3.zero};
         
            _currentPointIndexToSet =(int)HitPointName.Middle;
            _isSoccerBallClicked = false;
            _isBallLaunched = false;
            _isFirstPointSet = false;
            _isOnGoalAnimPlaying = false;
#if UNITY_EDITOR
            Debug.Log("Ball Re-Inited");
#endif
        }
        

    }

    private bool ClickedPointsContains(Vector3 point)
    {
        foreach (var p in _clickedpoints)
        {
            if (Vector3.Distance(p, point) < THIS_EPSILON)
            {
                return true;
            }
        }
        return false;
    }
    
    private void SetHitPoints(int index,Vector3 point)
    {
#if UNITY_EDITOR
        if (index % (int)HitPointName.Max == (int)HitPointName.Start)
        {
            Debug.Log("Index must not be Start");
            return; 
        }
#endif
        
        // 클릭포인트 보정부분입니다.
        // 반대방향 혹은 입실론보다 위치 차이가 적은경우의 결과를 무시합니다. 
         if (index % (int)HitPointName.Max == (int)HitPointName.End)
         {
             
             if (point.x < _clickedpoints[(int)HitPointName.Middle].x) return;
             var epsilon = 0.1f;// to ignore value that is smaller than epsilon.
             if (Mathf.Abs(point.x - _clickedpoints[(int)HitPointName.Middle].x) < epsilon) return;
         }

        _clickedpoints[index % (int)HitPointName.Max] = point;

        if ( 
            _isFirstPointSet &&
            _isSoccerBallClicked)
        {
            LaunchBall();
        }
        else
        {
#if UNITY_EDITOR
    Debug.Log("Ball Launch Denied // FirstPoint is Not Set Or SoccerBall Is Not Clicked");
#endif
        }

        
    }

    private bool _isBallLaunched;
    private void LaunchBall()
    {

       
        // 3개가 반영이 안된경우 중간포인트를 엔드포인트로 게산 하여 벡터를 계산합니다.
        if (_clickedpoints[(int)HitPointName.End] == Vector3.zero)
        {
            _clickedpoints[(int)HitPointName.End] = _clickedpoints[(int)HitPointName.Middle];
        }
        

        var direction = (_clickedpoints[(int)HitPointName.Start] - _clickedpoints[(int)HitPointName.End]).normalized;
        


        
        //y값무시 및 클릭포인트 순서로 인해 벡터방향이 골대의 반대방향이 나오는 경우 반대방향(음수)로 보정합니다. 
        var forceDirection = new Vector3(direction.x < 0? direction.x : -direction.x, 0,-direction.z).normalized;
        

        var kickPower = Mathf.Abs(_clickedpoints[(int)HitPointName.End].x - _clickedpoints[(int)HitPointName.Start].x);
        kickPower /= 2; //0.5~1.0 사이에 오도록 보정값 연산
        
#if UNITY_EDITOR
        Debug.Log($"KickPower Before Clamp: { kickPower:F4}");
#endif
        
        
        kickPower = Mathf.Clamp(kickPower, 0.5f, 1f);
        
        
#if UNITY_EDITOR
        Debug.Log($"KickPower { kickPower}");
        Debug.Log($"direction : {forceDirection}  (y ignored)");
#endif
        
        _ballRb.AddForce(forceDirection * power * kickPower,ForceMode.Impulse);

        var randomChar = (char)Random.Range('A', 'B' + 1);
        Managers.soundManager.Play(SoundManager.Sound.Effect, $"Audio/BB007/OnKick{randomChar}");
        _isBallLaunched = true;
        _isSoccerBallClicked = false;
        _clickedpoints = new Vector3[(int)HitPointName.Max]
            {Vector3.zero,Vector3.zero,Vector3.zero};

      

       
        
        OnLaunchBall?.Invoke();
        _kickPs.Play();
        DOVirtual.Float(0, 0, 2.0f, _ => { }).OnComplete(() =>
        {
            PutBallOnDefaultPos();
        });
        
        DOVirtual.Float(0, 0, 0.35f, _ => { }).OnComplete(() =>
        {
            _currentPointIndexToSet =(int)HitPointName.Middle;
        });
        
      
    }

    private bool _isBallMovingToDefaultPos;
    private void PutBallOnDefaultPos(float delay = 3.2f)
    {
        _ball.DOMove(_ballDefaultPos, 0.6f).OnStart(() =>
        {
            _isBallMovingToDefaultPos = true;
        }).OnComplete(() =>
        {
            _isFirstPointSet = false;
            _isBallLaunched = false;
            _isBallMovingToDefaultPos = false;
            
            _ballRb.velocity = Vector3.zero;
            _ballRb.angularVelocity = Vector3.zero;
            _ball.DORotateQuaternion(_defaultRotation, 0.5f);
        }).SetDelay(delay);

    }
}
