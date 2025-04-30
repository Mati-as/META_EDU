using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicked_animal : MonoBehaviour
{
    public int Number_animal;
    public int Clicked_number;
    private Manager_Seq_2 Manager_Seq;

    public bool Clickable = true;
    // Start is called before the first frame update
    void Start()
    {
        Clicked_number = 0;
        //Manager_Seq = Manager_obj_2.instance.Get_managerseq();
    }


    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    //Debug.Log("CLECKED");
    //    Manager_Seq_2.instance.animal_click(Number_animal);
    //}
    public void Click()
    {
        if (Clickable)
        {
            Manager_Seq = Manager_obj_2.instance.Get_managerseq();
            Manager_Seq.animal_click(Number_animal);
        }
    }

    public void Set_Clickednumber()
    {
        Clicked_number += 1;
    }
    public int Get_Clickednumber()
    {
        return Clicked_number;
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
