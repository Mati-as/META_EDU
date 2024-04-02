using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

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
    private int _currentPointIndexToSet;
    private readonly float THIS_EPSILON = 0.005f;
    private bool _isSoccerBallClicked;
    
    [Range(0,20)]
    public float power;

    private Transform _ball;
    private Rigidbody _ballRb;
    private Vector3 _ballDefaultPos;

    // 클릭실패(공발사 실패)시 한번만 실행되도록 합니다. 
    private bool _isBallInited;
    
    
    protected override void Init()
    {
        base.Init();

        _clickedpoints = new Vector3[(int)HitPointName.Max]
            {Vector3.zero,Vector3.zero,Vector3.zero};
        
        _ball = GameObject.Find("SoccerBall").transform;
        _ballRb = _ball.GetComponent<Rigidbody>();
        _ballDefaultPos = _ball.position;
    }

    protected override void OnRaySynced()
    {
        base.OnRaySynced();
        if (_isBallMovingToDefaultPos) return;

        foreach (var hit in GameManager_Hits)
        {
            if (hit.transform.gameObject.name.Contains("SoccerBall"))
            {
                if (!ClickedPointsContains(hit.point))
                {
                    _isSoccerBallClicked = true;
                    SetHitPoints(_currentPointIndexToSet, hit.point);
                    
                    
                    _currentPointIndexToSet++;
                    
                    DOVirtual.Float(0, 0, 0.45f, _ => { }).OnComplete(() =>
                    {

                        if (!_isBallLaunched && !_isBallInited)
                        {
                            _isBallInited = true;
                            InitBallAgain();
                            DOVirtual.Float(0, 0, 0.35f, _ => { }).OnComplete(() =>
                            {
                                _isBallInited = false;
                            });
                        }
                    });
                    
                    return;
                }
            }
        }

 

    }

    
    /// <summary>
    /// 공을 제대로 차지 못한경우 다시 할당하기 위해 사용.
    /// </summary>
    private void InitBallAgain()
    {
        if (!_isBallLaunched)
        {
            _clickedpoints = new Vector3[(int)HitPointName.Max]
                {Vector3.zero,Vector3.zero,Vector3.zero};
         
            _currentPointIndexToSet = 0;
            _isSoccerBallClicked = false;
            _isBallLaunched = false;
        }
        
#if UNITY_EDITOR
        Debug.Log("공 초기화");
#endif
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
         if (index % (int)HitPointName.Max == (int)HitPointName.Middle)
         {
             if (point.x < _clickedpoints[(int)HitPointName.Start].x) return;
             if (Mathf.Abs(point.x - _clickedpoints[(int)HitPointName.Start].x) < 0.1f) return;
         }
         
#if UNITY_EDITOR
         Debug.Log("Set Points");
#endif
        _clickedpoints[index % (int)HitPointName.Max] = point;

        if (_currentPointIndexToSet!=0 &&
            _currentPointIndexToSet!=1 &&
            _isSoccerBallClicked && 
            (index % (int)HitPointName.Max == 0)) 
        {
            LaunchBall();
        }
        
    }

    private bool _isBallLaunched;
    private void LaunchBall()
    {
#if UNITY_EDITOR
        Debug.Log("Launch Ball!");
#endif
        
        // 3개가 반영이 안된경우 중간포인트를 엔드포인트로 게산 하여 벡터를 계산합니다.
        if (_clickedpoints[(int)HitPointName.End] == Vector3.zero)
        {
            _clickedpoints[(int)HitPointName.End] = _clickedpoints[(int)HitPointName.Middle];
        }
        

        var direction = (_clickedpoints[(int)HitPointName.Start] - _clickedpoints[(int)HitPointName.End]).normalized;
        


        
        //y값무시..
        

        var forceDirection = new Vector3(direction.x < 0? direction.x : -direction.x, 0,direction.z).normalized;
#if UNITY_EDITOR
        Debug.Log($"direction : {forceDirection}  (y ignored)");
#endif
        _ballRb.AddForce(forceDirection * power,ForceMode.Impulse);
      
        _isSoccerBallClicked = false;
        
        _clickedpoints = new Vector3[(int)HitPointName.Max]
            {Vector3.zero,Vector3.zero,Vector3.zero};

      

        _isBallLaunched = true;
        DOVirtual.Float(0, 0, 1.2f, _ => { }).OnComplete(() =>
        {
            _isBallLaunched = false;
            PutBallOnDefaultPos();
        });
        
        _currentPointIndexToSet = 0;
        
        
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
            _isBallMovingToDefaultPos = false;
        }).SetDelay(delay);

    }
}
