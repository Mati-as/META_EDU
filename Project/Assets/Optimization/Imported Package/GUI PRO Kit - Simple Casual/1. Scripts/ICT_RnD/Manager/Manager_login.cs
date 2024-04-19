
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;

public class Manager_login : CLASS_XmlData
{
    public static Manager_login instance = null;
    private GameLauncher_ICT Launcher;

    public List<LoginData> OriginDataList;
    public List<LoginData> NewDataList;
    public string filePath;

    //private LoginData Student_data;
    private LoginData Selected_Student_data;
    private int Num_student;
    public bool Is_Logindatasaved = false;
    public bool Is_StudentDataSelected = false;

    [Header("[CONTENTS PAGE COMPONENT]")]
    public GameObject Text_Icon_group;
    private GameObject Picture_Off;
    private GameObject Picture_On;
    private Text Text_ID;
    private Text Text_Name;

    [Header("[LOGIN PAGE COMPONENT]")]
    public GameObject Prefab_StudentInfo;
    public Transform Panel_Left_Content;

    public GameObject InputField_group;
    public GameObject Message_DeleteCheck;
    public GameObject Message_EditGuide;
    public GameObject Message_EditNonSelect;

    private GameObject InputField_Name;
    private GameObject InputField_ID;
    private GameObject InputField_BirthDate;
    private GameObject Button_Student_Delete;
    private GameObject Button_Student_Save;

    private Stack<DialogueData> Recent_data = new Stack<DialogueData>();

    [Header("[LOGIN INFORMATION]")]
    [SerializeField]
    public string ID;
    public string Name;
    public string Birthdate;
    public string Date;
    public int Session;
    public string Data_1;
    public string Data_2;

    // Start is called before the first frame update
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
        Num_student = -1;

        Init_Text();
        Launcher = this.gameObject.GetComponent<GameLauncher_ICT>();


        filePath = Path.Combine(Application.persistentDataPath, "LOGININFO.xml");
        Check_XmlFile("LOGININFO");

