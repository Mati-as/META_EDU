using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_Launcher_ICT : MonoBehaviour, IPointerClickHandler
{
    /*
     * 0923 해당 되는 버튼 기능의 변수에 TRUE 또는 입력 할 것 
     * 
     *
     **/

    private GameLauncher_ICT Launcher;
    public bool Message_Contents = false;
    public bool Message_Contents_Login = false;

    public bool Tool = false;
    public bool Result = false;
    public bool Back = false;
    public bool Back_ToContent = false;
    public bool Back_ToMode = false;
    public bool Save_Tool = false;
    public bool Setting = false;
    public bool Close = false;
    public bool Home = false;
    public bool Login = false;
    public bool Survey = false;

    public int Num_contents = -1;
    public int Num_contents_Func = -1;
    public int Mode = -1;
    public bool Music_Content_End = false;
    //Teacher_UI 번호순서대로 콘텐츠 실행 

    // Start is called before the first frame update
    void Start()
    {
        Launcher = GameObject.Find("Launcher").GetComponent<GameLauncher_ICT>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //To_Message
        if (Message_Contents)
            Launcher.Button_Message_Contents();

        if (Message_Contents_Login && Num_contents != -1)
            Launcher.Button_Message_Contents_Select(Num_contents);


        //To_Page
        if (Back)
            Launcher.Button_Back_ToHome();

        if (Back_ToContent)
            Launcher.Button_Back_ToContent();

        if (Back_ToMode)
            Launcher.Button_Back_ToMode();

        if (Setting)
            Launcher.Button_Setting();

        if (Close)
            Launcher.Button_Setting_Close();

        if (Home)
            Launcher.Button_Home();

        if (Mode != -1)
            Launcher.Button_Mode(Mode);

        if (Num_contents_Func != -1)
            Launcher.Run_Contents_Func(Num_contents_Func);

        if (Save_Tool)
            Launcher.Button_Save_Tool();

        if (Tool)
            Launcher.Button_Tool();

        if (Result)
            Launcher.Button_Result();

        if (Music_Content_End)
            Launcher.Button_End_Musiccontent();

        if (Login)
            Launcher.Button_Login();

        if (Survey)
            Launcher.Button_Survey();
    }
}
