using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SensorAdjuster : MonoBehaviour
{
    public SensorManager manager;

    public Button Button_Sensor_Init;
    public Button Button_Sensor_Stop;
    public Button saveButton;
    public Button resetButton;
    public Image Lider_object;

    [Header("[ Sensor position ]")]
    public Slider offsetXSlider;
    public Slider offsetYSlider;
    public Slider offsetAngleSlider;

    public InputField offsetXInput;
    public InputField offsetYInput;
    public InputField offsetAngleInput;
    public Text xmlOffsetXText;
    public Text xmlOffsetYText;
    public Text xmlOffsetAngleText;
    public Text Now_xmlOffsetXText;
    public Text Now_xmlOffsetYText;
    public Text Now_xmlOffsetAngleText;

    public InputField GuideLine_WInputField;
    public InputField GuideLine_HInputField;

    [Header("[ Sensor adjust ]")]
    public Text ThresholdText;

    public InputField thresholdInputField;
    public InputField adjustYHorizontalInput;
    public InputField adjustYVerticalInput;
    public InputField adjustXDiagonalInput;
    public InputField adjustYDiagonalInput;
    public InputField adjustXDiagonalLeftInput;

    public InputField adjustTLifetimeInput;
    public InputField adjustMaxTInput;
    public InputField adjustTRangeInput;

    public Button Tsetting_saveButton;
    public Button Tsetting_resetButton;

    public Button toggleFeatureButton;

    [Header("[ Sensor calibration ]")]
    public Button Button_Guide_center;
    public Button Button_Guide_vertex;
    public Button Button_Guide_Screenratio;
    public Button Button_Calib_pos;
    public Button Button_Calib_adjust;

    public Button Button_Save_Calib_Touchpoint;
    public Button Button_Apply_Calib_Touchpoint;
    public Button Button_Cancel_Calib_Touchpoint;
    public Button Button_Calib_Screenratio_Left;
    public Button Button_Save_Screenratio;

    //public GameObject Center_Point;
    //public GameObject Vertex_Point;
   // public GameObject SR_Guide_Points;

    public Slider screenratioSlider;
    public InputField ScreenratioInput;
    public Text ScreenratioText;

    //보정값 적용 중인지 아닌지 판단
    public Text Calibration_state;
    //public Text Calibration_state_indetail;

    private const float CANVAS_Y_CENTER = 540.0f;
    private const float CANVAS_X_CENTER = 0.0f;


    private bool isFeatureActive = false;

    private bool sensor_button_Clicked = false;

    void Start()
    {
        
        // 프리팹분리로 인한 동적로드 25/0425
        var RaySynchronizer = GameObject.FindWithTag("RaySynchronizer");
        manager =RaySynchronizer.GetComponent<SensorManager>();
        Lider_object =RaySynchronizer.GetComponent<Image>();
        
        
        if (thresholdInputField != null)
        {
            thresholdInputField.onEndEdit.AddListener(UpdateThreshold);
            thresholdInputField.text = XmlManager.Instance.ClusterThreshold.ToString();
        }

        if (thresholdInputField != null)
        {
            thresholdInputField.onValueChanged.AddListener(value => UpdateLiveSettings());
        }

        if (adjustYHorizontalInput != null)
        {
            adjustYHorizontalInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }
        if (adjustYVerticalInput != null)
        {
            adjustYVerticalInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }
        if (adjustXDiagonalInput != null)
        {
            adjustXDiagonalInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }
        if (adjustYDiagonalInput != null)
        {
            adjustYDiagonalInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }
        if (adjustXDiagonalLeftInput != null)
        {
            adjustXDiagonalLeftInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }

        if (adjustTLifetimeInput != null)
        {
            adjustTLifetimeInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }
        if (adjustMaxTInput != null)
        {
            adjustMaxTInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }
        if (adjustTRangeInput != null)
        {
            adjustTRangeInput.onValueChanged.AddListener(value => UpdateLiveSettings());
        }

        if (toggleFeatureButton != null)
        {
            toggleFeatureButton.onClick.AddListener(ToggleFeature);
        }
        if (Tsetting_saveButton != null)
        {
            Tsetting_saveButton.onClick.AddListener(Save_Touchsetting);
        }
        if (Tsetting_resetButton != null)
        {
            Tsetting_resetButton.onClick.AddListener(Reset_Touchsetting);
        }

        Button_Sensor_Init.onClick.AddListener(Init_sensor);
        Button_Sensor_Stop.onClick.AddListener(Stop_sensor);

        offsetXSlider.onValueChanged.AddListener(delegate { UpdateUI_Sensor(); });
        offsetYSlider.onValueChanged.AddListener(delegate { UpdateUI_Sensor(); });
        offsetAngleSlider.onValueChanged.AddListener(delegate { UpdateUI_Sensor(); });

        offsetXSlider.onValueChanged.AddListener(value => UpdateInputField(offsetXInput, value));
        offsetYSlider.onValueChanged.AddListener(value => UpdateInputField(offsetYInput, value));
        offsetAngleSlider.onValueChanged.AddListener(value => UpdateInputField(offsetAngleInput, value));

        offsetXInput.onEndEdit.AddListener(value => UpdateSlider(offsetXSlider, value));
        offsetYInput.onEndEdit.AddListener(value => UpdateSlider(offsetYSlider, value));
        offsetAngleInput.onEndEdit.AddListener(value => UpdateSlider(offsetAngleSlider, value));

        // 현재 사이즈를 InputField에 표시
        GuideLine_WInputField.text = manager.Guideline.GetComponent<RectTransform>().sizeDelta.x.ToString("F0");
        GuideLine_HInputField.text = manager.Guideline.GetComponent<RectTransform>().sizeDelta.y.ToString("F0");

        GuideLine_WInputField.onEndEdit.AddListener(OnWidthChanged);
        GuideLine_HInputField.onEndEdit.AddListener(OnHeightChanged);

        saveButton.onClick.AddListener(SaveSensorSettings);
        resetButton.onClick.AddListener(ResetToDefault);

        Button_Guide_center.onClick.AddListener(Show_Guidecenter);
        Button_Guide_vertex.onClick.AddListener(Show_Guidevertex);
        Button_Guide_Screenratio.onClick.AddListener(Show_SRGuidepoints);

        Button_Calib_pos.onClick.AddListener(Calibration_sensor_position);
        //Button_Save_Calib_Touchpoint.onClick.AddListener(Calibration_sensor);
        //Button_Apply_Calib_Touchpoint.onClick.AddListener(Apply_calibration);
        //Button_Cancel_Calib_Touchpoint.onClick.AddListener(Cancel_calibration);
        Button_Calib_Screenratio_Left.onClick.AddListener(Calibration_screenratio);
        Button_Save_Screenratio.onClick.AddListener(Save_screenratio);

        //screen ratio 부분
        screenratioSlider.onValueChanged.AddListener(value => UpdateSRInputField(ScreenratioInput, value));
        ScreenratioInput.onEndEdit.AddListener(value => UpdateSRSlider(screenratioSlider, value));


        InitTouchSettingsInputs();
        LoadSensorSettings();

      
    }
    void Init_sensor()
    {
        if (!sensor_button_Clicked)
        {
            manager.AsyncInitSensor();
            manager.ResetTouchZones();
            sensor_button_Clicked = true;
            //나중에 화면상에 메시지도 출력되도록 구현 필요
        }
        else
            Debug.Log("센서 정지 상태 아님");
        //if (manager.isCalibrationApplied)
        //    Calibration_state.text = "보정 값 적용 중";

    }

    void Stop_sensor()
    {
        if (sensor_button_Clicked)
        {
            manager.StopSensor();
            sensor_button_Clicked = false;
        }
        else
            Debug.Log("센서 스캔 상태 아님");

    }

    void Save_Touchsetting()
    {
        XmlManager.Instance.ClusterThreshold = int.Parse(thresholdInputField.text);
        XmlManager.Instance.Yhorizontal = float.Parse(adjustYHorizontalInput.text);
        XmlManager.Instance.Yvertical = float.Parse(adjustYVerticalInput.text);
        XmlManager.Instance.Xdiagonal = float.Parse(adjustXDiagonalInput.text);
        XmlManager.Instance.Ydiagonal = float.Parse(adjustYDiagonalInput.text);
        XmlManager.Instance.XdiagonalLeft = float.Parse(adjustXDiagonalLeftInput.text);
        XmlManager.Instance.TouchzoneLifetime = float.Parse(adjustTLifetimeInput.text);
        XmlManager.Instance.MaxTouchzones = int.Parse(adjustMaxTInput.text);
        XmlManager.Instance.TouchRange = float.Parse(adjustTRangeInput.text);

        XmlManager.Instance.SaveSettings(); // XML 저장

    }

    void Reset_Touchsetting()
    {
        XmlManager.Instance.ClusterThreshold = 70;
        XmlManager.Instance.Yhorizontal = -50f;
        XmlManager.Instance.Yvertical = -25f;
        XmlManager.Instance.Xdiagonal = 25f;
        XmlManager.Instance.Ydiagonal = -25f;
        XmlManager.Instance.XdiagonalLeft = -25f;
        XmlManager.Instance.TouchzoneLifetime = 1.0f;
        XmlManager.Instance.MaxTouchzones = 20;
        XmlManager.Instance.TouchRange = 35f;

        InitTouchSettingsInputs();
        XmlManager.Instance.SaveSettings(); // XML 저장
    }

    void Show_Guidecenter()
    {
      //  Center_Point.SetActive(!Center_Point.activeSelf);
        manager.Guideline.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        manager.Guideline.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 1080);
        manager.Set_GuideLine();
    }
    void Show_Guidevertex()
    {
     //   Vertex_Point.SetActive(!Vertex_Point.activeSelf);
    }
    void Show_SRGuidepoints()
    {
      //  SR_Guide_Points.SetActive(!SR_Guide_Points.activeSelf);
        
        manager.Guideline.GetComponent<RectTransform>().anchoredPosition = new Vector2(-500, 0);
        manager.Guideline.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 800);
        manager.Set_GuideLine();
    }
    void Calibration_sensor_position()
    {
        //매니저의 해당 함수 호출
        manager.isCalibrationActive_SensorPos = true;
    }
    //void Calibration_sensor()
    //{
    //    //매니저의 해당 함수 호출
    //    //안전장치 구현 필요
    //    manager.isCalibrationActive = true;
    //}
    void Calibration_screenratio()
    {
        //SR 계산
        manager.isCalibration_SR_Active = true;
    }
    void Save_screenratio()
    {
        XmlManager.Instance.ScreenRatio = manager._screenRatio;
        XmlManager.Instance.SaveSettings(); //  XML 저장
    }
    //void Apply_calibration()
    //{
    //    manager.ApplyCalibration();
    //}
    //void Cancel_calibration()
    //{
    //    manager.isCalibrationApplied = false;
    //}


    void UpdateUI_Sensor()
    {
        float sensorOffsetX = offsetXSlider.value;
        float sensorOffsetY = offsetYSlider.value;
        float sensorOffsetAngle = offsetAngleSlider.value;

        if (Lider_object != null)
        {
            RectTransform lidarTransform = Lider_object.rectTransform;
            //offset 포지션 반영
            lidarTransform.anchoredPosition = new Vector2(sensorOffsetX, sensorOffsetY);

            //각도 반영
            lidarTransform.localRotation = Quaternion.Euler(0f, 0f, sensorOffsetAngle);
        }
    }
    void OnWidthChanged(string widthStr)
    {
        RectTransform guideRect = manager.Guideline.GetComponent<RectTransform>();

        if (float.TryParse(widthStr, out float width))
        {
            Vector2 currentSize = guideRect.sizeDelta;
            guideRect.sizeDelta = new Vector2(width, currentSize.y);
            manager.Set_GuideLine();
        }
    }

    void OnHeightChanged(string heightStr)
    {
        RectTransform guideRect = manager.Guideline.GetComponent<RectTransform>();

        if (float.TryParse(heightStr, out float height))
        {
            Vector2 currentSize = guideRect.sizeDelta;
            guideRect.sizeDelta = new Vector2(currentSize.x, height);
            manager.Set_GuideLine();
        }
    }
    void UpdateLiveSettings()
    {
        // SensorAdjuster 내부 변수 업데이트
        float.TryParse(thresholdInputField.text, out float thresholdDistance);
        float.TryParse(adjustYHorizontalInput.text, out float adjustYHorizontal);
        float.TryParse(adjustYVerticalInput.text, out float adjustYVertical);
        float.TryParse(adjustXDiagonalInput.text, out float adjustXDiagonal);
        float.TryParse(adjustYDiagonalInput.text, out float adjustYDiagonal);
        float.TryParse(adjustXDiagonalLeftInput.text, out float adjustXDiagonalLeft);
        float.TryParse(adjustTLifetimeInput.text, out float touchLifetime);
        int.TryParse(adjustMaxTInput.text, out int maxTouches);
        float.TryParse(adjustTRangeInput.text, out float touchRange);

        // SensorManager 즉시 반영
        if (manager != null)
        {
            manager.thresholdDistance = thresholdDistance;
            manager.adjustYHorizontal = adjustYHorizontal;
            manager.adjustYVertical = adjustYVertical;
            manager.adjustXDiagonal = adjustXDiagonal;
            manager.adjustYDiagonal = adjustYDiagonal;
            manager.adjustXDiagonalLeft = adjustXDiagonalLeft;
            //manager.touchZoneLifetime = touchLifetime;
            manager.maxTouchZones = maxTouches;
            manager.Touch_range = touchRange;
        }


    }
    void UpdateInputField(InputField input, float value)
    {
        input.text = value.ToString("0.00");
        //value값이 없음?
    }

    void UpdateSlider(Slider slider, string value)
    {
        if (float.TryParse(value, out float result))
        {
            slider.value = result;
            //여기도 전부 그게 없음?
        }
    }

    void UpdateSRInputField(InputField input, float value)
    {
        input.text = value.ToString("0.00");
        manager._screenRatio = value;
    }

    void UpdateSRSlider(Slider slider, string value)
    {
        if (float.TryParse(value, out float result))
        {
            slider.value = result;
            manager._screenRatio = result;
            ScreenratioText.text = slider.value.ToString("0.00");
        }
    }


    /// <summary>
    ///  XML에서 센서 설정 불러오기
    /// </summary>
    void LoadSensorSettings()
    {
        offsetXSlider.value = XmlManager.Instance.SensorPosX;
        offsetYSlider.value = XmlManager.Instance.SensorPosY;

        float Sensor_angle =  XmlManager.Instance.SensorAngle;
        if (Sensor_angle > 180f) Sensor_angle -= 360f;
        offsetAngleSlider.value = Sensor_angle;

        xmlOffsetXText.text = XmlManager.Instance.SensorPosX.ToString("0");
        xmlOffsetYText.text = XmlManager.Instance.SensorPosY.ToString("0");
        xmlOffsetAngleText.text = XmlManager.Instance.SensorAngle.ToString("0.00");

        Now_xmlOffsetXText.text = XmlManager.Instance.SensorPosX.ToString("0");
        Now_xmlOffsetYText.text = XmlManager.Instance.SensorPosY.ToString("0");
        xmlOffsetAngleText.text = XmlManager.Instance.SensorAngle.ToString("0.0");

        offsetXInput.text = offsetXSlider.value.ToString("0");
        offsetYInput.text = offsetYSlider.value.ToString("0");
        offsetAngleInput.text = offsetAngleSlider.value.ToString("0.00");

        screenratioSlider.value = manager._screenRatio;
        ScreenratioText.text = screenratioSlider.value.ToString("0.00");

        Lider_object.rectTransform.anchoredPosition= new Vector2(XmlManager.Instance.SensorPosX, XmlManager.Instance.SensorPosY);
        Lider_object.rectTransform.localRotation = Quaternion.Euler(0f, 0f, XmlManager.Instance.SensorAngle);
    }

    /// <summary>
    ///  XML에 센서 설정 저장
    /// </summary>
    void SaveSensorSettings()
    {
        //[수정]굳이 슬라이더의 값은 저장, 로드하지 않음
        //XmlManager.Instance.SensorOffsetX = offsetXSlider.value;
        //XmlManager.Instance.SensorOffsetY = offsetYSlider.value;

        //현재 lidar object의 위치로 저장
        XmlManager.Instance.SensorPosX = Lider_object.rectTransform.anchoredPosition.x;
        XmlManager.Instance.SensorPosY = Lider_object.rectTransform.anchoredPosition.y;
        XmlManager.Instance.SensorAngle = Lider_object.rectTransform.localRotation.eulerAngles.z;


        xmlOffsetXText.text = offsetXSlider.value.ToString("0.00");
        xmlOffsetYText.text = offsetYSlider.value.ToString("0.00");

        Now_xmlOffsetXText.text = XmlManager.Instance.SensorPosX.ToString("0.0");
        Now_xmlOffsetYText.text = XmlManager.Instance.SensorPosY.ToString("0.0");

        XmlManager.Instance.SaveSettings(); //  XML 저장
    }

    /// <summary>
    ///  센서 오프셋 기본값으로 초기화 후 XML 저장
    /// </summary>
    void ResetToDefault()
    {
        //XmlManager.Instance.SensorOffsetX = 0.5f;
        //XmlManager.Instance.SensorOffsetY = 0.5f;

        XmlManager.Instance.SensorPosX = 0;
        XmlManager.Instance.SensorPosY = 540;
        XmlManager.Instance.SensorAngle = 0f;

        offsetXSlider.value = 0;
        offsetYSlider.value = 540;
        offsetAngleSlider.value = 0;

        UpdateUI_Sensor();
        SaveSensorSettings();
    }

    private void UpdateThreshold(string input)
    {
        if (float.TryParse(input, out float newThreshold))
        {
            XmlManager.Instance.ClusterThreshold = (int)newThreshold;
            ThresholdText.text = XmlManager.Instance.ClusterThreshold.ToString("0.00");
            manager.thresholdDistance = XmlManager.Instance.ClusterThreshold;
        }
    }
    void InitTouchSettingsInputs()
    {
        if (thresholdInputField != null)
        {
            thresholdInputField.text = XmlManager.Instance.ClusterThreshold.ToString();
        }
        if (adjustYHorizontalInput != null)
        {
            adjustYHorizontalInput.text = XmlManager.Instance.Yhorizontal.ToString();
        }
        if (adjustYVerticalInput != null)
        {
            adjustYVerticalInput.text = XmlManager.Instance.Yvertical.ToString();
        }
        if (adjustXDiagonalInput != null)
        {
            adjustXDiagonalInput.text = XmlManager.Instance.Xdiagonal.ToString();
        }
        if (adjustYDiagonalInput != null)
        {
            adjustYDiagonalInput.text = XmlManager.Instance.Ydiagonal.ToString();
        }
        if (adjustXDiagonalLeftInput != null)
        {
            adjustXDiagonalLeftInput.text = XmlManager.Instance.XdiagonalLeft.ToString();
        }
        if (adjustTLifetimeInput != null)
        {
            adjustTLifetimeInput.text = XmlManager.Instance.TouchzoneLifetime.ToString();
        }
        if (adjustMaxTInput != null)
        {
            adjustMaxTInput.text = XmlManager.Instance.MaxTouchzones.ToString();
        }
        if (adjustTRangeInput != null)
        {
            adjustTRangeInput.text = XmlManager.Instance.TouchRange.ToString();
        }
    }
    private void ToggleFeature()
    {
        isFeatureActive = !isFeatureActive;
        toggleFeatureButton.transform.GetChild(1).gameObject.GetComponent<Text>().text = isFeatureActive ? "터치 이벤트 확인" : "실제 터치 좌표 확인";
        manager.isFeatureActive = isFeatureActive;
    }

    private void OnDestroy()
    {
        //[수정] 센서 중지 직접 호출하지 않고 스캔 상태일때만 실행
        //manager.StopSensor();
        Logger.SensorRelatedLog("센서 모터 종료 -----------------");
        Stop_sensor();
    }
    public void Button_close()
    {
        //가이드 표출 전부 비활성화
        // Center_Point.SetActive(false);
        // Vertex_Point.SetActive(false);
        // SR_Guide_Points.SetActive(false);

        manager.Guideline.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        manager.Guideline.GetComponent<RectTransform>().sizeDelta = new Vector2(1800, 1000);

        Stop_sensor();
    }
}
