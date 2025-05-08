using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicked_Block_14 : MonoBehaviour
{
    public int Number_shape;
    public int Number_table;

    private Manager_Seq_14 Manager_Seq;

    public bool Clickable = true;

    //[common] 3d game => SEQ : OnRaySynced() , 2d game => SEQ : Click()
    public void Click()
    {
        //Debug.Log("Clicked");
        if (Clickable)
        {
            Manager_Seq = Manager_Obj_14.instance.Get_managerseq();
            Manager_Seq.Click(this.gameObject, Number_shape, Number_table);
        }
    }
    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    Debug.Log("CLECKED");
    //     Manager_Seq_3.instance.Click(this.gameObject, Number_fruit, Number_table);
    //}

    public void Set_Number_emoji(int num)
    {
        Number_shape = num;
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
