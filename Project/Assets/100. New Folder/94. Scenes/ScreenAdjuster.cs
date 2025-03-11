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
    public Image guideLine; // ���̵���� �̹��� (����ڰ� ���� ����)
    public Button saveButton;
    public Button resetButton;

    private string xmlPath;
    private float screenSize = 1.0f;
    private float offsetX = 0.5f; // �⺻���� 0.5�� �����Ͽ� �߾� ����
    private float offsetY = 0.5f; // �⺻���� 0.5�� �����Ͽ� �߾� ����
    private float SensoroffsetX ; // �⺻���� 0.5�� �����Ͽ� �߾� ����
    private float SensoroffsetY ; // �⺻���� 0.5�� �����Ͽ� �߾� ����
    private readonly Vector2 defaultGuideSize = new Vector2(1920, 1080); // �⺻ �ػ� ����


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

        // �����̴� �� ���� �� �̺�Ʈ ����
        screenSizeSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetXSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetYSlider.onValueChanged.AddListener(delegate { UpdateUI(); });

        // �����̴� �� ���� �� �Է� �ʵ� ������Ʈ
        screenSizeSlider.onValueChanged.AddListener(value => UpdateInputField(screenSizeInput, value));
        offsetXSlider.onValueChanged.AddListener(value => UpdateInputField(offsetXInput, value));
        offsetYSlider.onValueChanged.AddListener(value => UpdateInputField(offsetYInput, value));

        // �Է� �ʵ� �� ���� �� �����̴� ������Ʈ
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

        //�ش� �ؽ�Ʈ �����͸� Ȯ��?
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
        ApplyScreenSettings(); // XML ������ ȭ�� ���� ����
    }

    void UpdateUI()
    {
        screenSize = screenSizeSlider.value;
        offsetX = offsetXSlider.value;
        offsetY = offsetYSlider.value;

        // ���̵���� ũ�� �� ��ġ ������Ʈ
        if (guideLine != null)
        {
            RectTransform guideTransform = guideLine.rectTransform;
            guideTransform.sizeDelta = defaultGuideSize * screenSize; // �⺻ �ػ� ���� ũ�� ����

            // �߾��� 0.5�� ���߰� �̵� ������ -1 ~ 1�� ����
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

        // �����̴� & �Է� �ʵ忡 �ݿ�
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

