using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


// https://app.diagrams.net/?src=about#G1oTy42sV_tIyZY60bED79XlyZ1FfcSRL0
// 시퀀스 흐름도입니다. 
public class GameManager : MonoBehaviour
{
    // 게임 시퀀스를 위한 스태틱 불 목록입니다.
    // GameManager이외에서 접근하지 않도록 합니다. 
    public static bool isCameraArrivedToPlay { get; set; }
    public static bool isGameStarted { get; private set; }
    private bool _initialRoundIsReady; //최초 라운드 시작 이전을 컨트롤 하기 위한 논리연산자 입니다. 
    public static bool isRoundReady { get; private set; }
    public static bool isRoundStarted { get; private set; }
    public static bool isCorrected { get; private set; }
    public static bool isGameFinished { get; private set; }
    public static bool isRoudnFinished { get; private set; }


    public float waitTimeOfinitialRoundStart;
    private float _elapsedForInitialRound;
    public float waitTimeForNextRound;
    public float gameStartWaitSeconds;
    public static string answer;

    //08-31 미사용 논리값 목록
    //public static bool isCorrectAnimationFinished{ get; private set; }
    // public float roundStartWaitSeconds;
    // public float correctAnimSeconds;
    // public float roundResetWaitSeconds;
    // public float finishAnimSeconds;


    [Header("Display Setting")] [Space(10f)]
    public int TARGET_FRAME;


    [Space(15f)] [Header("Game Rule Setting")] [Space(10f)]
    public int roundCount;

    private int _currentRoundCount;


    [Space(15f)] [Header("Game Start Setting")] [Space(10f)] [Space(10f)]
    public Transform moveOutPositionA;

    public Transform moveOutPositionB;


    [Space(30f)] [Header("Game Objects(Animal) Setting")]
    public GameObject tortoise;

    public GameObject rabbit;
    public GameObject dog;
    public GameObject parrot;
    public GameObject mouse;
    public GameObject cat;


    [Header("Animal Movement Setting")] [Space(10f)]
    public Transform lookAtPosition;

    public float rotationSpeed;
    [Space(15f)] public float waitingTime;

    public Dictionary<string, GameObject> animalDictionary = new();


    private readonly List<GameObject> animalList = new();
    private GameObject selectedAnimalGameObject; // 동물 이동의 시작위치 지정.
    private float elapsedForMovingTowardMoon;
    private bool isMovable;
    private Vector3 m_vecMouseDownPos;
    private string selectedAnimal;

    private int _randomeIndex;
    public float moveOutSpeed;

    [Header("Animal Size Setting")] [Space(10f)]
    public float newSize;

    public float sizeIncreasingSpeed;
    private float _defaultSize;

    [Header("Game Play Setting")] [Space(10f)]
    public Transform playPositionA;

    public Transform playPositionB;
    public Transform playPositionC;

    [Header("On Ready")] [Space(10f)] public float moveOutTime;
    private float _moveOutElapsed;

    [Header("On Play")] [Space(10f)] private float _moveInElapsed;
    public float moveInTime;
    public float rotationSpeedInRound;

    public float waitTimeForRoundFinished;
    private float elapsedForRoundFinished;


    [Header("On Correct")] [Space(10f)] [Space(10f)]
    public Transform AnimalMovePosition; //when user corrects an answer.

    [FormerlySerializedAs("movingTimeSec")]
    public float movingTimeSecWhenCorrect;


    private float _elapsedForNextRound;


    [FormerlySerializedAs("MoveInTimeWhenFinished")]
    [FormerlySerializedAs("finishMoveInTime")]
    [Header("On Finished")]
    [Space(10f)]
    public float FinishedMoveInTime;

    private float _elapsedForFinishMoveIn;

    //게임 종료시 동물들이 원래 위치로 돌아 올 수 있도록 하는 디폴트 위치 입니다. 
    //(08-31-22)
    //추후 어떤 동물을 어떻게 배치할지 정확히 정해지면, 변수명을 수정해야합니다.
    private  Transform[] defalutPositions = new Transform[6];

    public Transform _parrotDefaultPosition;
    public Transform _dogDefaultPosition;
    public Transform _tortoiseDefaultPosition;
    public Transform _mouseDefaultPosition;
    public Transform _rabbitDefaultPosition;
    public Transform _catDefaultPosition;

    private enum animalPosition
    {
        parrot,
        dog,
        tortoise,
        mouse,
        rabbit,
        cat
    }


    [Header("References")] [Space(10f)] public InstructionUI instructionUI;

    private Ray ray;
    private RaycastHit hitInfo;
    public LayerMask interactableLayer;


    private float _currentLerp;
    private float lerp;
    [SerializeField] private Light _spotLight;

