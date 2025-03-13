using System.Xml;
using System.IO;
using UnityEngine;

public class XmlManager
{
    private static XmlManager instance;
    private static bool isLoaded = false;
    private static string filePath = Path.Combine(Application.streamingAssetsPath, "GameSettingData.xml");

    public static XmlManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new XmlManager();
            }
            return instance;
        }
    }

    // ���� ������
    public float ScreenSize { get;  set; }
    public float ScreenPositionOffsetX { get;  set; }
    public float ScreenPositionOffsetY { get;  set; }
    public float SensorOffsetX { get;  set; }
    public float SensorOffsetY { get;  set; }
    public int ClusterThreshold { get;  set; }
    public float Yhorizontal { get;  set; }
    public float Yvertical { get;  set; }
    public float Xdiagonal { get;  set; }
    public float Ydiagonal { get;  set; }
    public float XdiagonalLeft { get;  set; }
    public float TouchzoneLifetime { get;  set; }
    public int MaxTouchzones { get;  set; }
    public float TouchRange { get;  set; }

    private XmlManager()
    {
        LoadSettings(); // XML �����͸� �� ���� �ҷ��ͼ� ĳ��
    }

    /// <summary>
    /// XML ������ �ε�
    /// </summary>
    private void LoadSettings()
    {
        if (isLoaded) return;

        // �⺻�� ����
        ScreenSize = 1.0f;
        ScreenPositionOffsetX = 0.5f;
        ScreenPositionOffsetY = 0.5f;
        SensorOffsetX = 0.5f;
        SensorOffsetY = 0.5f;
        ClusterThreshold = 70;
        Yhorizontal = -50f;
        Yvertical = -25f;
        Xdiagonal = 25f;
        Ydiagonal = -25f;
        XdiagonalLeft = -25f;
        TouchzoneLifetime = 1.0f;
        MaxTouchzones = 20;
        TouchRange = 35f;

        if (!File.Exists(filePath))
            return;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);
        XmlElement root = xmlDoc.DocumentElement;
        XmlElement settingNode = root.SelectSingleNode("GameSettingData") as XmlElement;

        if (settingNode != null)
        {
            ScreenSize = float.Parse(settingNode.GetAttribute("ScreenSize"));
            ScreenPositionOffsetX = float.Parse(settingNode.GetAttribute("ScreenPositionOffsetX"));
            ScreenPositionOffsetY = float.Parse(settingNode.GetAttribute("ScreenPositionOffsetY"));
            SensorOffsetX = float.Parse(settingNode.GetAttribute("SensorOffsetX"));
            SensorOffsetY = float.Parse(settingNode.GetAttribute("SensorOffsetY"));
            ClusterThreshold = int.Parse(settingNode.GetAttribute("ClusterThreshold"));
            Yhorizontal = float.Parse(settingNode.GetAttribute("Yhorizontal"));
            Yvertical = float.Parse(settingNode.GetAttribute("Yvertical"));
            Xdiagonal = float.Parse(settingNode.GetAttribute("Xdiagonal"));
            Ydiagonal = float.Parse(settingNode.GetAttribute("Ydiagonal"));
            XdiagonalLeft = float.Parse(settingNode.GetAttribute("XdiagonalLeft"));
            TouchzoneLifetime = float.Parse(settingNode.GetAttribute("TouchzoneLifetime"));
            MaxTouchzones = int.Parse(settingNode.GetAttribute("MaxTouchzones"));
            TouchRange = float.Parse(settingNode.GetAttribute("TouchRange"));
        }

        isLoaded = true; // ������ �ε� �Ϸ�
    }

    /// <summary>
    /// XML�� ����
    /// </summary>
    public void SaveSettings()
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement root;

        if (File.Exists(filePath))
        {
            xmlDoc.Load(filePath);
            root = xmlDoc.DocumentElement;
        }
        else
        {
            root = xmlDoc.CreateElement("Settings");
            xmlDoc.AppendChild(root);
        }

        XmlElement settingNode = root.SelectSingleNode("GameSettingData") as XmlElement;
        if (settingNode == null)
        {
            settingNode = xmlDoc.CreateElement("GameSettingData");
            root.AppendChild(settingNode);
        }

        // XML �Ӽ� ������Ʈ
        settingNode.SetAttribute("ScreenSize", ScreenSize.ToString("F2"));
        settingNode.SetAttribute("ScreenPositionOffsetX", ScreenPositionOffsetX.ToString("F2"));
        settingNode.SetAttribute("ScreenPositionOffsetY", ScreenPositionOffsetY.ToString("F2"));
        settingNode.SetAttribute("SensorOffsetX", SensorOffsetX.ToString("F2"));
        settingNode.SetAttribute("SensorOffsetY", SensorOffsetY.ToString("F2"));
        settingNode.SetAttribute("ClusterThreshold", ClusterThreshold.ToString());
        settingNode.SetAttribute("Yhorizontal", Yhorizontal.ToString("F2"));
        settingNode.SetAttribute("Yvertical", Yvertical.ToString("F2"));
        settingNode.SetAttribute("Xdiagonal", Xdiagonal.ToString("F2"));
        settingNode.SetAttribute("Ydiagonal", Ydiagonal.ToString("F2"));
        settingNode.SetAttribute("XdiagonalLeft", XdiagonalLeft.ToString("F2"));
        settingNode.SetAttribute("TouchzoneLifetime", TouchzoneLifetime.ToString("F2"));
        settingNode.SetAttribute("MaxTouchzones", MaxTouchzones.ToString());
        settingNode.SetAttribute("TouchRange", TouchRange.ToString("F2"));

        // XML ����
        xmlDoc.Save(filePath);
    }
}
