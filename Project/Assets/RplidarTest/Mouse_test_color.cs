using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Mouse_test_color : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        this.gameObject.GetComponent<Image>().color = Color.blue;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.gameObject.GetComponent<Image>().color = Color.red;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.gameObject.GetComponent<Image>().color = Color.white;
    }
}
