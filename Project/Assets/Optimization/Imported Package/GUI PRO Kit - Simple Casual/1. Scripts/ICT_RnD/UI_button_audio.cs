using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_button_audio : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Manager_audio.instance.Get_click();
            //Cursor.SetCursor(Manager_image.instance.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Manager_audio.instance.Get_hover();
        //Cursor.SetCursor(Manager_image.instance.Get_hand_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Cursor.SetCursor(Manager_image.instance.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

}
