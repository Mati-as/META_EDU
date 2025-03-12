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
    //0311 private -> public
    //0311 런처로 센서 기능 정상 테스트가 불가능하므로 수정
    public async void InitSensor()
    {
        //if (GameObject.FindWithTag("Launcher") == null)
        //    await InitSensorAsync();
        //else
        //    Logger.Log("게임 런쳐에서는 센서를 사용할 수 없습니다. 동작 시 태그 반드시 확인");

        await InitSensorAsync();

    }


    private async Task InitSensorAsync()
    {
        await Task.Delay(_refreshWaitTimeSpan);

        var result = await Task.Run(() => RplidarBinding.OnConnect(PORT));
        if (result < 0)
        {
            result = await Task.Run(() => RplidarBinding.OnConnect(PORT == "COM3" ? "COM4" : "COM3"));
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

        //0311 각각 가로,세로,대각선 별로 풀 준비함
        _SDPP_realpoint = new Stack<RectTransform>();
        SetPool(_SDPP_realpoint, "Rplidar/FP_REAL");

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
    private Stack<RectTransform> _SDPP_realpoint;


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
        //0311 센서 위치 보정 추가
        RT_Lidar_object = GetComponent<RectTransform>();

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

    //0311 센서 위치 보정 추가
    float Sensor_posx;
    float Sensor_posy;
    int _filteringAmount=2;
    private void GenerateDectectedPos()
    {
        //0311 센서 위치 보정 추가
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
                //6배
                if (_lidarDatas[i].theta > 90 && _lidarDatas[i].theta < 270) continue;


                sensored_X = Sensor_posx - _screenRatio *
                             (_lidarDatas[i].distant * Mathf.Cos((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad));
                sensored_Y = Sensor_posy - _screenRatio *
                             (_lidarDatas[i].distant * Mathf.Sin((90 - _lidarDatas[i].theta) * Mathf.Deg2Rad) -
                              UNITY_RECT_ZERO_COMMA_ZERO_POINT_OFFSET);


                if (i % _filteringAmount == 0)
                    if (min_x < sensored_X && sensored_X < max_x)
                        if (min_y < sensored_Y && sensored_Y < max_y)
                        {
                            //0311 추가
                            detectedPoints.Add(new Vector2(sensored_X, sensored_Y)); //  감지된 좌표 저장

                            if (SF_Active)
                            {
                                // _filteringAmount = 8;
                                //_filteringAmount = 4;
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                            else
                            {
                                //_filteringAmount = 3;
                                ShowFilteredSensorPos(sensored_X, sensored_Y);
                            }
                        }
#if UNITY_EDITOR
                // Debug.Log($"sensor: {sensored_X},{sensored_Y} , {_screenRatio}");
#endif
            }

            // 0311 감지된 좌표를 그룹화하여 물체 개수 판별
            if (isFeatureActive)
            {
                objectClusters = ClusterPoints(detectedPoints, thresholdDistance);

                Debug.Log($"감지된 물체 개수: {objectClusters.Count}");

                foreach (var cluster in objectClusters)
                {
                    string orientation = DetectFootOrientation(cluster);
                    Debug.Log($"발 방향 분석: {orientation}");

                    // ✅ 실터치 지점 계산 & 마커 생성 함수 자리 마련
                    Vector2 touchPoint = CalculateTouchPoint(cluster, orientation);
                    //발방향 분석한 부분 시각화 필요
                    CreateTouchMarker(touchPoint);
                }
            }

            m_datachanged = false;
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
    //#0311 정확도 개선 관련 부분
    //inputfield의 경우 오직 메인화면에서 센서 기능 개선 부분에서만 볼 수 있도록 할 것임
    public GameObject centerMarkerPrefab;
    private List<GameObject> centerMarkers = new List<GameObject>();

    private List<Vector2> detectedPoints = new List<Vector2>();
    private List<Vector2> centerPoints = new List<Vector2>();
    private List<List<Vector2>> objectClusters = new List<List<Vector2>>();

    public float thresholdDistance;      //그룹화를 위한 threshold
    public float adjustYHorizontal;
    public float adjustYVertical;
    public float adjustXDiagonal;
    public float adjustYDiagonal;
    public float adjustXDiagonalLeft;

    public bool isFeatureActive = true; //기능 활성화 여부

    //public float thresholdDistance = 70f;      //그룹화를 위한 threshold
    //public float adjustYHorizontal = -50f;
    //public float adjustYVertical = -25f;
    //public float adjustXDiagonal = 25f;
    //public float adjustYDiagonal = -25f;
    //public float adjustXDiagonalLeft = -25f;


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
        bool isLeftSide = center.x < Sensor_posx; // ✅ 센서 기준 왼쪽인지 확인

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
                return new Vector2(center.x + adjustXDiagonalLeft, center.y + adjustYDiagonal); // ✅ 왼쪽 보정
            else
                return new Vector2(center.x + adjustXDiagonal, center.y + adjustYDiagonal); // ✅ 오른쪽 보정
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

    //#0311



    //public void Instant_Ball(float temp_x, float temp_y)
    //{
    //    var Prefab_pos = Instantiate(BALLPrefab, UI_Canvas.transform.position, Quaternion.Euler(0, 0, 0),
    //        UI_Canvas.transform);
    //    Prefab_pos.GetComponent<RectTransform>().anchoredPosition = new Vector3(temp_x, temp_y, 0);
    //    Prefab_pos.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
    //}
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