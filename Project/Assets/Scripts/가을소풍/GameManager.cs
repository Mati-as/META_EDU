using System;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = System.Random;


// https://app.diagrams.net/?src=about#G1oTy42sV_tIyZY60bED79XlyZ1FfcSRL0
// 시퀀스 흐름도입니다. 
public class GameManager : MonoBehaviour
{
    [Header("Debug Mode")] [Space(10f)] 
    [Range(0.25f, 10f)] 
    public float GAME_PROGRESSING_SPEED_COPY = 1;
    public static float GAME_PROGRESSING_SPEED; // 디버그 용 입니다. 빌드 포함X

   
    
    [Header("Display Setting")] [Space(10f)]
    public int TARGET_FRAME; // 디버그 이외에 런타임에 바뀔 필요가 없기에 read-only 컨벤션으로 작성.
    
    // 9/21 콜라이더 디버그 완료로 CLICK_REPEAT_COUNT미사용..
    // //감도 향상 테스트를 위한 CLICK_REPEAT_COUNT 설정. 
    // [FormerlySerializedAs("clickRepeatCount")]
    // public int CLICK_REPEAT_COUNT;

 

    [Header("Screen Fx Settings")]  [Space(10f)] 
    public LayerMask playObejctInteractableLayer;
    public LayerMask UIInteractableLayer;
    public ParticleSystem clickParticleSystem;
    public ParticleSystem answerParticleSystem;
    
    
    //---------- 게임로직 및 데이터 관리의 큰 틀을 설정하는 구간 입니다. ----------
    [Space(10f)] [Header("Common Data & Animal Setting")] [Space(10f)] [SerializeField]
    private ShaderAndCommon _shaderAndCommon;
    [Space(5f)] 
    public List<AnimalData> allAnimals;
    // 게임 시퀀스를 위한 스태틱 불 목록입니다.
    // GameManager이외에서 접근하지 않도록 처리 했습니다.
    public static bool isAnimalTransformSet { get; set; }
    public static bool isCameraArrivedToPlay { get; set; }
    public static bool isGameStarted { get; private set; }
    private bool _initialRoundIsReady; //최초 라운드 시작 이전을 컨트롤 하기 위한 논리연산자 입니다. 
    public static bool isRoundReady { get; private set; }
    public static bool isRoundStarted { get; private set; }
    public static bool isCorrected { get; private set; }
    public static bool isRoundFinished { get; private set; }
    public static bool isGameFinished { get; private set; }
    public static bool isGameStopped { get; set; }
    private static readonly int IS_GAME_STOPPED = 0;


    private bool _isGameEventInvoked;


    public static string answer { get; private set; }

    // 플레이 로직 및 동물 데이터 저장을 위한 자료구조 입니다.
    // 각 자료구조 마다 활용방식을 설명합니다.

    /* -animalDictionary
     동물의 이름을 Key 각 Key에 대응하는 GameObject를 Value로 설정하여, 마우스 클릭시 GameObject의 이름을 얻어와
     해당 GameObject를 이동하거나 애니메이터를 얻어오기 위해 사용합니다.
     */
    public Dictionary<string, GameObject> animalGameOjbectDictionary = new();


    /*
    동물들을 배치할 때 랜덤하게 동물이름을 섞고, 이러한 결과값을 Answer에 저장하는데 사용하기 위한 List입니다.
    위 자료구조가 클릭이벤트 처리나 동물 데이터를 저장하는데 사용했다면, List형은 이러한 데이터를 편집해서 게임
    플레이 로직에 활용하기 위해 사용되었습니다.
    */
    private readonly List<GameObject> _animalList = new();

    // _selectedAnimals를 직접적으로 수정하거나,변형하지 않도록 하기 위한 리스트 
   [HideInInspector]
    public List<GameObject> _selectedAnimals = new();
    private List<GameObject> _inPlayTempAnimals;


    [Space(40f)] [Header("Game Rule Setting")] [Space(10f)] [SerializeField]
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
    

    private AnimalData _selectedAnimalData;


    [Space(15f)] [Header("In Play Setting")] [Space(10f)]

  

