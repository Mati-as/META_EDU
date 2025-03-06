using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class SensorManager : MonoBehaviour
{
    private enum SensorParams
    {
        Sensitivity,
        Height,
        Offset,
        Screen
    }


    public static readonly string PORT = "COM3";
    public static bool isMoterStarted { get; private set; }
    public static bool sensorImageView; //Test용 빌드에서 사용

    private static LidarData[] _lidarDatas;
    private RectTransform Img_Rect_transform;

    public static event Action<bool> OnSenSorInit;


    //=====071423
    public GameObject BALLPrefab;
    public GameObject MOUSEPrefab;
    public GameObject FPPrefab;
    public GameObject middlePrefab;
    public bool m_onscan;
    private Thread m_thread;


    public static readonly float SENSOR_DEFAULT_SENSITIVITY = 0.095f;

    private static readonly float SENSOR_SENTSITIVITY_TOLERANCE = 0.005f;
    private static float _sensorSensitivity;

    public static float sensorSensitivity
    {
        get => _sensorSensitivity;
        set
        {
            if (value < SENSOR_DEFAULT_SENSITIVITY)
            {
                _sensorSensitivity = SENSOR_DEFAULT_SENSITIVITY;
                Logger.LogWarning("sensitivity is too small. set as 0.05f");
            }
            else
            {
                if (Math.Abs(value - sensorSensitivity) < SENSOR_SENTSITIVITY_TOLERANCE) return;
                _poolReturnWait = new WaitForSeconds(sensorSensitivity);
                _sensorSensitivity = value;
                Logger.Log($"current sensitivity {value}");
            }
        }
    }


    private bool m_datachanged;

    //=====
    private Vector3 Temp_position;
    //=====


    //====1012
    public bool Test_check;
    private double number = 0f;

    public GameObject Guideline;
    public GameObject TESTUI;
    //

    //1015
    private static float _height = 172;

    public static float height
    {
        get => _height;
        set
        {
#if UNITY_EDITOR
            Debug.Log($"Height Value changed {_height}---------------------------------");
#endif
            Debug.Assert(!(Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML < 175)
                         || !(Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML > 190));
            _height = Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
            _height = Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
        }
    } //cm 

    private readonly float Resolution_Y = 1080;
    private float Resolution_X = 1920;
    private float _width; //비율통한계싼

    private readonly int HEIGHT_MAX = 200; //cm
    private readonly int HEIGHT_MIN = 100; //cm

    public float min_x;
    public float min_y;
    public float max_x;
    public float max_y;


    private Camera cameraToLookAt;

    public float Test_degree;
    public float Test_distance;

    [SerializeField] public GameObject temp_pos;

    public readonly float SENSOR_ROTATION = 0;

    //1121

    private GameObject UI_Canvas;
    private Camera UI_Camera;


    private float sensored_X;
    private float sensored_Y;
    private float pre_x;
    private float pre_y;

    private bool UI_Active;
    public static bool BallActive { get; set; }
    private bool SF_Active = true;
    private readonly int LIDAR_DATA_SIZE = 720;

    //슬라이더를 통한 감도조절기능 추가(민석) 불필요시삭제 10/4/24
    private Slider _sensitivitySlider;
    private TextMeshProUGUI _sensitivityText;


    // 센서 측정 런타임 수정을 위한 맴버 선언
    //07.18/24 기준 화면 사이즈 320x180a(cm)

    private Dictionary<int, Vector2> _projectorLookUpTable;
    private Button _sensorEditModeButton;
    private TextMeshProUGUI _TMP_sensorEditMode;
    private Image _sensorEditModCheckImage;

    public static bool isSensorEditMode { get; private set; }

    private float UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET;
    private readonly float SENSEOR_OFFSET_MAX_VALUE = 1000;


    private readonly float SCREEN_RATIO_MIN = 0.5f;
    private readonly float SCREEN_RATIO_MAX = 10;


    public int heightCm { get; set; } = 2;
    public float sensorDistanceFromProjection { get; set; } = 280;

    // private float screen_ratio;// 화면비 // 유니티 height 1080 : 실제 프로젝션 height (mm)를 비교하여 비례를 조정 
    private float _screenRatio = 0.782f;


    private Slider _heightSlider;
    private Slider sensorDistanceSlider;
    private Slider _screenRatioSlider;

    private TextMeshProUGUI _TMP_height;
    private TextMeshProUGUI _TMP_seonsorDistance;
    private TextMeshProUGUI _TMP_ScreenRatio;

    ////////////////// 0719- 센서 테스트용 멤버 새로 추가한 부분///////////////////////////////

    private float correction_value; // 화면과 유니티에서의 단위를 맞추기 위한 보정값.

    /// /////////////////
    private void Awake()
    {
        //런쳐도 센서로 터치 가능하도록 수정 09/24/2024
        //if (SceneManager.GetActiveScene().name.Contains("METAEDU")) return;

        _lidarDatas = new LidarData[LIDAR_DATA_SIZE];
        _sensitivitySlider = GameObject.Find("SensitivitySlider").GetComponent<Slider>();
        UI_Scene_StartBtn.OnSensorRefreshEvent -= InitSensor;
        UI_Scene_StartBtn.OnSensorRefreshEvent += InitSensor;
        // _width = _height * (Resolution_X / Resolution_Y);
    }

    private void OnDestroy()
    {
        UI_Scene_StartBtn.OnSensorRefreshEvent -= InitSensor;
        Destroy(gameObject);
        StopSensor();
    }


    private void RefreshSensor()
    {
        if (GameObject.FindWithTag("Launcher") == null) StartCoroutine(RefreshSensorCo());
        else
            Logger.Log("게임 런쳐에서는 센서를 사용할 수 없습니다. 동작 시 태그 반드시 확인");
    }

    private readonly WaitForSeconds _refreshWait = new(0.5f);

    private IEnumerator RefreshSensorCo()
    {
        Debug.Log("Sensor Refresh");
        RplidarBinding.EndScan();
        RplidarBinding.EndMotor();
        RplidarBinding.OnDisconnect();
        RplidarBinding.ReleaseDrive();

        m_thread?.Abort();
        m_onscan = false;

        yield return _refreshWait;

        var result = RplidarBinding.OnConnect(PORT);
        Debug.Log("Connect on " + PORT + " result:" + result);

        isMoterStarted = RplidarBinding.StartMotor();
        Debug.Log("StartMotor:" + isMoterStarted);

        m_onscan = RplidarBinding.StartScan();
        Debug.Log("StartScan:" + m_onscan);

        var isSensorOn = isMoterStarted || m_onscan;


        if (m_onscan)
        {
            m_thread = new Thread(GenerateMesh);
            m_thread.Start();
        }

        OnSenSorInit?.Invoke(isSensorOn);
    }
    private TimeSpan _refreshWaitTimeSpan = TimeSpan.FromSeconds(0.5f);
    private async void InitSensor()
    {
        if (GameObject.FindWithTag("Launcher") == null)
            await InitSensorAsync();
        else
            Logger.Log("게임 런쳐에서는 센서를 사용할 수 없습니다. 동작 시 태그 반드시 확인");
    }

   
    private async Task InitSensorAsync()
    {
        await Task.Delay(_refreshWaitTimeSpan);

        var result = await Task.Run(() => RplidarBinding.OnConnect(PORT));
        if (result < 0)
        {
            result = await Task.Run(() => RplidarBinding.OnConnect(PORT=="COM3"?"COM4":"COM3"));
        }
        
        
        
        isMoterStarted = await Task.Run(() => RplidarBinding.StartMotor());

        m_onscan = await Task.Run(() => RplidarBinding.StartScan());
        Debug.Log("Connect on " + PORT + " result:" + result + "\nStartMotor:" + isMoterStarted + "StartScan:" +
                  m_onscan);

        if (m_onscan)
        {
            m_thread = new Thread(GenerateMesh);
            m_thread.Start();
        }

        var isSensorOn = isMoterStarted || m_onscan;

        Img_Rect_transform = GetComponent<RectTransform>();

        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();

        // guide라인이랑 동기화 기능
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x -
                Guideline.GetComponent<RectTransform>().rect.width / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y -
                Guideline.GetComponent<RectTransform>().rect.height / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x +
                Guideline.GetComponent<RectTransform>().rect.width / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y +
                Guideline.GetComponent<RectTransform>().rect.height / 2;

        TESTUI.SetActive(false);

        _sensitivitySlider.onValueChanged.AddListener(_ =>
        {
            _sensorSensitivity = _sensitivitySlider.value;
            _poolReturnWait = new WaitForSeconds(sensorSensitivity);

            Logger.Log($"prefab limit time is {_sensitivitySlider.value}");
        });

        _projectorLookUpTable = new Dictionary<int, Vector2>();

        UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET =
            sensorDistanceFromProjection + _height * 10 / 2; // height의 절반을 mm로 단위로 계산

        height = Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
        _screenRatio = Resolution_Y / (height * 10);
        Debug.Log($"Height Set FROM XML:{Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML}");
        Debug.Log($"Ratio:{_screenRatio}");

        _sensorDetectedPositionPool = new Stack<RectTransform>();
        SetPool(_sensorDetectedPositionPool, "Rplidar/FP_New");

        OnSenSorInit?.Invoke(isSensorOn);
    }

    /// <summary>
    ///     C#기준으로 out을 사용하여 초기화 불필요, 반환형식으로 사용
    /// </summary>
    /// <param name="sliderName"></param>
    /// <param name="slider"></param>
    /// <param name="text"></param>
    private void InitializeSlider(string sliderName, out Slider slider)
    {
        slider = GameObject.Find(sliderName).GetComponent<Slider>();
    }

    private Stack<RectTransform> _sensorDetectedPositionPool;


    private static WaitForSeconds _poolReturnWait;

    protected IEnumerator ReturnToPoolAfterDelay(RectTransform obj, Stack<RectTransform> pool)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(sensorSensitivity);

        yield return _poolReturnWait;
        obj.gameObject.SetActive(false);

        pool.Push(obj); // Return the particle system to the pool
    }

    private void SetPool<T>(Stack<T> pool, string path, int poolCount = 100) where T : Object
    {
        //런쳐도 센서로 터치 가능하도록 수정 09/24/2024
        //if (SceneManager.GetActiveScene().name == "METAEDU_LAUNCHER") return;

        for (var poolSize = 0; poolSize < poolCount; poolSize++)
        {
            var prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("this gameObj to pool is null.");
#endif
                return;
            }

            var obj = Instantiate(prefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);

            var component = obj.GetComponent<T>();
            if (component == null)
            {
#if UNITY_EDITOR
                Debug.LogError("the loaded prefab does not have the required component.");
#endif
                return;
            }

            obj.SetActive(false);
            pool.Push(component);
        }
    }


    private RectTransform GetFromPool(Stack<RectTransform> pool)
    {
        if (pool.Count <= 0) return null;

        var obj = pool.Pop();
        return obj;
    }

    private void ShowFilteredSensorPos(float rectX, float rectY)
    {
        var detectedPosRect = GetFromPool(_sensorDetectedPositionPool);

        if (detectedPosRect == null) return;

#if UNITY_EDITOR
//        Debug.Log($"sensor: {rectX},{rectY}");
#endif
        detectedPosRect.anchoredPosition = new Vector2(rectX, rectY);
        detectedPosRect?.gameObject.SetActive(true);
        StartCoroutine(ReturnToPoolAfterDelay(detectedPosRect, _sensorDetectedPositionPool));
    }


    private void ConfigureSlider(Slider slider, float maxValue, UnityAction<float> onValueChanged,
        float minVal = 0)
    {
        slider.minValue = minVal;
        slider.maxValue = maxValue;
        slider.onValueChanged.AddListener(onValueChanged);
    }


    private void OnEditSensorModeBtnClicked()
    {
        isSensorEditMode = !isSensorEditMode;
        _sensorEditModCheckImage.enabled = isSensorEditMode;
        _TMP_sensorEditMode.text = isSensorEditMode ? "Sensor Edit Mode: ON" : "Sensor Edit Mode: OFF";
    }

    private void Start()
    {
        // InitSensor();
    }


    private void GenerateMesh()
    {
        while (true)
        {
            var datacount = RplidarBinding.GetData(ref _lidarDatas);

            if (datacount == 0)
                Thread.Sleep(5);
            else
                m_datachanged = true;
        }
    }


    private void PlayGenrateMeshCo()
    {
        StartCoroutine(GenerateMeshCo());
    }

    private readonly WaitForSeconds _waitForSensorGetMesh = new(0.005f);

    private IEnumerator GenerateMeshCo()
    {
        while (true)
        {
            var datacount = RplidarBinding.GetData(ref _lidarDatas);

            if (datacount == 0)
                yield return _waitForSensorGetMesh;
            else
                m_datachanged = true;
        }
    }


    private float _timer;
    private Vector3 lastTouchPos = Vector3.zero; // 마지막 터치 좌표
    private float lastTouchTime; // 마지막 터치 시간
    private readonly float moveThreshold = 0.02f; // 2cm 이상 움직여야 터치 인정
    private readonly float touchCooldown = 0.2f; // 200ms 동안 터치 1회만 허용

    private void GenerateDectectedPos()
    {
        if (!isMoterStarted) return;
        if (Managers.isGameStopped) return;
        _timer = 0f;


        if (m_datachanged)
        {
            for (var i = 0; i < 720; i++)
            {
                //6배
                if (_lidarDatas[i].theta > 90 && _lidarDatas[i].theta < 270) continue;


                sensored_X = -_screenRatio *
                             (_lidarDatas[i].distant * Mathf.Cos((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad));
                sensored_Y = -_screenRatio *
                             (_lidarDatas[i].distant * Mathf.Sin((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad) -
                              UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET);


                if (i % _filteringAmount == 0)
                    if (min_x < sensored_X && sensored_X < max_x)
                        if (min_y < sensored_Y && sensored_Y < max_y)
                        {
                            if (SF_Active)
                            {
                                // _filteringAmount = 8;
                                _filteringAmount = 4;
                                Filtering_touchpoint();
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                            else
                            {
                                _filteringAmount = 3;
                                Filtering_touchpoint();
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                        }
#if UNITY_EDITOR
                // Debug.Log($"sensor: {sensored_X},{sensored_Y} , {_screenRatio}");
#endif
            }

            m_datachanged = false;
        }
    }

    //0212
    private void Filtering_touchpoint()
    {
        if (Time.time - lastTouchTime > touchCooldown &&
            Vector3.Distance(lastTouchPos, new Vector3(sensored_X, sensored_Y, 0)) > moveThreshold)
        {
            lastTouchPos = new Vector3(sensored_X, sensored_Y, 0); // 터치 좌표 업데이트
            lastTouchTime = Time.time; // 터치 시간 업데이트
            ShowFilteredSensorPos(sensored_X, sensored_Y); // 터치 이벤트 실행
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer > sensorSensitivity)
        {
            _timer = 0;
            GenerateDectectedPos();
        }
    }

    private int _filteringAmount = 2;

    public void Instant_Ball(float temp_x, float temp_y)
    {
        var Prefab_pos = Instantiate(BALLPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0),
            UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }

    public void Instant_Mouse(float temp_x, float temp_y)
    {
        var Prefab_pos = Instantiate(MOUSEPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0),
            UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }

    public void Instant_FP(float temp_x, float temp_y)
    {
        var Prefab_pos = Instantiate(FPPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0),
            UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }


    public void InstantiateMiddlePointPrefab(float temp_x, float temp_y)
    {
        var Prefab_pos = Instantiate(middlePrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0),
            UI_Canvas.transform);
        Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
        Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    }

    private void OnApplicationQuit()
    {
        StopSensor();
    }

    private void StopSensor()
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
            TESTUI.SetActive(true);
        else if (UI_Active == false) TESTUI.SetActive(false);
        return UI_Active;
    }

    public bool Ball_Active_ONOFF()
    {
        BallActive = !BallActive;

        Logger.Log("Ball Image Active");
        return BallActive;
    }

    public bool SF_Active_ONOFF()
    {
        SF_Active = !SF_Active;

        return SF_Active;
    }
}