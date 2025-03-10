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
    private readonly Vector2 defaultGuideSize = new Vector2(1920, 1080); // 기본 해상도 설정

    void Start()
    {
        xmlPath = Path.Combine(Application.streamingAssetsPath, "GameSettingData.xml");
        LoadSettings();
        ApplySettings();

        // 슬라이더 값 변경 시 이벤트 연결
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
            SaveSettings(); // 기본값으로 XML 파일 생성
        }
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