    // ---------- 플레이 상황에서 쓰이는 메소드의 변수 목록입니다. 
    private Animator _selectedAnimator; // 정답동믈의 애니메이션을 재생하기 위한 인스턴스 입니다. 

    private bool _isAnimRandomized; // 랜덤 애니메이션을 한 번 만 하도록 보장합니다. 
    private Animator _tempAnimator;
    private int _previousAnswer = -1; // 초기에는 중복되는 정답이 있을 수 없도록 -1로 설정 합니다. 
    private int _randomAnswer; //static Answer에 할당 전, 랜덤값을 임시로 저장하는  인덱스 입니다.


    // ---------- 정답을 맞추기 전, 플레이 상황 도입 부 및 플레이 상황에서의 설정 입니다. ----------
    [Space(15f)] public float waitingTime;
   
    private Ray ray;
    private RaycastHit hitInfo;


    private GameObject _selectedAnimalGameObject; // 동물 이동의 시작위치 지정.
    private float _elapsedForMovingToSpotLight;
    private Vector3 m_vecMouseDownPos;
    private string _clickedAnimal; //입력 방식 바뀌는 경우 변수명 수정.
    private int _randomeIndex;

    [Space(15f)] [Header("On Ready Setting")] [Space(10f)]
    public float moveOutTime;

    public float rotationSpeedWhenMovingOut; // 회전 속도

    private float _moveOutElapsed;

    [Space(5f)] [Header("In Play Setting")] [Space(10f)]
    private float _moveInElapsed;

    public float moveInSeconds;

    [FormerlySerializedAs("rotationSpeedInPlay")]
    public float rotationSpeedInRound;

    [FormerlySerializedAs("waitTimeBetweenRounds")]
    public float waitTimeCorrectAnimFinish;

    public float waitTimeToBeSelectable; //동물들이 들어오고 나서 선택할 수 있게 되는 시간.

    private float _elapsedOfToBeSelectableWaitTime;
    private float _elapsedOfRoundFinished;


    [Header("In Play: Animal Size Setting")] [Space(10f)]
    public float newSize;

    public float sizeIncreasingSpeed;
    private float _defaultSize;

    [Header("In Play: Game Play Positions Setting")] [Space(10f)]
    public Transform[] inPlayPositionsWhen3 = new Transform[3];

    public Transform[] inPlayPositionsWhen4 = new Transform[4];

    [FormerlySerializedAs("inPlayPositionsWhen3ArrFistColumn")]
    [Space(10f)]
    [Header("Animal Position Settings : When three animals are selectable...")]
    [Space(15f)]
    public Transform[] inPlayPositionsWhen3FistColumn = new Transform[2];

    public Transform[] inPlayPositionsWhen3SecondColumn = new Transform[2];
    public Transform[] inPlayPositionsWhen3ThirdColumn = new Transform[2];

    [Space(10f)] [Header("Animal Position Settings : When four animals are selectable...")] [Space(15f)]
    public Transform[] inPlayPositionsWhen4FirstColumn = new Transform[2];

    public Transform[] inPlayPositionsWhen4SecondColumn = new Transform[2];
    public Transform[] inPlayPositionsWhen4ThirdColumn = new Transform[2];
    public Transform[] inPlayPositionsWhen4FourthColumn = new Transform[2];

    public Transform[,] inPlayPositionsWhen3Arr = new Transform[2, 3];
    public Transform[,] inPlayPositionsWhen4Arr = new Transform[2, 4];

    private readonly int[] randomNumberRedcord = new int[5]; // 모든 동물 배치가 뒤로가는 경우 방지용.


