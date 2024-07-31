using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SensorManager : MonoBehaviour
{
  
// #if UNITY_EDITOR
//     public bool USE_SENSOR;
//     public static bool useSensor;
//     private static string port = useSensor ? "COM3" : string.Empty;
// #else
//   
// #endif

    private static string port ="COM3";
    public static bool isMoterStarted { get; private set; }
    
    private static LidarData[] _s_lidarDatas;


    //=====0714
    public GameObject BALLPrefab; 
    public GameObject MOUSEPrefab;
    public GameObject FPPrefab; 
    public GameObject middlePrefab;
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
    private float _height = 138; //cm 
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

    [SerializeField]
    public GameObject temp_pos;

    public readonly float SENSOR_ROTATION = 0;

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
    private readonly int LIDAR_DATA_SIZE = 720;
    
    //슬라이더를 통한 감도조절기능 추가(민석) 불필요시삭제 2/28/24
    private Slider _sensitivitySlider;
    private TextMeshProUGUI _sensitivityText;

    
    // 센서 측정 런타임 수정을 위한 맴버 선언
    //07.18/24 기준 화면 사이즈 320x180a(cm)
    
    private Dictionary<int, Vector2> _projectorLookUpTable;
    private Button _sensorEditModeButton;
    private TextMeshProUGUI _TMP_sensorEditMode;
    private Image _sensorEditModCheckImage;

    public  bool isSensorEditMode { get; private set; }

    private float SENSOR_DISTANCE_FROM_PROJECTION = 280; //mm
    private float ZERO_POINT_FROM_SENSOR;
    private readonly float SENSEOR_OFFSET_MAX_VALUE =1000;
    
    
    private readonly float SCREEN_RATIO_MIN =0.5f;
    private readonly float SCREEN_RATIO_MAX =10;

    
    public int heightCm { get; set; } = 2;
    public float sensorDistanceFromProjection { get => SENSOR_DISTANCE_FROM_PROJECTION; set =>SENSOR_DISTANCE_FROM_PROJECTION = value; }
    // private float screen_ratio;// 화면비 // 유니티 height 1080 : 실제 프로젝션 height (mm)를 비교하여 비례를 조정 
    private float _screenRatio = 0.782f;
  
    
    private Slider _heightSlider;
    private Slider sensorDistance;
    private Slider _screenRatioSlider;
    
    private TextMeshProUGUI _TMP_hiehgt;
    private TextMeshProUGUI _TMP_seonsorDistance;
    private TextMeshProUGUI _TMP_ScreenRatio;
    private int _filteringRate = 2; // 숫자 클수록 많은 필터링 
    ////////////////// 0719- 센서 테스트용 멤버 새로 추가한 부분///////////////////////////////
    
    
    /// <summary>
    /// C#기준으로 out을 사용하여 초기화 불필요, 반환형식으로 사용
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
        
        if (SceneManager.GetActiveScene().name.Contains("METAEDU")) return;
        if (_s_lidarDatas != null) _s_lidarDatas = new LidarData[LIDAR_DATA_SIZE];
        
        // _width = _height * (Resolution_X / Resolution_Y);
        _screenRatio = Resolution_Y / (_height * 10);

        InitializeSlider("SensitivitySlider", out _sensitivitySlider, out _sensitivityText);
        InitializeSlider("HeightSlider", out _heightSlider, out _TMP_hiehgt);
        InitializeSlider("OffsetYSlider", out sensorDistance, out _TMP_seonsorDistance);
        InitializeSlider("ScreenRatioSlider", out _screenRatioSlider, out _TMP_ScreenRatio);

        ZERO_POINT_FROM_SENSOR = SENSOR_DISTANCE_FROM_PROJECTION + _height * 10 / 2; //height의 절반을 mm로 단위로 계산

        _heightSlider.value = heightCm;
        sensorDistance.value = sensorDistanceFromProjection;
        _screenRatioSlider.value = _screenRatio;

        ConfigureSlider(_heightSlider, HEIGHT_MAX, value =>
        {
            _height = value;
            //_screenRatio = (Resolution_Y / _height * 10);
            ZERO_POINT_FROM_SENSOR = SENSOR_DISTANCE_FROM_PROJECTION + _height * 10 / 2; //height의 절반을 mm로 단위로 계산
            _TMP_hiehgt.text = "HEIGHT : " + _height.ToString("F1");
        }, HEIGHT_MIN);

        _TMP_hiehgt.text = $"{nameof(heightCm)}: " + _height.ToString("F1");
        _TMP_seonsorDistance.text = "Sensor Distance: " + sensorDistanceFromProjection.ToString("F1");
        _TMP_ScreenRatio.text = "SCREEN RATIO: " + _screenRatio.ToString("F1");

        ConfigureSlider(sensorDistance, SENSEOR_OFFSET_MAX_VALUE, value =>
        {
            sensorDistanceFromProjection = value;
            _TMP_seonsorDistance.text = "DISTANCE_FROM_SENSOR: " + sensorDistanceFromProjection.ToString("F1");
            ZERO_POINT_FROM_SENSOR = sensorDistanceFromProjection + _height * 10 / 2; //height의 절반을 mm로 단위로 계산
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
        
        RplidarBinding.EndScan();
        RplidarBinding.EndMotor();
        
        
        int result = RplidarBinding.OnConnect(port);
        Debug.Log("Connect on " + port + " result:" + result);

        isMoterStarted = RplidarBinding.StartMotor();
        Debug.Log("StartMotor:" + isMoterStarted);

        m_onscan = RplidarBinding.StartScan();
        Debug.Log("StartScan:" + m_onscan);

        if (m_onscan)
        {
            m_thread = new Thread(GenerateMesh);
            m_thread.Start();
        }


     
    }

    private void Start()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();
        
        //guide라인이랑 동기화 기능
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x - (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y - (Guideline.GetComponent<RectTransform>().rect.height) / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x + (Guideline.GetComponent<RectTransform>().rect.width) / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y + (Guideline.GetComponent<RectTransform>().rect.height) / 2;

        TESTUI.SetActive(false);

        _sensitivitySlider.onValueChanged.AddListener(_ =>
        {
            FP_Prefab.Limit_Time = _sensitivitySlider.value * 2f;
            _sensitivityText.text = $"sensitivity : {FP_Prefab.Limit_Time:F2}";
        });
      
        
        
        _projectorLookUpTable = new Dictionary<int, Vector2>();
        
        //IGameManager init이후에 동작해야합니다. 따라서 Awake가 아닌 Start에서만 사용해야합니다. 4/4/24
        _sensitivitySlider.value = IGameManager.DEFAULT_SENSITIVITY / 2;
        
        
        _sensorDetectedPositionPool = new Stack<RectTransform>();
        SetPool(_sensorDetectedPositionPool, "Rplidar/FP");
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
    
    private void SetPool<T>(Stack<T> pool, string path, int poolCount = 500)  where T : UnityEngine.Object
    {
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
    

    private void ConfigureSlider(Slider slider, float maxValue, UnityEngine.Events.UnityAction<float> onValueChanged,
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

    private void OnApplicationQuit()
    {
        RplidarBinding.EndScan();
        RplidarBinding.EndMotor();
    }
    


    void GenerateMesh()
    {
        while (true)
        {
            int datacount = RplidarBinding.GetData(ref _s_lidarDatas);

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

    
    
    int GenerateKey(int angle, int distance)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 100 + angle.GetHashCode();
            hash = hash * 100 + distance.GetHashCode();
            return hash;
        }
    }


    private float _timer;

    private void GenerateDectectedPos()
    {

        if (!isMoterStarted) return;
        
        if (_timer < _sensitivitySlider.value)
        {
            _timer += Time.deltaTime;
            return;
        }
        
        _timer = 0f;
        
    
        if (m_datachanged)
        {
            for (int i = 0; i < 720; i++)
            {

                //var key = GenerateKey((int)_s_lidarDatas[i].theta * 10, (int)_s_lidarDatas[i].distant);
                //_lidarDatas[i].distant = Mathf.Clamp(_lidarDatas[i].distant, 0, 2550);
                
                if(_s_lidarDatas[i].theta >90 && _s_lidarDatas[i].theta <270)continue;


                x = -_screenRatio * (_s_lidarDatas[i].distant * Mathf.Cos((90-_s_lidarDatas[i].theta)* Mathf.Deg2Rad));
                y = -_screenRatio * (_s_lidarDatas[i].distant * Mathf.Sin((90-_s_lidarDatas[i].theta) * Mathf.Deg2Rad) - ZERO_POINT_FROM_SENSOR);
               
          
                if (i % _filteringRate == 0)
                {
                    if (min_x < x && x < max_x)
                    {
                        if (min_y < y && y < max_y)
                        {
                            if (SF_Active)
                            {
                                // _filteringAmount = 8;
                                _filteringRate = 4;
                                ShowFilteredSensorPos(x, y);
                            }
                            else
                            {
                                _filteringRate = 3;
                                ShowFilteredSensorPos(x, y);
                            }
                         
                        }
                        
                    }

                }
            }
            m_datachanged = false;
        }

    }
    
    void FixedUpdate()
    {

        GenerateDectectedPos();
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
    

    public void InstantiateMiddlePointPrefab(float temp_x, float temp_y)
    {
        GameObject Prefab_pos = Instantiate(middlePrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0), UI_Canvas.transform);
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