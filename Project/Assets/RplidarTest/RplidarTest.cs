using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Threading;

public class RplidarTest : MonoBehaviour
{
    public string port;
    //public GameObject Capsule;

    private LidarData[] data;
    private RectTransform Img_Rect_transform;
    private GameObject CANVAS;

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

    private float x;
    private float y;
    private float Cal_y;

    private void Awake()
    {
        data = new LidarData[720];
    }

    private void Sensing()
    {
        //// 1013 스크립트 들어있는 오브젝트 회전 확인용
        // Img_Rect_transform.Rotate(new Vector3(0, 0, Time.deltaTime * lidarSpinSpeed));

        //// 1015 캔버스 위 오브젝트 위치 확인용(1:1 비율로 캔버스장 좌표에 대입)
        // Img_Rect_transform.anchoredPosition = new Vector3(143, 0, 0);


        //Debug.Log("1. data_angle : " + Test_degree + "  2. data_distance : " + Test_distance);

        // degree, angle값을 기준으로 x,y좌표 계산/Test_degree, Test_disatnce값 변경하면 사용 가능
        Vector3 pos = new Vector3(Mathf.Cos(Test_degree * Mathf.Deg2Rad) * Test_distance, 540 + Mathf.Sin(Test_degree * Mathf.Deg2Rad) * Test_distance, 0);
        temp_pos.GetComponent<RectTransform>().anchoredPosition = pos;
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
            //    StartCoroutine(GenMesh());
            m_thread = new Thread(GenMesh);
            m_thread.Start();
        }

        Img_Rect_transform = this.GetComponent<RectTransform>();
        CANVAS = GameObject.Find("UIManager");
        //CANVAS = GameObject.Find("UI 캔버스 (UI Manager)");

        //기준점 position입력해주고 기준 위치 0,0,0에 생성
        //temp_pos = Instantiate(spherePrefab, Testobj.transform.position, Quaternion.Euler(0, 0, 90), CANVAS.transform);

        //degree, angle값을 기준으로 x,y좌표 계산
        //Vector3 pos = new Vector3(Mathf.Cos(Test_degree * Mathf.Deg2Rad) * Test_distance, Mathf.Sin(Test_degree * Mathf.Deg2Rad) * Test_distance, 0);
        //temp_pos.GetComponent<RectTransform>().anchoredPosition = pos;

        //guide라인이랑 동기화 기능
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x - (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y - (Guideline.GetComponent<RectTransform>().rect.height) / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x + (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y + (Guideline.GetComponent<RectTransform>().rect.height) / 2;

        //가이드라인 이미지에 맞춰서 minx, miny 결정하기

        //초기 해상도로 intializing
        //min_x = (float)(Resolution_X * -0.5);
        //min_y = (float)(Resolution_Y * -0.5);
        //max_x = (float)(Resolution_X * 0.5);
        //max_y = (float)(Resolution_Y * 0.5);

        //min_y = -540;
        //max_y = 540;
        //max_x = 960;
        //min_x = -960;

        //실제 거리를 중간에 보정값이랑 곱한 만큼 보정을 해서 추가가 필요함
        //10cm라 가정하고 50으로 환산
        Cal_y = 0;
        //Cal_y = -50;



    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //    Test_check = !Test_check;

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

                    //2. instantiate 부분 테스트, 결과 문제 없이 이미지 프리팹을 센서 센싱 위치로 생성할 수 잇는 것을 확인하였음
                    /*
                    GameObject Prefab_pos = Instantiate(spherePrefab, this.transform.position, Quaternion.identity, CANVAS.transform);
                    Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
                    Prefab_pos.transform.LookAt(transform.position + cameraToLookAt.transform.rotation * Vector3.back,
                    cameraToLookAt.transform.rotation * Vector3.down);
                    */

                    //3. 프리팹 생성 없이 해당 지점 그냥 가져와서 마우스 바로 이동시키는걸로 넘어감
                    //데이터가 너무 많아서 해당 지점이 찍히지 않는 다는 가정하에
                    //직접 가이드라인 만들어놓고 데이터를 측정해서 점이 몇개 없으면 측정이 가능할지 확인이 필요함
                    /*
                    temp_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
                    Temp_position = Camera.main.WorldToScreenPoint(temp_pos.transform.position);
                    Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y));
                    */

                    //4. 가이드라인 만들어놓고 측정되는 데이터 갯수를 줄이는거 테스트 해봤고 min, max값 조절해서 가이드라인을 조절할 수 있음 확인함
                    //그리고 난 다음에 마우스가 이동가능한지 확인함
                    //스페이스바 누르면 점이 찍힐지 마우스가 이동할지 기능 구현함


                    if (x != 0 || y != 0)
                    {
                        if (min_x < x && x < max_x)
                        {
                            if (min_y < y && y < max_y)
                            {
                                if (Test_check)
                                {
                                    //데모용, 마우스
                                    Guideline.SetActive(false);
                                    TESTUI.SetActive(false);
                                    GameObject Prefab_pos = Instantiate(MOUSEPrefab, this.transform.position, Quaternion.Euler(0, 0, 0), CANVAS.transform);
                                    Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x+30, y-30, 0);
                                    Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                                }
                                else
                                {
                                    //개발자용, 공
                                    Guideline.SetActive(true);
                                    TESTUI.SetActive(true);
                                    GameObject Prefab_pos = Instantiate(BALLPrefab, this.transform.position, Quaternion.Euler(0, 0, 0), CANVAS.transform);
                                    Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x + 30, y - 30, 0);
                                    Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                                }
                                //Debug.Log("BEFORE 1. x : " + x + "  2. y : " + y);
                                //Prefab_pos.transform.LookAt(transform.position + cameraToLookAt.transform.rotation * Vector3.back,
                                //cameraToLookAt.transform.rotation * Vector3.down);

                            }
                        }
                    }

