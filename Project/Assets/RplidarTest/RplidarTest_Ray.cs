using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RplidarTest_Ray : MonoBehaviour
{
    public string port;
    //public GameObject Capsule;

    private LidarData[] data;
    private RectTransform Img_Rect_transform;

    //=====0714
    public GameObject BALLPrefab; 
    public GameObject MOUSEPrefab;
    public GameObject FPPrefab; 

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


    private float x;
    private float y;
    private float pre_x;
    private float pre_y;

    private bool UI_Active = false;
    private bool BALL_Active = true;
    private bool SF_Active = true;
    
    //슬라이더를 통한 감도조절기능 추가(민석) 불필요시삭제 2/28/24
    private Slider _sensitivitySlider;
    private TextMeshProUGUI _sensitivityText;
    
    private void Awake()
    {
        data = new LidarData[720];
        
        //슬라이더를 통한 감도조절기능 추가(민석) 불필요시삭제 2/28/24
        _sensitivitySlider = GameObject.Find("SensitivitySlider").GetComponent<Slider>();
        _sensitivityText = GameObject.Find("SensitivityText").GetComponent<TextMeshProUGUI>();

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

        //guide라인이랑 동기화 기능
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x - (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y - (Guideline.GetComponent<RectTransform>().rect.height) / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x + (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y + (Guideline.GetComponent<RectTransform>().rect.height) / 2;

        TESTUI.SetActive(false);

        
        //IGameManager init이후에 동작해야합니다. 따라서 Awake가 아닌 Start에서만 사용해야합니다. 4/4/24
        _sensitivitySlider.value = IGameManager.defaultSensitivity / 2;
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
    //1212 수정
    void Update()
    {
        //슬라이더를 통한 감도조절기능 추가(민석) 불필요시삭제 2/28/24
        FP_Prefab.Limit_Time = _sensitivitySlider.value * 2f;
        _sensitivityText.text = $"sensitivity : {FP_Prefab.Limit_Time:F2}";

    }
    // Update is called once per frame
    void FixedUpdate()
    {

        if (m_datachanged)
        {
            for (int i = 0; i < 720; i++)
            {
                //센서 데이터 data[i].theta, distant
                //1. 화면과 센서를 일치화 시키기 위해서 theta를 마이너스 곱해줌, 추가로 회전 시켜주기 위해 Sensor_rotation 추가했고 위에서 아래 방향으로 내려다 보는것 기준으 90도 입력하면 댐
                //2. 0.74f는 실제 길이와 유니티내 맵핑이 일치하기 위한 보정값(빔프로젝터의 실제 화면과 오차가 있음), 1.07f는 발 위에 정확히 찍히기 위한 보정값
                // Ex) 실제에서 682 mm -> 유니티 position 상 500, 보정값 0.733 곱해서 맞춰줌 실제 데이터를 position으로 변환함
                //3. 763.565f은 유니티 상의 캔버스 기준이 정가운데이기 때문에 그에 맞추기 위해 y값을 그 만큼 위로 올림

                x = 54.24f+0.733f * Mathf.Cos((-data[i].theta + Sensor_rotation) * Mathf.Deg2Rad) * (data[i].distant * 1.07f);
                y = 763.565f + 0.733f * Mathf.Sin((-data[i].theta + Sensor_rotation) * Mathf.Deg2Rad) * (data[i].distant * 1.07f);

                if (i % 4 == 0)
                {
                    if (min_x < x && x < max_x)
                    {
                        if (min_y < y && y < max_y)
                        {
                            if (SF_Active)
                            {
                                //필터 On
                                if (BALL_Active)
                                {
                                    Instant_FP(x, y);
                                }
                                else
                                {
                                    //데모용 마우스?
                                }
                            }
                            else
                            {
                                //필터 off
                                if (BALL_Active)
                                {
                                    Instant_Ball(x, y);
                                }
                                else
                                {
                                    //데모용 마우스?
                                }
                            }
                        }
                    }

                }
            }
            m_datachanged = false;
        }

    }

    public void Instant_Ball(float temp_x, float temp_y)
    {
        GameObject Prefab_pos = Instantiate(BALLPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }
    public void Instant_Mouse(float temp_x, float temp_y)
    {
        GameObject Prefab_pos = Instantiate(MOUSEPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }
    public void Instant_FP(float temp_x, float temp_y)
    {
        GameObject Prefab_pos = Instantiate(FPPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
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

    public bool UI_Active_ONOFF()
    {
        UI_Active = !UI_Active;

        if (UI_Active)
        {
            TESTUI.SetActive(true);
        }
        else if (UI_Active == false)
        {
            TESTUI.SetActive(false);
        }
        return UI_Active;
    }
    public bool Ball_Active_ONOFF()
    {
        BALL_Active = !BALL_Active;

        return BALL_Active;
    }
    public bool SF_Active_ONOFF()
    {
        SF_Active = !SF_Active;

        return SF_Active;
    }
}