    private void SetTwoDimensionaTransformlArray()
    {
        // 3마리 경우의 2D 배열
        inPlayPositionsWhen3Arr[0, 0] = inPlayPositionsWhen3FistColumn[0];
        inPlayPositionsWhen3Arr[1, 0] = inPlayPositionsWhen3FistColumn[1];

        inPlayPositionsWhen3Arr[0, 1] = inPlayPositionsWhen3SecondColumn[0];
        inPlayPositionsWhen3Arr[1, 1] = inPlayPositionsWhen3SecondColumn[1];

        inPlayPositionsWhen3Arr[0, 2] = inPlayPositionsWhen3ThirdColumn[0];
        inPlayPositionsWhen3Arr[1, 2] = inPlayPositionsWhen3ThirdColumn[1];

        // 4마리 경우의 2D 배열
        inPlayPositionsWhen4Arr[0, 0] = inPlayPositionsWhen4FirstColumn[0];
        inPlayPositionsWhen4Arr[1, 0] = inPlayPositionsWhen4FirstColumn[1];

        inPlayPositionsWhen4Arr[0, 1] = inPlayPositionsWhen4SecondColumn[0];
        inPlayPositionsWhen4Arr[1, 1] = inPlayPositionsWhen4SecondColumn[1];

        inPlayPositionsWhen4Arr[0, 2] = inPlayPositionsWhen4ThirdColumn[0];
        inPlayPositionsWhen4Arr[1, 2] = inPlayPositionsWhen4ThirdColumn[1];

        inPlayPositionsWhen4Arr[0, 3] = inPlayPositionsWhen4FourthColumn[0];
        inPlayPositionsWhen4Arr[1, 3] = inPlayPositionsWhen4FourthColumn[1];
    }


    [Space(5f)] [Header("On Correct Setting")] [Space(10f)]
    public Transform animalMovePositionToSpotLight; // 정답을 맞춘 경우 움직여야 할 위치

    public float movingTimeSecWhenCorrect;
    private float _lerpForMovingDown;
    private float _elapsedForMovingToTouchDownPlace;
    public float moveDownTime;
    public Transform touchDownPosition;
    private bool _isSizeLerpInitialized;
    private float _currentSizeLerp;
    private float lerp;


    private float _elapsedForNextRound;

    [Space(5f)] [Header("On Finished Setting")]
    public float finishedMoveInTime;


    private float _elapsedForFinishMoveIn;

    [Space(10f)] [Header("References")] [Space(10f)]
    private AnimalShaderController _animalShaderController;

    private AnimalController _animalController;


    // UI 출력을 위한 Event 처리
    [Header("UI Events")] [Space(10f)]

    //AnimalController 관련 이벤트 
    public static int totalAnimalCount;

    public static event Action onAllAnimalsInitialized;
    private static int initializedAnimalsCount;
    public static event Action onGameStartEvent;
    public static event Action onRoundReadyEvent;
    public static event Action onCorrectedEvent;
    public static event Action onRoundStartedEvent;
    public static event Action onRoundFinishedEvent;
    public static event Action onGameFinishedEvent;


