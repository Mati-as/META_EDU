using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class ShootOut_GameManager : IGameManager
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
    private Vector3 _ballDefaultPos;
    private Quaternion _defaultRotation;

    public static event Action OnLaunchBall; 

    // 클릭실패(공발사 실패)시 한번만 실행되도록 합니다. 
    private bool _isBallInited;
    
    
    protected override void Init()
    {
        base.Init();
       
        _clickedpoints = new Vector3[(int)HitPointName.Max]
            {Vector3.zero,Vector3.zero,Vector3.zero};
        
        defaultSensitivity = 0.011f;
        
        _ball = GameObject.Find("SoccerBall").transform;
        _defaultRotation = _ball.rotation;
        _ballRb = _ball.GetComponent<Rigidbody>();
        _ballDefaultPos = _ball.position;
    }

    private bool _isFirstPointSet;
    protected override void OnRaySynced()
    {
        base.OnRaySynced();
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
             if (Mathf.Abs(point.x - _clickedpoints[(int)HitPointName.Middle].x) < 0.1f) return;
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
        

        var kickPower = Mathf.Abs(_clickedpoints[(int)HitPointName.End].x - _clickedpoints[(int)HitPointName.Middle].x);

        kickPower = Mathf.Clamp(kickPower, 0.4f, 1f);
#if UNITY_EDITOR
        Debug.Log($"KickPower { kickPower}");
        Debug.Log($"direction : {forceDirection}  (y ignored)");
#endif
        
        _ballRb.AddForce(forceDirection * power * kickPower,ForceMode.Impulse);
      
        _isSoccerBallClicked = false;
        
        _clickedpoints = new Vector3[(int)HitPointName.Max]
            {Vector3.zero,Vector3.zero,Vector3.zero};

      

        _isBallLaunched = true;
        OnLaunchBall?.Invoke();
        DOVirtual.Float(0, 0, 1.2f, _ => { }).OnComplete(() =>
        {
            _isBallLaunched = false;
            _isFirstPointSet = false;
            PutBallOnDefaultPos();
        });
        
        DOVirtual.Float(0, 0, 0.35f, _ => { }).OnComplete(() =>
        {
            _currentPointIndexToSet =(int)HitPointName.Middle;
        });
        
      
    }

    private bool _isBallMovingToDefaultPos;
    private void PutBallOnDefaultPos(float delay = 2.5f)
    {
        _ball.DOMove(_ballDefaultPos, 0.6f).OnStart(() =>
        {
            _isBallMovingToDefaultPos = true;
        }).OnComplete(() =>
        {
            _ballRb.velocity = Vector3.zero;
            _ballRb.angularVelocity = Vector3.zero;
            _ball.DORotateQuaternion(_defaultRotation, 0.5f);
            _isBallMovingToDefaultPos = false;
        }).SetDelay(delay);

    }
}
