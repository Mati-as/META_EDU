using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


// https://app.diagrams.net/?src=about#G1oTy42sV_tIyZY60bED79XlyZ1FfcSRL0
// 시퀀스 흐름도입니다. 
public class GameManager : MonoBehaviour
{
   
    [Header("Display Setting")] [Space(10f)]
    public int TARGET_FRAME; // 런타임에 바뀔 필요가 없기에 read-only 컨벤션으로 작성.
    
    //---------- 게임로직 및 데이터 관리의 큰 틀을 설정하는 구간 입니다. ----------
    
    // 게임 시퀀스를 위한 스태틱 불 목록입니다.
    // GameManager이외에서 접근하지 않도록 처리 했습니다.
    public static bool isAnimalTransformSet { get;set;}
    public static bool isCameraArrivedToPlay { get; set; }
    public static bool isGameStarted { get; private set; }
    private bool _initialRoundIsReady; //최초 라운드 시작 이전을 컨트롤 하기 위한 논리연산자 입니다. 
    public static bool isRoundReady { get; private set; }
    public static bool isRoundStarted { get; private set; }
    public static bool isCorrected { get; private set; }
    public static bool isGameFinished { get; private set; }
    public static bool isRoundFinished { get; private set; }
    
    public static string answer { get; private set; }

    // 플레이 로직 및 동물 데이터 저장을 위한 자료구조 입니다.
    // 각 자료구조 마다 활용방식을 설명합니다.
    
    /* -animalDictionary
     동물의 이름을 Key 각 Key에 대응하는 GameObject를 Value로 설정하여, 마우스 클릭시 GameObject의 이름을 얻어와 
     해당 GameObject를 이동하거나 애니메이터를 얻어오기 위해 사용합니다.
     */
     public Dictionary<string, GameObject> animalGameOjbectDictionary = new();
    
    
    /* - _animalDefaultPositionsDictionary
     동물의 이름을 토대로 해당 디폴트 값(게임종료 시 동물들이 돌아와야할 포지션을 설정)을 저장하기 위한 자료사전입니다.
     Awake시 동물들의 디폴트 자료형을 따로 저장하려고 했으나, 런타임오류 발생이슈로 해당 방식을 사용했습니다. 
     Awake시 동물들의 위치값을 디폴트로 받아오는 방식이 유지보수가 용이하기에, 해당 방식에 대한 이슈가 해결 가능하면 
     해당 방식으로 로직을 바꿔서 사용할 수 있을 것 입니다. 
    */

    // private Dictionary<GameObject, animalPosition> _animalDefaultPositionsDictionary;
    
    /*
    동물들을 배치할 때 랜덤하게 동물이름을 섞고, 이러한 결과값을 Answer에 저장하는데 사용하기 위한 List입니다.
    위 자료구조가 클릭이벤트 처리나 동물 데이터를 저장하는데 사용했다면, List형은 이러한 데이터를 편집해서 게임 
    플레이 로직에 활용하기 위해 사용되었습니다. 
    */
    private List<GameObject> _animalList =new();
    
   
    
    
    // _selectedAnimals를 직접적으로 수정하거나,변형하지 않돌독 하기위한 리스트 
    private List<GameObject> _selectedAnimals = new();
    private List<GameObject> _inPlayTempAnimals; 
    
    
    
    //08-31 미사용 논리값 목록
    //public static bool isCorrectAnimationFinished{ get; private set; }
    // public float roundStartWaitSeconds;
    // public float correctAnimSeconds;
    // public float roundResetWaitSeconds;
    // public float finishAnimSeconds;
    
    [Space(15f)] [Header("Game Rule Setting")] [Space(10f)]
    [SerializeField]
    public int roundCount;
    private int _currentRoundCount;
    
    public float waitTimeForNextRound;


    [Space(15f)] [Header("Before In-Play Setting")] [Space(10f)] [Space(10f)]
    //----------- In-Play 상황 이전에 설정해야 할 값들을 모아놓은 목록입니다. ----------
    public Transform moveOutPositionA;
    public Transform moveOutPositionB;
    
