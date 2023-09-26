using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEditor;


public class AnimalController : MonoBehaviour
{
   
   
    public AnimalData _animalData;
    [SerializeField]
    private ShaderAndCommon _shaderAndCommon;
    
    // [Header("Initial Setting")] [Space(10f)]
    // [Header("On GameStart")] [Space(10f)]
    // [Header("On Round Is Ready")] [Space(10f)]
    // [Header("On Round Started")] [Space(10f)]
    
    private float _moveInElapsed;

    private float _randomInterval;
    [Header("On Corrected")] [Space(10f)]
    private float _increaseSizeLerp;
    private float _currentSizeLerp;
    private bool isAnswer;
    // [Header("On Round Finished")] [Space(10f)]
    // [Header("On GameFinished")] [Space(10f)]
    
    //▼ 동물 이동 로직
    private readonly string TAG_ARRIVAL= "arrival";
    private bool isTouchedDown;
    private Animator _animator;
    private Rigidbody _rigidbody;
    public bool IsTouchedDown
    {
        get { return isTouchedDown;}
        set { isTouchedDown = value; }
    }
    
    
    /*
     아래 코루틴 변수들은 IEnumerator 컨테이너 역할만 담당합니다.
     어떤 함수가 사용되는지는 StartCoroutine에서확인 및 디버깅 해야합니다.
     */
    private Coroutine _coroutineA;
    private Coroutine _coroutineB;
    private Coroutine _coroutineC;
    private Coroutine _coroutineD;
    private Coroutine[] _coroutines;
    
