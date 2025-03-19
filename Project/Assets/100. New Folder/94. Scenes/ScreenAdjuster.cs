using UnityEngine;
using UnityEngine.UI;

public class ScreenAdjuster : MonoBehaviour
{
    public SensorManager manager;

    public Camera mainCamera;
    public Camera UICamera;

    public Slider screenSizeSlider;
    public Slider offsetXSlider;
    public Slider offsetYSlider;
    public Image guideLine;
    public Button saveButton;
    public Button resetButton;

    private readonly Vector2 defaultGuideSize = new Vector2(1920, 1080);

    public InputField screenSizeInput;
    public InputField offsetXInput;
    public InputField offsetYInput;

    public Text xmlScreenSizeText;
    public Text xmlOffsetXText;
    public Text xmlOffsetYText;
    public Text Now_xmlScreenSizeText;
    public Text Now_xmlOffsetXText;
    public Text Now_xmlOffsetYText;

    private float screenSize;
    private float screenOffsetX;
    private float screenOffsetY;


    void Awake()
    {
        //ApplySettings();
    }
    void Start()
    {
        LoadSettings();
        ApplySettings();

        screenSizeSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetXSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetYSlider.onValueChanged.AddListener(delegate { UpdateUI(); });

        screenSizeSlider.onValueChanged.AddListener(value => UpdateInputField(screenSizeInput, value));
        offsetXSlider.onValueChanged.AddListener(value => UpdateInputField(offsetXInput, value));
        offsetYSlider.onValueChanged.AddListener(value => UpdateInputField(offsetYInput, value));

        screenSizeInput.onEndEdit.AddListener(value => UpdateSlider(screenSizeSlider, value));
        offsetXInput.onEndEdit.AddListener(value => UpdateSlider(offsetXSlider, value));
        offsetYInput.onEndEdit.AddListener(value => UpdateSlider(offsetYSlider, value));

        saveButton.onClick.AddListener(SaveSettings);
        resetButton.onClick.AddListener(ResetToDefault);

        //if (mainCamera.targetTexture == null)
        //{
        //    RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        //    mainCamera.targetTexture = rt;
        //}
    }

    /// <summary>
    /// XML에서 데이터 로드
    /// </summary>
    void LoadSettings()
    {
        screenSize = XmlManager.Instance.ScreenSize;
        screenOffsetX = XmlManager.Instance.ScreenPositionOffsetX;
        screenOffsetY = XmlManager.Instance.ScreenPositionOffsetY;

        screenSizeSlider.value = screenSize;
        offsetXSlider.value = screenOffsetX;
        offsetYSlider.value = screenOffsetY;
    }

    /// <summary>
    /// XML에 설정 저장
    /// </summary>
    void SaveSettings()
    {
        XmlManager.Instance.ScreenSize = screenSizeSlider.value;
        XmlManager.Instance.ScreenPositionOffsetX = offsetXSlider.value;
        XmlManager.Instance.ScreenPositionOffsetY = offsetYSlider.value;

        XmlManager.Instance.SaveSettings(); // XML에 저장

        // 현재 저장된 XML 값을 UI에 반영
        Now_xmlScreenSizeText.text = $"{screenSize}";
        Now_xmlOffsetXText.text = $"{screenOffsetX}";
        Now_xmlOffsetYText.text = $"{screenOffsetY}";
    }

    /// <summary>
    /// XML 값을 UI에 적용
    /// </summary>
    public void ApplySettings()
    {
        Debug.Log("카메라 설정함");
        screenSizeSlider.value = screenSize;
        offsetXSlider.value = screenOffsetX;
        offsetYSlider.value = screenOffsetY;
        UpdateUI();
        ApplyScreenSettings();
    }
    //50000000 +
    /// <summary>
    /// UI 업데이트 (가이드라인 크기 및 위치 조정)
    /// </summary>
    void UpdateUI()
    {
        screenSize = screenSizeSlider.value;
        screenOffsetX = offsetXSlider.value;
        screenOffsetY = offsetYSlider.value;

        if (guideLine != null)
        {
            RectTransform guideTransform = guideLine.rectTransform;
            guideTransform.sizeDelta = defaultGuideSize * screenSize;

            float adjustedX = (screenOffsetX - 0.5f) * 1920;
            float adjustedY = (screenOffsetY - 0.5f) * 1080;

            guideTransform.anchoredPosition = new Vector2(adjustedX, adjustedY);
        }
    }

    /// <summary>
    /// 입력 필드 업데이트
    /// </summary>
    void UpdateInputField(InputField input, float value)
    {
        input.text = value.ToString("0.00");
    }

    /// <summary>
    /// 입력 필드 값 변경 시 슬라이더 업데이트
    /// </summary>
    void UpdateSlider(Slider slider, string value)
    {
        if (float.TryParse(value, out float result))
        {
            slider.value = result;
        }
    }

    /// <summary>
    /// 카메라 화면 설정 반영
    /// </summary>
    void ApplyScreenSettings()
    {
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(
                0.5f - screenSize / 2f + (screenOffsetX - 0.5f),
                0.5f - screenSize / 2f + (screenOffsetY - 0.5f),
                screenSize,
                screenSize
            );
        }

        //if (UICamera != null)
        //{
        //    UICamera.rect = new Rect(
        //        0.5f - screenSize / 2f + (screenOffsetX - 0.5f),
        //        0.5f - screenSize / 2f + (screenOffsetY - 0.5f),
        //        screenSize,
        //        screenSize
        //    );
        //}

        screenSizeSlider.value = screenSize;
        offsetXSlider.value = screenOffsetX;
        offsetYSlider.value = screenOffsetY;

        screenSizeInput.text = screenSize.ToString("0.00");
        offsetXInput.text = screenOffsetX.ToString("0.00");
        offsetYInput.text = screenOffsetY.ToString("0.00");

        UpdateXMLText();
    }

    /// <summary>
    /// XML UI 텍스트 업데이트
    /// </summary>
    void UpdateXMLText()
    {
        xmlScreenSizeText.text = $"Screen Size: {screenSize}";
        xmlOffsetXText.text = $"Offset X: {screenOffsetX}";
        xmlOffsetYText.text = $"Offset Y: {screenOffsetY}";
    }

    /// <summary>
    /// 기본값으로 리셋 후 XML 저장
    /// </summary>
    void ResetToDefault()
    {
        screenSize = 1.0f;
        screenOffsetX = 0.5f;
        screenOffsetY = 0.5f;
        ApplySettings();
        SaveSettings();

        manager.Set_Screenscale(1.0f);
    }
}
