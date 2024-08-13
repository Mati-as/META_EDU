using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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


    private static readonly string port = "COM3";
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
    private static float _height = 182;

    public static float height
    {
        get => _height;
        set
        {
#if UNITY_EDITOR
            Debug.Log($"Height Value changed {_height}---------------------------------");
#endif
            Debug.Assert(!(Managers.settingManager.SCREEN_PROJECTOER_HEIGHT_FROM_XML < 175)
                         || !(Managers.settingManager.SCREEN_PROJECTOER_HEIGHT_FROM_XML > 190));
            _height = Managers.settingManager.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
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
    private bool BALL_Active = true;
    private bool SF_Active = true;
    private readonly int LIDAR_DATA_SIZE = 720;

    //슬라이더를 통한 감도조절기능 추가(민석) 불필요시삭제 2/28/24
//pvate Slider _sensitivitySlider;
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
    private Slider sensorDistance;
    private Slider _screenRatioSlider;

    private TextMeshProUGUI _TMP_hiehgt;
    private TextMeshProUGUI _TMP_seonsorDistance;
    private TextMeshProUGUI _TMP_ScreenRatio;

    ////////////////// 0719- 센서 테스트용 멤버 새로 추가한 부분///////////////////////////////

    private float correction_value; // 화면과 유니티에서의 단위를 맞추기 위한 보정값.

    /// /////////////////
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("METAEDU")) return;

        Init();
        
        _lidarDatas = new LidarData[LIDAR_DATA_SIZE];

        TopMenuUI.OnRefreshEvent -= RefreshSensor;
        TopMenuUI.OnRefreshEvent += RefreshSensor;
        // _width = _height * (Resolution_X / Resolution_Y);
    }

    private void OnDestroy()
    {
        TopMenuUI.OnRefreshEvent -= RefreshSensor;
        Destroy(gameObject);
        UnBindLidar();
    }


    private void RefreshSensor()
    {
        StartCoroutine(RefreshSensorCo());
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

        var result = RplidarBinding.OnConnect(port);
        Debug.Log("Connect on " + port + " result:" + result);

        isMoterStarted = RplidarBinding.StartMotor();
        Debug.Log("StartMotor:" + isMoterStarted);

        m_onscan = RplidarBinding.StartScan();
        Debug.Log("StartScan:" + m_onscan);

        var isSensorOn = isMoterStarted || m_onscan;
        OnSenSorInit?.Invoke(isSensorOn);

        if (m_onscan)
        {
            m_thread = new Thread(GenerateMesh);
            m_thread.Start();
        }
    }


    /// <summary>
    ///     C#기준으로 out을 사용하여 초기화 불필요, 반환형식으로 사용
    /// </summary>
    /// <param name="sliderName"></param>
    /// <param name="slider"></param>
    /// <param name="text"></param>
    private void InitializeSlider(string sliderName, out Slider slider, out TextMeshProUGUI text)
    {
        slider = GameObject.Find(sliderName).GetComponent<Slider>();
        text = slider.transform.GetComponentInChildren<TextMeshProUGUI>();
    }

    private Stack<RectTransform> _sensorDetectedPositionPool;

    public void Init()
    {
        _screenRatio = Resolution_Y / (_height * 10);
#if UNITY_EDITOR
        Debug.Log($"Screen Ration is {_screenRatio}");
#endif
        InitializeSlider("HeightSlider", out _heightSlider, out _TMP_hiehgt);
        InitializeSlider("OffsetYSlider", out sensorDistance, out _TMP_seonsorDistance);
        InitializeSlider("ScreenRatioSlider", out _screenRatioSlider, out _TMP_ScreenRatio);

        
        ConfigureSlider(_heightSlider, HEIGHT_MAX, value =>
        {
             _height = value;
            _screenRatio = (Resolution_Y / _height * 10);
            UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET =sensorDistanceFromProjection + _height * 10 / 2; //height의 절반을 mm로 단위로 계산
            _TMP_hiehgt.text = "HEIGHT : " + height.ToString("F1");
        }, HEIGHT_MIN);
        
        _TMP_hiehgt.text = $"{nameof(heightCm)}: " + _height.ToString("F1");
        _TMP_seonsorDistance.text = "Sensor Distance: " + sensorDistanceFromProjection.ToString("F1");
        _TMP_ScreenRatio.text = "SCREEN RATIO: " + _screenRatio.ToString("F1");

        //InitializeSlider("SensitivitySlider", out _sensitivitySlider, out _sensitivityText);
    

        UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET =
            sensorDistanceFromProjection + _height * 10 / 2; //height의 절반을 mm로 단위로 계산
        
        _heightSlider.value = heightCm;
        sensorDistance.value = sensorDistanceFromProjection;
        _screenRatioSlider.value = _screenRatio;
        
        
        ConfigureSlider(sensorDistance, SENSEOR_OFFSET_MAX_VALUE, value =>
        {
            sensorDistanceFromProjection = value;
            _TMP_seonsorDistance.text = "DISTANCE_FROM_SENSOR: " + sensorDistanceFromProjection.ToString("F1");
            UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET =
                sensorDistanceFromProjection + _height * 10 / 2; //height의 절반을 mm로 단위로 계산
        }, -2000);
        
        ConfigureSlider(_screenRatioSlider, SCREEN_RATIO_MAX, value =>
        {
            _screenRatio = value;
            _screenRatioSlider.minValue = SCREEN_RATIO_MIN;
            _TMP_ScreenRatio.text = "SCREEN RATIO: " + _screenRatio.ToString("F2");
        });

        _sensorEditModeButton = GameObject.Find("SensorEditModeCheckBox").GetComponentInChildren<Button>();
        _TMP_sensorEditMode = GameObject.Find("SensorEditModeCheckBox").GetComponentInChildren<TextMeshProUGUI>();
        _sensorEditModCheckImage = GameObject.Find("EditModeCheckImage").GetComponent<Image>();

        _sensorEditModCheckImage.enabled = isSensorEditMode;
        _TMP_sensorEditMode.text = isSensorEditMode ? "Sensor Edit Mode: ON" : "Sensor Edit Mode: OFF";

        _sensorEditModeButton.onClick.AddListener(OnEditSensorModeBtnClicked);
    }

    private WaitForSeconds _poolReturnWait;

    protected IEnumerator ReturnToPoolAfterDelay(RectTransform obj, Stack<RectTransform> pool)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(FP_Prefab.Limit_Time);

        yield return _poolReturnWait;
        obj.gameObject.SetActive(false);
