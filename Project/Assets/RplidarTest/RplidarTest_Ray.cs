using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RplidarTest_Ray : MonoBehaviour
{
    public string port;
    //public GameObject Capsule;

    private LidarData[] _lidarDatas;
    private RectTransform Img_Rect_transform;

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

    private bool _isSensorEditMode = true;

    private float _offsetX = 0;
    private readonly float OFFSET_X_MAX = 1000;
    
    
    private float _offsetY = 763.565f;
    private readonly float OFFSET_Y_MAX =1000;
    
    
    private readonly float SCREEN_RATIO_MIN =0.5f;
    private readonly float SCREEN_RATIO_MAX =2;
    private float _screenRatio = 1;
    
    public float offsetX { get =>_offsetX; set => _offsetX =value; }
    public float offsetY { get => _offsetY; set =>_offsetY = value; }
    public float screenRatio { get => _screenRatio;set => _screenRatio = value;}
    
    private Slider _offsetXSlider;
    private Slider _offsetYSlider;
    private Slider _screenRatioSlider;
    
    private TextMeshProUGUI _TMP_offsetX;
    private TextMeshProUGUI _TMP_offsetY;
    private TextMeshProUGUI _TMP_ScreenRatio;
    
    
    
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name.Contains("METAEDU")) return;
        _lidarDatas = new LidarData[LIDAR_DATA_SIZE];
        Init();

    }

    
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

    private void Init()
    {
        InitializeSlider("SensitivitySlider", out _sensitivitySlider, out _sensitivityText);
        InitializeSlider("OffsetXSlider", out _offsetXSlider, out _TMP_offsetX);
        InitializeSlider("OffsetYSlider", out _offsetYSlider, out _TMP_offsetY);
        InitializeSlider("ScreenRatioSlider", out _screenRatioSlider, out _TMP_ScreenRatio);

        _TMP_offsetX.text =  "OFFSET X: " + offsetX.ToString("F2");
        _TMP_offsetY.text = "OFFSET Y: " + offsetY.ToString("F2");
        _TMP_ScreenRatio.text = "SCREEN RATIO: " + screenRatio.ToString("F2");
       
        _offsetXSlider.value = offsetX;
        _offsetYSlider.value = offsetY;
        _screenRatioSlider.value = screenRatio;
        
        
        ConfigureSlider(_offsetXSlider, OFFSET_X_MAX, value =>
        {
            offsetX = value;
            _TMP_offsetX.text = "OFFSET X: " + offsetX.ToString("F2");
          
        },minVal:-1000);
    
        ConfigureSlider(_offsetYSlider, OFFSET_Y_MAX, value =>
        {
            offsetY = value;
            _TMP_offsetY.text = "OFFSET Y: " + offsetY.ToString("F2");
      
        },minVal:-1000);
    
        ConfigureSlider(_screenRatioSlider, SCREEN_RATIO_MAX, value =>
        {
            screenRatio = value;
            _screenRatioSlider.minValue = SCREEN_RATIO_MIN;
            _TMP_ScreenRatio.text = "SCREEN RATIO: " + screenRatio.ToString("F2");
         
        });
  
        _sensorEditModeButton = GameObject.Find("SensorEditModeCheckBox").GetComponentInChildren<Button>();
        _TMP_sensorEditMode = GameObject.Find("SensorEditModeCheckBox").GetComponentInChildren<TextMeshProUGUI>();
        _sensorEditModCheckImage = GameObject.Find("EditModeCheckImage").GetComponent<Image>();
        
        _sensorEditModCheckImage.enabled = _isSensorEditMode;
        _TMP_sensorEditMode.text = _isSensorEditMode ? "Sensor Edit Mode: ON" : "Sensor Edit Mode: OFF";
        
        _sensorEditModeButton.onClick.AddListener(OnEditSensorModeBtnClicked);

        
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
        _isSensorEditMode = !_isSensorEditMode;
        _sensorEditModCheckImage.enabled = _isSensorEditMode;
        _TMP_sensorEditMode.text = _isSensorEditMode ? "Sensor Edit Mode: ON" : "Sensor Edit Mode: OFF";
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
            m_thread = new Thread(GenerateMesh);
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

        _sensitivitySlider.onValueChanged.AddListener(_ =>
        {
            FP_Prefab.Limit_Time = _sensitivitySlider.value * 2f;
            _sensitivityText.text = $"sensitivity : {FP_Prefab.Limit_Time:F2}";
        });
      
        
        
        _projectorLookUpTable = new Dictionary<int, Vector2>();
        
        //IGameManager init이후에 동작해야합니다. 따라서 Awake가 아닌 Start에서만 사용해야합니다. 4/4/24
        _sensitivitySlider.value = IGameManager.DEFAULT_SENSITIVITY / 2;
    }


    void GenerateMesh()
    {
        while (true)
        {
            int datacount = RplidarBinding.GetData(ref _lidarDatas);

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
    
    // Update is called once per frame
    void FixedUpdate()
    {

        if (_timer < _sensitivitySlider.value)
        {
            _timer += Time.deltaTime;
            return;
        }
        
        _timer = 0f;
        
    
        if (m_datachanged)
        {
            for (int i = 0; i < LIDAR_DATA_SIZE; i++)
            {
             
                
                //센서 데이터 data[i].theta, distant
                //1. 화면과 센서를 일치화 시키기 위해서 theta를 마이너스 곱해줌, 추가로 회전 시켜주기 위해 Sensor_rotation 추가했고
                //위에서 아래 방향으로 내려다 보는것 기준으 90도 입력하면 댐
                //2. 0.74f는 실제 길이와 유니티내 맵핑이 일치하기 위한 보정값(빔프로젝터의 실제 화면과 오차가 있음), 1.07f는 발 위
                //에 정확히 찍히기 위한 보정값
                // Ex) 실제에서 682 mm -> 유니티 position 상 500, 보정값 0.733 곱해서 맞춰줌 실제 데이터를 position으로 변환함
                //3. 763.565f은 유니티 상의 캔버스 기준이 정가운데이기 때문에 그에 맞추기 위해 y값을 그 만큼 위로 올림
                
                //계산되어있을때? ==> Mathf.Rad2Deg(-_lidarDatas[i].theta)
                var key = GenerateKey((int)_lidarDatas[i].theta * 10, (int)_lidarDatas[i].distant);
                
                
                var processedTheta = -_lidarDatas[i].theta * Mathf.Deg2Rad; // 프로젝터값 등을 고려한 값
                var processedDistance = _lidarDatas[i].distant * 1.07f;
                
                if (_projectorLookUpTable.ContainsKey(key))
                {
                    Debug.LogWarning($"LUT REFFERRING....key {key}");
                    x = _projectorLookUpTable[key].x; 
                    y = _projectorLookUpTable[key].y;
                }
                else if(!_projectorLookUpTable.ContainsKey(key))
                {
                    Debug.LogWarning($"LUT CALCULATING & SAVING....key: {key}");
             
                    x = offsetX + screenRatio * Mathf.Cos(processedTheta) * (processedDistance);
                    y = offsetY + screenRatio * Mathf.Sin(processedTheta) * (processedDistance);
                    
                    //x = 프로젝터 높이 * 계산수식(크기~) + offsetX
                    Debug.Log($"좌표 계산결과 {x},{y}");
                    
                    var coordinate = new Vector2(x, y);
                    _projectorLookUpTable.TryAdd(key, coordinate);
                }

                if (_isSensorEditMode)
                {
                    
                    InstantiateMiddlePointPrefab(x, y);
                    return;
                }
                
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
                                    //게임플레이 로직에 좌표를 전달하는 역할
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
                                    ////게임플레이 로직에 좌표를 전달하는 역할
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