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
    private float SensoroffsetX = 0.5f; // �⺻��
    private float SensoroffsetY = 0.5f; // �⺻��

    private const float CANVAS_Y_CENTER = 540.0f; // ĵ���� ���� Y ��ġ
    private const float CANVAS_X_CENTER = 0.0f;   // ĵ���� ���� X ��ġ

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

            // ĵ���� �߽� (0,540) �������� ��ǥ ��ȯ
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

    //XML���� ���� ������ �� �ε�
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

    //���� ������ ���� ���� (ȭ�� ũ�� ���� �������� ����)
    void SaveSensorSettings()
    {
        XmlManager.SaveSettings(screenSize, offsetX, offsetY, offsetXSlider.value, offsetYSlider.value);

        xmlOffsetXText.text = offsetXSlider.value.ToString("0.00");
        xmlOffsetYText.text = offsetYSlider.value.ToString("0.00");


        // UI�� ���� �� ǥ��
        Now_xmlOffsetXText.text = SensoroffsetX.ToString("0.00");
        Now_xmlOffsetYText.text = SensoroffsetY.ToString("0.00");
    }

    //���� �ʱ�ȭ ��ư (Reset ��ư) ��� ����
    void ResetToDefault()
    {
        SensoroffsetX = 0.5f;
        SensoroffsetY = 0.5f;

        //�����̴��� �Է� �ʵ� ���� ���� ������Ʈ
        offsetXSlider.value = SensoroffsetX;
        offsetYSlider.value = SensoroffsetY;

        UpdateUI();  // UI ����
        SaveSensorSettings();  // XML�� ����
    }
}
