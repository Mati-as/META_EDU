using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_Message : MonoBehaviour, IPointerClickHandler
{
    // Start is called before the first frame update

    private GameLauncher_ICT Launcher;

    public bool Contents = false;
    public bool Login_Setting = false;
    public bool Login_SaveCheck = false;
    public bool Login_DeleteCheck = false;
    void Start()
    {
        Launcher = GameObject.Find("Launcher").GetComponent<GameLauncher_ICT>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Contents)
            Launcher.Button_Contents();

        if (Login_Setting)
            Manager_login.instance.Setting_StudentInfo();

        if (Login_DeleteCheck)
            Manager_login.instance.Delete_StudentData();
    }
}
