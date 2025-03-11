using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

public class ScreenAdjuster : MonoBehaviour
{
    public Camera mainCamera;
    public Slider screenSizeSlider;
    public Slider offsetXSlider;
    public Slider offsetYSlider;
    public Image guideLine; // 가이드라인 이미지 (사용자가 직접 연결)
    public Button saveButton;
    public Button resetButton;

    private string xmlPath;
    private float screenSize = 1.0f;
    private float offsetX = 0.5f; // 기본값을 0.5로 설정하여 중앙 시작
    private float offsetY = 0.5f; // 기본값을 0.5로 설정하여 중앙 시작
    private float SensoroffsetX ; // 기본값을 0.5로 설정하여 중앙 시작
    private float SensoroffsetY ; // 기본값을 0.5로 설정하여 중앙 시작
    private readonly Vector2 defaultGuideSize = new Vector2(1920, 1080); // 기본 해상도 설정


    public InputField screenSizeInput;
    public InputField offsetXInput;
    public InputField offsetYInput;

    public Text xmlScreenSizeText;
    public Text xmlOffsetXText;
    public Text xmlOffsetYText;

    public Text Now_xmlScreenSizeText;
    public Text Now_xmlOffsetXText;
    public Text Now_xmlOffsetYText;

    void Start()
    {
        xmlPath = Path.Combine(Application.streamingAssetsPath, "GameSettingData.xml");
        LoadSettings();
        ApplySettings();

        // 슬라이더 값 변경 시 이벤트 연결
        screenSizeSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetXSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetYSlider.onValueChanged.AddListener(delegate { UpdateUI(); });

        // 슬라이더 값 변경 시 입력 필드 업데이트
        screenSizeSlider.onValueChanged.AddListener(value => UpdateInputField(screenSizeInput, value));
        offsetXSlider.onValueChanged.AddListener(value => UpdateInputField(offsetXInput, value));
        offsetYSlider.onValueChanged.AddListener(value => UpdateInputField(offsetYInput, value));

        // 입력 필드 값 변경 시 슬라이더 업데이트
        screenSizeInput.onEndEdit.AddListener(value => UpdateSlider(screenSizeSlider, value));
        offsetXInput.onEndEdit.AddListener(value => UpdateSlider(offsetXSlider, value));
        offsetYInput.onEndEdit.AddListener(value => UpdateSlider(offsetYSlider, value));

        saveButton.onClick.AddListener(SaveSettings);
        resetButton.onClick.AddListener(ResetToDefault);
    }

    void LoadSettings()
    {
        float screenSize, offsetX, offsetY, sensorOffsetX, sensorOffsetY;
        XmlManager.LoadSettings(out screenSize, out offsetX, out offsetY, out sensorOffsetX, out sensorOffsetY);

        screenSizeSlider.value = screenSize;
        offsetXSlider.value = offsetX;
        offsetYSlider.value = offsetY;
        SensoroffsetX = sensorOffsetX;
        SensoroffsetY = sensorOffsetY;
    }

    void SaveSettings()
    {
        XmlManager.SaveSettings(screenSizeSlider.value, offsetXSlider.value, offsetYSlider.value, SensoroffsetX, SensoroffsetY);

        //해당 텍스트 데이터를 확인?
        Now_xmlScreenSizeText.text = $"{screenSize}";
        Now_xmlOffsetXText.text = $"{offsetX}";
        Now_xmlOffsetYText.text = $"{offsetY}";
    }

    void ApplySettings()
    {
        screenSizeSlider.value = screenSize;
        offsetXSlider.value = offsetX;
        offsetYSlider.value = offsetY;
        UpdateUI();
        ApplyScreenSettings(); // XML 값으로 화면 설정 적용
    }

    void UpdateUI()
    {
        screenSize = screenSizeSlider.value;
        offsetX = offsetXSlider.value;
        offsetY = offsetYSlider.value;

        // 가이드라인 크기 및 위치 업데이트
        if (guideLine != null)
        {
            RectTransform guideTransform = guideLine.rectTransform;
            guideTransform.sizeDelta = defaultGuideSize * screenSize; // 기본 해상도 기준 크기 조정

            // 중앙을 0.5로 맞추고 이동 범위를 -1 ~ 1로 조정
            float adjustedX = (offsetX - 0.5f) * 1920;
            float adjustedY = (offsetY - 0.5f) * 1080;

            guideTransform.anchoredPosition = new Vector2(adjustedX, adjustedY);
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

    void ApplyScreenSettings()
    { 
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(
                0.5f - screenSize / 2f + (offsetX - 0.5f),
                0.5f - screenSize / 2f + (offsetY - 0.5f),
                screenSize,
                screenSize
            );
        }

        // 슬라이더 & 입력 필드에 반영
        screenSizeSlider.value = screenSize;
        offsetXSlider.value = offsetX;
        offsetYSlider.value = offsetY;

        screenSizeInput.text = screenSize.ToString("0.00");
        offsetXInput.text = offsetX.ToString("0.00");
        offsetYInput.text = offsetY.ToString("0.00");

        UpdateXMLText();

    }
    void UpdateXMLText()
    {
        xmlScreenSizeText.text = $"Screen Size: {screenSize}";
        xmlOffsetXText.text = $"Offset X: {offsetX}";
        xmlOffsetYText.text = $"Offset Y: {offsetY}";
    }

    void ResetToDefault()
    {
        screenSize = 1.0f;
        offsetX = 0.5f;
        offsetY = 0.5f;
        ApplySettings();
        SaveSettings();
    }
}

