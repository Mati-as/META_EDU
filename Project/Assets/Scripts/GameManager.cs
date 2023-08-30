using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


// https://app.diagrams.net/?src=about#G1oTy42sV_tIyZY60bED79XlyZ1FfcSRL0
// bool 및 seconds 사용 흐름도
public class GameManager : MonoBehaviour
{
    [Header("Display Setting")] [Space(10f)] 
    public int TARGET_FRAME;


   
    [Space(15f)]
    [Header("Game Start Setting")]
    [Space(10f)]

    
    public static bool isCameraArrivedToPlay;
    public float waitTimeForNextRound;
    public float gameStartWaitSeconds;
    public static bool isGameStarted;
    public float roundStartWaitSeconds;
    public static bool isRoundStarted;
    public static bool isCorrected;
    public float correctAnimSeconds;
    public static bool isCorrectAnimationFinished;
    public float roundResetWaitSeconds;
    public static bool isRoundReady;
    public static bool isGameFinished;
    public static bool isRoudnFinished;
    public float finishAnimSeconds;
    private bool _initialRoundIsReady;
    public float waitTimeOfinitialRoundStart;
    private float _elapsedForInitialRound;

    public static string answer;

    [Space(10f)] public Transform moveOutPositionA;
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

    [Header("On Ready")] [Space(10f)]
    public float moveOutTime;
    private float _moveOutElapsed;
    
    [Header("On Play")] [Space(10f)]
    private float _moveInElapsed;
    public float moveInTime;
    public float rotationSpeedInRound;

    public float waitTimeForRoundFinished;
    private float elapsedForRoundFinished;


    [Header("On Correct")] [Space(10f)] [Space(10f)]
    public Transform AnimalMovePosition; //when user corrects an answer.

    [FormerlySerializedAs("movingTimeSec")]
    public float movingTimeSecWhenCorrect;

  
    private float _elapsedForNextRound;

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

    [FormerlySerializedAs("_onCorrectLightOn")]
    [Header("Events")] [Space(10f)] 
    [SerializeField]
    private UnityEvent _onCorrectLightOnEvent;
    // [SerializeField]
    // private UnityEvent _onCorrectLightOn;
    [SerializeField]
    private UnityEvent _onRoundFinishedLightOff;

    [FormerlySerializedAs("_QuizMessageEvent")] [SerializeField]
    private UnityEvent _quizMessageEvent;
    
    [SerializeField]
    private UnityEvent _correctMessageEvent;
    
    [SerializeField]
    private UnityEvent _messageInitializeEvent;
    
    private void Awake()
    {
        _defaultSize = dog.transform.localScale.z;


        SetResolution(1920, 1080);
        SetAnimalIntoDictionaryAndList();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TARGET_FRAME;
        
        
        
        isRoudnFinished = true;
    }

    private bool _isRandomized;

