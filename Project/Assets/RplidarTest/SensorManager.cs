using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
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
    //private RectTransform Img_Rect_transform;

    public static event Action<bool> OnSenSorInit;


    //=====071423
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

    public GameObject Guideline;
    //

    //1015
    private static float _height = 172;

    public static float height
    {
        get => _height;
        set
        {
#if UNITY_EDITOR
            Logger.Log($"Height Value changed {_height}---------------------------------");
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

    [SerializeField] public GameObject temp_pos;

    public readonly float SENSOR_ROTATION = 0;

    //1121

    private GameObject UI_Canvas;
    private Camera UI_Camera;


    private float sensored_X;
    private float sensored_Y;

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
    public float _screenRatio = 0.782f;

    ////////////////// 0719- 센서 테스트용 멤버 새로 추가한 부분///////////////////////////////

    private float correction_value; // 화면과 유니티에서의 단위를 맞추기 위한 보정값.

    //0311 센서 위치 보정 위해 추가

    private RectTransform RT_Lidar_object; // 화면과 유니티에서의 단위를 맞추기 위한 보정값.

    private void Awake()
    {
        //런쳐도 센서로 터치 가능하도록 수정 09/24/2024
        //if (SceneManager.GetActiveScene().name.Contains("METAEDU")) return;

        _lidarDatas = new LidarData[LIDAR_DATA_SIZE];
        _sensitivitySlider = GameObject.Find("SensitivitySlider").GetComponent<Slider>();
        UI_Scene_StartBtn.OnSensorRefreshEvent -= AsyncInitSensor;
        UI_Scene_StartBtn.OnSensorRefreshEvent += AsyncInitSensor;
        // _width = _height * (Resolution_X / Resolution_Y);
    }

    private void OnDestroy()
    {
        UI_Scene_StartBtn.OnSensorRefreshEvent -= AsyncInitSensor;
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
    //0311 private -> public
    //0311 런처로 센서 기능 정상 테스트가 불가능하므로 수정
    public async void AsyncInitSensor()
    {
        await InitSensorAsync();
    }

    public async void BindSensorPortPath()
    {
         bindPortResult =  RplidarBinding.OnConnect(PORT);
        if (bindPortResult < 0)
        {
            bindPortResult =  RplidarBinding.OnConnect(PORT == "COM3" ? "COM4" : "COM3");
        }
    }
 

    private int bindPortResult;
    private async Task InitSensorAsync()
    {
        await Task.Delay(_refreshWaitTimeSpan);
        

        isMoterStarted = await Task.Run(() => RplidarBinding.StartMotor());
        m_onscan = await Task.Run(() => RplidarBinding.StartScan());
        
        Debug.Log("Connect on " + PORT + " result:" + bindPortResult + "\nStartMotor:" + isMoterStarted + "StartScan:" +
                  m_onscan);

        if (m_onscan)
        {
            m_thread = new Thread(GenerateMesh);
            m_thread.Start();
        }

        var isSensorOn = isMoterStarted || m_onscan;

        ////[삭제]
        //Img_Rect_transform = GetComponent<RectTransform>();

        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();


        // guide라인이랑 동기화 기능, 이 부분이 최초에 한번 실행되게끔 되는데 그 외에 다른 부분에서도 적용이 되었음 하는데
        Set_GuideLine();

        //[삭제]
        //Guideline.SetActive(false);

        _sensitivitySlider.onValueChanged.AddListener(_ =>
        {
            _sensorSensitivity = _sensitivitySlider.value;
            _poolReturnWait = new WaitForSeconds(sensorSensitivity);

            Logger.Log($"prefab limit time is {_sensitivitySlider.value}");
        });

        _projectorLookUpTable = new Dictionary<int, Vector2>();

        //[삭제]
        //UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET =
        //    sensorDistanceFromProjection + _height * 10 / 2; // height의 절반을 mm로 단위로 계산

        //height = Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
        //_screenRatio = Resolution_Y / (height * 10);
        //Debug.Log($"Height Set FROM XML:{Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML}");
        //Debug.Log($"Ratio:{_screenRatio}");

        _sensorDetectedPositionPool = new Stack<RectTransform>();
        SetPool(_sensorDetectedPositionPool, "Rplidar/FP_New");

        //0311 각각 가로,세로,대각선 별로 풀 준비함
        _SDPP_realpoint = new Stack<RectTransform>();
        SetPool(_SDPP_realpoint, "Rplidar/FP_REAL");

        OnSenSorInit?.Invoke(isSensorOn);
    }
    public void Set_GuideLine()
    {
        min_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x -
            Guideline.GetComponent<RectTransform>().rect.width / 2;
        min_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y -
                Guideline.GetComponent<RectTransform>().rect.height / 2;
        max_x = Guideline.GetComponent<RectTransform>().anchoredPosition.x +
                Guideline.GetComponent<RectTransform>().rect.width / 2;
        max_y = Guideline.GetComponent<RectTransform>().anchoredPosition.y +
                Guideline.GetComponent<RectTransform>().rect.height / 2;
    }


    #region 코루틴 센서연결 파트

    public void InitSensorCo()
    {
        StartCoroutine(InitSensorCoroutine());
    }

private IEnumerator InitSensorCoroutine()
{
    yield return _refreshWait;

    var result = RplidarBinding.OnConnect(PORT);
    isMoterStarted = RplidarBinding.StartMotor();

    m_onscan = RplidarBinding.StartScan();
    Debug.Log("Connect on " + PORT + " result:" + result + "\nStartMotor:" + isMoterStarted + "StartScan:" +
              m_onscan);

    if (m_onscan)
    {
        m_thread = new Thread(GenerateMesh);
        m_thread.Start();
    }

    var isSensorOn = isMoterStarted || m_onscan;
    
    InitUI();
    OnSenSorInit?.Invoke(isSensorOn);
}


private void InitUI()
{
    //[삭제]
    //Img_Rect_transform = GetComponent<RectTransform>();

    UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
    UI_Camera = Manager_Sensor.instance.Get_UIcamera();

    var guidelineRect = Guideline.GetComponent<RectTransform>();
    min_x = guidelineRect.anchoredPosition.x - guidelineRect.rect.width / 2;
    min_y = guidelineRect.anchoredPosition.y - guidelineRect.rect.height / 2;
    max_x = guidelineRect.anchoredPosition.x + guidelineRect.rect.width / 2;
    max_y = guidelineRect.anchoredPosition.y + guidelineRect.rect.height / 2;

        Guideline.SetActive(false);

    _sensitivitySlider.onValueChanged.AddListener(_ =>
    {
        _sensorSensitivity = _sensitivitySlider.value;
        _poolReturnWait = new WaitForSeconds(_sensorSensitivity);

        Logger.Log($"prefab limit time is {_sensitivitySlider.value}");
    });

    _projectorLookUpTable = new Dictionary<int, Vector2>();

        //[삭제]
    //UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET =
    //    sensorDistanceFromProjection + _height * 10 / 2;

    //height = Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
    //_screenRatio = Resolution_Y / (height * 10);

    //Debug.Log($"Height Set FROM XML:{Managers.Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML}");
    //Debug.Log($"Ratio:{_screenRatio}");

    _sensorDetectedPositionPool = new Stack<RectTransform>();
    SetPool(_sensorDetectedPositionPool, "Rplidar/FP_New");

    _SDPP_realpoint = new Stack<RectTransform>();
    SetPool(_SDPP_realpoint, "Rplidar/FP_REAL");
}

// 별도 쓰레드를 돌리던 GenerateMesh()를 코루틴으로 실행
private IEnumerator RunGenerateMesh()
{
    GenerateMesh();
    yield return null;
}

    #endregion
    
    
    
    
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
    private Stack<RectTransform> _SDPP_realpoint;


    private static WaitForSeconds _poolReturnWait;

    protected IEnumerator ReturnToPoolAfterDelay(RectTransform obj, Stack<RectTransform> pool)
    {
        if (_poolReturnWait == null) _poolReturnWait = new WaitForSeconds(sensorSensitivity);

        yield return _poolReturnWait;
        obj.gameObject.SetActive(false);

        pool.Push(obj); // Return the particle system to the pool
    }


    private Canvas _launcherSettingCanvas;
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
    private void SFSP_realpoint(float rectX, float rectY)
    {
        var detectedPosRect = GetFromPool(_SDPP_realpoint);

        if (detectedPosRect == null) return;

#if UNITY_EDITOR
        //        Debug.Log($"sensor: {rectX},{rectY}");
#endif
        detectedPosRect.anchoredPosition = new Vector2(rectX, rectY);
        detectedPosRect?.gameObject.SetActive(true);
        StartCoroutine(ReturnToPoolAfterDelay(detectedPosRect, _SDPP_realpoint));
    }

    //[삭제]
    //private void ConfigureSlider(Slider slider, float maxValue, UnityAction<float> onValueChanged,
    //    float minVal = 0)
    //{
    //    slider.minValue = minVal;
    //    slider.maxValue = maxValue;
    //    slider.onValueChanged.AddListener(onValueChanged);
    //}

    //[삭제]
    //private void OnEditSensorModeBtnClicked()
    //{
    //    isSensorEditMode = !isSensorEditMode;
    //    _sensorEditModCheckImage.enabled = isSensorEditMode;
    //    _TMP_sensorEditMode.text = isSensorEditMode ? "Sensor Edit Mode: ON" : "Sensor Edit Mode: OFF";
    //}

    private void Start()
    {
        //InitSensor();
        //Sensor calibration
        RT_Lidar_object = GetComponent<RectTransform>();
        Set_Screenscale(XmlManager.Instance.ScreenSize);
        if (Calibration_state_Screenratio != null) Calibration_state_Screenratio.text = _screenRatio.ToString("0.00");

        //(확인 필요)문제 생길시 센서 연동하는 타이밍으로 순서 변경 필요
        Get_xmlsetting();

        BindSensorPortPath();
    }
    private void OnApplicationQuit()
    {
        StopSensor();
    }



    //0311 private -> public
    public void StopSensor()
    {
        RplidarBinding.EndScan();
        RplidarBinding.EndMotor();
        RplidarBinding.OnDisconnect();
        RplidarBinding.ReleaseDrive();

        //StopCoroutine(GenMesh());

        m_thread?.Abort();
        m_onscan = false;

        BindSensorPortPath();
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
    
    public void Set_Screenscale(float num)
    {
        Screen_Scale = num;
    }

    private float _timer;

    //========== Sensor Calibration
    float Sensor_posx;
    float Sensor_posy;
    int _filteringAmount = 2;

    //[삭제]
    //private int calibrationStep = 0;

    public Text Calibration_state_indetail;
    public Text Calibration_state;
    public Text Calibration_state_Screenratio;

    public Text State_rotation;
    public Text State_scan;

    //[삭제]
    //public bool isCalibrationActive_Screenratio = false; //화면 비율 보정 모드 활성화 여부
    public bool isCalibrationActive_SensorPos = false; // 센서 위치 보정 모드 활성화 여부
    public bool isCalibration_SR_Active = false; //화면 비율 보정 모드 활성화 여부


    public float Screen_Scale = 1f;
    public Vector2 Center_Point = new Vector2(0, 0); //화면 중앙 기준 좌표
    public Vector2 screenRatioCalibrationTarget = new Vector2(-500f, 0f); // 화면 중앙 왼쪽점 기준 좌표



    private void GenerateDectectedPos()
    {
        //실시간 상황을 캐치할 수는 없음
        if(State_rotation!=null)State_rotation.text = isMoterStarted ? "Motor ON" : "Motor OFF";
        if(State_scan!=null)State_scan.text = m_onscan ? "Scan ON" : "Scan OFF";

        //센서 위치 보정
        Sensor_posx = RT_Lidar_object.anchoredPosition.x;
        Sensor_posy = RT_Lidar_object.anchoredPosition.y;

        List<Vector2> detectedPoints = new List<Vector2>(); // 감지된 포인트 리스트

        if (!isMoterStarted) return;
        if (Managers.isGameStopped) return;
        _timer = 0f;

        if (m_datachanged)
        {
            for (var i = 0; i < 720; i++)
            {
                // 6배
                if (_lidarDatas[i].theta > 90 && _lidarDatas[i].theta < 270) continue;

                // 기존 좌표 계산
                Vector2 rawSensorPos = new Vector2(
                    Screen_Scale * (Sensor_posx - _screenRatio * (_lidarDatas[i].distant * Mathf.Cos((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad))),
                    Screen_Scale * (Sensor_posy - _screenRatio * (_lidarDatas[i].distant * Mathf.Sin((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad)))
                );

                
                // 4개 좌표 TL,TR,BL,BR 보정용, 필요시 삭제 필요
                //원래 아래 내용이었으나 정리하면서 수정되었음
                //Vector2 correctedPos = isCalibrationApplied ? GetCorrectedPosition(rawSensorPos) : rawSensorPos;

                Vector2 correctedPos = rawSensorPos;
                sensored_X = correctedPos.x;
                sensored_Y = correctedPos.y;

                if (i % _filteringAmount == 0)
                    if (min_x < sensored_X && sensored_X < max_x)
                        if (min_y < sensored_Y && sensored_Y < max_y)
                        {
                            // ✅ 0311 추가
                            detectedPoints.Add(correctedPos); // ✅ 보정된 좌표 저장

                            if (SF_Active)
                            {
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                            else
                            {
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                        }
#if UNITY_EDITOR
                // Debug.Log($"sensor: {sensored_X},{sensored_Y} , {_screenRatio}");
#endif
            }

            //전체 센서 측정 좌표 그룹화
            objectClusters = ClusterPoints(detectedPoints, thresholdDistance);

            //Debug.Log($"감지된 물체 개수: {objectClusters.Count}");

            foreach (var cluster in objectClusters)
            {
                string orientation = DetectFootOrientation(cluster);

                // 실터치 지점 계산 & 마커 생성 
                Vector2 touchPoint = CalculateTouchPoint(cluster, orientation);

                if (!isFeatureActive)
                {
                    //실제 터치 발생하는 좌표 확인용, 터치 발생
                    CreateTouchMarker(touchPoint);
                }
                else if (isFeatureActive)
                {
                    //터치 이벤트 기능 확인용, 기존 createtouchmarker도 내부에 있으므로 터치도 마찬가지 발생
                    //여기서도 보고 싶을 것 같은데 그럼 parent설정을 해주는걸로
                    HandleTouchEvents(touchPoint);
                }

                // 센서 위치 보정
                if (isCalibrationActive_SensorPos)
                {
                    CalibrateSensor(touchPoint);
                }

                //Screen ratio 계산
                if (isCalibration_SR_Active)
                {
                    CalibrateScreenRatio(touchPoint);
                }

                //// 화면 비율 보정
                //if (isCalibrationActive_Screenratio)
                //{

                //    SaveSensorPoint(touchPoint);
                //}


                //// ✅ Homography 보정 활성화 시, 터치 좌표 수집 후 보정 수행
                //if (isCalibrationActive)
                //{
                //    CollectCalibrationPoint(touchPoint);
                //}
            }

            m_datachanged = false;
        }
    }

    //[삭제]
    ////센서 실제 화면 보정 관련
    //public bool isCalibrationActive = false; // ✅ 보정 모드 활성화 여부
    //public bool isCalibrationApplied = false;
    //public Vector2 TL_Point = new Vector2(-750, 350); // ✅ 화면 중앙 기준 좌표
    //public Vector2 TR_Point = new Vector2(750, 350); // ✅ 화면 중앙 기준 좌표
    //public Vector2 BL_Point = new Vector2(-750, -350); // ✅ 화면 중앙 기준 좌표
    //public Vector2 BR_Point = new Vector2(750, -350); // ✅ 화면 중앙 기준 좌표

    //private Vector2 Measure_TL_Point;
    //private Vector2 Measure_TR_Point;
    //private Vector2 Measure_BL_Point;
    //private Vector2 Measure_BR_Point;

    //// ✅ Homography 변환 행렬
    //private Matrix4x4 homographyMatrix;

    //private void CollectCalibrationPoint(Vector2 touchPoint)
    //{
    //    if (!isCalibrationActive) return;

    //    switch (calibrationStep)
    //    {
    //        case 0:
    //            Measure_TL_Point = touchPoint;
    //            calibrationStep++;
    //            Calibration_state_indetail.text = "TL 좌표 저장 완료";
    //            isCalibrationActive = false;
    //            break;
    //        case 1:
    //            Measure_TR_Point = touchPoint;
    //            calibrationStep++;
    //            Calibration_state_indetail.text = "TR 좌표 저장 완료";
    //            isCalibrationActive = false;
    //            break;
    //        case 2:
    //            Measure_BL_Point = touchPoint;
    //            calibrationStep++;
    //            Calibration_state_indetail.text = "BL 좌표 저장 완료";
    //            isCalibrationActive = false;
    //            break;
    //        case 3:
    //            Measure_BR_Point = touchPoint;
    //            calibrationStep++;
    //            Calibration_state_indetail.text = "BR 좌표 저장 완료";
    //            isCalibrationActive = false;
    //            break;
    //    }
    //    if (calibrationStep >= 4)
    //    {
    //        Debug.Log("✅ 모든 좌표가 저장되었습니다. 보정을 적용할 수 있습니다.");
    //        isCalibrationActive = false;
    //    }
    //}

    //public void ApplyCalibration()
    //{
    //    if (calibrationStep < 4)
    //    {
    //        Debug.LogWarning("⚠️ 모든 기준점을 설정해야 보정을 적용할 수 있습니다!");
    //        return;
    //    }

    //    homographyMatrix = ComputeHomography(
    //        new Vector2[] { Measure_TL_Point, Measure_TR_Point, Measure_BL_Point, Measure_BR_Point },
    //        new Vector2[] { TL_Point, TR_Point, BL_Point, BR_Point });

    //    isCalibrationApplied = true;
    //    isCalibrationActive = false;

    //    //텍스트 변경 필요
    //    Debug.Log("✅ Homography 보정 적용 완료!");
    //    Calibration_state.text = "보정 값 적용 중";
    //}

    //private Vector2 GetCorrectedPosition(Vector2 rawPos)
    //{
    //    Vector3 transformed = homographyMatrix.MultiplyPoint3x4(new Vector3(rawPos.x, rawPos.y, 1));
    //    return new Vector2(transformed.x, transformed.y);
    //}

    //private Matrix4x4 ComputeHomography(Vector2[] src, Vector2[] dst)
    //{
    //    return Matrix4x4.identity; // Homography 변환 알고리즘 추가 필요
    //}



    //  터치 영역 관리
    public List<GameObject> touchZoneObjects = new List<GameObject>(); //  터치 영역 리스트
    public Dictionary<Vector2, float> activeTouchZones = new Dictionary<Vector2, float>(); // 터치 위치별 지속 시간
    public List<Vector2> touchZoneList = new List<Vector2>(); // 현재 존재하는 터치 이미지 리스트
    public int maxTouchZones = 20; //  동시에 유지할 수 있는 최대 터치 영역 개수

    public GameObject touchZonePrefab; // 터치 영역을 시각화할 프리팹
    public Transform touchZoneParent; // 터치 영역을 관리할 부모 오브젝트
    //(에러) 위 오브젝트가 할당되지 않았을 때 에러가 뜨는 이유는 터치존을 찍고 있어서임, 콘텐츠 내에서 터치존을 볼지 말지를 결정을 해야할 것 같음

    public float Touch_range = 35f; // 터치 비교 범위

    //#0311 정확도 개선 관련 부분
    //inputfield의 경우 오직 메인화면에서 센서 기능 개선 부분에서만 볼 수 있도록 할 것임

    private List<List<Vector2>> objectClusters = new List<List<Vector2>>();

    public float thresholdDistance;      //그룹화를 위한 threshold
    public float adjustYHorizontal;
    public float adjustYVertical;
    public float adjustXDiagonal;
    public float adjustYDiagonal;
    public float adjustXDiagonalLeft;

    public bool isFeatureActive = true; //기본 상태 : true 실제 터치 좌표만 확인
    public float touchThreshold = 10f; // 터치 변화 감지 임계값


    /// 센서 보정 함수: 터치한 좌표를 기준으로 센서의 위치를 이동
    public void CalibrateSensor(Vector2 touchPoint)
    {
        Debug.Log("센서 위치 보정함");

        // 화면 정중앙과 터치 지점의 차이를 계산하여 보정
        float offsetX = Center_Point.x - touchPoint.x;
        float offsetY = Center_Point.y - touchPoint.y;

        Debug.Log($"🔧 센서 보정: OffsetX={offsetX}, OffsetY={offsetY}");

        // 보정된 값으로 센서 위치 이동 (this.transform 사용) 
        RectTransform rectTransform = this.GetComponent<RectTransform>();
        rectTransform.anchoredPosition += new Vector2(offsetX, offsetY);

        //최종 저장 값 xml 파일 저장
        XmlManager.Instance.SensorPosX = rectTransform.anchoredPosition.x;
        XmlManager.Instance.SensorPosY = rectTransform.anchoredPosition.y;
        XmlManager.Instance.SaveSettings();

        // 센서 보정 완료 후 보정 모드 비활성화
        isCalibrationActive_SensorPos = false;
    }

    ///  터치 이벤트 한번만 발생할 수 있도록 하는 기능
    private void HandleTouchEvents(Vector2 touchPoint)
    {
        GameObject existingZone = FindTouchZoneAtPoint(touchPoint);

        if (existingZone != null)
        {
            //기존 터치 영역 내에서 터치가 감지되면 타이머 리셋
            if (existingZone.GetComponent<TouchZone>() != null)
            {
                existingZone.GetComponent<TouchZone>().ResetTimer();
            }
            return;
        }

        if (touchZoneObjects.Count >= maxTouchZones)
        {
            RemoveOldestTouchZone();
        }

        GameObject newZone = CreateTouchZoneVisual(touchPoint);
        touchZoneObjects.Add(newZone);
        CreateTouchMarker(touchPoint);
    }
    /// 특정 터치 위치가 기존 터치 영역 내에 있는지 확인
    private GameObject FindTouchZoneAtPoint(Vector2 touchPoint)
    {
        foreach (GameObject zone in touchZoneObjects)
        {
            if (zone == null) continue; // 삭제된 오브젝트 무시

            Vector2 zonePos = zone.GetComponent<RectTransform>().anchoredPosition;

            if (Mathf.Abs(zonePos.x - touchPoint.x) <= Touch_range && Mathf.Abs(zonePos.y - touchPoint.y) <= Touch_range)
            {
                return zone;
            }
        }
        return null;
    }

    /// 터치 영역 위에 터치가 있는지 확인
    private bool IsTouchActive(Vector2 zonePos)
    {
        foreach (var kvp in touchZoneObjects)
        {
            Vector2 existingPos = kvp.GetComponent<RectTransform>().anchoredPosition;

            // 해당 터치 영역 위에 새로운 터치가 있는지 확인
            if (Mathf.Abs(existingPos.x - zonePos.x) <= Touch_range && Mathf.Abs(existingPos.y - zonePos.y) <= Touch_range)
            {
                //Debug.Log("터치 영역 위에 터치 포인트 있음");

                return true;
            }
            //Debug.Log($"🟢 터치 영역 위에 터치 포인트 없음! 비교한 데이터: {existingPos} {zonePos}");
        }
        return false;
    }

    ///  가장 오래된 터치 영역 삭제,
    ///  (중요) 기능 구현은 완료되었으나 실질적으로 테스트하지 못 햇음
    private void RemoveOldestTouchZone()
    {
        if (touchZoneObjects.Count > 0)
        {
            GameObject oldestZone = touchZoneObjects[0];
            touchZoneObjects.RemoveAt(0);
            Destroy(oldestZone); //  터치 영역 삭제
                                 // Debug.Log($"⚠️ 터치 영역 초과 - 가장 오래된 영역 제거");
        }
    }


    ///  터치 영역을 시각화하는 오브젝트 생성
    private GameObject CreateTouchZoneVisual(Vector2 position)
    {
        if (touchZonePrefab == null) return null;

        GameObject newTouchZone = Instantiate(touchZonePrefab);
        newTouchZone.transform.SetParent(touchZoneParent.transform, false);

        RectTransform rectTransform = newTouchZone.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = new Vector2(position.x, position.y);
        rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0f);

        return newTouchZone;
    }
    public void ResetTouchZones()
    {
        // 기존 터치 영역 삭제
        foreach (GameObject zone in touchZoneObjects)
        {
            Destroy(zone);
        }
        touchZoneObjects.Clear();

        //  터치 관련 변수 초기화
        activeTouchZones.Clear();
        touchZoneList.Clear();
        _timer = 0f;

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

    //[센서 데이터 클러스터링 및 터치 기능]
    /// Sensor data clustering
    private List<List<Vector2>> ClusterPoints(List<Vector2> points, float distanceThreshold)
    {
        List<List<Vector2>> clusters = new List<List<Vector2>>();
        HashSet<Vector2> visited = new HashSet<Vector2>();

        foreach (Vector2 point in points)
        {
            if (visited.Contains(point)) continue;

            List<Vector2> cluster = new List<Vector2>();
            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(point);

            while (queue.Count > 0)
            {
                Vector2 current = queue.Dequeue();
                if (visited.Contains(current)) continue;

                visited.Add(current);
                cluster.Add(current);

                foreach (Vector2 neighbor in points)
                {
                    if (!visited.Contains(neighbor) && Vector2.Distance(current, neighbor) < distanceThreshold)
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (cluster.Count > 0)
            {
                clusters.Add(cluster);
            }
        }
        return clusters;
    }

    /// 발 방향 판별
    private string DetectFootOrientation(List<Vector2> cluster)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var point in cluster)
        {
            if (point.x < minX) minX = point.x;
            if (point.x > maxX) maxX = point.x;
            if (point.y < minY) minY = point.y;
            if (point.y > maxY) maxY = point.y;
        }

        float width = maxX - minX;
        float height = maxY - minY;

        if (height > width * 1.2f) return "Vertical";
        else if (width > height * 1.2f) return "Horizontal";
        else return "Diagonal";
    }

    /// 실제 터치 지점 보정 위치 계산
    private Vector2 CalculateTouchPoint(List<Vector2> cluster, string orientation)
    {
        Vector2 center = CalculateCenterPoint(cluster);
        bool isLeftSide = center.x < Sensor_posx; // 센서 기준 왼쪽인지 확인

        if (orientation == "Horizontal")
        {
            return new Vector2(center.x, center.y + adjustYHorizontal);
        }
        else if (orientation == "Vertical")
        {
            return new Vector2(center.x, center.y + adjustYVertical);
        }
        else // Diagonal
        {
            if (isLeftSide)
                return new Vector2(center.x + adjustXDiagonalLeft, center.y + adjustYDiagonal); // 왼쪽 보정
            else
                return new Vector2(center.x + adjustXDiagonal, center.y + adjustYDiagonal); // 오른쪽 보정
        }
    }
    /// 중심좌표 계산
    private Vector2 CalculateCenterPoint(List<Vector2> cluster)
    {
        float sumX = 0f, sumY = 0f;
        foreach (var point in cluster)
        {
            sumX += point.x;
            sumY += point.y;
        }
        return new Vector2(sumX / cluster.Count, sumY / cluster.Count);
    }

    /// 실제 터치 지점 마커 생성
    private void CreateTouchMarker(Vector2 touchPoint)
    {
        Sensor_posx = touchPoint.x;
        Sensor_posy = touchPoint.y;
        SFSP_realpoint(Sensor_posx, Sensor_posy);
    }

    //[화면 비율 보정 부분]
    // 비율 기반 screenRatio 보정 함수
    private void CalibrateScreenRatio(Vector2 touchPoint)
    {
        float measuredX = touchPoint.x;
        float expectedX = screenRatioCalibrationTarget.x;

        float denom = Sensor_posx - measuredX;
        if (Mathf.Abs(denom) < 10f) return; // 안정성 보호

        float newRatio = (Sensor_posx - expectedX) / denom;

        if (newRatio < 0.2f || newRatio > 2.0f) return; // 비정상 보정 방지

        _screenRatio *= newRatio;
        Debug.Log($"✅ 보정 완료: newRatio={newRatio:F3}, 최종 _screenRatio={_screenRatio:F3}");

        isCalibration_SR_Active = false;

        //가이드라인 원상 복구
        Guideline.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
    }
    
    public bool UI_Active_ONOFF()
    {
        //[삭제]
        //UI_Active = !UI_Active;

        //if (UI_Active)
        //    Guideline.SetActive(true);
        //else if (UI_Active == false) Guideline.SetActive(false);
        return UI_Active;
    }

    public bool Ball_Active_ONOFF()
    {
        //[삭제]
        //BallActive = !BallActive;

        //Logger.Log("Ball Image Active");
        return BallActive;
    }

    public bool SF_Active_ONOFF()
    {
        SF_Active = !SF_Active;

        return SF_Active;
    }

    public void Get_xmlsetting()
    {
        //xml 데이터 전부 로드 및 저장
        Screen_Scale = XmlManager.Instance.ScreenSize;
        _screenRatio = XmlManager.Instance.ScreenRatio;

        RectTransform rectTransform = this.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(XmlManager.Instance.SensorPosX, XmlManager.Instance.SensorPosY);

        thresholdDistance = XmlManager.Instance.ClusterThreshold;
        adjustYHorizontal = XmlManager.Instance.Yhorizontal;
        adjustYVertical = XmlManager.Instance.Yvertical;
        adjustXDiagonal = XmlManager.Instance.Xdiagonal;
        adjustYDiagonal = XmlManager.Instance.Ydiagonal;
        adjustXDiagonalLeft = XmlManager.Instance.XdiagonalLeft;
        //touchZoneLifetime = XmlManager.Instance.TouchzoneLifetime;
        maxTouchZones = XmlManager.Instance.MaxTouchzones;
        Touch_range = XmlManager.Instance.TouchRange;

        //이전에 센서 캘리브레이션 기능 테스트 후 실행시 에러 방지, 가이드라인 원상복구
        isFeatureActive = true;
        Guideline.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);


        //[구현필요] 각 콘텐츠 마다 UI캠 어디있는지 확인 공통으로 묶어서 일괄적으로 적용될 수 있도록 조절이 필요함
        ////UI캠을 찾고
        //float screenOffsetX = XmlManager.Instance.ScreenPositionOffsetX;
        //float screenOffsetY = XmlManager.Instance.ScreenPositionOffsetY;

        ////초기에 카메라 조정
        //if (mainCamera != null)
        //{
        //    mainCamera.rect = new Rect(
        //        0.5f - Screen_Scale / 2f + (screenOffsetX - 0.5f),
        //        0.5f - Screen_Scale / 2f + (screenOffsetY - 0.5f),
        //        Screen_Scale,
        //        Screen_Scale
        //    );
        //}

    }
}