#if UNITY_EDITOR

#endif

        pool.Push(obj); // Return the particle system to the pool
    }

    private void SetPool<T>(Stack<T> pool, string path, int poolCount = 500) where T : Object
    {
        if (SceneManager.GetActiveScene().name == "METAEDU_LAUNCHER") return;
        
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

        if (detectedPosRect == null)
        {
#if UNITY_EDITOR
            Debug.LogError("No RectTransform available in the pool.");
#endif
            return;
        }
#if UNITY_EDITOR
//        Debug.Log("Get RectRansfrom Pool.");
#endif

        detectedPosRect.anchoredPosition = new Vector2(rectX, rectY);
        detectedPosRect.gameObject.SetActive(true);
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
        var result = RplidarBinding.OnConnect(port);
        isMoterStarted = RplidarBinding.StartMotor();

        m_onscan = RplidarBinding.StartScan();
        Debug.Log("Connect on " + port + " result:" + result + "\nStartMotor:" + isMoterStarted + "StartScan:" +
                  m_onscan);

        if (m_onscan)
        {
            m_thread = new Thread(GenerateMesh);
            m_thread.Start();
        }

        var isSensorOn = isMoterStarted || m_onscan;
        OnSenSorInit?.Invoke(isSensorOn);

        Img_Rect_transform = GetComponent<RectTransform>();

        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();

        //guide라인이랑 동기화 기능
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x -
                Guideline.GetComponent<RectTransform>().rect.width / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y -
                Guideline.GetComponent<RectTransform>().rect.height / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x +
                Guideline.GetComponent<RectTransform>().rect.width / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y +
                Guideline.GetComponent<RectTransform>().rect.height / 2;

        TESTUI.SetActive(false);

        // _sensitivitySlider.onValueChanged.AddListener(_ =>
        //    {
        //  FP_Prefab.Limit_Time = _sensitivitySlider.value * 2f;
        //  });


        _projectorLookUpTable = new Dictionary<int, Vector2>();

        //IGameManager init이후에 동작해야합니다. 따라서 Awake가 아닌 Start에서만 사용해야합니다. 4/4/24
        //  _sensitivitySlider.value = IGameManager.DEFAULT_SENSITIVITY / 2;


    
      
        
        height = Managers.settingManager.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
        
          

        ///////////////////////////////////////////////////////////////////////// (1)

        // y_offset = ((X_length / screen_ratio) / 2) + distance;
        //X_length = _height / THROW_RATIO;
        // Y_length = X_length / (1920 / 1080);
        // correction_value = -(Resolution_X / (_screenRatio)) * (_height / THROW_RATIO / Resolution_Y);

        ///////////////////////// Pool
        _sensorDetectedPositionPool = new Stack<RectTransform>();
        SetPool(_sensorDetectedPositionPool, "Rplidar/FP");
    }


    private void GenerateMesh()
    {
        while (true)
        {
            var datacount = RplidarBinding.GetData(ref _lidarDatas);

            if (datacount == 0)
                Thread.Sleep(2);
            else
                m_datachanged = true;
        }
    }


    private int GenerateKey(int angle, int distance)
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 100 + angle.GetHashCode();
            hash = hash * 100 + distance.GetHashCode();
            return hash;
        }
    }


    private float _timer;

    private void GenerateDectectedPos()
    {
        if (!isMoterStarted) return;

        //  if (_timer < _sensitivitySlider.value)
        //   {
        //   _timer += Time.deltaTime;
        ////     return;
        //   }

        _timer = 0f;


        if (m_datachanged)
        {
            for (var i = 0; i < 720; i++)
            {
                var key = GenerateKey((int)_lidarDatas[i].theta * 10, (int)_lidarDatas[i].distant);

                //_lidarDatas[i].distant = Mathf.Clamp(_lidarDatas[i].distant, 0, 2550);

                //6배
                if (_lidarDatas[i].theta > 90 && _lidarDatas[i].theta < 270) continue;


                sensored_X = -_screenRatio * (_lidarDatas[i].distant * Mathf.Cos((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad));
                sensored_Y = -_screenRatio * (_lidarDatas[i].distant * Mathf.Sin((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad) -
                                     UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET);


                if (i % _filteringAmount == 0)
                    if (min_x < sensored_X && sensored_X < max_x)
                        if (min_y < sensored_Y && sensored_Y < max_y)
                        {
                            if (SF_Active)
                            {
                                // _filteringAmount = 8;
                                _filteringAmount = 4;
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                            else
                            {
                                _filteringAmount = 3;
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                        }
            }

            m_datachanged = false;
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        GenerateDectectedPos();
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
        UnBindLidar();
    }

    private void UnBindLidar()
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
        BALL_Active = !BALL_Active;

        return BALL_Active;
    }

    public bool SF_Active_ONOFF()
    {
        SF_Active = !SF_Active;

        return SF_Active;
    }
}