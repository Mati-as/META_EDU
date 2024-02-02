using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_Act : MonoBehaviour, IPointerClickHandler
{
    private GameObject Launcher;
    public bool GameStart = false;
    public int Contents = -1;
    public bool Setting = false;
    public bool Close = false;

    public bool Back = false;
    // Start is called before the first frame update
    void Start()
    {
        Launcher = GameObject.Find("Launcher");
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameStart)
            Launcher.GetComponent<GameLauncher>().Button_Gamestart();

        if (Contents != -1)
        {
            Launcher.GetComponent<GameLauncher>().Button_Contents(Contents);
        }

        if (Setting)
            Launcher.GetComponent<GameLauncher_ICT>().Button_Setting();

        if (Close)
            Launcher.GetComponent<GameLauncher_ICT>().Button_Setting_Close();

        //if (Back)
            //Launcher.GetComponent<GameLauncher_ICT>().Button_Back_ToMode();

        //함수 실행
    }
}
