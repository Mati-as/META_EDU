using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using MyCustomizedEditor;
#endif


public class IdleLakeAnimalSetter : MonoBehaviour
{
    [Header("Debug Mode Set")]
    [SerializeField] private ReactiveProperty<float> _timer;
    [SerializeField] private float _waitTimeForStartRunning;
    
    //동물 객체 클릭 시 AnimalController에서 duration 증가를 set할 수 있도록하기 위해 public set 설정
    [SerializeField] public float _drinkingDuration { get; set; } 
   
   [Header("Move Speed Setting")]
    [Range(1, 20)]
    public float moveOutDuration;
    [Range(1, 20)]
    public float moveInDuration;
    
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    private enum AnimalNames
    {
        Duck,
        Horse,
        Pig,
        Dog
    }

    public static readonly int IDLE_ANIM = Animator.StringToHash("idle");
    public static readonly int RUN_ANIM = Animator.StringToHash("Run");
    public static readonly int EAT_ANIM = Animator.StringToHash("Eat");

    [Header("interval setting")] [Range(0, 60)]
    public float randomIntervalMin;

    [Range(0, 120)] public float randomIntervalMax;

   

    [Space(10f)] [Range(0, 30)] public float drinkingTimeRandomIntervalMin;

    [FormerlySerializedAs("drinkingTimeRandomIntervalMan")] [Range(0, 60)]
    public float drinkingTimeRandomIntervalMax;
    

    [Header("Animal List(Array)")]
    [Space(20f)]

#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "Duck", "Horse", "Pig", "Dog"
    })]
#endif
    
    public GameObject[] animals = new GameObject[4];


#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "Duck", "Horse", "Pig", "Dog"
    })]
#endif
    public Transform[] defaultPositions = new Transform[4];

#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "Duck", "Horse", "Pig", "Dog"
    })]
#endif
    
    public Rigidbody[] _rigidbodies = new Rigidbody[4];
    private readonly Animator[] animalAnimators = new Animator[4];
    //private Rigidbody[] _rigidbodies = new Rigidbody[4];

    [Header("Location List(Drinkable Location)")]
    public Transform[] drinkableLocation = new Transform[4];

    private readonly IdleLakeAnimalController[] _idleLakeAnimalControllers
        = new IdleLakeAnimalController[4];

    private Coroutine _setAnimalRunCoroutine;
    
    public static event Action onArrivalToLake;
    public static event Action onArrivalToDefaultPosition;

    #if UNITY_EDITOR
    [Range(0,20)]
     public float GAME_SPEED;
    #endif
    

    private void Awake()
    {
#if UNITY_EDITOR
        GAME_SPEED = 1;
#endif
        DOTween.Init(false,true,LogBehaviour.ErrorsOnly);
        CacheComponents();
        SetRandomInterval();


#if UNITY_EDITOR
        Debug.Log($"맨처음으로 선택된 동물: {(AnimalNames)_selectedAnimalNum}");
#endif
        _timer = new ReactiveProperty<float>(0f);
    }


    private void Start()
    {
        _timer
            .Where(time => time >= _waitTimeForStartRunning
            && _isOnDeFaultLocation)
            .Subscribe(_ =>
            {
                // 동물설정은 여기서 하며, 나머지는 디폴트로 설정된 동물의 컴포넌트에 로직할당..
                // 로직할당이 모두 종료 된 후, 동물을 다시 설정하게 됩니다. 
                _setAnimalRunCoroutine = StartCoroutine(SetRunCoroutine());
#if UNITY_EDITOR
                Debug.Log($"현재 선택된 동물: {(AnimalNames)_selectedAnimalNum}");
                Debug.Log("동물 호수로 보내기");
#endif
            });


    }

    private void Update()
    {
        #if UNITY_EDITOR
        Time.timeScale = GAME_SPEED;
        #endif
        
        if(_isOnDeFaultLocation)
        {
            _timer.Value += Time.deltaTime;
        }
        else
        {
            _timer.Value = 0f;
        }
      

#if UNITY_EDITOR

#endif
    }

    private void DeactivateAllAnimation()
    {
        //animalAnimators[_selectedAnimalNum].SetBool(IDLE_ANIM, false);
        animalAnimators[_selectedAnimalNum].SetBool(EAT_ANIM, false);
        animalAnimators[_selectedAnimalNum].SetBool(RUN_ANIM, false);
    }

    private void SetRandomInterval()
    {
        _waitTimeForStartRunning = Random.Range(randomIntervalMin, randomIntervalMax);
        _drinkingDuration = Random.Range(drinkingTimeRandomIntervalMin, drinkingTimeRandomIntervalMax);
    }


    private float _elapsedTimeForDrinkingDuration;
    private Coroutine _animalDrinkingCoroutine;
    private bool _isOnDeFaultLocation =true;
    /// <summary>
    ///     도착한 후 물 마시고, 떠나는 액션 및 초기화로직
    /// </summary>
    /// <returns></returns>
    private IEnumerator DrinkAndLeaveCoroutine()
    {
        if (null != _setAnimalRunCoroutine)
        {
            StopCoroutine(_setAnimalRunCoroutine);
        }

        
        _elapsedTimeForDrinkingDuration = 0f;
        DeactivateAllAnimation();
        SetAnimation(EAT_ANIM);

        while (_elapsedTimeForDrinkingDuration < _drinkingDuration)
        {
            _elapsedTimeForDrinkingDuration += Time.deltaTime;
            _timer.Value = 0f;

            yield return null;
        }

        DeactivateAllAnimation();
        SetAnimation(RUN_ANIM);
        
        var directionToLook =
            defaultPositions[_ranNumForLocation].position
            - animals[_selectedAnimalNum].transform.position;
                            
        var lookRotation = Quaternion.LookRotation(directionToLook);
        animals[_selectedAnimalNum].transform.DORotate(lookRotation.eulerAngles,1.6f)
            .OnStart(() =>
            {
                DOVirtual.Float(0.1f, 1.5f, 7.5f,value=> animalAnimators[_selectedAnimalNum].speed = value);
                
            })
            .OnComplete(() =>
            {
                animals[_selectedAnimalNum].transform.
                    DOMove(defaultPositions[_selectedAnimalNum].position, moveOutDuration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        onArrivalToDefaultPosition?.Invoke();
                        _isOnDeFaultLocation = true;
                
                    });
            });
        
        