    // 코루틴 WaitForSeconds 캐싱 자료사전
    private Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds))
        {
            waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        }
        return waitForSecondsCache[seconds];
    }
    
    // ▼ Unity Loop  -----------------------------------------------
    
    
    
    private void Awake()
    {
        Reset();
        SetCoroutine();
        SubscribeGameManagerEvents();
       
    }
    
    void Start()
    {
        InitializeTransform();

    }
    
    void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }
    
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TAG_ARRIVAL))
        {
            isTouchedDown = true;
#if UNITY_EDITOR
            Debug.Log("Touched Down!");
#endif
        }
    }
    

    
    
    // ▼ 메소드 목록 ----------------------------------------------------------------------------

 
    
    /// <summary>
    /// 이벤트 처리를 위해 GameManager Actions를 구독합니다. 
    /// </summary>
    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;
        
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onRoundReadyEvent += OnRoundReady;

        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onCorrectedEvent += OnCorrect;

        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundFinishedEvent += OnRoundFinished;

        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onRoundStartedEvent += OnRoundStarted;
        
        GameManager.onGameFinishedEvent -= OnGameFinished;
        GameManager.onGameFinishedEvent += OnGameFinished;
    }

    
    /// <summary>
    /// 게임 종료시 구독해제하여 중복 구독을 방지합니다.
    /// 중복 구독 방지 로직은 Subscribe에도 있습니다. 
    /// </summary>
    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
    }
    
    // 1. 상태 기준 분류 --------------------------------------------
   

    private void OnGameStart()
    {
        // ▼ 1회 실행.
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        SubscribeGameManagerEvents();
    }

    private void OnRoundReady()
    {
        isTouchedDown = false;
       
    }
   
    private void OnRoundStarted()
    {
       
        // ▼ 이전 코루틴 중지.
        StopCoroutineWithNullCheck(_coroutines);
        
        // ▼ 1회 실행. 
        InitialzeAllAnimatorParams(_animator);
        InitializeSize();
        StandAnimalUpright();
        RotateAnimal();
      
        // ▼ 코루틴.
        _coroutines[0] = StartCoroutine(MoveAndRotateCoroutine());
        _coroutines[1] = StartCoroutine(SetRandomAnimationWhenWhenRoundStartCoroutine());
    }

    private void OnCorrect()
    {
        // ▼ 이전 코루틴 중지.
       
      
        // ▼ 1회 실행. 
        if (CheckIsAnswer())
        {
           
            _animator.SetBool(AnimalData.RUN_ANIM,true);
        }
        
        // ▼ 코루틴.
       
        _coroutines[0] = StartCoroutine(MoveToSpotLightCoroutine());
       
     
        
    }
    
    private void OnRoundFinished()
    {
        // ▼ 1회 실행. 
        InitialzeAllAnimatorParams(_animator);
        isTouchedDown = false;
        _rigidbody.isKinematic = false;
        
        // ▼ 코루틴.
        _coroutines[0] = StartCoroutine(DecreaseScale());
        
       // if (animalData.inPlayPosition != null) animalData.inPlayPosition = null;
    }


    private void OnGameFinished()
    {
        // ▼ 1회 실행. 
        InitialzeAllAnimatorParams(_animator);
        
        _coroutines[0] = StartCoroutine((MoveInWhenGameIsFinishedCoroutine()));
        _coroutines[1] = StartCoroutine(SetRandomAnimationWhenWhenRoundStartCoroutine());
    }
    
    
    // 2. IEnumerator 및 기타 함수 ------------------------------------------------------------------------

    /// <summary>
    /// 코루틴 종료 함수 입니다.
    /// </summary>
    /// <param name="coroutines"></param>
    private void StopCoroutineWithNullCheck(Coroutine[] coroutines)
    {
        Debug.Log("코루틴 종료");
        foreach (Coroutine cR in coroutines)
        {
            if (cR  != null)
            {
                StopCoroutine(cR);
            }
        }
    }
    
    
    /// <summary>
    /// 코루틴을 배열에 저장합니다.
    /// </summary>
    private void SetCoroutine()
    {
        _coroutines = new Coroutine[4];
        _coroutines[0] = _coroutineA;
        _coroutines[1] = _coroutineB;
        _coroutines[2] = _coroutineC;
        _coroutines[3] = _coroutineD;

    }
  
    
    /// <summary>
    /// 지금 현재 동물이 정답과 일치하는지 체크 및 bool값을 반환합니다.
    /// </summary>
    private bool CheckIsAnswer()
    {
        if (_animalData.englishName == GameManager.answer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
   

    /// <summary>
    /// 씬에 배치된 동물객체의 위치를 저장합니다. (개발 편의를 위해 분리 설계 하였습니다.)
    /// </summary>
    private void InitializeTransform()
    {
        if (GameManager.isAnimalTransformSet == false)
        { 
            _animalData.initialPosition = transform.position;
            _animalData.initialRotation = transform.rotation;
            
            GameManager.AnimalInitialized();
            Destroy(gameObject);
        }
    }
    
    private void InitializeSize() => gameObject.transform.localScale = Vector3.one * _animalData.defaultSize;
    
    IEnumerator MoveToTouchDownPlace()
    {
        while (isTouchedDown == false)
        {
            yield return null;
        }
    }
    
 
    
   
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator IncreaseScaleCoroutine()
    {
        //초기화.
        _currentSizeLerp = 0f;
        
        while (CheckIsAnswer() && !GameManager.isRoundFinished)
        {
            IncreaseScale(gameObject, _animalData.defaultSize, _animalData.increasedSize);
            
            if (GameManager.isRoundFinished)
            {
                Debug.Log("Increase 코루틴 종료");
                StopCoroutineWithNullCheck(_coroutines);
            }
            
            yield return null;
        }
        
        
    }
    private void IncreaseScale(GameObject gameObject ,float defaultSize, float increasedSize)
    {
        _currentSizeLerp += _shaderAndCommon.sizeIncreasingSpeed * Time.deltaTime;
        _currentSizeLerp = Mathf.Clamp(_currentSizeLerp,0,1);

        _increaseSizeLerp =
            Lerp2D.EaseInBounce(
                defaultSize, increasedSize,
                _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * _increaseSizeLerp;
        
      
    }

    private bool _isDecreasingScale;
    IEnumerator DecreaseScale()
    {
        //초기화
        _currentSizeLerp = 0f;
        _isDecreasingScale = false;
        
        while (CheckIsAnswer() && GameManager.isRoundFinished)
        {
            _isDecreasingScale = true;
            DecreaseScale(gameObject, _animalData.defaultSize, _animalData.increasedSize);
            
            if (!GameManager.isRoundFinished)
            {
                Debug.Log("Decrease 코루틴 종료");
                StopCoroutineWithNullCheck(_coroutines);
            }
            
            yield return null;
        }
    }
    
    private void DecreaseScale(GameObject gameObject, float defaultSize, float increasedSize)
    {
        _currentSizeLerp += _shaderAndCommon.sizeDecreasingSpeed * Time.deltaTime;
        _currentSizeLerp = Mathf.Clamp(_currentSizeLerp,0,1);
        
        _increaseSizeLerp =
            Lerp2D.EaseOutBounce(increasedSize,defaultSize,_currentSizeLerp);
        
        gameObject.transform.localScale = Vector3.one * _increaseSizeLerp;
        
    }

    private bool _isAnimRandomized;
    private void SetCorrectedAnim(Animator selectedAnimator)
    {
        _isAnimRandomized = false;
             
        selectedAnimator.SetBool(AnimalData.SELECTABLE_A,false);
        selectedAnimator.SetBool(AnimalData.SELECTABLE_B,false);
        selectedAnimator.SetBool(AnimalData.SELECTABLE_C,false);
            
        if (_isAnimRandomized == false)
        {
            var randomAnimNum = Random.Range(0, 3);
            _isAnimRandomized = true;

            switch (randomAnimNum)
            {
                case 0:
                    selectedAnimator.SetBool(AnimalData.ROLL_ANIM, true);
                    break;
                
                case 1:
                    selectedAnimator.SetBool(AnimalData.SPIN_ANIM, true);
                    break;
                
                case 2:
                    selectedAnimator.SetBool(AnimalData.FLY_ANIM, true);
                    break;
            }
        }
    }

    IEnumerator OnRoundFinishedCoroutine()
    {
        yield return null;
    }
    private void InitialzeAllAnimatorParams(Animator animator)
    {
            animator.SetBool(AnimalData.SIT_ANIM, false);
            animator.SetBool(AnimalData.RUN_ANIM, false);
            animator.SetBool(AnimalData.FLY_ANIM, false);
            animator.SetBool(AnimalData.ROLL_ANIM, false);
            animator.SetBool(AnimalData.SPIN_ANIM, false);
            animator.SetBool(AnimalData.SELECTABLE_A,false);
            animator.SetBool(AnimalData.SELECTABLE_B,false);
            animator.SetBool(AnimalData.SELECTABLE_C,false);
    }


    private void RotateAnimal()
    {
        float randomRotation = 
            Random.Range(_animalData.randomRotatableRangeMin,
                _animalData.randomRotatableRangeMax);
            
       
        StandAnimalUpright(gameObject);
        transform.rotation =  Quaternion.Euler(0, randomRotation, 0);;
       
        
        Debug.Log($"TargetRotation  {randomRotation}");
    }
    private bool _isrotated;
    IEnumerator MoveAndRotateCoroutine()
    {
        _moveInElapsed = 0f;
        _elapsedForRotationInPlay = 0f;
       
          
        
        
        while (GameManager.isRoundStarted)
        {
            MoveAndRotateInPlay(_animalData.moveInTime,0,AnimalData.LOOK_AT_POSITION);
            
            if (GameManager.isCorrected)
            {
                StopCoroutineWithNullCheck(_coroutines);
                break;
            }
            
            yield return null;
        }
       
    }

    private float _elapsedForRotationInPlay;
    
   
    
    private void MoveAndRotateInPlay(float moveInTime, float rotationSpeedInRound,Transform lookTarget)
    {
       
        Vector3 position = _animalData.inPlayPosition.position;
        GameObject obj;
        _moveInElapsed += Time.deltaTime;
            
        gameObject.transform.position = new Vector3(
            Mathf.Lerp(gameObject.transform.position.x, position.x, _moveInElapsed / moveInTime),
            (obj = gameObject).transform.position.y,
            Mathf.Lerp(obj.transform.position.z, position.z, _moveInElapsed / moveInTime)
        );


      

        //gameObject.transform.Rotate(0, rotationSpeedInRound * Time.deltaTime, 0);
       
    }

    private float elapsedForAnimationWhenRoundStart;
    
    private void SetRandomPlayIdleAnimationWhenRoundStart(bool boolean)
    {
        int randomAnimNum = Random.Range(0, 3);
        _isAnimRandomized = true;

        switch (randomAnimNum)
        {
            case 0:
                _animator.SetBool(AnimalData.JUMP_ANIM, boolean);
                break;
            case 1:
                _animator.SetBool(AnimalData.SIT_ANIM, boolean);
                break;
            case 2:
                _animator.SetBool(AnimalData.BOUNCE_ANIM, boolean);
                break;
        }
    }

    private void InitializeAnimation(bool boolean)
    {
        _animator.SetBool(AnimalData.SIT_ANIM,boolean);
        _animator.SetBool(AnimalData.JUMP_ANIM, boolean);
        _animator.SetBool(AnimalData.BOUNCE_ANIM, boolean);
        
     
        
    }

    private void InitializeCorrectAnimatin(bool boolean)
    {
         // _animator.SetBool(AnimalData.FLY_ANIM, boolean);
        _animator.SetBool(AnimalData.SPIN_ANIM, boolean);
        _animator.SetBool(AnimalData.ROLL_ANIM, boolean);
    }
    IEnumerator SetRandomAnimationWhenWhenRoundStartCoroutine()
    {
        _randomInterval = Random.Range(_animalData.animationPlayIntervalMin, _animalData.animationPlayIntervalMax);
        while (!GameManager.isCorrected)
        {
            yield return GetWaitForSeconds(_randomInterval);
            SetRandomPlayIdleAnimationWhenRoundStart(true);
            yield return GetWaitForSeconds(_animalData.animationDuration);
            InitializeAnimation(false);
            
            yield return null;
        }
    }
    
    /// <summary>
    /// 동물의 Y축을 월드좌표 Y축에 맞게 수정합니다.
    /// Rigidbody 동물이 다른 충돌에 의해 축이 변형될 수 있으므로 사용합니다. 
    /// </summary>
    private void StandAnimalUpright()
    {
        gameObject.transform.rotation = Quaternion.Euler(0,gameObject.transform.rotation.y,0);
    }


   
    private float _elapsedForMovingWhenCorrect;
    private bool _isArrivedTouchDownSpot;
    IEnumerator MoveToSpotLightCoroutine()
    {
        
        _elapsedForMovingWhenCorrect = 0f;
        _isArrivedTouchDownSpot = false;
        isTouchedDown = false;
        
        while (!GameManager.isRoundFinished)
        {
            _elapsedForMovingWhenCorrect += Time.deltaTime;
            
            if (CheckIsAnswer())
            {
                if (isTouchedDown == false)
                {
                    _animator.SetBool(AnimalData.RUN_ANIM, true);
                    MoveToTouchDownSpot();
                }
                
                else
                {
                    MoveToSpotLight();
                    _animator.SetBool(AnimalData.RUN_ANIM,false);
                   
                    _rigidbody.isKinematic = true;
                    if (!_isArrivedTouchDownSpot)
                    {
                        _elapsedForMovingWhenCorrect = 0f;
                        _isArrivedTouchDownSpot = true;
                        
                        SetCorrectedAnim(_animator);
                        _coroutines[1] = StartCoroutine(IncreaseScaleCoroutine());
                        _coroutines[2] = StartCoroutine(InitializeAnimationoroutine());
                    }

                  

                }
            }
           
            
            
            yield return null;
        }
        
     
    }

    IEnumerator InitializeAnimationoroutine()
    {
        yield return GetWaitForSeconds(_animalData.correctAnimTime);
        Debug.Log("코렉트 애니메이션 중지!");
        InitializeCorrectAnimatin(false); //fly anmation 제외 초기화 
    }
   

    private void MoveAndRotateToward(Transform TargetPosition,Transform lookAt, float moveTime, float rotationTime)
    {
        //Move
        float t = Mathf.Clamp01(_elapsedForMovingWhenCorrect / moveTime);
       
        
        gameObject.transform.position =
            Vector3.Lerp(gameObject.transform.position,
                TargetPosition.position, t);

        Vector3 directionToTarget =
            lookAt.position - gameObject.transform.position;

        //Rotate
        float t2 = Mathf.Clamp01(_elapsedForMovingWhenCorrect / rotationTime);
        
        
        Quaternion targetRotation =
            Quaternion.LookRotation(directionToTarget);
        
        gameObject.transform.rotation =
            Quaternion.Slerp(
                gameObject.transform.rotation, targetRotation,
                t2 * Time.deltaTime);
    }
    
    private void MoveAndRotateTowardTouchDownPlace(Transform TargetPosition,Transform lookAt, float moveTime, float rotationTime)
    {
        float t = Mathf.Clamp01(_elapsedForMovingWhenCorrect / moveTime);
        float t2 = Mathf.Clamp01(_elapsedForMovingWhenCorrect / rotationTime);
        
        Vector3 newPosition = Vector3.Lerp(gameObject.transform.position, TargetPosition.position, t);

        // y 값을 고정하여 변경하지 않습니다.
        newPosition.y = gameObject.transform.position.y;

        gameObject.transform.position = newPosition;

        Vector3 directionToTarget = lookAt.position - gameObject.transform.position;

        Quaternion targetRotation =
            Quaternion.LookRotation(directionToTarget);
        
        gameObject.transform.rotation =
            Quaternion.Slerp(
                gameObject.transform.rotation, targetRotation,
                t2 * Time.deltaTime);
    }

    private void MoveToSpotLight()
    {
            Debug.Log("Moving Up To The Moon.");
            MoveAndRotateToward(AnimalData.SPOTLIGHT_POSITION_FOR_ANIMAL,AnimalData.LOOK_AT_POSITION,
                _animalData.movingTimeSecWhenCorrectToSpotLight,_animalData.rotationTimeWhenCorrect);
    }

    private void MoveToTouchDownSpot()
    {
        Debug.Log("Moving To Touch Down.");
    
        MoveAndRotateTowardTouchDownPlace(AnimalData.TOUCH_DOWN_POSITION,AnimalData.LOOK_AT_POSITION,
            _animalData.movingTimeSecWhenCorrectToTouchDownSpot,_animalData.rotationTimeWhenCorrect);
    }
    
    private float _elapsedForFinishMoveIn;
    IEnumerator MoveInWhenGameIsFinishedCoroutine()
    {
        StopCoroutineWithNullCheck(_coroutines);
        _elapsedForFinishMoveIn = 0f;
        
        while (true)
        {
            _elapsedForFinishMoveIn += Time.deltaTime;
            MoveInWhenGameIsFinished(_elapsedForFinishMoveIn);
            yield return null;
        }
    }
    private void MoveInWhenGameIsFinished(float elapsedTime)
    {

        transform.position = Vector3.Lerp(transform.position, _animalData.initialPosition,
            elapsedTime / _animalData.finishedMoveInTime);
        
        transform.rotation = Quaternion.Lerp(transform.rotation, _animalData.initialRotation,
            elapsedTime / _animalData.finishedMoveInTime);

    }
    
    private bool _isAnimalSetUpright;

    /// <summary>
    ///     StandAnimalVertically, OrientAnimalUpwards, AlignAnimalUplight
    /// </summary>
    private void StandAnimalUpright(GameObject animal)
    {
        //FromToRotation : a축을 b축으로 -> animal.up축을 월드좌표 up축으로.
        animal.transform.rotation = Quaternion.Euler(0, animal.transform.rotation.y, 0);

        Debug.Log("upright완료");
    }


    
    /// <summary>
    /// 게임 재시작시 모든 파라미터를 초기화해줍니다.
    /// </summary>
    private void Reset()
    {
        _animator = GetComponent<Animator>();
        InitialzeAllAnimatorParams(_animator);
        if (_coroutines != null)
        {
            StopCoroutineWithNullCheck(_coroutines);
        }
      
        
    }
    
}
