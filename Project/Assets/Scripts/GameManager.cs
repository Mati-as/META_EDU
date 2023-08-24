using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool IsGameStarted;

    [Header("Game Start Setting")]
    [Space(10f)]
    public float gamestartTime;

    public Transform AnimalMovePosition; //when user corrects an answer.

    [Space(30f)]
    [Header("Game Objects(Animal) Setting")]
    [Space(10f)]
    public GameObject cow;
    public GameObject pig;
    public GameObject horse;
    public GameObject swan;
    public GameObject chicken;
    public GameObject goat;
    public GameObject raccoon;
    public GameObject deer;
    public GameObject fox;
    public GameObject giraffe;

    [Header("Animal Movement Setting")]
    [Space(10f)]
    public float movingTimeSec;

    public float waitingTime;

    public Dictionary<string, GameObject> animalDictionary = new();

    private GameObject defaultAnimalPosition; // 동물 이동의 시작위치 지정.
    private float elapsedTime;
    private bool isMovable;
    private Vector3 m_vecMouseDownPos;
    private string selectedAnimal;

    [Header("Game Settings")]
    [Space(10f)]
    public int TARGET_FRAME = 30;
    
    // 게임 플레이 로직 
    private bool isCorrected; 
    

    private void Awake()
    {
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
            CheckMovable();
            
            //두 함수 중 동물 선택 로직은 하나로 정합니다. (클릭O or 클릭X)

            SelectObject();
            //SelectObjectWithoutClick();
            
            if (selectedAnimal != null) SetAnimal(selectedAnimal);
            // 동물의 움직임이 시작됨을 표시
            ContinueMovingAnimal();
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
                    elapsedTime = 0;
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

    public void ContinueMovingAnimal()
    {
        if (isMovable == false)
        {
            Debug.Log($"{selectedAnimal} is Moving!");

            var t = Mathf.Clamp01(elapsedTime / movingTimeSec);
            // Lerp2D.EaseInQuad 함수에 대한 정의가 누락되어 있어 직접 t 값을 사용합니다.
            if (defaultAnimalPosition != null)
                defaultAnimalPosition.transform.position = Vector3.Lerp(defaultAnimalPosition.transform.position,
                    AnimalMovePosition.position, t);
        }
    }

    public void SetAnimal(string animalName)
    {
        if (animalDictionary.TryGetValue(animalName, out var animalObj))
            if (animalObj != null)
                defaultAnimalPosition = animalObj;
    }

    private void ReloadCurrentScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 해당 인덱스의 씬을 다시 로드합니다.
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void SetAnimalIntoDictionary()
    {
        animalDictionary.Add(nameof(cow), cow);
        animalDictionary.Add(nameof(pig), pig);
        animalDictionary.Add(nameof(horse), horse);
        animalDictionary.Add(nameof(swan), swan);
        animalDictionary.Add(nameof(chicken), chicken);
        animalDictionary.Add(nameof(goat), goat);
        animalDictionary.Add(nameof(raccoon), raccoon);
        animalDictionary.Add(nameof(deer), deer);
        animalDictionary.Add(nameof(fox), fox);
        animalDictionary.Add(nameof(giraffe), giraffe);
    }

    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
    }
}