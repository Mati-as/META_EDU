using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class AnimalController : MonoBehaviour
{
    
    public AnimalData _animalData;
    [Header("Initial Setting")] [Space(10f)]
    [Header("On GameStart")] [Space(10f)]
    [Header("On Round Is Ready")] [Space(10f)]
    [Header("On Round Started")] [Space(10f)]
    [Header("On Corrected")] [Space(10f)]
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
        _animator = GetComponent<Animator>();
        SubscribeGameManagerEvents();
    }
    
    void Start()
    {
        InitializeTransform();
    }
    
    void OnDestroy()
    {
        GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
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


    private void SubscribeGameManagerEvents()
    {
        // 중복 구독 방지.
        GameManager.AllAnimalsInitialized -= OnAllAnimalsInitialized;
        GameManager.AllAnimalsInitialized += OnAllAnimalsInitialized;

        GameManager.isRoundReadyEvent -= OnRoundReady;
        GameManager.isRoundReadyEvent += OnRoundReady;

        GameManager.isCorrectedEvent -= OnCorrect;
        GameManager.isCorrectedEvent += OnCorrect;

        GameManager.isRoundFinishedEvent -= OnRoundFinished;
        GameManager.isRoundFinishedEvent += OnRoundFinished;
    }
    
    private void OnAllAnimalsInitialized()
    {
        GameManager.isAnimalTransformSet = true;
    }

    private void OnRoundReady()
    {
        isTouchedDown = false;
        
    }

    private void OnCorrect()
    {
        SetAnimation(_animator);
        
        StopCoroutineWithNullCheck(_coroutineA);
        _coroutineA = StartCoroutine(IncreaseScale());
    }
    private void OnRoundFinished()
    {
#if UNITY_EDITOR
        Debug.Log("Round Finish Animal Event Running..");
#endif
        InitializedAnimatorParameters(_animator);
        
        StopCoroutineWithNullCheck(_coroutineA);
        _coroutineA = StartCoroutine(DecreaseScale());
    }

    private void StopCoroutineWithNullCheck(Coroutine coroutine)
    {
        foreach (Coroutine cR in _coroutines)
        {
            if (cR  != null)
            {
                StopCoroutine(coroutine);
            }
        }
    }

    private void InitializeTransform()
    {
        _animalData.initialPosition = transform.position;
        _animalData.initialRotation = transform.rotation;
        
        if (GameManager.isAnimalTransformSet == false)
        { 
            GameManager.AnimalInitialized();
            Destroy(gameObject);
        }
    }

    IEnumerator MoveToTouchDownPlace()
    {
        while (isTouchedDown == false)
        {
            yield return null;
        }
    }
    public float sizeIncreasingSpeed;
    private float lerp;
    private float _currentSizeLerp;
   
    IEnumerator IncreaseScale()
    {
        float elapsedTime = 0f;
        _currentSizeLerp = 0f;
        
        while (GameManager.answer == _animalData.englishName)
        {
            IncreaseScale(gameObject, _animalData.defaultSize, _animalData.increasedSize);
            yield return null;
        }
        
    }
    private void IncreaseScale(GameObject gameObject ,float defaultSize, float increasedSize)
    {
        _currentSizeLerp += sizeIncreasingSpeed * Time.deltaTime;

        lerp =
            Lerp2D.EaseInBounce(
                defaultSize, increasedSize,
                _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * lerp;
    }
    
    IEnumerator DecreaseScale()
    {
        float elapsedTime = 0f;
        _currentSizeLerp = 0f;
        
        while (GameManager.answer == _animalData.englishName)
        {
            DecreaseScale(gameObject, _animalData.defaultSize, _animalData.increasedSize);
            yield return null;
        }
        
        if (_coroutineA != null)
        {
 
            StopCoroutine(_coroutineA);
        }
      
    }
    
    private void DecreaseScale(GameObject gameObject, float defaultSize, float increasedSize)
    {
        _currentSizeLerp += sizeIncreasingSpeed * Time.deltaTime;

        lerp =
            Lerp2D.EaseInBounce(
                defaultSize,increasedSize,
                1 - _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * lerp;
    }

    private bool _isAnimRandomized;
    private void SetAnimation(Animator selectedAnimator)
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
    private void InitializedAnimatorParameters(Animator animator)
    {
            animator.SetBool(AnimalData.RUN_ANIM, false);
            animator.SetBool(AnimalData.FLY_ANIM, false);
            animator.SetBool(AnimalData.ROLL_ANIM, false);
            animator.SetBool(AnimalData.SPIN_ANIM, false);
            animator.SetBool(AnimalData.SELECTABLE_A,false);
            animator.SetBool(AnimalData.SELECTABLE_B,false);
            animator.SetBool(AnimalData.SELECTABLE_C,false);
    }
    
    
}
