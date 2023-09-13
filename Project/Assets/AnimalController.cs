using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class AnimalController : MonoBehaviour
{
    [Header("Reference")] [Space(10f)] [SerializeField]
    private GameManager _gameManager;
    public AnimalData animalData;
    [SerializeField]
    private ShaderAndCommon _shaderAndCommon;
    
    [Header("Initial Setting")] [Space(10f)]
    [Header("On GameStart")] [Space(10f)]
    [Header("On Round Is Ready")] [Space(10f)]
    [Header("On Round Started")] [Space(10f)]
    private float _moveInElapsed;
    [Header("On Corrected")] [Space(10f)]
    private bool isAnswer;
    [Header("On Round Finished")] [Space(10f)]
    [Header("On GameFinished")] [Space(10f)]
    
    //▼ 동물 이동 로직
    private readonly string TAG_ARRIVAL= "arrival";
    private bool isTouchedDown;
    private Animator _animator;
    
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
    
    // ▼ Unity Loop  -----------------------------------------------
    private void Awake()
    {
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

    
    // ▼ 메소드 목록 ------------------------------------------

 
    //이벤트 처리 위한 구독
    private void SubscribeGameManagerEvents()
    {
        // 중복 구독 방지.
        GameManager.onAllAnimalsInitialized -= OnOnAllAnimalsInitialized;
        GameManager.onAllAnimalsInitialized += OnOnAllAnimalsInitialized;

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
    }

    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onAllAnimalsInitialized -= OnOnAllAnimalsInitialized;
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onRoundReadyEvent -= OnRoundReady;
        GameManager.onCorrectedEvent -= OnCorrect;
        GameManager.onRoundFinishedEvent -= OnRoundFinished;
        GameManager.onRoundStartedEvent -= OnRoundStarted;
    }
    
    // 1. 상태 기준 분류 ----------------------------------------------------
    private void OnOnAllAnimalsInitialized()
    {
        GameManager.isAnimalTransformSet = true;
    }

    private void OnGameStart()
    {
        _animator = GetComponent<Animator>();
        SubscribeGameManagerEvents();
    }

    private void OnRoundReady()
    {
        isTouchedDown = false;
        if (animalData.inPlayPosition != null) animalData.inPlayPosition = null;
    }
   
    private void OnRoundStarted()
    {
        InitialzeAllAnimatorParams(_animator);
        InitializeSize();
        StandAnimalUpright();
        
    }

    private void OnCorrect()
    {
        StopCoroutineWithNullCheck(_coroutines);
        
        if (CheckIsAnswer())
        {
            SetCorrectedAnim(_animator);
        }
        
        _coroutines[0] = StartCoroutine(IncreaseScale());
    }
    
    private void OnRoundFinished()
    { 
        StopCoroutineWithNullCheck(_coroutines);
#if UNITY_EDITOR
        Debug.Log("Round Finish Animal Event Running..");
#endif
        InitialzeAllAnimatorParams(_animator);
        _coroutines[0] = StartCoroutine(DecreaseScale());
    }
    
    
    // 2. 코루틴 및 기타 함수 -----------------------------------------------------------------------

    private void SetCoroutine()
    {
        _coroutines = new Coroutine[4];
        _coroutines[0] = _coroutineA;
        _coroutines[1] = _coroutineB;
        _coroutines[2] = _coroutineC;
        _coroutines[3] = _coroutineD;

    }
    /// <summary>
    /// 지금 현재 동물이 정답과 일치하는지 체크하고 bool값을 반환합니다.
    /// </summary>
    /// 

    private bool CheckIsAnswer()
    {
        if (animalData.englishName == GameManager.answer)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void StopCoroutineWithNullCheck(Coroutine[] coroutines)
    {
        foreach (Coroutine cR in coroutines)
        {
            if (cR  != null)
            {
                StopCoroutine(cR);
            }
        }
    }

    private void InitializeTransform()
    {
        animalData.initialPosition = transform.position;
        animalData.initialRotation = transform.rotation;
        
        if (GameManager.isAnimalTransformSet == false)
        { 
            GameManager.AnimalInitialized();
            Destroy(gameObject);
        }
    }
    
    private void InitializeSize() => gameObject.transform.localScale = Vector3.one * animalData.defaultSize;
    
    IEnumerator MoveToTouchDownPlace()
    {
        while (isTouchedDown == false)
        {
            yield return null;
        }
    }
    
    private float lerp;
    private float _currentSizeLerp;
    
   
    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator IncreaseScale()
    {
        Debug.Log("Increasing Scale...");
        _currentSizeLerp = 0f;
        
        while (CheckIsAnswer() && _isDecreasingScale == false)
        {
                IncreaseScale(gameObject, animalData.defaultSize, animalData.increasedSize);
            
            yield return null;
        }
        
    }
    private void IncreaseScale(GameObject gameObject ,float defaultSize, float increasedSize)
    {
        _currentSizeLerp += _shaderAndCommon.sizeIncreasingSpeed * Time.deltaTime;
        _currentSizeLerp = Mathf.Clamp(_currentSizeLerp,0,1);

        lerp =
            Lerp2D.EaseInBounce(
                defaultSize, increasedSize,
                _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * lerp;
    }

    private bool _isDecreasingScale;
    IEnumerator DecreaseScale()
    {
        Debug.Log("Decreasing Scale...");
        _currentSizeLerp = 0f;
        _isDecreasingScale = false;
        while (CheckIsAnswer())
        {
            _isDecreasingScale = true;
            DecreaseScale(gameObject, animalData.defaultSize, animalData.increasedSize);
            yield return null;
        }
        
        if (_coroutineA != null)
        {
            StopCoroutine(_coroutineA);
        }
      
    }
    
    private void DecreaseScale(GameObject gameObject, float defaultSize, float increasedSize)
    {
        _currentSizeLerp += _shaderAndCommon.sizeDecreasingSpeed * Time.deltaTime;
        _currentSizeLerp = Mathf.Clamp(_currentSizeLerp,0,1);

        Debug.Log($"{_currentSizeLerp} <- _currentSizeLerp In Decreasing");
        lerp =
            Lerp2D.EaseOutBounce(
                increasedSize,defaultSize,
                 _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * lerp;
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
                    selectedAnimator.SetBool(AnimalData.FLY_ANIM, true);
                    break;
                case 2:
                    selectedAnimator.SetBool(AnimalData.SPIN_ANIM, true);
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
            animator.SetBool(AnimalData.RUN_ANIM, false);
            animator.SetBool(AnimalData.FLY_ANIM, false);
            animator.SetBool(AnimalData.ROLL_ANIM, false);
            animator.SetBool(AnimalData.SPIN_ANIM, false);
            animator.SetBool(AnimalData.SELECTABLE_A,false);
            animator.SetBool(AnimalData.SELECTABLE_B,false);
            animator.SetBool(AnimalData.SELECTABLE_C,false);
    }

    private void StartMoveAndRotateCrtine()
    {
        StopCoroutineWithNullCheck(_coroutines);
        StartCoroutine(MoveAndRotate(animalData.moveInTime,animalData.rotationSpeedInRound));
    }
    IEnumerator MoveAndRotate(float moveInTime, float rotationSpeedInRound)
    {
        // AnimalData animal = _gameManager._selectedAnimals.Find
        //     (a => a.name == animalData.englishName).GetComponent<AnimalData>();

        if (animalData.inPlayPosition != null)
        {
            Vector3 position = animalData.inPlayPosition.position;
            GameObject o;
            
            gameObject.transform.position = new Vector3(
                Mathf.Lerp(gameObject.transform.position.x, position.x, _moveInElapsed / moveInTime),
                (o = gameObject).transform.position.y,
                Mathf.Lerp(o.transform.position.z, position.z, _moveInElapsed / moveInTime)
            );
            
            gameObject.transform.Rotate(0, animalData.rotationSpeedInRound * Time.deltaTime, 0);

            
        }
        
        yield return null;
    }
    private void StandAnimalUpright()
    {
        gameObject.transform.rotation = Quaternion.Euler(0,gameObject.transform.rotation.y,0);
    }
    
    
}
