using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Footstep_click : MonoBehaviour, IPointerClickHandler
{
    //해당 동물 마지막 발판인지 여부 확인
    public bool Last_footstep = false;

    //챕터(BG) 마지막 발판인지 여부 확인
    public bool Last_chapfootstep = false;

    private GameObject Gamemanager;

    public void OnPointerClick(PointerEventData eventData)
    {
       //Debug.Log("CLICK");
       //동물, 발판 제어 파트
       if(Last_footstep)
        {
            //다음 동물
            Gamemanager.GetComponent<S1_Controller>().ActNextchapter();
        }
        else
        {
            //다음 스탭
            Gamemanager.GetComponent<S1_Controller>().ActNextstep();
        }

        //UI 제어 파트
        if (Last_chapfootstep)
        {
            Gamemanager.GetComponent<S1_Controller>().ActNextlevel();
        }

    }

    void Start()
    {
        Gamemanager = GameObject.Find("Gamemanager");
    }
}
