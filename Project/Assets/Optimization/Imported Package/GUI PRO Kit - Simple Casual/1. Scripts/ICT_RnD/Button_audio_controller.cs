using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_audio_controller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Manager_audio.instance.Get_click();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
