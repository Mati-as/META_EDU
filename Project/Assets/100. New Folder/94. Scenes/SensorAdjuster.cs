using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensorAdjuster : MonoBehaviour
{
    public Button Button_Sensor_Init;
    public Button Button_Sensor_Stop;
    public Button saveButton;
    public Button resetButton;

    public Image Lider_object;
    public Slider offsetXSlider;
    public Slider offsetYSlider;

    public InputField offsetXInput;
    public InputField offsetYInput;

    public Text xmlOffsetXText;
    public Text xmlOffsetYText;

    public Text Now_xmlOffsetXText;
    public Text Now_xmlOffsetYText;

    public SensorManager manager;

    private float screenSize;
    private float offsetX;
    private float offsetY;
    private float SensoroffsetX = 0.5f; // 기본값
    private float SensoroffsetY = 0.5f; // 기본값

    private const float CANVAS_Y_CENTER = 540.0f; // 캔버스 기준 Y 위치
    private const float CANVAS_X_CENTER = 0.0f;   // 캔버스 기준 X 위치

    void Start()
    {
        Button_Sensor_Init.onClick.AddListener(Init_sensor);
        Button_Sensor_Stop.onClick.AddListener(Stop_sensor);

        offsetXSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetYSlider.onValueChanged.AddListener(delegate { UpdateUI(); });

        offsetXSlider.onValueChanged.AddListener(value => UpdateInputField(offsetXInput, value));
        offsetYSlider.onValueChanged.AddListener(value => UpdateInputField(offsetYInput, value));

        offsetXInput.onEndEdit.AddListener(value => UpdateSlider(offsetXSlider, value));
        offsetYInput.onEndEdit.AddListener(value => UpdateSlider(offsetYSlider, value));

        saveButton.onClick.AddListener(SaveSensorSettings);
        resetButton.onClick.AddListener(ResetToDefault);

        LoadSensorSettings();
    }

    void Init_sensor()
    {
        manager.InitSensor();
    }

    void Stop_sensor()
    {
        manager.StopSensor();
    }

    void UpdateUI()
    {
        SensoroffsetX = offsetXSlider.value;
        SensoroffsetY = offsetYSlider.value;

        if (Lider_object != null)
        {
            RectTransform lidarTransform = Lider_object.rectTransform;

            // 캔버스 중심 (0,540) 기준으로 좌표 변환
            float adjustedX = (SensoroffsetX - 0.5f) * 1920 + CANVAS_X_CENTER;
            float adjustedY = (SensoroffsetY - 0.5f) * 1080 + CANVAS_Y_CENTER;

            lidarTransform.anchoredPosition = new Vector2(adjustedX, adjustedY);
        }

    }

    void UpdateInputField(InputField input, float value)
    {
        input.text = value.ToString("0.00");
    }

    void UpdateSlider(Slider slider, string value)
    {
        if (float.TryParse(value, out float result))
        {
            slider.value = result;
        }
    }

    //XML에서 센서 오프셋 값 로드
    void LoadSensorSettings()
    {
        float screensize, offsetXOld, offsetYOld, sensorOffsetX, sensorOffsetY;
        XmlManager.LoadSettings(out screensize, out offsetXOld, out offsetYOld, out sensorOffsetX, out sensorOffsetY);

        screenSize = screensize;
        offsetX = offsetXOld;
        offsetY = offsetYOld;

        SensoroffsetX = sensorOffsetX;
        SensoroffsetY = sensorOffsetY;

        offsetXSlider.value = SensoroffsetX;
        offsetYSlider.value = SensoroffsetY;

        xmlOffsetXText.text = SensoroffsetX.ToString("0.00");
        xmlOffsetYText.text = SensoroffsetY.ToString("0.00");
    }

    //센서 오프셋 값만 저장 (화면 크기 값은 변경하지 않음)
    void SaveSensorSettings()
    {
        XmlManager.SaveSettings(screenSize, offsetX, offsetY, offsetXSlider.value, offsetYSlider.value);

        xmlOffsetXText.text = offsetXSlider.value.ToString("0.00");
        xmlOffsetYText.text = offsetYSlider.value.ToString("0.00");


        // UI에 현재 값 표시
        Now_xmlOffsetXText.text = SensoroffsetX.ToString("0.00");
        Now_xmlOffsetYText.text = SensoroffsetY.ToString("0.00");
    }

    //센서 초기화 버튼 (Reset 버튼) 기능 수정
    void ResetToDefault()
    {
        SensoroffsetX = 0.5f;
        SensoroffsetY = 0.5f;

        //슬라이더와 입력 필드 값도 같이 업데이트
        offsetXSlider.value = SensoroffsetX;
        offsetYSlider.value = SensoroffsetY;

        UpdateUI();  // UI 갱신
        SaveSensorSettings();  // XML에 저장
    }
}
