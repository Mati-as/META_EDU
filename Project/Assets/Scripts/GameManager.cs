using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")] [Space(10f)] public int TARGET_FRAME;


    [Header("Game Start Setting")] public static bool IsGameStarted;
    [Space(10f)] public float gamestartTime;

    public Transform AnimalMovePosition; //when user corrects an answer.

    [Space(30f)] [Header("Game Objects(Animal) Setting")] [Space(10f)]
    public GameObject parrot;

    public GameObject dog;
    public GameObject mouse;
    public GameObject rabbit;
    public GameObject cat;


    [Header("Animal Movement Setting")] [Space(10f)]
    public Transform lookAtPosition;

    public float rotationSpeed;
    [Space(15f)] public float movingTimeSec;
    public float waitingTime;

    public Dictionary<string, GameObject> animalDictionary = new();

    private GameObject selectedAnimalGameObject; // 동물 이동의 시작위치 지정.
    private float elapsedTime;
    private bool isMovable;
    private Vector3 m_vecMouseDownPos;
    private string selectedAnimal;

    [Header("Animal Size Setting")] [Space(10f)]
  
    public float newSize;
    
    public float sizeIncreasingSpeed;
    private float _defaultSize;

    [Header("Game Play Setting")] [Space(10f)]

    public Transform playPositionA;
    public Transform playPositionB;
    public Transform playPositionC;

    public static bool isPlayStared;
    public static bool isCameraArrivedToPlay;


 
    
    private float _elapsedForNextRound; 
    public float playWaitTime; // gamestarted time과 구분해서 사용.  
    // 게임 플레이 로직 
    private bool isCorrected;
    private int correctAnim = Animator.StringToHash("corrected");
    
    private void Awake()
    {
        _defaultSize = dog.transform.localScale.z;

        
        SetResolution(1920, 1080);
        SetAnimalIntoDictionary();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TARGET_FRAME;
    }


    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (gamestartTime < elapsedTime && !IsGameStarted) IsGameStarted = true;

        if (IsGameStarted)
        {
            
            _elapsedForNextRound += Time.deltaTime;

            if (_elapsedForNextRound > playWaitTime)
            {
                isPlayStared = true; 
            }
                
            
            //두 함수 중 동물 선택 로직은 두가지 중 하나로 정합니다. (클릭O or 클릭X)
            CheckMovable();
            SelectObject();
            //SelectObjectWithoutClick();
            // 동물의 움직임이 시작됨을 표시
            if (selectedAnimal != null)
            { 
               
                SetAnimal(selectedAnimal);
                ContinueMovingAnimal();
                IncreaseScale();
            }
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
        // 터치 시
        if (Input.touchCount > 0)
#endif
        {
#if UNITY_EDITOR
            m_vecMouseDownPos = Input.mousePosition;
#else
            m_vecMouseDownPos = Input.GetTouch(0).position;
            if (Input.GetTouch(0).phase != TouchPhase.Began)
                return;
#endif
            // 카메라에서 스크린에 마우스 클릭 위치를 통과하는 광선을 반환합니다.
            var ray = Camera.main.ScreenPointToRay(m_vecMouseDownPos);
            RaycastHit hit;

            // 광선으로 충돌된 collider를 hit에 넣습니다.
            if (Physics.Raycast(ray, out hit))
                if (animalDictionary.ContainsKey(hit.collider.name))
                {
                    selectedAnimal = hit.collider.name;
                    
                    //moving에서의 lerp
                    elapsedTime = 0;
                    
                    //sizeIncrease()의 lerp
                    _currentLerp = 0;
                }
        }
    }

    private Ray ray;
    private RaycastHit hitInfo;
    public LayerMask interactableLayer;

    private void SelectObjectWithoutClick()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, interactableLayer))
        {
            Debug.Log("Mouse over: " + hitInfo.collider.gameObject.name);
            if (hitInfo.collider.name != null)
            {
                selectedAnimal = hitInfo.collider.name;
                elapsedTime = 0;
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

    private float _currentLerp;
    private float lerp;

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
    public void ContinueMovingAnimal()
    {
        if (isMovable == false)
        {
            var t = Mathf.Clamp01(elapsedTime / movingTimeSec);

            if (selectedAnimalGameObject != null)
            {
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

    private void SetAnimalIntoDictionary()
    {
        animalDictionary.Add(nameof(cat), cat);
        animalDictionary.Add(nameof(rabbit), rabbit);
        animalDictionary.Add(nameof(dog), dog);
        animalDictionary.Add(nameof(parrot), parrot);
        animalDictionary.Add(nameof(mouse), mouse);
    }

    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
    }

    public float correctReactionOffset; //정답을 맞추고 동물의 애니메이션 걸리기 시간. 
    // IEnumerator SetAnimation()
    // {
    //     yield return new WaitForSeconds(correctReactionOffset);
    //     Animator animator = selectedAnimalGameObject.GetComponent<Animator>();
    //     animator.SetBool(correctAnim,true);
    //     yield return new WaitForNextFrameUnit;
    // }

}