    public float waitTimeOfinitialRoundStart;
    public float gameStartWaitSecondsForBackgroundChange;
    
    
    private float _elapsedForInitialRound;

    [Space(30f)] [Header("Game Objects(Animal) Setting")]
    public List<AnimalData> allAnimals;

    private AnimalData _selectedAnimalData;

    // ---------- 플레이에 필요한 동물 목록이며, 목록이 수정되는 경우 자료구조에도 추가해야됩니다. ----------
    // public GameObject tortoise;
    // public GameObject rabbit;
    // public GameObject dog;
    // public GameObject parrot;
    // public GameObject mouse;
    // public GameObject cat;
    
    
    
    
    [Space(15f)] [Header("In Play Setting")] [Space(10f)]
    
    // ---------- 플레이 상황에서 쓰이는 메소드의 변수 목록입니다. 
    private Animator _selectedAnimator; // 정답동믈의 애니메이션을 재생하기 위한 인스턴스 입니다. 
    private bool _isAnimRandomized; // 랜덤 애니메이션을 한 번 만 하도록 보장합니다. 
    private Animator _tempAnimator; 
    private int _previousAnswer = -1; // 초기에는 중복되는 정답이 있을 수 없도록 -1로 설정 합니다. 
    private int _randomAnswer; //static Answer에 할당 전, 랜덤값을 임시로 저장하는  인덱스 입니다.
    
    // ---------- 정답을 맞추기 전, 플레이 상황 도입 부 및 플레이 상황에서의 설정 입니다. ----------
    public Transform lookAtPosition;

    public float rotationSpeed;
    [Space(15f)] public float waitingTime;
    
    private GameObject _selectedAnimalGameObject; // 동물 이동의 시작위치 지정.
    private float _elapsedForMovingToSpotLight;
 
    
    private Vector3 m_vecMouseDownPos;
    private string _clickedAnimal; //입력 방식 바뀌는 경우 변수명 수정.
    private int _randomeIndex;
  

    [Header("In Play: Animal Size Setting")] [Space(10f)]
    public float newSize;
    public float sizeIncreasingSpeed;
    private float _defaultSize;
    [Header("In Play: Game Play Positions Setting")] [Space(10f)]
    public Transform playPositionA;
    public Transform playPositionB;
    public Transform playPositionC;

    [Space(15f)]
    [Header("On Ready Setting")] [Space(10f)] 
    public float moveOutTime;
    public float rotationSpeedWhenMovingOut;  // 회전 속도
    
    private float _moveOutElapsed;

    [Space(5f)] 
    [Header("In Play Setting")] [Space(10f)] private float _moveInElapsed;
    public float moveInSeconds;
    public float rotationSpeedInPlay;
    public float waitTimeBetweenRounds;
    public float waitTimeToBeSelectable; //동물들이 들어오고 나서 선택할 수 있게 되는 시간.

    private float _elapsedOfToBeSelectableWaitTime;
    private float _elapsedOfRoundFinished;

    [Space(5f)] 
    [Header("On Correct Setting")] [Space(10f)] [Space(10f)]
    public Transform animalMovePositionToSpotLight; // 정답을 맞춘 경우 움직여야 할 위치
    public float movingTimeSecWhenCorrect;
    
    
    private float _elapsedForNextRound;
    
    [Space(5f)] 
    [Header("On Finished Setting")]
    public float finishedMoveInTime;
    
    
    private float _elapsedForFinishMoveIn;

    [Space(10f)] [Header("References")] [Space(10f)]
    
    private AnimalShaderController _animalShaderController;
    private AnimalController _animalController;
    
    public LayerMask playObejctInteractableLayer;
    public LayerMask UIInteractableLayer;
    public ParticleSystem clickParticleSystem;
    
   
    private Ray ray;
    private RaycastHit hitInfo;
   

    private float _currentSizeLerp;
    private float lerp;
    
    
  
    
    // UI 출력을 위한 Event 처리
    [Header("UI Events")] [Space(10f)] 
    
    [SerializeField]
    private UnityEvent _onCorrectLightOnEvent;
    
    [SerializeField] 
    private UnityEvent _onRoundFinishedLightOff;
    
    [SerializeField]
    private UnityEvent _quizMessageEvent;

