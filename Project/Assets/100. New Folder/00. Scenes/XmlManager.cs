//xml 파일 기본값 설정 (0325)
/*
    ScreenSize = 1.0f;
    ScreenRatio = 0.782f;
    SensorPosX = 0
    SensorPosX = 540
    SensorAngle = 0f;
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
*/

using System.Xml;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

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
                instance = new XmlManager();
            return instance;
        }
    }

    // [설정 데이터, GameSettingData]
    public float ScreenSize
    {
        get; set;
    }
    public float ScreenRatio
    {
        get; set;
    }
    public float SensorPosX
    {
        get; set;
    }
    public float SensorPosY
    {
        get; set;
    }
    public float SensorAngle
    {
        get; set;
    }
    public float ScreenPositionOffsetX
    {
        get; set;
    }
    public float ScreenPositionOffsetY
    {
        get; set;
    }
    public float SensorOffsetX
    {
        get; set;
    }
    public float SensorOffsetY
    {
        get; set;
    }
    public int ClusterThreshold
    {
        get; set;
    }
    public float Yhorizontal
    {
        get; set;
    }
    public float Yvertical
    {
        get; set;
    }
    public float Xdiagonal
    {
        get; set;
    }
    public float Ydiagonal
    {
        get; set;
    }
    public float XdiagonalLeft
    {
        get; set;
    }
    public float TouchzoneLifetime
    {
        get; set;
    }
    public int MaxTouchzones
    {
        get; set;
    }
    public float TouchRange
    {
        get; set;
    }

    private XmlManager()
    {
        LoadGameSettings();   // 기존 게임 설정
        LoadMenuSettings();   // 월/Scene 설정
    }

    public void LoadGameSettings()
    {
        if (isLoaded) return;
        if (!File.Exists(filePath)) return;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(filePath);
        XmlElement root = xmlDoc.DocumentElement;
        XmlElement settingNode = root.SelectSingleNode("GameSettingData") as XmlElement;

        if (settingNode != null)
        {
            ScreenSize = float.Parse(settingNode.GetAttribute("ScreenSize"));
            ScreenRatio = float.Parse(settingNode.GetAttribute("ScreenRatio"));
            SensorPosX = float.Parse(settingNode.GetAttribute("SensorPosX"));
            SensorPosY = float.Parse(settingNode.GetAttribute("SensorPosY"));
            SensorAngle = float.Parse(settingNode.GetAttribute("SensorAngle"));
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

        isLoaded = true;
    }

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

        settingNode.SetAttribute("ScreenSize", ScreenSize.ToString("F2"));
        settingNode.SetAttribute("ScreenRatio", ScreenRatio.ToString("F3"));
        settingNode.SetAttribute("SensorPosX", SensorPosX.ToString("F1"));
        settingNode.SetAttribute("SensorPosY", SensorPosY.ToString("F1"));
        settingNode.SetAttribute("SensorAngle", SensorAngle.ToString("F1"));
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

        xmlDoc.Save(filePath);
    }

    // [설정 데이터, MenuSettingData]
    public class SceneData
    {
        public string Id;
        public bool IsActive;
        public string Category;
        public string Month;
    }

    private static string menuSettingPath = Path.Combine(Application.streamingAssetsPath, "MenuSettingData.xml");

    private static readonly string[] monthKeys = new string[]
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    public Dictionary<string, bool> MenuSettings = new Dictionary<string, bool>();
    public Dictionary<string, SceneData> SceneSettings = new Dictionary<string, SceneData>();

    public void LoadMenuSettings()
    {
        MenuSettings.Clear();
        SceneSettings.Clear();

        if (!File.Exists(menuSettingPath))
        {
            foreach (var key in monthKeys)
                MenuSettings[key] = false;

            SaveMenuSettings();
            return;
        }

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(menuSettingPath);

        // 월별 설정 불러오기
        XmlNodeList settingNodes = xmlDoc.GetElementsByTagName("Setting");
        foreach (XmlNode node in settingNodes)
        {
            string name = node.Attributes["name"].Value;
            bool value = bool.Parse(node.Attributes["value"].Value);
            MenuSettings[name] = value;
        }

        // Scene 설정 불러오기
        XmlNodeList sceneNodes = xmlDoc.GetElementsByTagName("Scene");
        foreach (XmlNode node in sceneNodes)
        {
            string id = node.Attributes["id"].Value;
            bool value = bool.Parse(node.Attributes["value"].Value);
            string category = node.Attributes["category"]?.Value ?? "";
            string month = node.Attributes["month"]?.Value ?? ""; // ✅ 추가

            SceneData data = new SceneData
            {
                Id = id,
                IsActive = value,
                Category = category,
                Month = month
            };

            SceneSettings[id] = data;
        }

        // 누락된 월 기본값 false 처리
        foreach (var key in monthKeys)
        {
            if (!MenuSettings.ContainsKey(key))
                MenuSettings[key] = false;
        }
    }

    public void SaveMenuSettings()
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement root = xmlDoc.CreateElement("MenuSettings");
        xmlDoc.AppendChild(root);

        // 월별 저장
        foreach (var kv in MenuSettings)
        {
            XmlElement setting = xmlDoc.CreateElement("Setting");
            setting.SetAttribute("name", kv.Key);
            setting.SetAttribute("value", kv.Value.ToString().ToLower());
            root.AppendChild(setting);
        }

        // Scene 저장
        foreach (var kv in SceneSettings)
        {
            XmlElement scene = xmlDoc.CreateElement("Scene");
            scene.SetAttribute("id", kv.Value.Id);
            scene.SetAttribute("value", kv.Value.IsActive.ToString().ToLower());
            scene.SetAttribute("category", kv.Value.Category);
            scene.SetAttribute("month", kv.Value.Month ?? "None"); 
            root.AppendChild(scene);
        }

        xmlDoc.Save(menuSettingPath);
    }

    public bool GetMenuSetting(string key)
    {
        return MenuSettings.ContainsKey(key) ? MenuSettings[key] : false;
    }

    public void SetMenuSetting(string key, bool value)
    {
        MenuSettings[key] = value;
        SaveMenuSettings();
    }

    public bool GetSceneSetting(string id)
    {
        return SceneSettings.ContainsKey(id) ? SceneSettings[id].IsActive : false;
    }

    public void SetSceneSetting(string id, bool value)
    {
        if (SceneSettings.ContainsKey(id))
        {
            SceneSettings[id].IsActive = value;
        }
        else
        {
            SceneSettings[id] = new SceneData
            {
                Id = id,
                IsActive = value,
                Category = "Uncategorized"
            };
        }

        SaveMenuSettings();
    }

    // (선택적) 카테고리까지 업데이트하는 함수
    public void SetSceneSetting(string id, bool value, string category)
    {
        if (SceneSettings.ContainsKey(id))
        {
            SceneSettings[id].IsActive = value;
            SceneSettings[id].Category = category;
        }
        else
        {
            SceneSettings[id] = new SceneData
            {
                Id = id,
                IsActive = value,
                Category = category
            };
        }

        SaveMenuSettings();
    }
}
