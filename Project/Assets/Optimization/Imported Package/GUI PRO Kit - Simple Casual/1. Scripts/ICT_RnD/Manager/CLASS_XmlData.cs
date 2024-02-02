using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using Application = UnityEngine.Application;

//Manager_data
public class DialogueData
{
    //[XmlAttribute]
    public string ID;
    public string Name;
    public string Birth_date;
    public string Date;
    public string Session;
    public string Data_1;
    public string Data_2;
}

// Manager_login
public class LoginData
{
    //[XmlAttribute]
    public string ID;
    public string Name;
    public string Birth_date;
}

// Manager_survey
public class SurveyData
{
    public string ID;
    public string Name;
    public string Date;
    public string Session;
    public string Data_S1;
    public string Data_S2;
    public string Data_S3;
    public string Data_S4;
    public string Data_S5;
    public string Data_S6;
    public string Data_S7;
    public string Data_S8;
}

public class Result_IndetailData
{
    public string ID;
    public string Name;
    public string Date;
    public string Session;
    public List<string> Data = new List<string>();
}

public abstract class CLASS_XmlData : MonoBehaviour
{

    public abstract void Write();
    //public abstract void Read();

    public void Check_XmlFile(string fileName)
    {
        //string filePath = Path.Combine(Application.persistentDataPath, "LOGININFO.xml");
        string filePath = Path.Combine(Application.persistentDataPath, fileName+".xml");

        if (File.Exists(filePath))
        {
            Debug.Log(fileName+"XML FILE EXIST");
        }
        else
        {
            //TextAsset XmlFilepath = Resources.Load<TextAsset>("LOGININFO");
            TextAsset XmlFilepath = Resources.Load<TextAsset>(fileName);
            XmlDocument Document = new XmlDocument();
            Document.LoadXml(XmlFilepath.ToString());
            Document.Save(filePath);

            Debug.Log(fileName+".xml FILE NOT EXIST");
        }
}
}

