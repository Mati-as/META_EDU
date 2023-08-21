using System;
using UnityEngine;
using Random = System.Random;

public class DragonflyController : MonoBehaviour
{
    private string _dragonflyName;
    private Animator _animator;


    private bool _isClicked;
    private Transform _currentLandingPosition;
    public Transform upperPosition;

    private readonly int _dragonflyFly = Shader.PropertyToID("fly");
    
    private float _elapsedTime;

    public float movingTimeSec;
    public float waitTimeToGetBack;
    private bool _isReadyToGetBack;

    private Transform[] _landingPositions;
    
    public Transform landingPositionA;   
    public Transform landingPositionB;   
    public Transform landingPositionC;
    public Transform landingPositionD;
    public Transform landingPositionE;   
    public Transform landingPositionF;   

    
    private void Awake()
    {
        _currentLandingPosition = transform;
        
        _landingPositions = new Transform[6];

        _landingPositions[0] = landingPositionA;
        _landingPositions[1] = landingPositionB;
        _landingPositions[2] = landingPositionC;
        _landingPositions[3] = landingPositionD;
        _landingPositions[4] = landingPositionE;
        _landingPositions[5] = landingPositionF;
        
        _animator = GetComponent<Animator>();

        _dragonflyName = gameObject.name;
#if DEFINE_TEST
        Debug.Log($"gameobject name is {_dragonflyName}");
#endif
    }

    // Update is called once per frame
    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (Input.GetMouseButtonDown(0))
        {
            _elapsedTime = 0f;
            CheckClickedAndAnimateDragonfly();
        }

        if (_isClicked)
        {
            MoveUpToDisappear();
            if (_isReadyToGetBack) LandToGround();
        }
    }

    /// <summary>
    ///     잠자리를 올리는 함수.
    ///     잠자리 클릭 시, 카메라에 안보이는 위(y축) 방향으로 상승했다가, 지정한 포지션 중 랜덤 한 곳에 떨어지도록 설계
    /// </summary>
  
    private void MoveUpToDisappear()
    {
        CheckReadyToGetBack();
        var t = Mathf.Clamp01(_elapsedTime / movingTimeSec);
        transform.position = Vector3.Lerp(_currentLandingPosition.transform.position, upperPosition.position, t);
    }

    /// <summary>
    ///     잠자리가 준비되었는지 체크하는 함수.
    /// </summary>
    private void CheckReadyToGetBack()
    {
        _isReadyToGetBack = _elapsedTime > waitTimeToGetBack;
        _isClicked = _elapsedTime > waitTimeToGetBack;
    }

    public float moveSpeed;

    private int randomPositionIndex;
    private void LandToGround()
    {
        transform.position = Vector3.Lerp(upperPosition.position,_landingPositions[randomPositionIndex].position ,
            Time.deltaTime * moveSpeed);
    }

    private void CheckClickedAndAnimateDragonfly()
    {
        System.Diagnostics.Debug.Assert(Camera.main != null, "Camera.main != null");
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name != _dragonflyName) return;

            randomPositionIndex = UnityEngine.Random.Range(0, 6); // 수정된 부분
        
            _animator.SetBool(_dragonflyFly, true);
        
            _currentLandingPosition.position = transform.position;
        
            _isClicked = true;
        }
       
    }
}