using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Footstep_click : MonoBehaviour, IPointerClickHandler
{
    //�ش� ���� ������ �������� ���� Ȯ��
    public bool Last_footstep = false;

    //é��(BG) ������ �������� ���� Ȯ��
    public bool Last_chapfootstep = false;

    private GameObject Gamemanager;

    //25.02.21 S1_controller ������ ���� �ּ�ó����
    public void OnPointerClick(PointerEventData eventData)
    {
       //Debug.Log("CLICK");
       //����, ���� ���� ��Ʈ
       if(Last_footstep)
        {
            //���� ����
            //Gamemanager.GetComponent<S1_Controller>().ActNextchapter();
        }
        else
        {
            //���� ����
            //Gamemanager.GetComponent<S1_Controller>().ActNextstep();
        }

        //UI ���� ��Ʈ
        if (Last_chapfootstep)
        {
           // Gamemanager.GetComponent<S1_Controller>().ActNextlevel();
        }

    }

    void Start()
    {
        Gamemanager = GameObject.Find("Gamemanager");
    }
}