    private readonly List<GameObject> selectedAnimals = new();
    private List<GameObject> tempAnimals;


    // 애니메이션 로직
    private readonly int ROLL_ANIM = Animator.StringToHash("Roll");
    private readonly int CLICK_ANIM = Animator.StringToHash("Clicked");
    private readonly int SPIN_ANIM = Animator.StringToHash("Spin");
    private readonly int RUN_ANIM = Animator.StringToHash("Run");
    private readonly int IDLE_ANIM = Animator.StringToHash("idle");


    // UI 출력을 위한 Event 처리

    [FormerlySerializedAs("_onCorrectLightOn")] [Header("Events")] [Space(10f)] [SerializeField]
    private UnityEvent _onCorrectLightOnEvent;

    // [SerializeField]
    // private UnityEvent _onCorrectLightOn;
    [SerializeField] private UnityEvent _onRoundFinishedLightOff;

    [FormerlySerializedAs("_QuizMessageEvent")] [SerializeField]
    private UnityEvent _quizMessageEvent;

    [SerializeField] private UnityEvent _correctMessageEvent;

    [SerializeField] private UnityEvent _messageInitializeEvent;

    [SerializeField] private UnityEvent _finishedMessageEvent;


    // 플레이상 중복 방지 및 기타 버그 방지용 로직


    private Dictionary<GameObject, animalPosition> animalDict;

    private void Awake()
    {
        animalDict = new Dictionary<GameObject, animalPosition>
        {
            { parrot, animalPosition.parrot },
            { dog, animalPosition.dog },
            { tortoise, animalPosition.tortoise },
            { mouse, animalPosition.mouse },
            { rabbit, animalPosition.rabbit },
            { cat, animalPosition.cat }
        };


        SetResolution(1920, 1080);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TARGET_FRAME;


        SetAnimalIntoDictionaryAndList();
    }

    private void Start()
    {
        isRoudnFinished = true; // 첫번째 라운드 세팅을 위해 true 로 설정하고 시작. 리팩토링 예정
        _defaultSize = dog.transform.localScale.z;
        SetDefaultPositions();
    }

    private bool _isRandomized;

    /// <summary>
    ///     시퀀스 구조로, 각 조건마다 조건에 해당하는 애니메이션을 실행할 수 있도록 구성.
    /// </summary>
    private void Update()
    {
        //카메라 도착 시 애니메이션
        if (isCameraArrivedToPlay && gameStartWaitSeconds > elapsedForMovingTowardMoon)
        {
           
            elapsedForMovingTowardMoon += Time.deltaTime;
            if (gameStartWaitSeconds < elapsedForMovingTowardMoon && !isGameStarted) isGameStarted = true;
        }


        if (isGameStarted && isGameFinished == false)
        {
            // 맨 첫번째 라운드 시작 전, 대기시간 및 로직 설정 
            _elapsedForInitialRound += Time.deltaTime;
            CheckInitialReady();


            // 첫 라운드 이전을 roundfinish로 설정.


            if (isRoundReady)
            {
                CheckGameFinished();
                isRoundStarted = true;


                // 동물 리스트 초기화. 
                ResetAndInitializeBeforeStartingRound();

                //correct anim 관련 bool 초기화.
                _isAnimRandomized = false;

                //동물 애니메이션, 로컬스케일 초기화.
                StartRound();

                // 리스트 랜덤구성
                SelectRandomThreeAnimals();
                Debug.Log("준비 완료");
            }


            if (isRoundStarted)
            {
                isRoundReady = false;
                elapsedForRoundFinished = 0f;


                _moveInElapsed += Time.deltaTime;
                Debug.Log("라운드 시작!");

                MoveAndRotateAnimals(moveInTime, rotationSpeedInRound);
                SelectObject();

                _quizMessageEvent.Invoke(); // ~의 그림자를 맞춰보세요 재생. 
            }

            if (isCorrected)
            {
                Debug.Log("정답!");
                isRoundStarted = false;

                PlayCorrectAnimation();

                _onCorrectLightOnEvent.Invoke(); // D-Light reffering..
                _correctMessageEvent.Invoke(); // UI Instruction referring..

                elapsedForRoundFinished += Time.deltaTime;
                if (elapsedForRoundFinished > waitTimeForRoundFinished) isRoudnFinished = true;

                _moveOutElapsed = 0f;
            }

            if (isRoudnFinished)
            {
                isCorrected = false;

                Debug.Log("라운드 종료!");

                MoveOutOfScreen();
                _onRoundFinishedLightOff.Invoke();

                _moveOutElapsed += Time.deltaTime;
                _elapsedForNextRound += Time.deltaTime;
            }


            if (_elapsedForNextRound > waitTimeForNextRound)
            {
                //InstructionUI 의 in
                _messageInitializeEvent.Invoke();
                isRoundReady = true;
                isRoudnFinished = false;
            }
        }


        if (isGameFinished)
        {
            isGameStarted = false;
            Debug.Log("게임종료");
            
            
            if (_elapsedForFinishMoveIn < FinishedMoveInTime) _elapsedForFinishMoveIn += Time.deltaTime;

           
            MoveInWhenGameIsFinished();
            _finishedMessageEvent.Invoke();
        }


        //SelectObjectWithoutClick();
        // 동물의 움직임이 시작됨을 표시
        if (Input.GetKeyDown(KeyCode.R)) ReloadCurrentScene();
    }


