using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicked_EMOJI : MonoBehaviour
{
    public int Number_emoji;
    public int Number_table;

    private Manager_SEQ_5 Manager_Seq;

    public bool Clickable = true;

    //[common] 3d game => SEQ : OnRaySynced() , 2d game => SEQ : Click()
    public void Click()
    {
        //Debug.Log("Clicked");
        if (Clickable)
        {
            Manager_Seq = Manager_obj_5.instance.Get_managerseq();
            //Manager_Seq.Click(this.gameObject, Number_emoji, Number_table);
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
