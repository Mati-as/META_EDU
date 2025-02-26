using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Clicked_emoji : MonoBehaviour, IPointerEnterHandler
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

    //그래픽 레이캐스트로 구현 필요
    public void Click()
    {
        //Debug.Log("Clicked");
        if (Clickable)
        {
            Manager_Seq = Manager_obj_4.instance.Get_managerseq();
            //이모지로 바뀌면 그에 따라서 수정 필요
            //Manager_Seq.Click(this.gameObject, Number_fruit, Number_table);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("클릭 이벤트 확인");
    }
}
