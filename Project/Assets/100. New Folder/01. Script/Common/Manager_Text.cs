using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Manager_Text : MonoBehaviour
{

    private GameObject UI_Text;
    private GameObject UI_Message;
    private GameObject UI_Panel;

    private Sequence Seq_panel;

    [Header("[ COMPONENT CHECK ]")]
    //Common
    public int Content_Seq = 0;
    public int Number_Prev_message = -1;

    public GameObject[] UI_Text_array;
    public GameObject[] UI_Message_array;

    void Start()
    {

    }

    //텍스트 저장
    public void Init_UI_text(GameObject text, GameObject message, GameObject panel)
    {
        UI_Text = text;
        UI_Message = message;
        UI_Panel = panel;

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
    //0507 임시 EA014를 위한 배열 저장 함수
    public void Init_UI_text_array(GameObject[] text)
    {
        if (UI_Text != null)
        {
            UI_Text_array = text;

        }
    }

    //public void Set_Audio_seq_narration(AudioClip[] audio)
    //{
    //    Audio_seq_narration = audio;
    //}
    public void Init_UI_panel(GameObject panel, float time)
    {
        UI_Panel = panel;
        Seq_panel = DOTween.Sequence().SetAutoKill(false);

        Seq_panel.Append(UI_Panel.transform.DOScale(1, 0.1f).From(0));
        Seq_panel.Append(UI_Panel.transform.DOScale(0, 0.1f).From(1).SetDelay(time));
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
        UI_Text_array[Content_Seq].transform.DOScale(0, 0.5f).SetEase(Ease.OutElastic).OnComplete(() => UI_Text_array[Content_Seq].SetActive(false));
    }

    public void Inactive_UI_Text(float timer = 0.5f)
    {
        UI_Text_array[Content_Seq].transform.DOScale(0, 0.5f).SetEase(Ease.OutElastic).OnComplete(() => UI_Text_array[Content_Seq].SetActive(false)).SetDelay(timer);
    }
    public void Active_UI_message(int Number)
    {
        //메시지 보여주는 기능
        //메시지에 대한 나레이션은 해당 메시지 오브젝트에 붙어있다고 가정하였음
        //메시지 나레이션은 obj에서 사전 저장된걸 가져다 줌

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
    public void Changed_UI_message_c3(int Number, int Fruit_num, bool Eng_mode)
    {
        Inactive_UI_Text();
        //메시지 최초 실행시

        GameObject Message_table;
        Message_table = UI_Message_array[Number];

        if (Number_Prev_message != -1)
        {
            UI_Message_array[Number_Prev_message].SetActive(false);
        }

        if (Eng_mode)
        {
            Message_table.transform.GetChild(1).GetComponent<Image>().sprite = Manager_obj_3.instance.Msg_textsprite_eng[Fruit_num];
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_3.instance.Msg_narration_eng[Fruit_num], 1f);
        }
        else
        {
            Message_table.transform.GetChild(1).GetComponent<Image>().sprite = Manager_obj_3.instance.Msg_textsprite[Fruit_num];
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_3.instance.Msg_narration[Fruit_num], 1f);

        }
        UI_Message_array[Number].SetActive(true);
        UI_Message_array[Number].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);
        UI_Message_array[Number].transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic).SetDelay(2f);

        //Debug.Log("텍스트");

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
        UI_Panel.SetActive(true);
        Seq_panel.Restart();
    }
    public void Inactive_UI_Panel()
    {
        UI_Panel.SetActive(false);
    }
}
