using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Clicked_emoji : MonoBehaviour
{
    public int Number_emoji;
    public int Number_table;

    private Manager_SEQ_4 Manager_Seq;

    public bool Clickable = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    Debug.Log("CLECKED");
    //   // Manager_Seq_3.instance.Click(this.gameObject, Number_fruit, Number_table);
    //}

    //[common] 3d 일 경우 raysync 안에, 2d 일경우 버튼 컴포넌트 이벤트 안에
    public void Click()
    {
        //Debug.Log("Clicked");
        if (Clickable)
        {
            Manager_Seq = Manager_obj_4.instance.Get_managerseq();
            Manager_Seq.Click(this.gameObject, Number_emoji, Number_table);
        }
    }

    public void Set_Number_emoji(int num)
    {
        Number_emoji = num;
    }
    public void Set_Number_table(int num)
    {
        Number_table = num;
    }

    public void Active_Clickable()
    {
        Clickable = true;
    }
    public void Inactive_Clickable()
    {
        Clickable = false;
    }
}