    [SerializeField] 
    private UnityEvent _correctMessageEvent;

    [SerializeField]
    private UnityEvent _messageInitializeEvent;

    [SerializeField]
    private UnityEvent _finishedMessageEvent;


    // ------------------------- ▼ 유니티 루프 ----------------------------


 

    private void Awake()
    {
        SetResolution(1920, 1080,TARGET_FRAME);
        
        totalAnimalCount = allAnimals.Count;
    }

    private void Start()
    {
      // 동물은 시작할 때 setActive=false됨에 주의
        isRoundFinished = true; // 첫번째 라운드 세팅을 위해 true 로 설정하고 시작. 리팩토링 예정
        SetAndInitializedAnimals();
        
    }

  
    
    /// <summary>
    ///     시퀀스 구조로, 각 조건마다 조건에 해당하는 애니메이션을 실행할 수 있도록 구성.
    /// </summary>
    private void Update()
    {
        //카메라 도착 시 일정시간 지나면 게임 시작.
        if (isCameraArrivedToPlay && gameStartWaitSecondsForBackgroundChange > _elapsedForMovingToSpotLight)
        {
            CheckPossibleToStartGame();
        }
        
        //1. 게임 시작 시 ---------------------------------
        if (isGameStarted && isGameFinished == false)
        {
            // 스크린 클릭 시 파티클 효과 
            PlayClickOnScreenEffect();
            // 맨 첫번째 라운드 시작 전, 대기시간 및 로직 설정
            CheckInitialReady();
            // 첫 라운드 이전을 roundfinish로 설정.
            //2. 라운드 시작 준비 완료 시 ------------------------
            if (isRoundReady) //1회 실행 보장.
            {
                OnRoundisReady();
                InitializeOnReady();
                CheckGameFinished();
                ResetAndInitializeBeforeStartingRound();  // 동물 리스트 초기화.
                StartRound(); //동물 애니메이션, 로컬스케일 초기화.
                SelectRandomThreeAnimals();  // 리스트 랜덤구성
#if UNITY_EDITOR
                Debug.Log("준비 완료");
#endif
            }


            //3. 라운드 시작 시 ------------------
            if (isRoundStarted)
            {
#if UNITY_EDITOR
                Debug.Log("라운드 시작!");
#endif
                InitializeAndSetTimerOnStarted();
                PlayInPlayAnimation();
                MoveAndRotateAnimals(moveInSeconds, rotationSpeedInPlay);

                
                //동물 등장 후 특정 시간 이후에 동물 선택 가능.
                if (_elapsedOfToBeSelectableWaitTime > waitTimeToBeSelectable )
                {
                    SelectObject();
                }
                
                _quizMessageEvent.Invoke(); // "~의 그림자를 맞춰보세요" UI 재생. 
            }

           
            //4. 정답 맞춘 경우 ------------------
            if (isCorrected)
            {
                isRoundStarted = false;
#if UNITY_EDITOR
                Debug.Log("정답!");  
#endif
                MoveToSpotLigt();
                InitializeAndSetTimerOnCorrect();
                
                
                _onCorrectLightOnEvent.Invoke(); // D-Light 참조 중
                _correctMessageEvent.Invoke(); // UI Instruction 클래스 참조
                
            }

            //5. 라운드 끝난 경우 ------------------ -> 1 or 6
            if (isRoundFinished)
            {
                InitializeAndSetTimerOnRoundFinished();
                
#if UNITY_EDITOR
                Debug.Log("라운드 종료!");
#endif
                MoveOutOfScreen();
                _onRoundFinishedLightOff.Invoke();
            }


            if (_elapsedForNextRound > waitTimeForNextRound)
            {
                isRoundReady = true;
                isRoundFinished = false;
                
                //UI의 문구를 초기화 하는 이벤트 입니다.
                _messageInitializeEvent.Invoke();
                
            }
        }


        //6. 모든 라운드가 끝난 경우 ------------------
        if (isGameFinished)
        {
            InitializeAndSetTimerOnGameFinished();
#if UNITY_EDITOR
            Debug.Log("게임종료");
#endif

          
            MoveInWhenGameIsFinished();
            
            _finishedMessageEvent.Invoke();
        }


        //SelectObjectWithoutClick();
        // 동물의 움직임이 시작됨을 표시
        if (Input.GetKeyDown(KeyCode.R)) ReloadCurrentScene();
    }


    
    // ------------------------- ▼ 메소드 목록 ------------------------------------------------
    
