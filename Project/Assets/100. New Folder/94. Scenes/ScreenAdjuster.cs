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
    private readonly Vector2 defaultGuideSize = new Vector2(1920, 1080); // �⺻ �ػ� ����

    void Start()
    {
        xmlPath = Path.Combine(Application.streamingAssetsPath, "GameSettingData.xml");
        LoadSettings();
        ApplySettings();

        // �����̴� �� ���� �� �̺�Ʈ ����
        screenSizeSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetXSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        offsetYSlider.onValueChanged.AddListener(delegate { UpdateUI(); });

        saveButton.onClick.AddListener(SaveSettings);
        resetButton.onClick.AddListener(ResetToDefault);
    }

    void LoadSettings()
    {
        if (File.Exists(xmlPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlNode node = xmlDoc.SelectSingleNode("/Settings/GameSettingData");

            if (node != null)
            {
                screenSize = float.Parse(node.Attributes["screenSize"].Value);
                offsetX = float.Parse(node.Attributes["offsetX"].Value);
                offsetY = float.Parse(node.Attributes["offsetY"].Value);
            }
        }
        else
        {
            screenSize = 1.0f;
            offsetX = 0.5f;
            offsetY = 0.5f;
            SaveSettings(); // �⺻������ XML ���� ����
        }
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
    }

    void SaveSettings()
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement root = xmlDoc.CreateElement("Settings");
        XmlElement gameSetting = xmlDoc.CreateElement("GameSettingData");

        gameSetting.SetAttribute("screenSize", screenSize.ToString("F2"));
        gameSetting.SetAttribute("offsetX", offsetX.ToString("F2"));
        gameSetting.SetAttribute("offsetY", offsetY.ToString("F2"));

        root.AppendChild(gameSetting);
        xmlDoc.AppendChild(root);
        xmlDoc.Save(xmlPath);
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
