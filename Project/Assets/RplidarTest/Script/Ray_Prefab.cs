using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Ray_Prefab : MonoBehaviour
{
    private float timer = 0f;
    //public GameObject Text;
    // Start is called before the first frame update


    //=====0822
    private Vector3 Temp_position;
    //

    //====1122
    private GameObject UI_Canvas;
    private Camera UI_Camera;

    //====1130

    public bool MouseClickOn = false;

    private float timer_2;

    private float Prev_x;
    private float Prev_y;
    public RectTransform Ray_position;
    private float Pos_x;
    private float Pos_y;

    public GameObject BALLPrefab;
    // Start is called before the first frame update

    void Start()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();
        BALLPrefab = Manager_Sensor.instance.BALLPrefab;


        //Temp_position = UI_Camera.WorldToScreenPoint(this.transform.position);

        Ray_position = this.GetComponent<RectTransform>();
        Pos_x = Ray_position.anchoredPosition.x;
        Pos_y = Ray_position.anchoredPosition.y;
        //Manager_Sensor.instance.Set_RayPosition(this.transform.position);
        ShootRay();
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
            Destroy_obj();
        }
    }

    void Destroy_obj()
    {
        Destroy(this.gameObject);
    }


    void ShootRay()
    {
        Prev_x = Manager_Sensor.instance.Prev_Ray_position_x;
        Prev_y = Manager_Sensor.instance.Prev_Ray_position_y;

        if (Prev_x - Pos_x < -50 || Prev_x - Pos_x > 50)
        {
            if (Prev_y - Pos_y < -50 || Prev_y - Pos_y > 50)
            {
                Debug.Log("Moving " + " Prev " + " x: " + Prev_x + " y: " + Prev_y + " Pos " + " x: " + Pos_x + " y: " + Pos_y);

                //이동일 경우 Ball 프리팹 생성
                GameObject Prefab_pos = Instantiate(BALLPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
                Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(Pos_x, Pos_y, 0);
                Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

                ////메인 카메라 레이 캐스트
                //Ray ray = Camera.main.ScreenPointToRay(Temp_position);

                //RaycastHit hit;

                //Color randomColor = new Color(Random.value, Random.value, Random.value);

                ////UI 카메라 레이 캐스트
                //PED.position = Temp_position;
                //List<RaycastResult> results = new List<RaycastResult>();
                //GR.Raycast(PED, results);

            }
        }
        else
        {
            //Holding 상태
            Debug.Log("Holding");
        }

        Manager_Sensor.instance.Set_PrevRayPosition(Pos_x, Pos_y);
    }
}


//1. Rplidar에서 센서 데이터 기반 해당 지점에 프리팹 생성
//2. 프리팹에서 현재 위치 데이터 Manager 센서에 전달
//3. 매니저 센서에서 현재 가져온 위치정보를 받아오고 현재 상태를 판별함
//4. 판별한걸 기반으로 위치에 Ball 오브젝트를 만들어냄


//void ShootRay_color()
//{
//    //Temp_position = UI_Camera.WorldToScreenPoint(this.transform.position);

//    //메인 카메라 레이 캐스트
//    Ray ray = Camera.main.ScreenPointToRay(Temp_position);

//    RaycastHit hit;

//    Color randomColor = new Color(Random.value, Random.value, Random.value);

//    //UI 카메라 레이 캐스트
//    PED.position = Temp_position;
//    List<RaycastResult> results = new List<RaycastResult>();
//    GR.Raycast(PED, results);

//    if (Physics.Raycast(ray, out hit))
//    {
//        Debug.Log(hit.transform.name);

//        hit.collider.gameObject.GetComponent<MeshRenderer>().material.color = randomColor;
//    }

//    if (results.Count > 0)
//    {
//        Debug.Log(results[0].gameObject.name);
//        results[0].gameObject.GetComponent<Image>().color = randomColor;
//    }

//}