using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_Launcher_Login : MonoBehaviour, IPointerClickHandler
{


    private GameLauncher_ICT Launcher;

    public bool Login_Select = false;
    public bool Login_Edit = false;
    public bool Login_Register = false;

    public bool Login_Save = false;
    public bool Login_Delete = false;
    // Start is called before the first frame update
    void Start()
    {
        Launcher = GameObject.Find("Launcher").GetComponent<GameLauncher_ICT>();
    }

    // Update is called once per frame
    public void OnPointerClick(PointerEventData eventData)
    {
        if (Login_Register)
            Manager_login.instance.Button_Register_StudentData();

        if (Login_Select)
            Launcher.Button_Message_Login_SelectedStudentCheck();


        if (Login_Edit)
            Manager_login.instance.Button_EditStudentData();

        if (Login_Save)
            Manager_login.instance.Button_SaveSelectedData();

        if (Login_Delete)
            Manager_login.instance.Button_DeleteSelectedData();
        //수정
        //** 학생 활성화 될 경우, 학생 삭제 버튼 구현 필요

    }
}