#if UNITY_EDITOR
        Debug.Log("동물 돌아오는중..");
#endif
        
    }

    private void CacheComponents()
    {
        for (var i = 0; i < 4; i++)
        {
            animalAnimators[i] = animals[i].GetComponent<Animator>();
            _idleLakeAnimalControllers[i] = animals[i].GetComponent<IdleLakeAnimalController>();
            _rigidbodies[i] = animals[i].GetComponent<Rigidbody>();
        }
    }

    private int _selectedAnimalNum;

    private void SetRandomAnimal()
    {
        _selectedAnimalNum = Random.Range(0, 4);

#if UNITY_EDITOR
        Debug.Log($"선택된 동물: {(AnimalNames)_selectedAnimalNum}");
#endif
    }

    private void SetAnimation(int animalID)
    {
        animalAnimators[_selectedAnimalNum].SetBool(animalID, true);
    }

    private int _ranNumForLocation;

    private IEnumerator SetRunCoroutine()
    {
        if (_animalDrinkingCoroutine != null) StopCoroutine(_animalDrinkingCoroutine);

        //_timer control
#if UNITY_EDITOR
        Debug.Log("호수로 가는 코루틴 실행중..");
#endif
        _isOnDeFaultLocation = false;

        SetRandomAnimal();
        SetAnimation(RUN_ANIM);
        _ranNumForLocation = Random.Range(0, 4);

        var directionToLook = drinkableLocation[_ranNumForLocation].position
                              - animals[_selectedAnimalNum].transform.position;
        var lookRotation = Quaternion.LookRotation(directionToLook);
        animals[_selectedAnimalNum].transform.DORotate(lookRotation.eulerAngles, 2)
            .SetEase(Ease.InOutCirc)
            .OnComplete(() => { });
        animals[_selectedAnimalNum].transform
            .DOMove(drinkableLocation[_ranNumForLocation].position, moveInDuration)
            .OnStart(() =>
            {
                DOVirtual.Float(2f, 0.45f, 6.3f, value => animalAnimators[_selectedAnimalNum].speed = value);
            })
            .OnComplete(OnArrivalAtLake);
        yield return null;
    }


    private void OnArrivalAtLake()
    {
        onArrivalToLake?.Invoke();
        if (_setAnimalRunCoroutine != null)
        {
            StopCoroutine(_setAnimalRunCoroutine);
        }
        _animalDrinkingCoroutine = StartCoroutine(DrinkAndLeaveCoroutine());
    }
    
}