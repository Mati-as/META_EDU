using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Manager_anim_5 : MonoBehaviour
{
    //[common, EDIT] Manager
    public int Content_Seq = 0;
    private Manager_Text Manager_Text;
    private Manager_SEQ_5 Manager_Seq;
    private Manager_obj_5 Manager_Obj;
    private bool Eng_mode;

    //[common] Camera
    private GameObject Main_Camera;
    private GameObject Camera_position;
    private Sequence[] Camera_seq;
    private int Number_Camera_seq;


    //[EDIT]
    private Tween[] Emoji_seq_loop;

    private GameObject Fruit_position;

    private GameObject Main_Icon_1;
    private GameObject[] Main_Icon_1_array;
    private GameObject Main_Icon_2;
    private GameObject[] Main_Icon_2_array;
    private GameObject Main_Icon_3;
    private GameObject[] Main_Icon_3_array;
    private GameObject Icon_buttion_position;


    private int round_number;

    //[EDIT] object position transform

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Camera_pos_array;
    public GameObject[] Fruit_pos_array;
    public GameObject[] Box_pos_array;

    void Start()
    {
        //Camera_position = Manager_obj_4.instance.Camera_position;
        //Main_Camera = Manager_obj_4.instance.Main_Camera;

        Manager_Seq = Manager_obj_5.instance.Get_managerseq();
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();

        //Init_Seq_camera();
    }

    //[common] Camera
    //void Init_Seq_camera()
    //{
    //    Camera_pos_array = new GameObject[Camera_position.transform.childCount];
    //    Camera_seq = new Sequence[Camera_position.transform.childCount];
    //    Number_Camera_seq = 0;

    //    for (int i = 0; i < Camera_position.transform.childCount; i++)
    //    {
    //        Camera_pos_array[i] = Camera_position.transform.GetChild(i).gameObject;

    //        Camera_seq[i] = DOTween.Sequence();

    //        Transform pos = Camera_position.transform.GetChild(i).transform;
    //        Camera_seq[i].Append(Main_Camera.transform.DORotate(pos.transform.rotation.eulerAngles, 1f));
    //        Camera_seq[i].Join(Main_Camera.transform.DOMove(pos.transform.position, 1f).SetEase(Ease.InOutQuad));
    //        Camera_seq[i].Pause();


    //        //Debug.Log("length " + Camera_seq.Length);
    //    }
    //}
    public void Move_Seq_camera()
    {
        Camera_seq[Number_Camera_seq].Play();
        Number_Camera_seq++;
        //Debug.Log("C_SEQ = " + Number_Camera_seq);
    }

    //[common]
    public void Anim_Active(GameObject obj)
    {
        //Sequence seq = DOTween.Sequence();
        //seq.Append(obj.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => obj.SetActive(true))).SetDelay(2f);
        obj.transform.DOScale(1, 1f).SetEase(Ease.OutElastic).OnComplete(() => obj.SetActive(true));
    }

    public void Anim_Inactive(GameObject obj)
    {
        obj.transform.DOScale(0, 0.5f).SetEase(Ease.OutElastic).OnComplete(() => obj.SetActive(false));
    }

    //[EDIT] Contents camera sequence
    public void Change_Animation(int Number_seq)
    {
        Content_Seq = Number_seq;
        //if (Content_Seq == 11 || Content_Seq == 12 || Content_Seq == 17)
        //{
        //    Move_Seq_camera();
        //    //Debug.Log("SEQ = " + Content_Seq);
        //}
    }

    public void Init_Icon_array()
    {
        Main_Icon_1_array = Manager_obj_5.instance.Main_Icon_1_array;
        //Main_Icon_2_array = Manager_obj_5.instance.Main_Icon_2_array;

        Icon_buttion_position = Manager_obj_5.instance.Icon_buttion_position;
    }
    public void Activate_all_emoji1()
    {
        GameObject emoji;
        Main_Icon_1_array = Manager_obj_5.instance.Main_Icon_1_array;
        for (int i = 0; i < Main_Icon_1_array.Length; i++)
        {
            emoji = Main_Icon_1_array[i].transform.GetChild(0).gameObject;
            Animator animator = emoji.GetComponent<Animator>();

            animator.SetBool("ON", true);
        }
    }
    public void Inactivate_all_emoji1()
    {
        GameObject emoji;
        for (int i = 0; i < Main_Icon_1_array.Length; i++)
        {
            emoji = Main_Icon_1_array[i].transform.GetChild(0).gameObject;
            Animator animator = emoji.GetComponent<Animator>();
            animator.SetBool("ON", false);
        }
    }
    public void Activate_emoji(GameObject Emoji)
    {

        GameObject emoji;
        emoji = Emoji.transform.GetChild(0).gameObject;
        //자연스럽게 커졌다가 작아지는 부분
        emoji.transform.DOScale(1.5f, 1).SetEase(Ease.OutQuad).OnComplete(() =>
         emoji.transform.DOScale(1f, 0.5f).SetEase(Ease.OutQuad)
         );
        Manager_Seq = Manager_obj_5.instance.Get_managerseq();
        Animator animator = emoji.GetComponent<Animator>();

        animator.SetBool("ON", true);
    }
    public void Activate_emoji_forgame(GameObject Emoji)
    {
        GameObject emoji;
        emoji = Emoji.transform.GetChild(0).gameObject;
        Manager_Seq = Manager_obj_5.instance.Get_managerseq();
        Animator animator = emoji.GetComponent<Animator>();

        animator.SetBool("ON", true);
    }
    public void Inactivate_emoji(GameObject Emoji)
    {
        GameObject emoji;
        emoji = Emoji.transform.GetChild(0).gameObject;
        Manager_Seq = Manager_obj_5.instance.Get_managerseq();
        Animator animator = emoji.GetComponent<Animator>();

        animator.SetBool("ON", false);
    }
    public void Activate_emojitext_popup(GameObject Emoji)
    {
        GameObject Selected_emoji_text;
        int emoji_number;
        Selected_emoji_text = Emoji.transform.GetChild(1).gameObject;
        emoji_number = Emoji.GetComponent<Clicked_emoji>().Number_emoji;

        Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_5.instance.Msg_narration[emoji_number], 1f);

        Sequence seq_read = DOTween.Sequence();
        seq_read.Append(Selected_emoji_text.transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad));
    }


    public void Inactive_Seq_Icon_1()
    {
        GameObject Emoji;

        for (int i = 0; i < 5; i++)
        {
            Emoji = Main_Icon_1_array[i];
            Emoji.transform.DOScale(0, 1f).SetEase(Ease.OutElastic);
            Emoji.SetActive(false);
        }
    }
    public void Active_Seq_Icon_1()
    {
        GameObject Emoji;

        for (int i = 0; i < 5; i++)
        {
            Emoji = Main_Icon_1_array[i];
            Emoji.transform.DOScale(1, 1f).SetEase(Ease.OutElastic);
            Emoji.SetActive(true);
        }
    }
    int Max_emoji = 24;
    int Round_number_emoji = 0;
    public void Setting_Seq_Icon_2()
    {
        Main_Icon_2_array = Manager_obj_4.instance.Main_Icon_2_array;

        Manager_obj_4.instance.Main_Icon_2.SetActive(true);

        StartCoroutine(Setting_icon_2());
    }

    //여기에서 전체를 세팅하는거 같음
    IEnumerator Setting_icon_2(float time = 0.2f)
    {
        if (Round_number_emoji == Max_emoji)
        {
            StopCoroutine(Setting_icon_2(time));
        }
        else
        {
            yield return new WaitForSeconds(time);

            GameObject Selected_emoji;

            Selected_emoji = Main_Icon_2_array[Round_number_emoji];

            Selected_emoji.SetActive(true);
            Selected_emoji.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);

            //나중에 효과음 추가한다면 여기에 추가 필요
            //Managers.soundManager.Play(SoundManager.Sound.Narration, Manager_obj_4.instance.Msg_narration[emoji_number], 1f);

            //클릭 할 수 있도록 기능 활성화
            //Manager_Seq.Active_emoji_clickable(Selected_emoji);

            Round_number_emoji += 1;

            StartCoroutine(Setting_icon_2(time));
        }
    }
    //패널, 제시 활성화
    public void Setting_Seq_Eachgame(int round)
    {
        //Main_Icon_3_array = Manager_obj_4.instance.Main_Icon_3_array;

        //Main_Icon_3_array[round].SetActive(true);
        //Main_Icon_3_array[round].transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);
        //this.transform.DOShakeScale(5f, 1, 10, 90, true).SetEase(Ease.OutQuad)
        //    .OnComplete(() => Main_Icon_3_array[round].SetActive(false));

        //그 이전꺼 비활성화 해야하는데
        //화나요 부터 활성화 되어있을 거고 화나요 시작에는 호출 필요가 없음
        if (round >= 1)
        {
            //여기에 그냥 애니메이션으로 구현
            Manager_obj_4.instance.Main_Icon_3_array[round - 1].transform.DOScale(0, 1f).SetEase(Ease.OutElastic);
        }

        for (int i = 0; i < Main_Icon_2_array.Length; i++)
        {
            int num = Main_Icon_2_array[i].GetComponent<Clicked_emoji>().Number_emoji;
            Inactivate_emoji(Main_Icon_2_array[i]);

            if (round == num)
            {
                Main_Icon_2_array[i].transform.DOScale(1.2f, 1f).From(0).SetEase(Ease.OutElastic);
                Activate_emoji_forgame(Main_Icon_2_array[i]);
                Main_Icon_2_array[i].GetComponent<Image>().sprite = Manager_obj_4.instance.Yellow;

                //Manager_Seq.Active_emoji_clickable(Main_Icon_2_array[i]);
            }
        }

    }

    public void Read_Seq_Emoji()
    {
        round_number = 0;
        StartCoroutine(Temp_Message());
    }



    IEnumerator Temp_Message(float time = 2f)
    {
        if (round_number == 6)
        {
            //5, Active next button
            if (Content_Seq == 1)
                Manager_obj_5.instance.Btn_Next.SetActive(true);

            Sequence seq = DOTween.Sequence();
            seq.Append(Manager_obj_5.instance.Btn_Next.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic));

            StopCoroutine(Temp_Message(time));
            round_number = 0;
        }
        else
        {
            yield return new WaitForSeconds(time);

            GameObject Selected_emoji;
            int emoji_number;

            Selected_emoji = Main_Icon_1_array[round_number].transform.gameObject;
            emoji_number = Selected_emoji.GetComponent<Clicked_emoji>().Number_emoji;

            //Shake dotween, Active emoji animation
            Activate_emoji(Selected_emoji);
            //Selected_emoji.transform.DOShakeScale(1f, 1, 10, 90, true).SetEase(Ease.OutQuad);
            Activate_emojitext_popup(Selected_emoji);
            this.transform.DOShakeScale(2f, 1, 10, 90, true).SetEase(Ease.OutQuad).OnComplete(() => Inactivate_emoji(Selected_emoji));


            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_5.instance.Msg_narration[emoji_number], 1f);
            //Manager_Seq.Active_emoji_clickable(Selected_emoji);

            round_number += 1;

            StartCoroutine(Temp_Message(time));
        }
    }
}