    private void SetDefaultPositions()
    {
        defalutPositions[(int)animalPosition.parrot] = _parrotDefaultPosition;
        defalutPositions[(int)animalPosition.dog] = _dogDefaultPosition;
        defalutPositions[(int)animalPosition.tortoise] = _tortoiseDefaultPosition;
        defalutPositions[(int)animalPosition.mouse] = _mouseDefaultPosition;
        defalutPositions[(int)animalPosition.rabbit] = _rabbitDefaultPosition;
        defalutPositions[(int)animalPosition.cat] = _catDefaultPosition;
    }


    /// <summary>
    ///     게임이 끝나고 원래위치고 돌아오기 위해 사용합니다.
    /// </summary>
    private void MoveInWhenGameIsFinished()
    {

        
        foreach (var pair in animalDict)
        {
            var gameObj = pair.Key;
            var position = pair.Value;

            gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                defalutPositions[(int)position].position, _elapsedForFinishMoveIn / FinishedMoveInTime);
         
        }
    }


    private void CheckInitialReady()
    {
        if (_elapsedForInitialRound > waitTimeOfinitialRoundStart && _initialRoundIsReady == false)
        {
            _initialRoundIsReady = true;
            isRoudnFinished = true;
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
        if (Input.GetMouseButtonDown(0))
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


            if (Physics.Raycast(ray, out hit) && isCorrected == false) // 정답을 맞추지 않은 상태라면...(중복정답 방지)
                if (animalDictionary.ContainsKey(hit.collider.name))
                {
                    selectedAnimal = hit.collider.name;
                    if (selectedAnimal == answer)
                    {
                        isCorrected = true;
                        SetAnimal(selectedAnimal);
                        InvokeOncorrect();
                    }

                    //moving에서의 lerp
                    elapsedForMovingTowardMoon = 0;

                    //sizeIncrease()의 lerp
                    _currentLerp = 0;
                }
        }
    }

    private void InvokeOncorrect()
    {
        _correctMessageEvent.Invoke();
        _onCorrectLightOnEvent.Invoke();
    }


    private void IncreaseScale()
    {
        _currentLerp += sizeIncreasingSpeed * Time.deltaTime;

        lerp =
            Lerp2D.EaseInBounce(
                _defaultSize, newSize,
                _currentLerp);


        selectedAnimalGameObject.transform.localScale = Vector3.one * lerp;
    }


    /// <summary>
    ///     마우스로 선택 된 동물을 이동시키는 함수 입니다.
    ///     선택된 동물이 이동 시, 다른 동물들은 선택될 수 없도록 추가 로직이 필요합니다.
    /// </summary>
    private Animator _selectedAnimator;

    private bool _isAnimRandomized;

    public void PlayCorrectAnimation()
    {
        var t = Mathf.Clamp01(elapsedForMovingTowardMoon / movingTimeSecWhenCorrect);


        if (isCorrected)
        {
            _selectedAnimator = selectedAnimalGameObject.GetComponent<Animator>();

            if (_isAnimRandomized == false)
            {
                var randomAnimNum = Random.Range(0, 3);
                _isAnimRandomized = true;

                switch (randomAnimNum)
                {
                    case 0:
                        _selectedAnimator.SetBool(ROLL_ANIM, true);
                        break;
                    case 1:
                        _selectedAnimator.SetBool(CLICK_ANIM, true);
                        break;
                    case 2:
                        _selectedAnimator.SetBool(SPIN_ANIM, true);
                        break;
                }
            }


            IncreaseScale();
            selectedAnimalGameObject.transform.position =
                Vector3.Lerp(selectedAnimalGameObject.transform.position,
                    AnimalMovePosition.position, t);

            var directionToTarget =
                lookAtPosition.position - selectedAnimalGameObject.transform.position;

            var targetRotation =
                Quaternion.LookRotation(directionToTarget);
            selectedAnimalGameObject.transform.rotation =
                Quaternion.Slerp(
                    selectedAnimalGameObject.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    ///     각종 러프 함수 및 자료를 다음 라운드를 위해 초기화 하는 함수 입니다.
    /// </summary>
    public void ResetAndInitializeBeforeStartingRound()
    {
        _moveInElapsed = 0f;
        _elapsedForNextRound = 0f;
        selectedAnimals.Clear();
    }

    public void SetAnimal(string animalName)
    {
        if (animalDictionary.TryGetValue(animalName, out var animalObj))
            selectedAnimalGameObject = animalObj;
    }

    private void ReloadCurrentScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 해당 인덱스의 씬을 다시 로드합니다.
        SceneManager.LoadScene(currentSceneIndex);
    }


    /// <summary>
    ///     딕셔너리는 클릭 시, hit.name과 딕셔너리 안의 자료를 비교할 떄 사용합니다.
    ///     리스트 자료구조는 동물의 순서를 랜덤으로 섞고 동물들을 배치할 때 사용합니다.
    /// </summary>
    private void SetAnimalIntoDictionaryAndList()
    {
        animalDictionary.Add(nameof(tortoise), tortoise);
        animalDictionary.Add(nameof(cat), cat);
        animalDictionary.Add(nameof(rabbit), rabbit);
        animalDictionary.Add(nameof(dog), dog);
        animalDictionary.Add(nameof(parrot), parrot);
        animalDictionary.Add(nameof(mouse), mouse);

        animalList.Add(tortoise);
        animalList.Add(cat);
        animalList.Add(rabbit);
        animalList.Add(dog);
        animalList.Add(parrot);
        animalList.Add(mouse);
    }


    private Animator _tempAnimator;

    private void MoveOutOfScreen()
    {
        foreach (var gameObj in animalList)
        {
            _tempAnimator = gameObj.GetComponent<Animator>();
            _tempAnimator.SetBool(RUN_ANIM, true);

            if (_randomeIndex % 2 == 0)
                gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                    moveOutPositionA.position, _moveOutElapsed / moveOutTime);

            else
                gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                    moveOutPositionB.position, _moveOutElapsed / moveOutTime);
            _randomeIndex++;
        }
    }

    private int _previousAnswer = -1; // 초기에는 중복되는 정답이 있을 수 없도록 -1로 설정합니다. 
    private int _randomAnswer;

    public void SelectRandomThreeAnimals()
    {
        tempAnimals = new List<GameObject>(animalList);

        for (var i = 0; i < 3; i++)
        {
            Debug.Log("동물 랜덤 고르기 완료");
            var randomIndex = Random.Range(0, tempAnimals.Count);
            selectedAnimals.Add(tempAnimals[randomIndex]);
            tempAnimals.RemoveAt(randomIndex);
        }


        _randomAnswer = Random.Range(0, tempAnimals.Count);

        while (_randomAnswer == _previousAnswer) _randomAnswer = Random.Range(0, tempAnimals.Count);


        answer = selectedAnimals[_randomAnswer].name;

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

        foreach (var gameObj in animalList)
        {
            _tempAnimator = gameObj.GetComponent<Animator>();
            _tempAnimator.SetBool(RUN_ANIM, false);
            _tempAnimator.SetBool(CLICK_ANIM, false);
            _tempAnimator.SetBool(ROLL_ANIM, false);
            _tempAnimator.SetBool(SPIN_ANIM, false);
            gameObj.transform.localScale = Vector3.one * _defaultSize;
        }
    }


    /// <summary>
    ///     동물을 회전시키는 함수 입니다.
    ///     정답을 맞추기 전, 동물이 플레이화면에 나타날때 회전하는 함수입니다.
    ///     정답을 맞춘 후 나오는 함수와 구분하여 사용합니다.
    /// </summary>
    public void MoveAndRotateAnimals(float _moveInTime, float _rotationSpeedInRound)
    {
        selectedAnimals[0].transform.position =
            Vector3.Lerp(selectedAnimals[0].transform.position,
                playPositionA.position, _moveInElapsed / _moveInTime);

        selectedAnimals[0].transform.Rotate(0, _rotationSpeedInRound * Time.deltaTime, 0);


        selectedAnimals[1].transform.position =
            Vector3.Lerp(selectedAnimals[1].transform.position,
                playPositionB.position, _moveInElapsed / _moveInTime);

        selectedAnimals[1].transform.Rotate(0, _rotationSpeedInRound * Time.deltaTime, 0);


        selectedAnimals[2].transform.position =
            Vector3.Lerp(selectedAnimals[2].transform.position,
                playPositionC.position, _moveInElapsed / _moveInTime);

        selectedAnimals[2].transform.Rotate(0, _rotationSpeedInRound * Time.deltaTime, 0);
    }

    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
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