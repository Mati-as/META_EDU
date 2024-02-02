using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_Launcher_Survey : MonoBehaviour, IPointerClickHandler
{
    private GameLauncher_ICT Launcher;

    public bool Prev = false;
    public bool Next = false;
    public bool Submit = false;
    public bool SurveyStart = false;
    public int Answer = -1;

    void Start()
    {
        Launcher = GameObject.Find("Launcher").GetComponent<GameLauncher_ICT>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {

        //Music content
        if (Prev)
            Manager_Survey.instance.Button_Prev();

        if (Next)
            Manager_Survey.instance.Button_Next();

        if (Submit)
            Manager_Survey.instance.Button_Submit();

        if (SurveyStart)
            Manager_Survey.instance.Button_Start();

        if (Answer!=-1)
            Manager_Survey.instance.Button_Answer(Answer);

    }
}
