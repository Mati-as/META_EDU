using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Clicked_fruit : MonoBehaviour,IPointerClickHandler
{
    public int Number_fruit;
    public int Number_table;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("CLECKED");
        Manager_Seq_3.instance.Click(this.gameObject, Number_fruit, Number_table);
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
