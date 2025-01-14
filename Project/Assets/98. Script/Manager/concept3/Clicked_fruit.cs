using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Clicked_fruit : MonoBehaviour
{
    public int Number_fruit;
    public int Number_table;


    private Manager_Seq_3 Manager_Seq;
    // Start is called before the first frame update
    void Start()
    {
        Manager_Seq = Manager_obj_3.instance.Get_managerseq();
    }

    

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CLECKED");
       // Manager_Seq_3.instance.Click(this.gameObject, Number_fruit, Number_table);
    }
    public void Click()
    {
        Manager_Seq.Click(this.gameObject, Number_fruit, Number_table);
    }

    public void Set_Number_fruit(int num)
    {
        Number_fruit = num;
    }
    public void Set_Number_table(int num)
    {
        Number_table = num;

    }
}
