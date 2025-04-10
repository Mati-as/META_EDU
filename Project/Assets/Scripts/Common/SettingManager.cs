using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;



/// <summary>
/// 스크립트 우선실행 필수
/// </summary>
public class SettingManager : MonoBehaviour
{
    public  float SCREEN_PROJECTOER_HEIGHT_FROM_XML{get;private set;}
    public  float MAIN_VOLIUME{get;private set;}
    public  float EFFECT_VOLUME{get;private set;}
    public  float BGM_VOLUME{get;private set;}
    public  float NARRATION_VOLUME{get;private set;}

    private string GameSettingData;
    private string DataRoot;


      

    // Update is called once per frame
    
    public void Init()
    {
        SetXMLPath();
        CheckAndGenerateXmlFile(nameof(GameSettingData),settingXmlPath);
        LoadSettingParams();
        
        Base_GameManager.OnSceneLoad -= OnSceneLoad;
        Base_GameManager.OnSceneLoad += OnSceneLoad;
    }
    
    private void OnDestroy()
    {
        Base_GameManager.OnSceneLoad -= OnSceneLoad;
    }

    private void OnSceneLoad(string _, DateTime __)
    {
        XmlManager.Instance.LoadSettings();
        Logger.Log("센서관련 XML데이터 로드완료----------------------------------");
    }
#region  XML을 통한 세팅 초기화 및 저장

    public static XmlDocument xmlDoc_Setting;
    public static string settingXmlPath;


    private void SetXMLPath()
    {
        settingXmlPath = System.IO.Path.Combine(Application.persistentDataPath, nameof(GameSettingData) + ".xml");
    }
    private void LoadSettingParams()
    {
        Utils.ReadXML(ref xmlDoc_Setting,settingXmlPath);
        XmlNode root =xmlDoc_Setting.DocumentElement;
        var nodes = root.SelectNodes(nameof(GameSettingData));

        var count = 0;
        foreach (XmlNode node in nodes)
        {
            Debug.Assert(count!=1);
            
            var projectorScreenHeight = node.Attributes["projectorscreenheight"].Value;
            Logger.SensorRelatedLog($"Value From Xml Height: {projectorScreenHeight}(cm)");

            float xmlHeight = 0;
            xmlHeight = float.Parse(projectorScreenHeight);
            SensorManager.height = xmlHeight;
            SCREEN_PROJECTOER_HEIGHT_FROM_XML = xmlHeight;
            Logger.SensorRelatedLog($"load projecotr Xmlhegiht ({xmlHeight}) preset suceess. height: {SCREEN_PROJECTOER_HEIGHT_FROM_XML} (cm)");
           
            var mainVol = node.Attributes["mainvolume"].Value;
            MAIN_VOLIUME = float.Parse(mainVol);
            
            var bgmVol = node.Attributes["bgmvol"].Value;
            BGM_VOLUME= float.Parse(bgmVol);
            
            var effectVol = node.Attributes["effectvol"].Value;
            EFFECT_VOLUME = float.Parse(effectVol);
            
            var narrationVol = node.Attributes["narrationvol"].Value;
            NARRATION_VOLUME = float.Parse(narrationVol);
            
            
            count++;
        }
        Debug.Assert(count !=0,$"setting Failed there's no {nameof(GameSettingData)}" );
        
        Logger.SensorRelatedLog($"initioal setting completed:  {SensorManager.height} (cm) MainVol: {MAIN_VOLIUME}" );
    }

    public static void SaveCurrentSetting(float projectorScreenHeight, float mainVolume, float effectVol, float bgmVol, float narrationVol)
    {
        var tempRootSetting = xmlDoc_Setting.DocumentElement;
        tempRootSetting.RemoveAllAttributes();

        var setting = xmlDoc_Setting.CreateElement(nameof(GameSettingData));
        setting.SetAttribute("projectorscreenheight", projectorScreenHeight.ToString("F2"));
        setting.SetAttribute("mainvolume", Managers.Sound.volumes[(int)SoundManager.Sound.Main].ToString("F2"));
        setting.SetAttribute("bgmvol", Managers.Sound.volumes[(int)SoundManager.Sound.Bgm].ToString("F2"));
        setting.SetAttribute("effectvol", Managers.Sound.volumes[(int)SoundManager.Sound.Effect].ToString("F2"));
        setting.SetAttribute("narrationvol", Managers.Sound.volumes[(int)SoundManager.Sound.Narration].ToString("F2"));


        tempRootSetting.AppendChild(setting);

        WriteXML(xmlDoc_Setting, settingXmlPath);  
    }
    
    public void CheckAndGenerateXmlFile(string fileName,string path)
    {

        if (File.Exists(path))
        {
            Logger.SensorRelatedLog(fileName + "XML FILE EXIST");
            Utils.ReadXML(ref xmlDoc_Setting,settingXmlPath);
        }
        else
        {
            var newXml = new XmlDocument();

            XmlDeclaration xmlDeclaration = newXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            newXml.AppendChild(xmlDeclaration);

            XmlElement root = newXml.CreateElement("Settings");
            newXml.AppendChild(root);

            XmlElement initSetting = newXml.CreateElement(nameof(GameSettingData));

            initSetting.SetAttribute("projectorscreenheight", SensorManager.height.ToString("F2"));
            initSetting.SetAttribute("mainvolume", "0.50");
            initSetting.SetAttribute("bgmvol", "0.50");
            initSetting.SetAttribute("effectvol", "0.50");
            initSetting.SetAttribute("narrationvol", "0.50");

            root.AppendChild(initSetting); // Append initSetting to the root element

            newXml.Save(path);
            Logger.SensorRelatedLog(fileName + ".xml FILE NOT EXIST, new file's been created at " + path);

          
        }
        Logger.SensorRelatedLog("History Checker Active");
    }
    
    public static void WriteXML(XmlDocument document, string path)
    {
        document.Save(path);
        Debug.Log($"{path}");
        //Debug.Log("SAVED DATA WRITE");
    }


#endregion
}