    public static int totalAnimalCount;
    public static event Action AllAnimalsInitialized;
    private static int initializedAnimalsCount = 0;
    public static event Action isRoundReadyEvent;
    public static event Action isCorrectedEvent;
    public static event Action isRoundFinishedEvent;
  
    public static void AnimalInitialized()
    {
        initializedAnimalsCount++;
        Debug.Log($"totalAnimals: {totalAnimalCount}");
        Debug.Log($"initializedAnimalsCount: {initializedAnimalsCount}");
        if (initializedAnimalsCount >= totalAnimalCount)
        {
            AllAnimalsInitialized?.Invoke();
            Debug.Log($"Initializing Event Occured!");
        }
    }

    public static void OnRoundisReady()
    {
        isRoundReadyEvent?.Invoke();
    }

    public static void OnCorrectedInvokeAnimalFunc()
    {
        isCorrectedEvent?.Invoke();
    }
    
    
   
    
    
    private bool _isRandomized;

    private void InitializeAndSetTimerOnGameFinished()
    {
        isGameStarted = false;
          
        
        if (_elapsedForFinishMoveIn < finishedMoveInTime) _elapsedForFinishMoveIn += Time.deltaTime;

    }

    private void InitializeAndSetTimerOnRoundFinished()
    {
        isCorrected = false;
        _moveOutElapsed += Time.deltaTime;
        _elapsedForNextRound += Time.deltaTime;
    }

    private void InitializeAndSetTimerOnCorrect()
    {
        isRoundStarted = false;
        _moveOutElapsed = 0f;
                
        _elapsedOfRoundFinished += Time.deltaTime;
        if (_elapsedOfRoundFinished > waitTimeBetweenRounds) isRoundFinished = true;
    }

    private void CheckPossibleToStartGame()
    {
        _elapsedForMovingToSpotLight += Time.deltaTime;
        
        if (gameStartWaitSecondsForBackgroundChange < _elapsedForMovingToSpotLight && !isGameStarted)
        {
            isGameStarted = true;
        }
    }

    private void InitializeOnReady()
    {
        isRoundStarted = true;
        _isAnimalSetUpright = false;
        _isSizeLerpInitialized = false; // 사이즈 감소 시 사용
        isCorrected = false;
        _isAnimRandomized = false; //correct anim 관련 bool 초기화.
    }

    private void InitializeAndSetTimerOnStarted()
    {
       
        _moveInElapsed += Time.deltaTime;
        _elapsedOfToBeSelectableWaitTime += Time.deltaTime;
   
        isRoundReady = false;
        _elapsedOfRoundFinished = 0f;
        _elapsedForMovingToTouchDownPlace = 0f;
    }

