using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Devmenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    bool Inactive_click = true;
    public int Check_click = 0;
    //커서기능만 살려둠
    public GameObject Btn_SensorSettings;
    public GameObject Btn_TestBuild;

    public void Awake()
    {
        Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
        
        //(임시) 해당 버튼 비활성화 기능 추가
        Btn_SensorSettings.SetActive(false);
        Btn_TestBuild.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);

            if (Inactive_click)
            {
                Check_click += 1;
                Check_DevMenu();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        //Cursor.SetCursor(Managers.CursorImage.Get_hand_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
      //  Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    void Check_DevMenu()
    {
        if (Check_click == 5)
        {
            Btn_SensorSettings.SetActive(true);
            Btn_TestBuild.SetActive(true);
            Inactive_click = false;
        }
    }
}