using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;



/// <summary>
/// 스크립트 우선실행 필수
/// </summary>
public class A_SettingManager : MonoBehaviour
{

    public static float MAIN_VOLIUME{get;private set;}
    public static float EFFECT_VOLUME{get;private set;}
    public static float BGM_VOLUME{get;private set;}
    public static float NARRATION_VOLUME{get;private set;}
    
    public float screenProjectorHeight{get;private set;}
    
    
    void Awake()
    {
        Init();
    }

    // Update is called once per frame
    
    void Init()
    {
        SetXMLPath();
        CheckAndGenerateXmlFile("SettingParams",settingXmlPath);
        LoadSettingParams();
    }
    
        #region  XML을 통한 세팅 초기화 및 저장

    public XmlDocument xmlDoc_Setting;
    public string settingXmlPath;


    private void SetXMLPath()
    {
        settingXmlPath = System.IO.Path.Combine(Application.persistentDataPath, "SettingParams.xml");
    }
    private void LoadSettingParams()
    {
        XmlNode root =xmlDoc_Setting.DocumentElement;
        var nodes = root.SelectNodes("SettingParams");

        var count = 0;
        foreach (XmlNode node in nodes)
        {
            Debug.Assert(count!=1);
           
            
            var projectorScreenHeight = node.Attributes["projectorScreenHeight"].Value;
          
            SensorManager.height = float.Parse(projectorScreenHeight);
            Debug.Log($"load projecotr hegiht preset suceess. height: {SensorManager.height} (cm)");
            var mainVol = node.Attributes["mainVolume"].Value;
            MAIN_VOLIUME = float.Parse(mainVol);
            
            var effectVol = node.Attributes["effectVol"].Value;
            EFFECT_VOLUME = float.Parse(effectVol);
            
            var bgmVol = node.Attributes["bgmVol"].Value;
            BGM_VOLUME= float.Parse(bgmVol);
            
            var narrationVol = node.Attributes["narrationVol"].Value;
            NARRATION_VOLUME = float.Parse(narrationVol);
            
            
            count++;
        }
       
    }

    public void SaveCurrentSetting(float projectorScreenHeight, float mainVolume, float effectVol, float bgmVol, float narrationVol)
    {
        var tempRootSetting = xmlDoc_Setting.DocumentElement;
        tempRootSetting.RemoveAllAttributes();

        var setting = xmlDoc_Setting.CreateElement("SettingParams");
        setting.SetAttribute("projectorscreenheight", projectorScreenHeight.ToString("F2"));
        
        setting.SetAttribute("mainvolume", Managers.soundManager.volumes[(int)SoundManager.Sound.Main].ToString("F2"));
        setting.SetAttribute("bgmvol", Managers.soundManager.volumes[(int)SoundManager.Sound.Bgm].ToString("F2"));
        setting.SetAttribute("effectvol", Managers.soundManager.volumes[(int)SoundManager.Sound.Effect].ToString("F2"));
        setting.SetAttribute("narrationvol", Managers.soundManager.volumes[(int)SoundManager.Sound.Narration].ToString("F2"));


        tempRootSetting.AppendChild(setting);

        WriteXML(xmlDoc_Setting, settingXmlPath);  
    }
    
    public void CheckAndGenerateXmlFile(string fileName,string path)
    {
        //string filePath = Path.Combine(Application.persistentDataPath, "LOGININFO.xml");
       

        if (File.Exists(path))
        {
            Debug.Log(fileName + "XML FILE EXIST");
        }
        else
        {
            var newXml = new XmlDocument();
            
         
            XmlDeclaration xmlDeclaration = newXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = newXml.DocumentElement;
            newXml.InsertBefore(xmlDeclaration, root);

           
            XmlElement rootElement = newXml.CreateElement("SettingParams");
            newXml.AppendChild(rootElement);
            
            newXml.Save(path);
            Debug.Log(fileName + ".xml FILE NOT EXIST, new file's been created at " + path);
        }
        Debug.Log("History Checker Active");
    }
    
    public void WriteXML(XmlDocument document, string path)
    {
        document.Save(path);
        Debug.Log($"{path}");
        //Debug.Log("SAVED DATA WRITE");
    }


    #endregion
}
