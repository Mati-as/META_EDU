using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Button_Development_Menu : MonoBehaviour, IPointerClickHandler
{
    public RplidarTest_Ray rplidar;

    public bool UI_ONOFF = false;
    public bool BALL_ONOFF = false;
    public bool SF_ONOFF = false;

    private Text txt;
    public void Start()
    {
        txt = this.transform.GetChild(0).gameObject.GetComponent<Text>();
        
        // 2.28 임시로 코드 추가 (민석)
        rplidar = GameObject.FindWithTag("RaySynchronizer").GetComponent<RplidarTest_Ray>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Check_text();
    }

    void Check_text()
    {
        if (UI_ONOFF)
        {
            if (rplidar.UI_Active_ONOFF())
            {
                txt.text = "UI - ON";
            }
            else
            {
                txt.text = "UI - OFF";
            }
        }
        else if (BALL_ONOFF)
        {
            if (rplidar.Ball_Active_ONOFF())
            {
                txt.text = "BALL - ON";
            }
            else
            {
                txt.text = "BALL - OFF";
            }
        }
        else if (SF_ONOFF)
        {

            if (rplidar.SF_Active_ONOFF())
            {
                txt.text = "SensorFilter - ON";
            }
            else
            {
                txt.text = "SensorFilter - OFF";
            }
        }
    }
}
