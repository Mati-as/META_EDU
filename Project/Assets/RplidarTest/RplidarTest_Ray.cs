using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RplidarTest_Ray : MonoBehaviour
{
    public string port;
    //public GameObject Capsule;

    private LidarData[] data;
    private RectTransform Img_Rect_transform;

    //=====0714
    public GameObject BALLPrefab; //spherePrefab을 받을 변수 
    public GameObject MOUSEPrefab; //spherePrefab을 받을 변수 

    public bool m_onscan = false;
    private Thread m_thread;
    private bool m_datachanged = false;
    //=====
    private Vector3 Temp_position;

    //=====


    //====1012
    public bool Test_check = false;
    double number = 0f;

    public GameObject Guideline;
    public GameObject TESTUI;
    //

    //1015

    private float Resolution_Y = 1080;
    private float Resolution_X = 1920;

    public float min_x;
    public float min_y;
    public float max_x;
    public float max_y;


    private Camera cameraToLookAt;

    public float Test_degree;
    public float Test_distance;

    [SerializeField]
    public GameObject temp_pos;

    public float Sensor_rotation = 0;

    //1121

    private GameObject UI_Canvas;
    private Camera UI_Camera;
    GraphicRaycaster GR;
    PointerEventData PED;


    private float x;
    private float y;
    private float Cal_y;

    private void Awake()
    {
        data = new LidarData[720];
    }

    void Start()
    {
        int result = RplidarBinding.OnConnect(port);
        Debug.Log("Connect on " + port + " result:" + result);

        bool r = RplidarBinding.StartMotor();
        Debug.Log("StartMotor:" + r);

        m_onscan = RplidarBinding.StartScan();
        Debug.Log("StartScan:" + m_onscan);

        if (m_onscan)
        {
            m_thread = new Thread(GenMesh);
            m_thread.Start();
        }

        Img_Rect_transform = this.GetComponent<RectTransform>();

        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();

        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);

        //guide라인이랑 동기화 기능
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x - (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y - (Guideline.GetComponent<RectTransform>().rect.height) / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x + (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y + (Guideline.GetComponent<RectTransform>().rect.height) / 2;

        //실제 거리를 중간에 보정값이랑 곱한 만큼 보정을 해서 추가가 필요함
        //10cm라 가정하고 50으로 환산
        Cal_y = 0;
        //Cal_y = -50;



    }

    void Update()
    {

    }
    void GenMesh()
    {
        while (true)
        {
            int datacount = RplidarBinding.GetData(ref data);

            if (datacount == 0)
            {
                Thread.Sleep(5);
            }
            else
            {
                m_datachanged = true;
            }
        }
    }
    //해상도 1920,1080

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_datachanged)
        {
            for (int i = 0; i < 720; i++)
            {
                //센서 데이터 data[i].theta, distant
                //1. 화면과 센서를 일치화 시키기 위해서 theta를 마이너스 곱해줌, 추가로 회전 시켜주기 위해 Sensor_rotation 추가했고 위에서 아래 방향으로 내려다 보는것 기준으 90도 입력하면 댐
                x = 0.74f * Mathf.Cos((-data[i].theta + Sensor_rotation) * Mathf.Deg2Rad) * data[i].distant;
                y = Cal_y + 540 + 0.74f * Mathf.Sin((-data[i].theta + Sensor_rotation) * Mathf.Deg2Rad) * data[i].distant;

                if (i % 4 == 0)
                {

                   
                    if (x != 0 || y != 0)
                    {
                        if (min_x < x && x < max_x)
                        {
                            if (min_y < y && y < max_y)
                            {
                                if (Test_check)
                                {
                                    //데모용, 마우스
                                    //Guideline.SetActive(false);
                                    //TESTUI.SetActive(false);
                                    GameObject Prefab_pos = Instantiate(MOUSEPrefab, this.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
                                    Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x + 30, y - 30, 0);
                                    Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

                                    Debug.Log("MOUSE CLICKED");

                                }
                                else
                                {
                                    //개발자용, 공
                                    Guideline.SetActive(true);
                                    TESTUI.SetActive(true);
                                    GameObject Prefab_pos = Instantiate(BALLPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
                                    Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x + 30, y - 30, 0);
                                    Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

                                }


                            }
                        }
                    }

                }
            }
            m_datachanged = false;
        }

    }
   
    void OnDestroy()
    {
        RplidarBinding.EndScan();
        RplidarBinding.EndMotor();
        RplidarBinding.OnDisconnect();
        RplidarBinding.ReleaseDrive();

        //StopCoroutine(GenMesh());

        m_thread?.Abort();

        m_onscan = false;
    }
}
