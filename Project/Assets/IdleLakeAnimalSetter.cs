using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using MyCustomizedEditor;
#endif


public class IdleLakeAnimalSetter : MonoBehaviour
{
    
    [Header("Reference")] 
    [SerializeField]
    private IdleLakeAnimalController idleLakeAnimalController;
    
    enum AnimalNames
    {
        Duck, 
        Horse, 
        Pig,
        Dog
    } 
    public static readonly int IDLE_ANIM = Animator.StringToHash("idle"); 
    public static readonly int RUN_ANIM = Animator.StringToHash("Run");
    public static readonly int EAT_ANIM = Animator.StringToHash("Eat");
    
    [Header("interval setting")] 
    [Range(0,60)]
    public float randomIntervalMin;
    [Range(20,120)]
    public float randomIntervalMax;

    private float _waitTimeForStartRunning;


    [Range(0,30)]
    public float drinkingTimeRandomIntervalMin;
    [Range(0,45)]
    public float drinkingTimeRandomIntervalMan;
    private float _drinkingDuration;
    
    [SerializeField]
    private ReactiveProperty<float> _timer;
    [SerializeField]
    private ReactiveProperty<bool> isAnimalActing;
    [Header("Animal List(Array)")] 
    [Space(20f)]
    
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "Duck", "Horse", "Pig", "Dog"
    })]
#endif
    
    
    public GameObject[] animals = new GameObject[4];

    private Vector3[] defaultPositions = new Vector3[4];
    private Animator[] animalAnimators = new Animator[4];
    //private Rigidbody[] _rigidbodies = new Rigidbody[4];

    [Header("Location List(Drinkable Location)")]
    public Transform[] drinkableLocation = new Transform[4]; 

    private void Awake()
    {
        DOTween.Init();
        
        GetAnimators();
        SetRandomInterval();
        
        _timer = new ReactiveProperty<float>();
    }


    private void Start()
    {
     
        
        _timer
            .Where(time => time >= _waitTimeForStartRunning)
            .Subscribe(_ =>
            {
                SetRandomAnimal();
                SetAnimation(RUN_ANIM);
                MoveAnimalRandomly();
            });
        
        //Location의 콜라이더에 충돌, 즉 도달했을때
        idleLakeAnimalController.isArrivedAtDrinkablePosition
            .Where(_ => _ == true)
            .Subscribe
            (_ =>
                {
                    _animalDrinkingCoroutine = StartCoroutine(AnimalDrinkingActionCoroutine());
                }
            );
    }
    
    private void Update()
    {
        _timer.Value += Time.deltaTime;
    }

    private void DeactivateAllAnimation()
    {
        animalAnimators[_selectedAnimalNum].SetBool(IDLE_ANIM,false);
        animalAnimators[_selectedAnimalNum].SetBool(EAT_ANIM,false);
        animalAnimators[_selectedAnimalNum].SetBool(RUN_ANIM,false);
        
    }

 

    private void SetRandomInterval()
    {
        _waitTimeForStartRunning = Random.Range(randomIntervalMin, randomIntervalMax);
    }


    private float _elapsedTimeForDrinkingDuration;
    private Coroutine _animalDrinkingCoroutine;
    
    
    /// <summary>
    /// 도착한 후 물 마시고, 떠나는 액션 및 초기화로직
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimalDrinkingActionCoroutine()
    {
        
        DeactivateAllAnimation();
        SetAnimation(EAT_ANIM);
      
        while (_elapsedTimeForDrinkingDuration < _drinkingDuration)
        {
            _elapsedTimeForDrinkingDuration = Time.deltaTime;
            _timer.Value= 0f;
            yield return null;
        }
        
        DeactivateAllAnimation();
        SetAnimation(RUN_ANIM);
        animals[_selectedAnimalNum].transform.
            DOMove(defaultPositions[_selectedAnimalNum], 10f);
        idleLakeAnimalController.isArrivedAtDrinkablePosition.Value = false;
       
        
        SetRandomInterval();
      


    }
    
    private void GetAnimators()
    {
        for (int i = 0; i < 4; i++)
        {
            animalAnimators[i] = animals[i].GetComponent<Animator>();
            defaultPositions[i] = animals[i].GetComponent<Transform>().position;
            //  _rigidbodies[i] = animals[i].GetComponent<Rigidbody>();

        }
    }

    private int _selectedAnimalNum;
    private void SetRandomAnimal()
    {
        _selectedAnimalNum = (int)Random.Range(0, 4);
    }



    private void SetAnimation(int animalID)
    {
      animalAnimators[_selectedAnimalNum].SetBool(animalID,true);
    }

    private void MoveAnimalRandomly()
    {
        animals[_selectedAnimalNum].transform
            .DOMove(drinkableLocation[Random.Range(0, 4)].position,
                5f);
    }
    
}
