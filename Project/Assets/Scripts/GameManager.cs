using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")] [Space(10f)] public int TARGET_FRAME;


    [Header("Game Start Setting")]

    // https://app.diagrams.net/?src=about#G1oTy42sV_tIyZY60bED79XlyZ1FfcSRL0
    // bool 및 seconds 사용 흐름도
    public static bool isCameraArrivedToPlay;

    public float gameStartWaitSeconds;
    public static bool isGameStarted;
    public float roundStartWaitSeconds;
    public static bool isRoundStarted;
    public static bool isCorrected;
    public float correctAnimSeconds;
    public static bool isCorrectAnimationFinished;
    public float roundResetWaitSeconds;
    public static bool isRoundFinished;
    public static bool isGameFinished;
    public float finishAnimSeconds;

    public static string answer;

    [Space(10f)] public Transform moveOutPositionA;
    public Transform moveOutPositionB;

    [Space(10f)] public Transform AnimalMovePosition; //when user corrects an answer.

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
    [Space(15f)] public float movingTimeSec;
    public float waitingTime;

    public Dictionary<string, GameObject> animalDictionary = new();
    private readonly List<GameObject> animalList = new();
    private GameObject selectedAnimalGameObject; // 동물 이동의 시작위치 지정.
    private float elapsedTime;
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


    [Header("References")] public InstructionUI instructionUI;

    private Ray ray;
    private RaycastHit hitInfo;
    public LayerMask interactableLayer;

    private float _elapsedForNextRound;
    private float _currentLerp;
    private float lerp;

    private readonly List<GameObject> selectedAnimals = new();
    private List<GameObject> tempAnimals;


    // 애니메이션 로직
    private int rollAnim = Animator.StringToHash("Roll");
    private int clickedAnim = Animator.StringToHash("Clicked");
    private int spinAnim = Animator.StringToHash("Spin");
    private int runAnim = Animator.StringToHash("Run");


    private void Awake()
    {
        _defaultSize = dog.transform.localScale.z;


        SetResolution(1920, 1080);
        SetAnimalIntoDictionaryAndList();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TARGET_FRAME;
    }


    private void Update()
    {
        if (isCameraArrivedToPlay)
        {
            elapsedTime += Time.deltaTime;
            if (gameStartWaitSeconds < elapsedTime && !isGameStarted) isGameStarted = true;
        }


        if (isGameStarted)
        {
            if (isRoundStarted == false)
            {
                Debug.Log("동물 스크린 밖 이동");
                MoveOutOfScreen();
            }

            _elapsedForNextRound += Time.deltaTime;

            if (_elapsedForNextRound > roundStartWaitSeconds)
            {
                Debug.Log("라운드 시작!");
                isRoundStarted = true;
                SelectRandomThreeAnimals();
                StartRound();
            }

            if (isRoundStarted)
            {
                CheckMovable();
                SelectObject();
                PlayCorrectAnimation();
            }


            //SelectObjectWithoutClick();
            // 동물의 움직임이 시작됨을 표시

        }

        if (Input.GetKeyDown(KeyCode.R)) ReloadCurrentScene();
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

            if (isCorrected == false) // 정답을 맞추지 않은 상태라면...(중복정답 방지)
                if (Physics.Raycast(ray, out hit))
                    if (animalDictionary.ContainsKey(hit.collider.name))
                    {
                        selectedAnimal = hit.collider.name;
                        if (selectedAnimal == answer)
                        {
                            SetAnimal(selectedAnimal);
                            isCorrected = true;
                        }

                        //moving에서의 lerp
                        elapsedTime = 0;

                        //sizeIncrease()의 lerp
                        _currentLerp = 0;
                    }
        }
    }





    public void CheckMovable()
    {
        if (elapsedTime < movingTimeSec)
            isMovable = false;
        else
            isMovable = true;
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
        var t = Mathf.Clamp01(elapsedTime / movingTimeSec);


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
    }

    public void SetAnimal(string animalName)
    {
        if (animalDictionary.TryGetValue(animalName, out var animalObj))
            if (animalObj != null)
                selectedAnimalGameObject = animalObj;
    }

    private void ReloadCurrentScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 해당 인덱스의 씬을 다시 로드합니다.
        SceneManager.LoadScene(currentSceneIndex);
    }


    /// <summary>
    ///  딕셔너리는 클릭할 때,
    ///  리스트 자료구조는 랜덤으로 섞어 동물들을 배치할 때 사용합니다.
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

    public float correctReactionOffset;



    private Animator _tempAnimator;

    private void MoveOutOfScreen()
    {

        foreach (GameObject gameObj in animalList)
        {

            _tempAnimator = gameObj.GetComponent<Animator>();
            _tempAnimator.SetBool(runAnim, true);
            
            if (_randomeIndex % 2 == 0)
            {
                gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                    moveOutPositionA.position, moveOutSpeed * Time.deltaTime);

            }

            else
            {
                gameObj.transform.position = Vector3.Lerp(gameObj.transform.position,
                    moveOutPositionB.position, moveOutSpeed * Time.deltaTime);

            }



            _randomeIndex++;
        }
    }



    public void SelectRandomThreeAnimals()
    {
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

    public float moveInSpeed;

    public void StartRound()
    {
        SelectRandomThreeAnimals();

        selectedAnimals[0].transform.position =
            Vector3.Lerp(selectedAnimals[0].transform.position,
                playPositionA.position, moveInSpeed * Time.deltaTime);


        selectedAnimals[1].transform.position =
            Vector3.Lerp(selectedAnimals[1].transform.position,
                playPositionB.position, moveInSpeed * Time.deltaTime);

        selectedAnimals[2].transform.position =
            Vector3.Lerp(selectedAnimals[2].transform.position,
                playPositionC.position, moveInSpeed * Time.deltaTime);
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

