using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_button_SD : MonoBehaviour, IPointerClickHandler
{
    public Text Student_Name;
    public string Student;
    public int Result_num = -1;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Result_num != -1)
            {
                Manager_Result.instance.Change_result(Result_num);
            }
        }
    }

    void Start() {
        Student_Name.text = Student;
    }
}
