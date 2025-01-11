using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEditor.PlayerSettings;

public class Manager_Text : MonoBehaviour
{

    private GameObject UI_Text;
    private GameObject UI_Message;
    private GameObject Panel;

    private Sequence Seq_panel;

    [Header("[ COMPONENT CHECK ]")]
    //Common
    public int Content_Seq = 0;
    public int Number_Prev_message = -1;

    public GameObject[] UI_Text_array;
    public GameObject[] UI_Message_array;

    void Start()
    {
        Init_UI_text();
        Init_UI_Panel(10f);
    }

    //텍스트 저장
    void Init_UI_text()
    {
        UI_Text = Manager_obj_3.instance.UI_Text;
        UI_Message = Manager_obj_3.instance.UI_Message;

        if (UI_Text != null)
        {
            UI_Text_array = new GameObject[UI_Text.transform.childCount];

            for (int i = 0; i < UI_Text.transform.childCount; i++)
            {
                UI_Text_array[i] = UI_Text.transform.GetChild(i).gameObject;
                //Debug.Log(i+"+"+UI_Text_array[i]);
            }
        }

        if (UI_Message != null)
        {
            UI_Message_array = new GameObject[UI_Message.transform.childCount];

            for (int i = 0; i < UI_Message.transform.childCount; i++)
            {
                UI_Message_array[i] = UI_Message.transform.GetChild(i).gameObject;
            }
        }
    }

    //public void Set_Audio_seq_narration(AudioClip[] audio)
    //{
    //    Audio_seq_narration = audio;
    //}
    public void Init_UI_Panel(float time)
    {
        Panel = Manager_obj_3.instance.Panel;
        Seq_panel = DOTween.Sequence().SetAutoKill(false);

        Seq_panel.Append(Panel.transform.DOScale(1, 0.1f).From(0));
        Seq_panel.Append(Panel.transform.DOScale(0, 0.1f).From(1).SetDelay(time));
        //Panel.SetActive(false);
    }
    //텍스트 활성화, 이전 텍스트 비활성화, 애니메이션 재생
    public void Change_UI_text(int Number_seq)
    {
        Content_Seq = Number_seq;
        if (Content_Seq != 0)
        {
            UI_Text_array[Content_Seq - 1].SetActive(false);
        }
        UI_Text_array[Content_Seq].SetActive(true);
        UI_Text_array[Content_Seq].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);
    }

    public void Inactive_UI_Text()
    {
        UI_Text_array[Content_Seq].SetActive(false);
    }

    public void Active_UI_message(int Number)
    {
        Inactive_UI_Text();
        //메시지 최초 실행시
        if (Number_Prev_message != -1)
        {
            UI_Message_array[Number_Prev_message].SetActive(false);
        }
        UI_Message_array[Number].SetActive(true);
        UI_Message_array[Number].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);

        Number_Prev_message = Number;
    }

    //(임시) 3컨셉을 위한 도구
    public void Changed_UI_message_c3(int Number, int Target_num)
    {
        //해당하는 UI 이미지 스프라이트 변경하고, 오디오 소스의 나레이션 변경하고
        Inactive_UI_Text();
        //메시지 최초 실행시
        if (Number_Prev_message != -1)
        {
            UI_Message_array[Number_Prev_message].SetActive(false);
        }
        UI_Message_array[Number].GetComponent<Image>().sprite = Manager_obj_3.instance.Msg_textsprite[Target_num];
        UI_Message_array[Number].GetComponent<AudioSource>().clip = Manager_obj_3.instance.Msg_narration[Target_num];
        UI_Message_array[Number].SetActive(true);
        UI_Message_array[Number].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);
        UI_Message_array[Number].transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic).SetDelay(2f);

        Debug.Log("텍스트");
        //금방 비활성화 해야함

        Number_Prev_message = Number;
    }

    public void Inactive_UI_message(int Number)
    {
        UI_Message_array[Number].SetActive(false);
    }

    public void Inactiveall_UI_message()
    {
        for (int i = 0; i < UI_Message.transform.childCount; i++)
        {
            UI_Message_array[i].SetActive(false);
        }
    }
    public void Active_UI_Panel()
    {
        Panel.SetActive(true);
        Seq_panel.Restart();
    }
    public void Inactive_UI_Panel()
    {
        Panel.SetActive(false);
    }
}