                    //1. 마우스 이동 없이 센서 데이터 확인 기능
                    //2. 마우스 이동 기능

                    //Debug.Log("BEFORE 1. x : " + x + "  2. y : " + y);

                    //위치 추출해서 마우스 이동시키는 함수
                    //Temp_position = Camera.main.WorldToScreenPoint(Prefab_pos.transform.position);
                    //Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y));


                    //이미지 카메라 바라보게 하는 함수
                    //Prefab_pos.transform.LookAt(transform.position + cameraToLookAt.transform.rotation * Vector3.back,
                    //cameraToLookAt.transform.rotation * Vector3.down);

                }
            }
            m_datachanged = false;
        }

    }
    void Test_NONSENSOR()
    {
        //그냥 같은 속도로 랜덤 함수를 호출해서 위치를 바꾸는 테스트 진행함
        // 결과 : 정상적으로 범위 안에서 랜덤함수 통해 위치를 변경할 경우 잘 바뀌는 것을 확인하였음

        //x = 0.5f * Mathf.Cos(data[i].theta * Mathf.Deg2Rad) * data[i].distant;
        //y = 0.5f * Mathf.Sin(data[i].theta * Mathf.Deg2Rad) * data[i].distant;

        x = UnityEngine.Random.Range(-1080, 1080);
        y = UnityEngine.Random.Range(-1920, 1920);

        //Debug.Log("BEFORE 1. x : " + x + "  2. y : " + y);

        if (x != 0 || y != 0)
        {
            //Temp pos 위치 바꾸는 코드
            //temp_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
            //Debug.Log("NOT 0 AFTER 1. x : " + x + "  2. y : " + y);

            //화면 해상도에 따라서 그 안에서만 생성이 될 수 있게 가이드라인 해주는 코드
            if (min_x < x && x < max_x)
            {
                if (min_y < y && y < max_y)
                {
                    //마우스 커서 위치 바꾸는 코드
                    //Mouse.current.WarpCursorPosition(new Vector2(x, y));

                    //테스트 오브젝트 위치 바꾸고 해당 좌표에 마우스 이동시키는 코드
                    temp_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
                    // Debug.Log("Image position  1. x : " + x + "  2. y : " + y);

                    Temp_position = Camera.main.WorldToScreenPoint(temp_pos.transform.position);

                    Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y));
                    // Debug.Log("mouse position  1. x : " + Temp_position.x + "  2. y : " + Temp_position.y);

                }
            }
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

    //private void OnGUI()
    //{
    //    DrawButton("Connect", () =>
    //    {
    //        if (string.IsNullOrEmpty(port))
    //        {
    //            return;
    //        }

    //        int result = RplidarBinding.OnConnect(port);

    //        Debug.Log("Connect on " + port + " result:" + result);
    //    });

    //    DrawButton("DisConnect", () =>
    //    {
    //        bool r = RplidarBinding.OnDisconnect();
    //        Debug.Log("Disconnect:" + r);
    //    });

    //    DrawButton("StartScan", () =>
    //    {
    //        m_onscan = RplidarBinding.StartScan();
    //        Debug.Log("StartScan:" + m_onscan);

    //        if (m_onscan)
    //        {
    //            //    StartCoroutine(GenMesh());
    //            m_thread = new Thread(GenMesh);
    //            m_thread.Start();
    //        }
    //    });

    //    DrawButton("EndScan", () =>
    //    {
    //        bool r = RplidarBinding.EndScan();
    //        Debug.Log("EndScan:" + r);
    //    });

    //    DrawButton("StartMotor", () =>
    //    {
    //        bool r = RplidarBinding.StartMotor();
    //        Debug.Log("StartMotor:" + r);
    //        //Capsule.GetComponent<LidarRotateScript>().check = true;
    //    });

    //    DrawButton("EndMotor", () =>
    //    {
    //        bool r = RplidarBinding.EndMotor();
    //        Debug.Log("EndMotor:" + r);
    //    });


    //    DrawButton("Release Driver", () =>
    //    {
    //        bool r = RplidarBinding.ReleaseDrive();
    //        Debug.Log("Release Driver:" + r);
    //    });


    //    DrawButton("GrabData", () =>
    //    {
    //        int count = RplidarBinding.GetData(ref data);

    //        Debug.Log("GrabData:" + count);
    //        if (count > 0)
    //        {
    //            for (int i = 0; i < 20; i++)
    //            {
    //                Debug.Log("d:" + data[i].distant + " " + data[i].quality + " " + data[i].syncBit + " " + data[i].theta);
    //            }
    //        }

    //    });
    //}

    //void DrawButton(string label, Action callback)
    //{
    //    if (GUILayout.Button(label, GUILayout.Width(200), GUILayout.Height(75)))
    //    {
    //        if (callback != null)
    //        {
    //            callback.Invoke();
    //        }
    //    }
    //}
}