    /// <summary>
    ///     시퀀스 구조로, 각 조건마다 조건에 해당하는 애니메이션을 실행할 수 있도록 구성.
    /// </summary>
    private void Update()
    {
        //카메라 도착 시 애니메이션
        if (isCameraArrivedToPlay)
        {
            elapsedForMovingTowardMoon += Time.deltaTime;
            if (gameStartWaitSeconds < elapsedForMovingTowardMoon && !isGameStarted) isGameStarted = true;
        }

        
        
        
        if (isGameStarted)
        {
            // 맨 첫번째 라운드 시작 전, 대기시간 및 로직 설정 
            _elapsedForInitialRound += Time.deltaTime;
            CheckInitialReady();
            
            
            // 첫 라운드 이전을 roundfinish로 설정.
          
            
            if (isRoundReady)
            {
                _moveInElapsed = 0f;
                _elapsedForNextRound = 0f;
                StartRound();
                Debug.Log("준비 완료");
                
               
                isRoundStarted = true;
                
               
            }


            if (isRoundStarted)
            {
                _moveInElapsed += Time.deltaTime;
                
                Debug.Log("라운드 시작!");
              
                isRoundReady = false;
              
                
               // ~의 그림자를 맞춰보세요 재생. 
                
                MoveAndRotateAnimals(moveInTime, rotationSpeedInRound);
                SelectObject();
                
               
                
                elapsedForRoundFinished = 0f;
                _quizMessageEvent.Invoke();
           
            }

            if (isCorrected)
            {
                Debug.Log("정답!");
                isRoundStarted = false;

                PlayCorrectAnimation();
                _onCorrectLightOnEvent.Invoke();
                
                elapsedForRoundFinished += Time.deltaTime;
                if (elapsedForRoundFinished > waitTimeForRoundFinished) isRoudnFinished = true;
                
                _moveOutElapsed = 0f;
                _previousAnswer = answer;
              
            }

            if (isRoudnFinished)
            {  
                isCorrected = false;

                Debug.Log("라운드 종료!");
                
                MoveOutOfScreen();
                _onRoundFinishedLightOff.Invoke();
               
                _moveOutElapsed += Time.deltaTime;
                _elapsedForNextRound += Time.deltaTime;

              
               
                if (_elapsedForNextRound > waitTimeForNextRound)
                {
                  
                    _messageInitializeEvent.Invoke();
                    isRoundReady = true;
                    isRoudnFinished = false;
                }
                
               
              
            }
        }

     


        //SelectObjectWithoutClick();
        // 동물의 움직임이 시작됨을 표시
        if (Input.GetKeyDown(KeyCode.R)) ReloadCurrentScene();
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
    public void PlayCorrectAnimation()
    {
        var t = Mathf.Clamp01(elapsedForMovingTowardMoon / movingTimeSecWhenCorrect);


        if (isCorrected)
        {
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

    public void ResetAndInitialize()
    {
        answer = string.Empty;
        _elapsedForNextRound = 0f;
        elapsedForRoundFinished = 0f;
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
    ///     딕셔너리는 클릭할 때,
    ///     리스트 자료구조는 랜덤으로 섞어 동물들을 배치할 때 사용합니다.
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

    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
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
                    moveOutPositionA.position, _moveOutElapsed/moveOutTime);

            else
                gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                    moveOutPositionB.position, _moveOutElapsed/moveOutTime);
            _randomeIndex++;
        }
    }


    private string _previousAnswer;
    public void SelectRandomThreeAnimals()
    {
        Debug.Log("동물 랜덤 고르기 완료");
        tempAnimals = new List<GameObject>(animalList);
        for (var i = 0; i < 3; i++)
        {
            var randomIndex = Random.Range(0, tempAnimals.Count);
            selectedAnimals.Add(tempAnimals[randomIndex]);
            tempAnimals.RemoveAt(randomIndex);
        }

        var randomAnswer = Random.Range(0, tempAnimals.Count);

       
        answer = selectedAnimals[randomAnswer].name;
       
    }


    /// <summary>
    /// 동물을 플레이 위치에 놓고 선택할 수 있도록합니다. 
    /// </summary>
    public void StartRound()
    {

        foreach (var gameObj in animalList)
        {
            _tempAnimator = gameObj.GetComponent<Animator>();
            _tempAnimator.SetBool(RUN_ANIM, false);
            gameObj.transform.localScale = Vector3.one *_defaultSize;
        }

        SelectRandomThreeAnimals();
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
}


//정답을 맞추고 동물의 애니메이션 걸리기 시간. 
// IEnumerator SetAnimation()
// {
//     yield return new WaitForSeconds(correctReactionOffset);
//     Animator animator = selectedAnimalGameObject.GetComponent<Animator>();
//     animator.SetBool(correctAnim,true);
//     yield return new WaitForNextFrameUnit;
// }

/// <summary>
/// 마우스로 입력받는 방식이 아닌 경우 예시.
/// </summary>
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