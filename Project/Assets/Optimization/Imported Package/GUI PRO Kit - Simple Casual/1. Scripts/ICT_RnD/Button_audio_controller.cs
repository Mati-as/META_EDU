using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_audio_controller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void Awake()
    {
        Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       
        Cursor.SetCursor(Managers.CursorImage.Get_hand_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

}