        if (filePath != null)
        {
            Recent_data.Clear();
            //Debug.Log(Application.dataPath);
            OriginDataList = Read();
            NewDataList = Read();

            for (int i = 0; i < OriginDataList.Count; ++i)
            {
                LoginData item = OriginDataList[i];

                GameObject myInstance = Instantiate(Prefab_StudentInfo, Panel_Left_Content);
                myInstance.GetComponent<UI_button_StudentInfo>().Result_num = i;
                myInstance.GetComponent<UI_button_StudentInfo>().Student_Name = item.Name;
                myInstance.GetComponent<UI_button_StudentInfo>().Student_ID = item.ID;
                myInstance.GetComponent<UI_button_StudentInfo>().Student_BirthDate = item.Birth_date;
            }
            Init_Registermenu();
        }
    }
    public void Set_ButtonPrefab()
    {
        for (int i = 0; i < NewDataList.Count; ++i)
        {
            LoginData item = NewDataList[i];

            GameObject myInstance = Instantiate(Prefab_StudentInfo, Panel_Left_Content);
            myInstance.GetComponent<UI_button_StudentInfo>().Result_num = i;
            myInstance.GetComponent<UI_button_StudentInfo>().Student_Name = item.Name;
            myInstance.GetComponent<UI_button_StudentInfo>().Student_ID = item.ID;
            myInstance.GetComponent<UI_button_StudentInfo>().Student_BirthDate = item.Birth_date;
        }
    }
    public void Destroy_ButtonPrefab()
    {
        for (int i = 0; i < Panel_Left_Content.transform.childCount; i++)
        {
            GameObject Button = Panel_Left_Content.transform.GetChild(i).gameObject;
            Destroy(Button);
        }
    }

    public override void Write()
    {
        XmlDocument Document = new XmlDocument();
        XmlElement ItemListElement = Document.CreateElement("Login_Info_data");
        Document.AppendChild(ItemListElement);

        foreach (LoginData data in NewDataList)
        {
            XmlElement ItemElement = Document.CreateElement("Login_Info_data");
            ItemElement.SetAttribute("ID", data.ID);
            ItemElement.SetAttribute("Name", data.Name);
            ItemElement.SetAttribute("Birthdate", data.Birth_date);
            ItemListElement.AppendChild(ItemElement);
        }
        Document.Save(filePath);
        //Debug.Log("SAVED DATA WRITE");
    }

    public List<LoginData> Read()
    {
        XmlDocument Document = new XmlDocument();
        Document.Load(filePath);
        XmlElement ItemListElement = Document["Login_Info_data"];
        List<LoginData> ItemList = new List<LoginData>();

        foreach (XmlElement ItemElement in ItemListElement.ChildNodes)
        {
            LoginData Item = new LoginData();
            Item.ID = ItemElement.GetAttribute("ID");
            Item.Name = ItemElement.GetAttribute("Name");
            Item.Birth_date = ItemElement.GetAttribute("Birthdate");

            ItemList.Add(Item);
        }
        return ItemList;
    }
    public string Init_RandomID()
    {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
        var ID = new char[6];
        var random = new System.Random();

        for (int i = 0; i < ID.Length; i++)
        {
            ID[i] = characters[random.Next(characters.Length)];
        }

        string Temp_ID = new string(ID);

        //중복일경우 마지막 0으로 변경
        foreach (LoginData data in NewDataList)
        {
            if (data.ID == Temp_ID)
            {
                ID[5] = '0';
                Temp_ID = new string(ID);
                Debug.Log("생성된 아이디 중복" + "ID : " + Temp_ID);
            }
            else
            {
            }
        }
        return Temp_ID;
    }

    public void Init_Registermenu()
    {
        string Temp_ID = "HeHo_Plus_" + Init_RandomID();
        InputField_ID.GetComponent<TMP_InputField>().text = Temp_ID;
        InputField_Name.GetComponent<TMP_InputField>().text = "";
        InputField_BirthDate.GetComponent<TMP_InputField>().text = "";

        //현재 선택된 학생 삭제 필요
        Is_StudentDataSelected = false;
        Num_student = -1;
        Selected_Student_data = null;
    }
    public void Setting_StudentInfo()
    {
        if (Is_StudentDataSelected)
        {
            ID = Selected_Student_data.ID;
            Name = Selected_Student_data.Name;
            Birthdate = Selected_Student_data.Birth_date;
            Date = DateTime.Now.ToString(("yyyy.MM.dd"));

            Picture_On.SetActive(true);
            Picture_Off.SetActive(false);
            Text_ID.text = ID;
            Text_Name.text = Name;

            Is_StudentDataSelected = false;
            Is_Logindatasaved = true;

            Init_Registermenu();
            Launcher.Button_Message_Login_Completed();
        }
        else
        {
            //데이터 초기화 하는 부분, 추후에 적절한 곳으로 수정 필요 1129
            Picture_Off.SetActive(true);
            Picture_On.SetActive(false);
            Text_Name.text = "미선택";
            Text_ID.text = "HeHo_Plus_OOOOOO";
        }
    }

    public bool Get_Islogindatasaved()
    {
        return Is_Logindatasaved;
    }
    public bool Get_Is_StudentDataSelected()
    {
        return Is_StudentDataSelected;
    }

    public void Set_Selectednumber(int num)
    {
        Is_StudentDataSelected = true;
        Num_student = num;
        Selected_Student_data = NewDataList[Num_student];
    }
    public string Get_SelectedStudentName()
    {
        return Selected_Student_data.Name;
    }
    public string Get_SelectedStudentID()
    {
        return Selected_Student_data.ID;
    }

    public void Button_Register_StudentData()
    {
        LoginData Item = new LoginData();

        Item.ID = InputField_ID.GetComponent<TMP_InputField>().text;
        Item.Name = InputField_Name.GetComponent<TMP_InputField>().text;
        Item.Birth_date = InputField_BirthDate.GetComponent<TMP_InputField>().text;

        if (string.IsNullOrEmpty(Item.Name) || string.IsNullOrEmpty(Item.Name))
        {
            //Debug.Log("Name, Date Empty");
            Launcher.Button_Message_Login_FieldEmpty();
        }
        else
        {
            Add_StudentData(Item);
        }
    }
    public void Add_StudentData(LoginData SelectedData)
    {
        //only for Button_Register_StudentData
        NewDataList.Add(SelectedData);

        if (NewDataList[NewDataList.Count - 1].ID == SelectedData.ID)
        {
            Launcher.Button_Message_Login_StudentDataSaved();
            Write();
        }
        Destroy_ButtonPrefab();
        Set_ButtonPrefab();
        Init_Registermenu();
    }

    public void Button_EditStudentData()
    {
        if (Selected_Student_data != null)
        {
            InputField_ID.GetComponent<TMP_InputField>().text = Selected_Student_data.ID;
            InputField_Name.GetComponent<TMP_InputField>().text = Selected_Student_data.Name;
            InputField_BirthDate.GetComponent<TMP_InputField>().text = Selected_Student_data.Birth_date;

            Button_Student_Delete.SetActive(true);
            Button_Student_Save.SetActive(true);


            Message_EditGuide.SetActive(true);
            //학생 버튼 색 변경
        }
        else
        {
            Message_EditNonSelect.SetActive(true) ;
        }
    }
    public void Edit_StudentData()
    {
        LoginData Item = new LoginData();

        Item.ID = InputField_ID.GetComponent<TMP_InputField>().text;
        Item.Name = InputField_Name.GetComponent<TMP_InputField>().text;
        Item.Birth_date = InputField_BirthDate.GetComponent<TMP_InputField>().text;

        NewDataList[Num_student] = Item;

        if (NewDataList[Num_student].ID == Selected_Student_data.ID)
        {
            Launcher.Button_Message_Login_StudentDataSaved();
            Write();
        }
       // Debug.Log("Student data edited!");

        //Message_EditCompleted.SetActive(true);
        Destroy_ButtonPrefab();
        Set_ButtonPrefab();
        Init_Registermenu();

        Button_Student_Delete.SetActive(false);
        Button_Student_Save.SetActive(false);
    }

    public void Delete_StudentData()
    {
        if (NewDataList[Num_student].ID == Selected_Student_data.ID)
        {
            NewDataList.RemoveAt(Num_student);
            Launcher.Button_Message_Login_StudentDataSaved();
            Write();

            Destroy_ButtonPrefab();
            Set_ButtonPrefab();
        }
        Button_Student_Delete.SetActive(false);
        Button_Student_Save.SetActive(false);
        //Debug.Log("Student data deleted!");
        
    }
    public void Button_SaveSelectedData()
    {
        Edit_StudentData();
    }
    public void Button_DeleteSelectedData()
    {
        Message_DeleteCheck.SetActive(true);
        Message_DeleteCheck.GetComponent<Message_SelectedStudentInfo>().Change_Info("학생 정보를 삭제할까요?");
    }
    public void Init_Text()
    {
        Picture_Off = Text_Icon_group.transform.GetChild(0).gameObject;
        Picture_On = Text_Icon_group.transform.GetChild(1).gameObject;
        Text_ID = Text_Icon_group.transform.GetChild(2).gameObject.GetComponent<Text>();
        Text_Name = Text_Icon_group.transform.GetChild(3).gameObject.GetComponent<Text>();

        InputField_Name = InputField_group.transform.GetChild(0).gameObject;
        InputField_ID = InputField_group.transform.GetChild(1).gameObject;
        InputField_BirthDate = InputField_group.transform.GetChild(2).gameObject;
        Button_Student_Delete = InputField_group.transform.GetChild(3).gameObject;
        Button_Student_Save = InputField_group.transform.GetChild(4).gameObject;

    }
}
