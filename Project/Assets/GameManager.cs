using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dictionary<string, GameObject> animalDict = new Dictionary<string, GameObject>();

    public GameObject cow;
    public GameObject horse;
    public GameObject pig;
    public GameObject swan;
    public GameObject chicken;

    public Transform AnimalMovePosition; //when user corrects an answer.

    private string selectedAnimal;

    public float gamestartTime;
    public static bool IsGameStarted;
    private float elapsedTime;

    Vector3 m_vecMouseDownPos;

    private void Awake()
    {
        animalDict.Add("Cow", cow);
        animalDict.Add("Horse", horse);
        animalDict.Add("Pig", pig);
        animalDict.Add("Swan", swan);
        animalDict.Add("Chicken", chicken);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(gamestartTime < elapsedTime)
        {
            IsGameStarted = true;
        }
        if (IsGameStarted)
        {
            SelectObject();
            MoveAnimalByName(selectedAnimal);
        }
       
    }

    /// <summary>
    /// 오브젝트를 선택하는 함수 입니다. 
    /// Linked lIst를 활용해 자료를 검색하고 해당하는 메세지를 카메라 및, 게임 다음 동작에 전달합니다. 
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
            if(Input.GetTouch(0).phase != TouchPhase.Began)
                return;
#endif
            // 카메라에서 스크린에 마우스 클릭 위치를 통과하는 광선을 반환합니다.
            Ray ray = Camera.main.ScreenPointToRay(m_vecMouseDownPos);
            RaycastHit hit;

            // 광선으로 충돌된 collider를 hit에 넣습니다.
            if (Physics.Raycast(ray, out hit))
            {
              
                selectedAnimal = hit.collider.name;
                
                MoveAnimalByName(selectedAnimal);
                /*
                // 어떤 오브젝트인지 로그를 찍습니다.
                Debug.Log(hit.collider.name);

                // 오브젝트 별로 코드를 작성할 수 있습니다.
                if (hit.collider.name == "Cow")
                {
                    Debug.Log("소");
                    selectedAnimal = hit.collider.name;
                }
               
            
            else if (hit.collider.name == "Horse")
                {
                    Debug.Log("말");
                    selectedAnimal = hit.collider.name;
                }
               
            else if (hit.collider.name == "Pig")
                Debug.Log("돼지");
            else if (hit.collider.name == "Swan")
                Debug.Log("백조");
            else if (hit.collider.name == "Chicken")
                Debug.Log("닭");

                */
            }
        }
    }
    
    public float movingTimeSec;
    private bool isMoveStart = false;
    private Transform defaultAnimalPosition;
    public void MoveAnimalByName(string animalName)
    {
        if (!isMoveStart)
        {
            isMoveStart = true;
            elapsedTime = 0f;
        }
        if (animalDict.TryGetValue(animalName, out GameObject animalObj))
        {
            defaultAnimalPosition = animalObj.transform;
            Debug.Log($"{selectedAnimal} is Moving!");
            elapsedTime += Time.deltaTime;
                // Lerp의 t값을 계산 (0 ~ 1 사이)
                float t = Mathf.Clamp01(elapsedTime / movingTimeSec);
                t = Lerp2D.EaseInQuad(0, 1, t);
            animalObj.transform.position = Vector3.Lerp(defaultAnimalPosition.position, AnimalMovePosition.position, t);

            if(elapsedTime > movingTimeSec)
            {
                isMoveStart = false;
            }
        }
        else
        {
            Debug.LogWarning("No animal with that name found!");
        }
    }
}
