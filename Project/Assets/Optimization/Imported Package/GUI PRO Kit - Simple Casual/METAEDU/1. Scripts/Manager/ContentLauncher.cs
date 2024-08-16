using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class ContentLauncher : MonoBehaviour
{
    public GameObject UI_Page;
    private GameObject Loading;
    private GameObject Home;
    private GameObject Setting;
    private GameObject Login;
    private GameObject Tool;
    private GameObject Result;
    private GameObject Contents;
    private GameObject Mode;
    private GameObject Survey;
    private GameObject Monitoring_Music;

    public GameObject Message_UI;
    private GameObject Message_YESNO;
    private GameObject Message_Intro;
    private GameObject Message_OK;
    private Message_anim_controller MAC;

    private GameObject Prev_page;
    private GameObject Next_page;
    private bool Is_Toolsaved = false;


    // Start is called before the first frame update
    [Header("[LOADING PAGE COMPONENT]")]
    [SerializeField]
    public Slider progressBar;
    public Text loadingPercent;
    public Image loadingIcon;

    private bool loadingCompleted;
    private int nextScene;

    public int Session;


    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(LoadScene());
        StartCoroutine(RotateIcon());

        loadingCompleted = false;
        nextScene = 0;
        Init_page();
    }

    IEnumerator LoadScene()
    {
        //yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;
        //while (!op.isDone)
        while (true)
        {
            //yield return null;

            timer += Time.deltaTime;

            if (op.progress >= 0.9f)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);
                loadingPercent.text = "progressBar.value";

                if (progressBar.value == 1.0f)
                    op.allowSceneActivation = true;
            }
            else
            {
                progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
                if (progressBar.value >= op.progress)
                {
                    timer = 0f;

                    //End of scene index
                    if (nextScene == 2 && loadingCompleted)
                    {
                        StopAllCoroutines();
                    }
                }
            }
        }
    }

    IEnumerator RotateIcon()
    {
        float timer = 0f;
        while (true)
        {
            yield return new WaitForSeconds(0.01f);
            timer += Time.deltaTime;

            //Debug.Log(progressBar.value);
            //Debug.Log("check");
            if (progressBar.value < 100f)
            {
                progressBar.value = Mathf.RoundToInt(Mathf.Lerp(progressBar.value, 100f, timer / 8));
                loadingIcon.rectTransform.Rotate(new Vector3(0, 0, 100 * Time.deltaTime));
                loadingPercent.text = progressBar.value.ToString();
            }
            else
            {
                StopAllCoroutines();
                //Debug.Log("100%");

                Next_page = Home;
                UI_change();

                //Loading.SetActive(false);
                ////Mode.SetActive(true);
                //Home.SetActive(true);
            }
        }
    }

    public void UI_change()
    {
        GameObject page;
        for (int i = 0; i < UI_Page.transform.childCount; i++)
        {
            page = UI_Page.transform.GetChild(i).gameObject;
            if (page.gameObject.activeSelf)
            {
                Prev_page = page.gameObject;
                //Debug.Log(Prev_page);
            }
        }
        Prev_page.SetActive(false);
        Next_page.SetActive(true);

        //씬이 전환될 경우 Nextpage는 실행하지 않음
    }

    public void Button_Save_Tool()
    {
        //저작도구 저장여부
        Is_Toolsaved = true;

        Next_page = Home;
        UI_change();
    }
    public void Button_Back_ToHome()
    {
        Next_page = Home;
        UI_change();
    }
    public void Button_Back_ToContent()
    {
        Next_page = Contents;
        UI_change();
    }
    public void Button_Back_ToMode()
    {
        Next_page = Mode;
        UI_change();
    }
    public void Button_Setting()
    {
        Setting.SetActive(true);
    }

    public void Button_Setting_Close()
    {
        Setting.SetActive(false);
    }

    public void Button_Home()
    {
        //콘텐츠 실행 중일 경우 해당 콘텐츠 비활성화 기능 구현 필요
        Next_page = Home;
        UI_change();
    }
    public void Button_Tool()
    {
        Next_page = Tool;
        UI_change();
    }
    public void Button_Result()
    {
        Next_page = Result;
        UI_change();
        Manager_Result.instance.Refresh_data();
    }
    public void Button_Contents()
    {
        Next_page = Mode;
        UI_change();
    }
    public void Button_Mode(char char_mode)
    {
        //0 : Music, 1 : Contents
        if (char_mode == 'A')
        {
            Next_page = Contents;
            UI_change();
        }
        else if (char_mode == 'B')
        {
            //Run_Contents();
            Message_OK.SetActive(true);
            MAC.Change_text("준비 중 입니다");
            MAC.Change_text_sub(" 확인 버튼을 눌러주세요");
        }
        else if (char_mode == 'C')
        {
            //Run_Contents();
            Message_OK.SetActive(true);
            MAC.Change_text("준비 중 입니다");
            MAC.Change_text_sub(" 확인 버튼을 눌러주세요");
        }
    }
    public void Button_BackToMainscene(int page)
    {
        //0 : Home, 1 : Content
        SceneManager.LoadScene(SceneManager.sceneCountInBuildSettings - 1);
        //Debug.Log(SceneManager.sceneCountInBuildSettings - 1);

        if (page == 0)
        {
            Next_page = Home;
            UI_change();
        }
        else if (page == 1)
        {
            Next_page = Contents;
            UI_change();
        }
    }

    public void Run_Mode(int contentname)
    {
        Session = contentname;

        Next_page = Mode;
        UI_change();
    }

    public void Run_Contents()
    {
        //상태 반환
        Is_Toolsaved = false;

        //Next_page = Monitoring_C4;
        //MAC.Change_text("(테스트)친구들 옥수수에 대해 알아볼까요?");
        //MAC.Animation_On_Off();

        Next_page = Contents;
        UI_change();
        //SceneManager.LoadSceneAsync(1);
    }
    public void Run_Contents_Func(int content_func)
    {
        //Test
        if (content_func == 2)
        {
            //Debug.Log("CONTENT 2");
            Message_OK.SetActive(true);
            MAC.Change_text("준비 중 입니다");
            MAC.Change_text_sub(" 확인 버튼을 눌러주세요");
        }
        else if (content_func == 3)
        {
            Message_OK.SetActive(true);
            MAC.Change_text("준비 중 입니다");
            MAC.Change_text_sub(" 확인 버튼을 눌러주세요");
        }
        else
        {
            SceneManager.LoadScene(content_func);
            Next_page.SetActive(false);
        }
    }


    //저작 도구, 학생 저장 데이터 확인
    public void Button_Message_Contents()
    {
        if (Is_Toolsaved)
        {
            Button_Contents();
        }
        else
        {
            //Message_Tool.SetActive(true);
        }
    }
    public void Button_Message_Contents_Select(int Num_content)
    {
        Run_Mode(Num_content);
    }
    public void Button_Login()
    {
        Login.SetActive(true);
    }
    public void Button_Survey()
    {
        bool Is_Logindatasaved = Manager_login.instance.Get_Islogindatasaved();

        if (Is_Logindatasaved)
        {
            Survey.SetActive(true);
            Manager_Survey.instance.Init_Survey();
        }
        else
        {
            Message_OK.SetActive(true);
            //텍스트 변경 함수 추가
        }
    }

    public void Button_Message_Login_SelectedStudentCheck()
    {
        bool Is_Studentdatasaved = Manager_login.instance.Get_Is_StudentDataSelected();

        if (Is_Studentdatasaved)
        {
            Message_OK.SetActive(true);
            //텍스트 변경 함수 추가
        }
        else
        {
            Message_OK.SetActive(true);
            //텍스트 변경 함수 추가
        }
    }
    public void Button_Message_Login_StudentNotSelect()
    {
        Message_OK.SetActive(true);
        //텍스트 변경 함수 추가
    }
    public void Button_Message_Login_StudentDataSaved()
    {
        Message_OK.SetActive(true);
        //텍스트 변경 함수 추가
    }
    public void Button_Message_Login_FieldEmpty()
    {
        Message_OK.SetActive(true);
        //텍스트 변경 함수 추가
    }


    public void Save_Data()
    {
        DialogueData Saved_data = new DialogueData();

        Saved_data.ID = Manager_login.instance.ID;
        Saved_data.Name = Manager_login.instance.Name;
        Saved_data.Birth_date = Manager_login.instance.Birthdate;
        Saved_data.Date = Manager_login.instance.Date;
        Saved_data.Session = Manager_login.instance.Session.ToString();
        Saved_data.Data_1 = Manager_login.instance.Data_1;
        Saved_data.Data_2 = Manager_login.instance.Data_2;
        Manager_Result.instance.Add_data(Saved_data);
        Manager_Result.instance.Write();

    }
    void Dummy_setting_content()
    {
        //콘텐츠 실행
    }
    void Dummy_setting_content_Func()
    {
        //콘텐츠 내부 기능 실행
    }

    void Init_page()
    {
        Loading = UI_Page.transform.GetChild(0).gameObject;
        Home = UI_Page.transform.GetChild(1).gameObject;
        Result = UI_Page.transform.GetChild(2).gameObject;
        Mode = UI_Page.transform.GetChild(3).gameObject;
        Contents = UI_Page.transform.GetChild(4).gameObject;

        Setting = UI_Page.transform.GetChild(5).gameObject;
        Login = UI_Page.transform.GetChild(6).gameObject;
        Survey = UI_Page.transform.GetChild(7).gameObject;

        Message_YESNO = Message_UI.transform.GetChild(0).gameObject;
        Message_OK = Message_UI.transform.GetChild(1).gameObject;
        Message_Intro = Message_UI.transform.GetChild(2).gameObject;

        //Message_Intro setting, Inspector에서 scale 0,0,0으로 변경
        //Message_Intro.SetActive(true);
        MAC = Message_OK.GetComponent<Message_anim_controller>();

    }

    public void UI_Back()
    {
        Prev_page.SetActive(true);
        Next_page.SetActive(false);
    }
}
