using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Manager_Survey : CLASS_XmlData
{
    public static Manager_Survey instance = null;
    public static List<SurveyData> OriginDataList;
    private List<SurveyData> NewDataList;

    private string filePath;

    private int Question_Number;
    private List<int> Answer_Number = new List<int>();
    private List<string> Text_QuestionList = new List<string>();


    public GameObject Message_AnswerNotSelected;
    public GameObject Message_SubmitCompleted;
    public GameObject Text_group;
    private Text Text_Number;
    private Text Text_Question;

    public GameObject Survey_UI;
    private GameObject Intro;
    private GameObject Question;
    private GameObject Submit;

    private bool Is_AnswerSelected;
    private int Num_Answer;

    [Header("[SURVEY INFORMATION]")]
    [SerializeField]
    public string ID;
    public string Name;
    public string Date;
    public string Session;
    public string Data_1;
    public string Data_2;
    public string Data_3;
    public string Data_4;
    public string Data_5;
    public string Data_6;
    public string Data_7;
    public string Data_8;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
                Destroy(this.gameObject);
        }
    }
    void Start()
    {
        Init_Survey();

        filePath = Path.Combine(Application.persistentDataPath, "SURVEYRESULT.xml");
        Check_XmlFile("SURVEYRESULT");

        if (filePath != null)
        {
            NewDataList = Read();
        }
    }
    public override void Write()
    {
        XmlDocument Document = new XmlDocument();
        XmlElement ItemListElement = Document.CreateElement("Survey_data");
        Document.AppendChild(ItemListElement);

        foreach (SurveyData data in NewDataList)
        {
            XmlElement ItemElement = Document.CreateElement("Survey_data");
            ItemElement.SetAttribute("ID", data.ID);
            ItemElement.SetAttribute("Name", data.Name);
            ItemElement.SetAttribute("Date", data.Date);
            ItemElement.SetAttribute("Session", data.Session);
            ItemElement.SetAttribute("Data_1", data.Data_S1);
            ItemElement.SetAttribute("Data_2", data.Data_S2);
            ItemElement.SetAttribute("Data_3", data.Data_S3);
            ItemElement.SetAttribute("Data_4", data.Data_S4);
            ItemElement.SetAttribute("Data_5", data.Data_S5);
            ItemElement.SetAttribute("Data_6", data.Data_S6);
            ItemElement.SetAttribute("Data_7", data.Data_S7);
            ItemElement.SetAttribute("Data_8", data.Data_S8);
            ItemListElement.AppendChild(ItemElement);
        }
        Document.Save(filePath);
    }

    public List<SurveyData> Read()
    {
        XmlDocument Document = new XmlDocument();
        Document.Load(filePath);
        XmlElement ItemListElement = Document["Survey_data"];
        List<SurveyData> ItemList = new List<SurveyData>();

        foreach (XmlElement ItemElement in ItemListElement.ChildNodes)
        {
            SurveyData Item = new SurveyData();
            Item.ID = ItemElement.GetAttribute("ID");
            Item.Name = ItemElement.GetAttribute("Name");
            Item.Date = ItemElement.GetAttribute("Date");
            Item.Session = ItemElement.GetAttribute("Session");
            Item.Data_S1 = ItemElement.GetAttribute("Data_1");
            Item.Data_S2 = ItemElement.GetAttribute("Data_2");
            Item.Data_S3 = ItemElement.GetAttribute("Data_3");
            Item.Data_S4 = ItemElement.GetAttribute("Data_4");
            Item.Data_S5 = ItemElement.GetAttribute("Data_5");
            Item.Data_S6 = ItemElement.GetAttribute("Data_6");
            Item.Data_S7 = ItemElement.GetAttribute("Data_7");
            Item.Data_S8 = ItemElement.GetAttribute("Data_8");

            ItemList.Add(Item);
        }
        return ItemList;
    }
    //만족도 조사를 계속해서 진행하게 될 경우 리스트에 추가적으로 업데이트가 되는가?

    public void Add_Surveydata()
    {
        SurveyData Item = new SurveyData();

        ID = Manager_login.instance.ID;
        Name = Manager_login.instance.Name;
        Date = DateTime.Now.ToString(("yyyy.MM.dd.HH.mm"));

        Session = Item.Session;
        Data_1 = Answer_Number[0].ToString();
        Data_2 = Answer_Number[1].ToString();
        Data_3 = Answer_Number[2].ToString();
        Data_4 = Answer_Number[3].ToString();
        Data_5 = Answer_Number[4].ToString();
        Data_6 = Answer_Number[5].ToString();
        Data_7 = Answer_Number[6].ToString();
        Data_8 = Answer_Number[7].ToString();

        Item.ID = ID;
        Item.Name = Name;
        Item.Date = Date;
        Item.Data_S1 = Data_1;
        Item.Data_S2 = Data_2;
        Item.Data_S3 = Data_3;
        Item.Data_S4 = Data_4;
        Item.Data_S5 = Data_5;
        Item.Data_S6 = Data_6;
        Item.Data_S7 = Data_7;
        Item.Data_S8 = Data_8;

        NewDataList.Add(Item);

        if (NewDataList[NewDataList.Count - 1].ID == Item.ID)
        {
            Write();
            Debug.Log("Survey data saved!");
            Message_SubmitCompleted.SetActive(true);
        }
    }
    // Start is called before the first frame update
   
    public void ChangeText()
    {
        Text_Number.text = (Question_Number+1) + "/8";
        Text_Question.text = Text_QuestionList[Question_Number];
    }

    public void Button_Next()
    {
        if (Is_AnswerSelected)
        {
            if (Question_Number < 7)
            {
                Answer_Number.Add(Num_Answer);
                Question_Number += 1;
                //Debug.Log("현재" + Question_Number);
                ChangeText();
                Is_AnswerSelected = false;
            }
            else
            {
                Answer_Number.Add(Num_Answer);
                Question.SetActive(false);
                Submit.SetActive(true);
            }
        }
        else
        {
            Message_AnswerNotSelected.SetActive(true);
        }
    }
    public void Button_Prev()
    {
        if (Question_Number == 0)
        {

        }
        else
        {
            Question_Number -= 1;
            Answer_Number.RemoveAt(Question_Number);
            ChangeText();
            Is_AnswerSelected = false;
        }
    }
    public void Button_Answer(int num_Answer)
    {
        Is_AnswerSelected = true;
        Num_Answer = num_Answer;

        //다음 버튼 애니메이션 재생 되게끔
    }
    public void Button_Start()
    {
        Intro.SetActive(false);
        Question.SetActive(true );
    }
    public void Button_Submit()
    {
        Add_Surveydata();
    }

    
    public void Init_Survey()
    {
        //Debug.Log("초기화 확인");
        
        Intro = Survey_UI.transform.GetChild(0).gameObject;
        Question = Survey_UI.transform.GetChild(1).gameObject;
        Submit = Survey_UI.transform.GetChild(2).gameObject;

        Text_Number = Text_group.transform.GetChild(0).gameObject.GetComponent<Text>();
        Text_Question = Text_group.transform.GetChild(1).gameObject.GetComponent<Text>();

        Text_QuestionList.Add("프로그램이 마음에 들었나요?");
        Text_QuestionList.Add("프로그램이 재미있었나요?");
        Text_QuestionList.Add("프로그램이 사용하기 쉬웠나요?");
        Text_QuestionList.Add("프로그램에서 꽃벵이/옥수수/당근/알로에를 쉽게 알아볼 수 있었나요?");
        Text_QuestionList.Add("프로그램을 통해 꽃벵이/옥수수/당근/알로에의 이름을 잘 알게 되었나요?");
        Text_QuestionList.Add("프로그램을 사용해서 배우니 꽃벵이/옥수수/당근/알로에를 이해하는데 더 도움이 되었나요?");
        Text_QuestionList.Add("프로그램의 사용이 만족스러운가요?");
        Text_QuestionList.Add("다른 친구에게 프로그램의 사용법을 설명하기 쉬운가요?");


        Intro.SetActive(true);
        Question.SetActive(false);
        Submit.SetActive(false);
        Question_Number = 0;
        Answer_Number.Clear();
        ChangeText();

    }
}
