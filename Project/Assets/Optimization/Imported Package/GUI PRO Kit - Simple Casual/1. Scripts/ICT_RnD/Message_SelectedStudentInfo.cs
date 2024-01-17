using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Message_SelectedStudentInfo : MonoBehaviour
{
    public GameObject Text_group;

    private GameObject Text_Name;
    private GameObject Text_ID;

    private string Student_ID;
    private string Student_Name;
    // Start is called before the first frame update
    void Start()
    {
        Text_Name = Text_group.transform.GetChild(0).gameObject;
        Text_ID = Text_group.transform.GetChild(1).gameObject;
    }

    public void Change_Info(string Text)
    {
        //Text에 Student_Name다음으로 올 텍스트 입력

        Text_Name = Text_group.transform.GetChild(0).gameObject;
        Text_ID = Text_group.transform.GetChild(1).gameObject;

        Student_Name = Manager_login.instance.Get_SelectedStudentName();
        Student_ID = Manager_login.instance.Get_SelectedStudentID();


        if (Text_Name != null)
            Text_Name.GetComponent<Text>().text = Student_Name + Text;

        if (Text_ID != null)
            Text_ID.GetComponent<Text>().text = "학생 ID : " + Student_ID;
    }
}
