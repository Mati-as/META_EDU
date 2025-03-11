using System.Xml;
using System.IO;
using UnityEngine;

public class XmlManager
{
    private static string filePath = Path.Combine(Application.streamingAssetsPath, "GameSettingData.xml");

    public static void SaveSettings(float screenSize, float offsetX, float offsetY, float sensorOffsetX, float sensorOffsetY)
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

        // 값 업데이트
        settingNode.SetAttribute("screenSize", screenSize.ToString("F2"));
        settingNode.SetAttribute("offsetX", offsetX.ToString("F2"));
        settingNode.SetAttribute("offsetY", offsetY.ToString("F2"));
        settingNode.SetAttribute("SensoroffsetX", sensorOffsetX.ToString("F2"));
        settingNode.SetAttribute("SensoroffsetY", sensorOffsetY.ToString("F2"));

        // XML 저장
        xmlDoc.Save(filePath);
    }

    public static void LoadSettings(out float screenSize, out float offsetX, out float offsetY, out float sensorOffsetX, out float sensorOffsetY)
    {
        screenSize = 1.0f;
        offsetX = 0.5f;
        offsetY = 0.5f;
        sensorOffsetX = 0.5f;
        sensorOffsetY = 0.5f;

        if (!File.Exists(filePath))
            return;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);
        XmlElement root = xmlDoc.DocumentElement;
        XmlElement settingNode = root.SelectSingleNode("GameSettingData") as XmlElement;

        if (settingNode != null)
        {
            screenSize = float.Parse(settingNode.GetAttribute("screenSize"));
            offsetX = float.Parse(settingNode.GetAttribute("offsetX"));
            offsetY = float.Parse(settingNode.GetAttribute("offsetY"));
            sensorOffsetX = float.Parse(settingNode.GetAttribute("SensoroffsetX"));
            sensorOffsetY = float.Parse(settingNode.GetAttribute("SensoroffsetY"));
        }
    }
}
