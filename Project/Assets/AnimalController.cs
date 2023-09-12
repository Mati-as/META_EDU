using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimalController : MonoBehaviour
{
    
    public AnimalData _animalData;
   
    
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
    
    // ▼ Unity Loop  -----------------------------------------------
    private void Awake()
    {
        SubscribeGameManagerEvents();
    }
    
    
    void Start()
    {
        InitializeTransform();
    }
    
    private void Update()
    {
        if (GameManager.isRoundReady)
        {
            OnRoundReady();
        }
       
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
            Debug.Log("Touched Down!");
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
        
        StopCoroutineWithNullCheck(_coroutine);
        _coroutine = StartCoroutine(IncreaseScale());
    }
    private void OnRoundFinished()
    {
        StopCoroutineWithNullCheck(_coroutine);
        _coroutine = StartCoroutine(DecreaseScale());
    }

    private void StopCoroutineWithNullCheck(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
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

    private float lerp;
    public float sizeIncreasingSpeed;
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
        
        if (_coroutine != null)
        {
 
            StopCoroutine(_coroutine);
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
    
    
}
