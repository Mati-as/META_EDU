using System.Collections;
using System.Collections.Generic;
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
    public Slider offsetXSlider;
    public Slider offsetYSlider;

    public InputField offsetXInput;
    public InputField offsetYInput;

    public Text xmlOffsetXText;
    public Text xmlOffsetYText;

    public Text Now_xmlOffsetXText;
    public Text Now_xmlOffsetYText;

    public Text ThresholdText;

    public InputField thresholdInputField;
    public InputField adjustYHorizontalInput;
    public InputField adjustYVerticalInput;
    public InputField adjustXDiagonalInput;
    public InputField adjustYDiagonalInput;
    public InputField adjustXDiagonalLeftInput;

    public Button toggleFeatureButton;  //���� ��� onoff ��ư


    private float thresholdDistance = 70f;      //�׷�ȭ�� ���� threshold
    private float adjustYHorizontal = -50f;
    private float adjustYVertical = -25f;
    private float adjustXDiagonal = 25f;
    private float adjustYDiagonal = -25f;
    private float adjustXDiagonalLeft = -25f;

    private float screenSize;
    private float offsetX;
    private float offsetY;
    private float SensoroffsetX = 0.5f; // �⺻��
    private float SensoroffsetY = 0.5f; // �⺻��

    private const float CANVAS_Y_CENTER = 540.0f; // ĵ���� ���� Y ��ġ
    private const float CANVAS_X_CENTER = 0.0f;   // ĵ���� ���� X ��ġ


    private bool isFeatureActive = true; //��� Ȱ��ȭ ����


    void Start()
    {
        //Sensor position
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


        //Sensor cluster threshold
        if (thresholdInputField != null)
        {
            thresholdInputField.onEndEdit.AddListener(UpdateThreshold);
            thresholdInputField.text = thresholdDistance.ToString();
        }

        //Sensor touch calibration value
        //(Ȯ��) 
        if (adjustYHorizontalInput != null)
        {
            adjustYHorizontalInput.onEndEdit.AddListener(value => { adjustYHorizontal = float.Parse(value); UpdateCalibrationValue(); });
            adjustYHorizontalInput.text = adjustYHorizontal.ToString();
        }
        if (adjustYVerticalInput != null)
        {
            adjustYVerticalInput.onEndEdit.AddListener(value => { adjustYVertical = float.Parse(value); UpdateCalibrationValue(); });
            adjustYVerticalInput.text = adjustYVertical.ToString();
        }
        if (adjustXDiagonalInput != null)
        {
            adjustXDiagonalInput.onEndEdit.AddListener(value => { adjustXDiagonal = float.Parse(value); UpdateCalibrationValue(); });
            adjustXDiagonalInput.text = adjustXDiagonal.ToString();
        }
        if (adjustYDiagonalInput != null)
        {
            adjustYDiagonalInput.onEndEdit.AddListener(value => { adjustYDiagonal = float.Parse(value); UpdateCalibrationValue(); });
            adjustYDiagonalInput.text = adjustYDiagonal.ToString();
        }
        if (adjustXDiagonalLeftInput != null)
        {
            adjustXDiagonalLeftInput.onEndEdit.AddListener(value => { adjustXDiagonalLeft = float.Parse(value); UpdateCalibrationValue(); });
            adjustXDiagonalLeftInput.text = adjustXDiagonalLeft.ToString();
        }

        if (toggleFeatureButton != null)
        {
            toggleFeatureButton.onClick.AddListener(ToggleFeature);
        }

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

    //Sensor cluster threshold

    private void UpdateThreshold(string input)
    {
        if (float.TryParse(input, out float newThreshold))
        {
            thresholdDistance = newThreshold;
            ThresholdText.text = thresholdDistance.ToString("0.00");

            Debug.Log($"Threshold ���� {thresholdDistance}�� �����");
            //���⿡�� �׳� ���� ���� �ٲ��ִ°� ���� �� ������
            manager.thresholdDistance = thresholdDistance;
        }
    }

    private void UpdateCalibrationValue()
    {
        //�����Ͱ� �� �ٲ�� ����?
        manager.adjustYHorizontal = adjustYHorizontal;
        manager.adjustYVertical = adjustYVertical;
        manager.adjustXDiagonal = adjustXDiagonal;
        manager.adjustYDiagonal = adjustYDiagonal;
        manager.adjustXDiagonalLeft = adjustXDiagonalLeft;
    }

    //Assume touch direction
    private void ToggleFeature()
    {
        isFeatureActive = !isFeatureActive;
        Debug.Log($"�� ���� ���� ���: {(isFeatureActive ? "ON" : "OFF")}");
        //���⿡�� �׳� ���� ���� �ٲ��ִ°� ���� �� ������
        manager.isFeatureActive = isFeatureActive;
    }
}