    /// <summary>
    ///     게임이 끝나고 원래위치고 돌아오기 위해 사용합니다.
    /// </summary>
    private void MoveInWhenGameIsFinished()
    {
        foreach (var pair in animalGameOjbectDictionary)
        {
            // var animalName = pair.Key;
            var gameObject = pair.Value;
            
            //Scriptable 오브젝트의 Monobehaviour 상속 불가능으로 인한 참조 방식 수정 
            AnimalController _aniamalcontroller = gameObject.GetComponent<AnimalController>();
            AnimalData _animalData = _aniamalcontroller._animalData;
            
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,
                _animalData.initialPosition, _elapsedForFinishMoveIn / finishedMoveInTime);
 
            PlayGameFinishAnimation(gameObject);
        }
    }

    private void PlayGameFinishAnimation(GameObject gameObj)
    {
        Animator animator = gameObj.GetComponent<Animator>();
        animator.SetBool(AnimalData.IS_GAME_FINISHED_ANIM, true);
    }

    private void CheckInitialReady()
    {
        _elapsedForInitialRound += Time.deltaTime;
        
        if (_elapsedForInitialRound > waitTimeOfinitialRoundStart && _initialRoundIsReady == false)
        {
            _initialRoundIsReady = true;
            isRoundFinished = true;
        }
    }

    /// <summary>
    ///     오브젝트를 선택하는 함수 입니다.
    ///     Linked lIst를 활용해 자료를 검색하고 해당하는 메세지를 카메라 및, 게임 다음 동작에 전달합니다.
    /// </summary>
    private void SelectObject()
    {
#if UNITY_EDITOR
        // 마우스 클릭 시
        if (Input.GetMouseButtonDown(0) && !isCorrected)
#else
        //        터치 시
        // if (Input.touchCount > 0)
#endif
        {
#if UNITY_EDITOR
            m_vecMouseDownPos = Input.mousePosition;
#else
            // m_vecMouseDownPos = Input.GetTouch(0).position;
            // if (Input.GetTouch(0).phase != TouchPhase.Began)
            //     return;
#endif

            var ray = Camera.main.ScreenPointToRay(m_vecMouseDownPos);
            RaycastHit hit;
            
          



            if (Physics.Raycast(ray, out hit, Mathf.Infinity, playObejctInteractableLayer) )
            {
                if (animalGameOjbectDictionary.ContainsKey(hit.collider.name)&& isCorrected == false)
                {
                    _clickedAnimal = hit.collider.name;
                    
                    //정답인 경우
                    if (_clickedAnimal == answer)
                    {
                       
                        isCorrected = true;
                       
                        SetAnimalData(_clickedAnimal);
                        
                        OnCorrectedInvokeAnimalFunc();
                        InvokeOncorrectUI();
                    }

                    //moving에서의 lerp
                    _elapsedForMovingToSpotLight = 0;

                    //sizeIncrease()의 lerp
                    _currentSizeLerp = 0;
                    
                }

              

            }// 정답을 맞추지 않은 상태라면...(중복정답 방지)
            
               
        }
    }

    private void PlayClickOnScreenEffect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_vecMouseDownPos = Input.mousePosition;
            
            var raySecond = Camera.main.ScreenPointToRay(m_vecMouseDownPos);
            RaycastHit hitSecond;
            
            if (Physics.Raycast(raySecond, out hitSecond, Mathf.Infinity, UIInteractableLayer) )
            {
                
                Debug.Log("파티클 재생");
                clickParticleSystem.Stop(); // 현재 재생 중인 파티클이 있다면 중지합니다.
                clickParticleSystem.transform.position = hitSecond.point; // 파티클 시스템을 클릭한 위치로 이동시킵니다.
                clickParticleSystem.Play(); // 파티클 시스템을 재생합니다.
            }
            SelectObject();
        }
    }
    
    
    /// <summary>
    /// 정답 맞출 시 UI에게 지시문을 플레이 하도록 하는 이벤트 발생용 함수입니다. 
    /// </summary>
    private void InvokeOncorrectUI()
    {
        _correctMessageEvent.Invoke();
        _onCorrectLightOnEvent.Invoke();
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
   
    private void DecreaseScale(GameObject gameObject, float defaultSize, float increasedSize)
    {
        _currentSizeLerp += sizeIncreasingSpeed * Time.deltaTime;

        lerp =
            Lerp2D.EaseInBounce(
                increasedSize,defaultSize,
                _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * lerp;
    }


    /// <summary>
    ///     마우스로 선택 된 동물을 이동시키는 함수 입니다.
    ///     선택된 동물이 이동 시, 다른 동물들은 선택될 수 없도록 추가 로직이 필요합니다.
    /// </summary>
    public void MoveToSpotLigt()
    {
        var t = Mathf.Clamp01(_elapsedForMovingToSpotLight / movingTimeSecWhenCorrect);
        
        if (isCorrected)
        {
           // IncreaseScale(_selectedAnimalGameObject, _selectedAnimalData.defaultSize,_selectedAnimalData.increasedSize);
            
            _selectedAnimalGameObject.transform.position =
                Vector3.Lerp(_selectedAnimalGameObject.transform.position,
                    animalMovePositionToSpotLight.position, t);

            var directionToTarget =
                lookAtPosition.position - _selectedAnimalGameObject.transform.position;

            var targetRotation =
                Quaternion.LookRotation(directionToTarget);
            _selectedAnimalGameObject.transform.rotation =
                Quaternion.Slerp(
                    _selectedAnimalGameObject.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    ///     각종 러프 함수 및 자료를 다음 라운드를 위해 초기화 하는 함수 입니다.
    /// </summary>
    public void ResetAndInitializeBeforeStartingRound()
    {
        _elapsedOfToBeSelectableWaitTime = 0f;
        _moveInElapsed = 0f;
        _elapsedForNextRound = 0f;
        _selectedAnimals.Clear();// 0,1,2 인덱스에 동물 리스트 쌓이지 않도록 초기화
    }

    public void SetAnimalData(string animalName)
    {
        if (animalGameOjbectDictionary.TryGetValue(animalName, out var animalObj))
        {
            _selectedAnimalGameObject = animalObj;
            _selectedAnimalData  = _selectedAnimalGameObject.GetComponent<AnimalController>()._animalData;
        }
      
    }

    /// <summary>
    ///     딕셔너리는 클릭 시, hit.name과 딕셔너리 안의 자료를 비교할 떄 사용합니다.
    ///     리스트 자료구조는 동물의 순서를 랜덤으로 섞고 동물들을 배치할 때 사용합니다.
    /// </summary>
    [SerializeField]
    public GameObject animal;
    private void SetAndInitializedAnimals()
    {
        foreach (AnimalData animalData in allAnimals)
        {
            //생성
            GameObject thisAnimal =  Instantiate(animalData.animalPrefab, 
                animalData.initialPosition + animalData.animalPositionOffset, animalData.initialRotation,
            animal.transform);


            // 이름 뒤에 (Clone) 자동으로 붙는 것 제거.
            thisAnimal.name = animalData.englishName;
            
            //크기 지정
            thisAnimal.transform.localScale = Vector3.one * animalData.defaultSize;
            Debug.Log($"animal Default Size{animalData.defaultSize}");
            
            // 자료구조에 추가..
            animalGameOjbectDictionary.Add(animalData.englishName,thisAnimal);
            
            _animalList.Add(thisAnimal);
            
        }
    }
    
   
    private float _lerpForMovingDown;
    private float _elapsedForMovingToTouchDownPlace;
    public float moveDownTime;
    public Transform touchDownPosition;
    private bool _isSizeLerpInitialized;
    
    private void MoveOutOfScreen()
    {
        
        foreach (var gameObj in _animalList)
        {
            _animalShaderController = gameObj.GetComponentInChildren<AnimalShaderController>();
            _animalController = gameObj.GetComponentInChildren<AnimalController>();
            if (gameObj.name != answer)
            {
                _tempAnimator = gameObj.GetComponent<Animator>();
                _tempAnimator.SetBool(AnimalData.RUN_ANIM, true);

                RandomRotateAndMoveOut(gameObj);
            }
            
            
           
            else if (gameObj.name == answer && !_animalController.IsTouchedDown)
            {
                _elapsedForMovingToTouchDownPlace += Time.deltaTime;
                _lerpForMovingDown += _elapsedForMovingToTouchDownPlace/ moveDownTime;
                gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                    touchDownPosition.position, _lerpForMovingDown);
            }
            
            else if (gameObj.name == answer && _animalController.IsTouchedDown)
            { 
                _tempAnimator = gameObj.GetComponent<Animator>();
                
                if (_isSizeLerpInitialized == false)
                {
                    _isSizeLerpInitialized = true;
                    _currentSizeLerp = 0f;
                }
                DecreaseScale(_selectedAnimalGameObject,
                    _selectedAnimalData.increasedSize,_selectedAnimalData.defaultSize);
                
                InitializeAllAnimatorParameters(_tempAnimator);
                _tempAnimator.SetBool(AnimalData.RUN_ANIM, true);
                RandomRotateAndMoveOut(gameObj);
                
            }
            _randomeIndex++;
        }
    }


    private void RandomRotateAndMoveOut(GameObject gameObj)
    {
        if (_randomeIndex % 2 == 0)
        {
            gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                moveOutPositionA.position, _moveOutElapsed / moveOutTime);

            RotateTowards(gameObj.transform, moveOutPositionA.position);
        }
        else
        {
            gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                moveOutPositionB.position, _moveOutElapsed / moveOutTime);
            RotateTowards(gameObj.transform, moveOutPositionB.position);
             
        }
    }

    /// <summary>
    /// StandAnimalVertically, OrientAnimalUpwards, AlignAnimalUplight
    /// </summary>
    private bool _isAnimalSetUpright;
    private void StandAnimalUpright(GameObject animal)
    {
        //FromToRotation : a축을 b축으로 -> animal.up축을 월드좌표 up축으로.
        animal.transform.rotation = Quaternion.Euler(0,animal.transform.rotation.y,0);

        Debug.Log("upright완료");
    }
    
    /// <summary>
    /// 캐릭터가 밖으로 나갈 때, TargetPosition방향으로 회전하는 함수 입니다.
    /// <param name ="currentPosition"> 회전 시킬 객체 </param>
    /// <param name ="targetPosition">  바라 볼 방향</param>
    /// </summary>
    void RotateTowards(Transform currentPosition ,Vector3 targetPosition)
    {
        
        Vector3 direction = targetPosition - currentPosition.position;
        
       
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        
        currentPosition.rotation = Quaternion.Slerp(currentPosition.rotation, targetRotation, rotationSpeedWhenMovingOut * Time.deltaTime);
    }

    public int selectableAnimalsCount;
    public void SelectRandomThreeAnimals()
    {
        _inPlayTempAnimals = new List<GameObject>(_animalList);

        for (var i = 0; i < selectableAnimalsCount; i++)
        {
            Debug.Log("동물 랜덤 고르기 완료");
            var randomIndex = Random.Range(0, _inPlayTempAnimals.Count);
            _selectedAnimals.Add(_inPlayTempAnimals[randomIndex]);
            _inPlayTempAnimals.RemoveAt(randomIndex);
        }


        _randomAnswer = Random.Range(0,_selectedAnimals.Count);

        while (_randomAnswer == _previousAnswer) _randomAnswer = Random.Range(0,_selectedAnimals.Count);


        answer = _selectedAnimals[_randomAnswer].name;

        _previousAnswer = _randomAnswer;
    }


    /// <summary>
    ///     동물 애니메이션, 로컬스케일 초기화 합니다.
    ///     동물을 플레이 위치에 놓고 선택할 수 있도록합니다.
    /// </summary>
    public void StartRound()
    {
        _currentRoundCount++;

#if DEFINE_TEST
        Debug.Log($"현재 라운드: {roundCount}");
#endif

        foreach (var gameObj in _animalList)
        {
            _tempAnimator = gameObj.GetComponent<Animator>();
            AnimalController _animalController = gameObj.GetComponent<AnimalController>();
            
            InitializeAllAnimatorParameters(_tempAnimator);
            gameObj.transform.localScale = Vector3.one * _animalController._animalData.defaultSize;
        }
        
        
       
    }

    /// <summary>
    /// 모든 애니메이션의 파라미터를 false로 초기화 합니다. 
    /// </summary>
    
    private void InitializeAllAnimatorParameters(Animator animator)
    {
        animator.SetBool(AnimalData.RUN_ANIM, false);
        animator.SetBool(AnimalData.FLY_ANIM, false);
        animator.SetBool(AnimalData.ROLL_ANIM, false);
        animator.SetBool(AnimalData.SPIN_ANIM, false);
        animator.SetBool(AnimalData.SELECTABLE_A,false);
        animator.SetBool(AnimalData.SELECTABLE_B,false);
        animator.SetBool(AnimalData.SELECTABLE_C,false);
    }
    private int _randomInPlayAnimationNumber;
    private void PlayInPlayAnimation()
    {
        foreach (var gameObj in _animalList)
        {
            _tempAnimator = gameObj.GetComponent<Animator>();
            InitializeAllAnimatorParameters(_tempAnimator);
            
            
            // 선택 장면 시, 애니메이션을 재생하는 방식으로 원할 경우..
            // _tempAnimator.SetBool(SELECTABLE,true);
            //
            // _randomInPlayAnimationNumber = Random.Range(0, 2);
            // if (_randomInPlayAnimationNumber == 0)
            // {
            //     _tempAnimator.SetBool(SELECTABLE_A,true);
            // }
            // else
            // {
            //     _tempAnimator.SetBool(SELECTABLE_B,true);
            // }
            // gameObj.transform.localScale = Vector3.one * _defaultSize;
        }
    }
    
    
    /// <summary>
    ///     동물을 회전시키는 함수 입니다.
    ///     정답을 맞추기 전, 동물이 플레이화면에 나타날때 회전하는 함수입니다.
    ///     정답을 맞춘 후 나오는 함수와 구분하여 사용합니다.
    /// </summary>
    /// <param name="출발지점 부터 도착지점까지 오는 데 걸리는 총 시간(초)"> </param>
    /// <param name="회전 속도 (Time.delta이용)"> </param>
    public void MoveAndRotateAnimals(float _moveInTime, float _rotationSpeedInRound)
    {
        //y축 수직
        if (!_isAnimalSetUpright)
        {
            StandAnimalUpright(_selectedAnimals[0]);
            StandAnimalUpright(_selectedAnimals[1]);
            StandAnimalUpright(_selectedAnimals[2]);
            
            _isAnimalSetUpright = true; 
        }
       
      
        //이동
            _selectedAnimals[0].transform.position = new Vector3(
                Mathf.Lerp(_selectedAnimals[0].transform.position.x, playPositionA.position.x,_moveInElapsed / _moveInTime),
                _selectedAnimals[0].transform.position.y,
              Mathf.Lerp(_selectedAnimals[0].transform.position.z, playPositionA.position.z, _moveInElapsed / _moveInTime)
            );
          
            _selectedAnimals[1].transform.position = new Vector3(
                Mathf.Lerp(_selectedAnimals[1].transform.position.x, playPositionB.position.x, _moveInElapsed / _moveInTime),
                _selectedAnimals[1].transform.position.y,
                Mathf.Lerp(_selectedAnimals[1].transform.position.z, playPositionB.position.z, _moveInElapsed / _moveInTime)
            );

            _selectedAnimals[2].transform.position = new Vector3(
                Mathf.Lerp(_selectedAnimals[2].transform.position.x, playPositionC.position.x, _moveInElapsed / _moveInTime),
                _selectedAnimals[2].transform.position.y,
               Mathf.Lerp(_selectedAnimals[2].transform.position.z, playPositionC.position.z, _moveInElapsed / _moveInTime)
            );
        
       
       
        //회전
        _selectedAnimals[0].transform.Rotate(0, _rotationSpeedInRound * Time.deltaTime, 0);
        _selectedAnimals[1].transform.Rotate(0, _rotationSpeedInRound * Time.deltaTime, 0);
        _selectedAnimals[2].transform.Rotate(0, _rotationSpeedInRound * Time.deltaTime, 0);
    }


    private void SetResolution(int width, int height, int targetFrame)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrame;
    }

    
    private void OnGameFinished()
    {
    }

    
    private void CheckGameFinished()
    {
        if (_currentRoundCount >= roundCount)
        {
            isGameFinished = true;
            isGameStarted = false;
        }
    }
    
    
    /// <summary>
    /// 09-01-23
    /// static 변수는 다시 원래대로 돌아오지 않으므로, 해당 부분을 초기화하는 로직을 추후 작성해야합니다.
    /// </summary>
    private void ReloadCurrentScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 해당 인덱스의 씬을 다시 로드합니다.
        SceneManager.LoadScene(currentSceneIndex);
    }
}


//8-31 아래 코드 미사용.
///// <summary>
///// 마우스 클릭 이벤트 없는 경우.
///// </summary>
// private void SelectObjectWithoutClick()
// {
//     ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//
//     if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, interactableLayer))
//     {
//         Debug.Log("Mouse over: " + hitInfo.collider.gameObject.name);
//         if (hitInfo.collider.name != null)
//         {
//             selectedAnimal = hitInfo.collider.name;
//             elapsedTime = 0;
//         }
//     }
// }