    /// <summary>
    ///     디버그용 씬 재로드 함수 입니다. static 변수는 초기화 되지 않으므로 사용 시 주의합니다.
    /// </summary>
    private void ReloadCurrentScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 해당 인덱스의 씬을 다시 로드합니다.
        SceneManager.LoadScene(currentSceneIndex);
    }

    /// <summary>
    ///     디버그용 재생속도 컨트롤 함수 입니다.
    /// </summary>
    /// <param name="speed"></param>
    public static void SetTimeScale(float speed)
    {
        Time.timeScale = speed;
    }

    private void SetRandomSeed()
    {
        var random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        var value = Mathf.PerlinNoise(Time.time, 0.0f);
    }

    // ------------------------- ▼ 유니티 루프 ----------------------------
    private void Awake()
    {
        SetTimeScale(1);

        SetRandomSeed();
        SetResolution(1920, 1080, TARGET_FRAME);
        totalAnimalCount = allAnimals.Count;
        onAllAnimalsInitialized += SetAndInitializedAnimals;
        onCorrectedEvent += PlayAnswerParticle;
        SetTwoDimensionaTransformlArray();
    }

    private void Start()
    {
        isRoundFinished = true; // 첫번째 라운드 세팅을 위해 true 로 설정하고 시작. 리팩토링 예정
    }


    /// <summary>
    ///     시퀀스 구조로, 각 조건마다 조건에 해당하는 애니메이션을 실행할 수 있도록 구성.
    ///     Status 추가 및 대규모 로직 구성 시 FSM으로 재설계 권장
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ReloadCurrentScene();
        
        //디버그 및 UI용 재생속도 조정 함수
        if (!isGameStopped)
            SetTimeScale(GAME_PROGRESSING_SPEED_COPY);
        else if (isGameStopped) SetTimeScale(IS_GAME_STOPPED);

        
        //카메라 도착 시 일정시간 지나면 게임 시작.
        if (isCameraArrivedToPlay && gameStartWaitSecondsForBackgroundChange > _elapsedForMovingToSpotLight)
            StartGameAndInvoke();
        

        //1. 게임 시작 시 ---------------------------------
        if (isGameStarted && isGameFinished == false)
        {
                CheckInitialReady();
                PlayClickOnScreenEffect();//파티클
            
                //2. 라운드 시작 준비 완료 시 ------------------------------
                if (isRoundReady) //1회 실행 보장.
                {
                    IncrementRoundCount();
                    if (isGameFinished = CheckGameFinished())
                    {
                        return;
                    }
                    
                    onRoundReadyEvent?.Invoke();
                    InitializeOnReady();
                    ResetAndInitializeBeforeStartingRound(); // 동물 리스트 초기화.
                    //동물 애니메이션, 로컬스케일 초기화.
                    selectableAnimalsCount = UnityEngine.Random.Range(3, 5);
                    SelectRandomAnimals(selectableAnimalsCount);
#if UNITY_EDITOR
                    Debug.Log("준비 완료");
#endif
                    onRoundStartedEvent?.Invoke();
                }

                //3. 라운드 시작 시 ------------------
                if (isRoundStarted)
                {
                    //PlayInPlayAnimation();
#if UNITY_EDITOR
                    Debug.Log("라운드 시작!");
#endif
                    InitializeAndSetTimerOnStarted();
                    //동물 쉐이더 글로우가 켜질 때 선택가능.
                    if (AnimalShaderController.isGlowOn)
                    {
#if UNITY_EDITOR

#endif
                        ClickOnObject();
                    }
                }
         

            //4. 정답 맞춘 경우 ------------------
            if (isCorrected)
            {
                isRoundStarted = false;
#if UNITY_EDITOR
                Debug.Log("정답!");
#endif
                InitializeAndSetTimerOnCorrect();
            }

            //5. 라운드 끝난 경우 ------------------ -> 1 or 6
            if (isRoundFinished)
            {
                IntializeAndInvokeWhenRoundFinished();
#if UNITY_EDITOR
                Debug.Log("라운드 종료!");
#endif
                MoveOutOfScreen();
            }


            if (_elapsedForNextRound > waitTimeForNextRound)
            {
                isRoundReady = true;
                isRoundFinished = false;

                //UI의 문구를 초기화 하는 이벤트 입니다.
            }
        }

        //6. 게임종료 신 ------------------
        if (isGameFinished)
        {
            InitializeAndSetTimerOnGameFinished();
#if UNITY_EDITOR
            Debug.Log("게임종료");
#endif
            if (!_isGameEventInvoked)
            {
                onGameFinishedEvent?.Invoke();
                _isGameEventInvoked = true;
            }
        }
    }


    // ------------------------- ▼ 메소드 목록 ------------------------------------------------

    public static void AnimalInitialized()
    {
        initializedAnimalsCount++;

        if (initializedAnimalsCount >= totalAnimalCount)
        {
            isAnimalTransformSet = true;
            Debug.Log("Initializing Event Occured!");
            onAllAnimalsInitialized?.Invoke();
        }
    }


    private bool _isRandomized;

    private void InitializeAndSetTimerOnGameFinished()
    {
        isGameStarted = false;


        if (_elapsedForFinishMoveIn < finishedMoveInTime) _elapsedForFinishMoveIn += Time.deltaTime;
    }

    private void IntializeAndInvokeWhenRoundFinished()
    {
        if (isCorrected)
        {
            onRoundFinishedEvent?.Invoke();
            isCorrected = false;
        }

        _moveOutElapsed += Time.deltaTime;
        _elapsedForNextRound += Time.deltaTime;
    }

    private void InitializeAndSetTimerOnCorrect()
    {
        isRoundStarted = false;
        _moveOutElapsed = 0f;

        _elapsedOfRoundFinished += Time.deltaTime;
        if (_elapsedOfRoundFinished > waitTimeCorrectAnimFinish) isRoundFinished = true;
    }

    private bool _isGameStartEventRun;

    private void StartGameAndInvoke()
    {
        _elapsedForMovingToSpotLight += Time.deltaTime;

        if (gameStartWaitSecondsForBackgroundChange < _elapsedForMovingToSpotLight && !isGameStarted)
        {
            isGameStarted = true;

            onGameStartEvent?.Invoke();
            _isGameStartEventRun = true;
        }
    }

    private void InitializeOnReady()
    {
        isRoundStarted = true;
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
        ;
        foreach (var pair in animalGameOjbectDictionary)
        {
            // var animalName = pair.Key;
            var gameObject = pair.Value;

            //Scriptable 오브젝트의 Monobehaviour 상속 불가능으로 인한 참조 방식 수정 
            var _aniamalcontroller = gameObject.GetComponent<AnimalController>();
            var _animalData = _aniamalcontroller._animalData;

            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position,
                _animalData.initialPosition, _elapsedForFinishMoveIn / finishedMoveInTime);

            PlayGameFinishAnimation(gameObject);
        }
    }

    private void PlayGameFinishAnimation(GameObject gameObj)
    {
        var animator = gameObj.GetComponent<Animator>();
        animator.SetBool(AnimalData.IS_GAME_FINISHED_ANIM, true);
    }

    private int _currentRepeatCount;

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
    private void ClickOnObject()
    {
        // 마우스 클릭 시
        if (Input.GetMouseButtonDown(0) && !isCorrected)
        {
            m_vecMouseDownPos = Input.mousePosition;

            var ray = Camera.main.ScreenPointToRay(m_vecMouseDownPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, playObejctInteractableLayer))
                if (animalGameOjbectDictionary.ContainsKey(hit.collider.name))
                {
                    Debug.Log("클릭!");
                    _clickedAnimal = hit.collider.name;


                    //정답인 경우
                    if (_clickedAnimal == answer)
                    {
                        //1회실행 보장용
                        if (!isCorrected)
                        {
                            SetAnimalData(_clickedAnimal);
                            onCorrectedEvent?.Invoke();
                            isCorrected = true;
                        }

                        //moving에서의 lerp
                        _elapsedForMovingToSpotLight = 0;

                        //sizeIncrease()의 lerp
                        _currentSizeLerp = 0;
                    }
                }
        }
    }

    private RaycastHit hitSecond;

    private void PlayClickOnScreenEffect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_vecMouseDownPos = Input.mousePosition;

            var raySecond = Camera.main.ScreenPointToRay(m_vecMouseDownPos);


            if (Physics.Raycast(raySecond, out hitSecond, Mathf.Infinity, UIInteractableLayer))
            {
                Debug.Log("파티클 재생");
                clickParticleSystem.Stop(); // 현재 재생 중인 파티클이 있다면 중지합니다.
                clickParticleSystem.transform.position = hitSecond.point; // 파티클 시스템을 클릭한 위치로 이동시킵니다.
                clickParticleSystem.Play(); // 파티클 시스템을 재생합니다.
            }
        }
    }

    private void PlayAnswerParticle()
    {
        Debug.Log("정답 파티클 재생");
        answerParticleSystem.Stop(); // 현재 재생 중인 파티클이 있다면 중지합니다.
        answerParticleSystem.transform.position = hitSecond.point; // 파티클 시스템을 클릭한 위치로 이동시킵니다.
        answerParticleSystem.Play(); // 파티클 시스템
    }


    /// <summary>
    ///     정답 맞출 시 UI에게 지시문을 플레이 하도록 하는 이벤트 발생용 함수입니다.
    /// </summary>
    private void IncreaseScale(GameObject gameObject, float defaultSize, float increasedSize)
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
                increasedSize, defaultSize,
                _currentSizeLerp);


        gameObject.transform.localScale = Vector3.one * lerp;
    }


    /// <summary>
    ///     각종 러프 함수 및 자료를 다음 라운드를 위해 초기화 하는 함수 입니다.
    /// </summary>
    public void ResetAndInitializeBeforeStartingRound()
    {
        _elapsedOfToBeSelectableWaitTime = 0f;
        _moveInElapsed = 0f;
        _elapsedForNextRound = 0f;
        _selectedAnimals.Clear(); // 0,1,2 인덱스에 동물 리스트 쌓이지 않도록 초기화
    }

    public void SetAnimalData(string animalName)
    {
        if (animalGameOjbectDictionary.TryGetValue(animalName, out var animalObj))
        {
            _selectedAnimalGameObject = animalObj;
            _selectedAnimalData = _selectedAnimalGameObject.GetComponent<AnimalController>()._animalData;
        }
    }

    /// <summary>
    ///     딕셔너리는 클릭 시, hit.name과 딕셔너리 안의 자료를 비교할 떄 사용합니다.
    ///     리스트 자료구조는 동물의 순서를 랜덤으로 섞고 동물들을 배치할 때 사용합니다.
    /// </summary>
    [SerializeField] public GameObject animal;

    private void SetAndInitializedAnimals()
    {
        foreach (var animalData in allAnimals)
        {
            //생성
            var thisAnimal = Instantiate(animalData.animalPrefab,
                animalData.initialPosition, animalData.initialRotation,
                animal.transform);


            // 이름 뒤에 (Clone) 자동으로 붙는 것 제거.
            thisAnimal.name = animalData.englishName;

            //크기 지정
            thisAnimal.transform.localScale = Vector3.one * animalData.defaultSize;


            // 자료구조에 추가..
            animalGameOjbectDictionary.Add(animalData.englishName, thisAnimal);

            _animalList.Add(thisAnimal);
        }
    }


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
                _lerpForMovingDown += _elapsedForMovingToTouchDownPlace / moveDownTime;
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

                // DecreaseScale(_selectedAnimalGameObject, _selectedAnimalData.increasedSize,_selectedAnimalData.defaultSize);

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

            var animalData = gameObj.GetComponent<AnimalController>()._animalData;
            animalData.inPlayPosition = moveOutPositionA.transform;
        }
        else
        {
            gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                moveOutPositionB.position, _moveOutElapsed / moveOutTime);
            RotateTowards(gameObj.transform, moveOutPositionB.position);

            var animalData = gameObj.GetComponent<AnimalController>()._animalData;
            animalData.inPlayPosition = moveOutPositionB.transform;
        }
    }

 
    /// <summary>
    ///     캐릭터가 밖으로 나갈 때, TargetPosition방향으로 회전하는 함수 입니다.
    ///     <param name="currentPosition"> 회전 시킬 객체 </param>
    ///     <param name="targetPosition">  바라 볼 방향</param>
    /// </summary>
    private void RotateTowards(Transform currentPosition, Vector3 targetPosition)
    {
        var direction = targetPosition - currentPosition.position;

        var targetRotation = Quaternion.LookRotation(direction);

        currentPosition.rotation = Quaternion.Slerp(currentPosition.rotation, targetRotation,
            rotationSpeedWhenMovingOut * Time.deltaTime);
    }

    public int selectableAnimalsCount;

    /// <summary>
    ///     2차원 배열을 사용한 포지션 세팅에서, 인덱스를 랜덤으로 가져옵니다.
    /// </summary>
    private bool CheckCurrentAnimalsAreOnBackside(int[] randomNumberRedcord, int index, int animalCount)
    {
        if (index < animalCount - 1 || animalCount < 3 || animalCount > randomNumberRedcord.Length) return false;
        for (var i = 1; i <= animalCount - 1; i++)
            if (randomNumberRedcord[index - i] != 1)
                return false;
        return true;
    }

    public void SelectRandomAnimals(int animalCount)
    {
        _inPlayTempAnimals = new List<GameObject>(_animalList);

        for (var animalOrder = 0; animalOrder < animalCount; animalOrder++)
        {
            var randomIndex = UnityEngine.Random.Range(0, _inPlayTempAnimals.Count);

            //가운데 기린이 오는 것 방지.
            while (animalOrder == 1 && _inPlayTempAnimals[randomIndex].name == "giraffe")
                randomIndex = UnityEngine.Random.Range(0, _inPlayTempAnimals.Count);


            _selectedAnimals.Add(_inPlayTempAnimals[randomIndex]);
            var animalData = _selectedAnimals[animalOrder].GetComponent<AnimalController>()._animalData;

            if (animalCount == 3)
            {
                if (CheckCurrentAnimalsAreOnBackside(randomNumberRedcord, animalOrder, animalCount))
                {
                    animalData.inPlayPosition = inPlayPositionsWhen3Arr[0, animalOrder];
                }
                else
                {
                    var randomIndexFrontOrBack = UnityEngine.Random.Range(0, 2);
                    randomNumberRedcord[animalOrder] = randomIndexFrontOrBack;
                    Debug.Log($"i값과 animalCount{animalOrder}, {animalCount}");
                    animalData.inPlayPosition = inPlayPositionsWhen3Arr[randomIndexFrontOrBack, animalOrder];
                }
            }
            else if (animalCount == 4)
            {
                if (CheckCurrentAnimalsAreOnBackside(randomNumberRedcord, animalOrder, animalCount))
                {
                    animalData.inPlayPosition = inPlayPositionsWhen4Arr[0, animalOrder];
                }
                else
                {
                    var randomIndexFrontOrBack = UnityEngine.Random.Range(0, 2);
                    randomNumberRedcord[animalOrder] = randomIndexFrontOrBack;
                    animalData.inPlayPosition = inPlayPositionsWhen4Arr[randomIndexFrontOrBack, animalOrder];
                }
            }

            _inPlayTempAnimals.RemoveAt(randomIndex); //중복 동물 선택 방지
            Debug.Log("동물 랜덤 고르기 완료");
        }

        _randomAnswer = UnityEngine.Random.Range(0, _selectedAnimals.Count);

        while (_randomAnswer == _previousAnswer) _randomAnswer = UnityEngine.Random.Range(0, _selectedAnimals.Count);

        answer = _selectedAnimals[_randomAnswer].name;
        _previousAnswer = _randomAnswer;
    }


    /// <summary>
    ///     동물 애니메이션, 로컬스케일 초기화 합니다.
    ///     동물을 플레이 위치에 놓고 선택할 수 있도록합니다.
    /// </summary>
    public void IncrementRoundCount()
    {
        _currentRoundCount++;
        
#if DEFINE_TEST
        Debug.Log($"현재 라운드: {roundCount}");
#endif
    }

    /// <summary>
    ///     모든 애니메이션의 파라미터를 false로 초기화 합니다.
    /// </summary>
    private void InitializeAllAnimatorParameters(Animator animator)
    {
        animator.SetBool(AnimalData.RUN_ANIM, false);
        animator.SetBool(AnimalData.FLY_ANIM, false);
        animator.SetBool(AnimalData.ROLL_ANIM, false);
        animator.SetBool(AnimalData.SPIN_ANIM, false);
        animator.SetBool(AnimalData.SELECTABLE_A, false);
        animator.SetBool(AnimalData.SELECTABLE_B, false);
        animator.SetBool(AnimalData.SELECTABLE_C, false);
    }

    private int _randomInPlayAnimationNumber;

    private void SetResolution(int width, int height, int targetFrame)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrame;
    }

    private bool CheckGameFinished()
    {
        if (_currentRoundCount >= roundCount)
        {
            return true;
        }
        
        return false;
    }

    
    /// <summary>
    /// 게임 재시작 시 각종 파라미터 초기화를 위한 함수입니다. 
    /// </summary>
    private void Reset()
    {
        isGameFinished = false;
        isGameStarted = false;
        isCameraArrivedToPlay = false;
        isRoundFinished = false;
        isRoundStarted = false;
        isCorrected = false;
        
        roundCount = 0;
        answer = null;
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