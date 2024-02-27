using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_panel_move : MonoBehaviour, IPointerClickHandler
{
    // Start is called before the first frame update
    public GameObject panel;
    private bool flag = false;
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Check");
        if (flag == false)
        {
            panel.GetComponent<Animation>().Play("Seq_board_on");
            
            flag = true;
        }else if (flag == true)
        {
            panel.GetComponent<Animation>().Play("Seq_board_off");
            flag = false;
        }
    }
}