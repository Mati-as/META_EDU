using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager_anim_4 : MonoBehaviour
{
    //[common, EDIT] Manager
    public int Content_Seq = 0;

    private Manager_Text Manager_Text;
    private Manager_SEQ_4 Manager_Seq;
    private Manager_obj_4 Manager_Obj;
    private bool Eng_mode;

    //[common] Camera
    private GameObject Main_Camera;
    private GameObject Camera_position;
    private Sequence[] Camera_seq;
    private int Number_Camera_seq;


    //[EDIT]
    private GameObject Fruit_position;

    private GameObject Main_Icon_1;
    private GameObject[] Main_Icon_1_array;
    private GameObject Main_Icon_2;
    private GameObject[] Main_Icon_2_array;
    private GameObject Main_Icon_3;
    private GameObject[] Main_Icon_3_array;
    private GameObject Icon_buttion_position;


    private GameObject Selected_fruit;
    private int fruit_number;
    private int round_number;
    private int Box_number;

    private Sequence[] Reveal_f_seq;

    //[EDIT] object position transform
    private Transform[] B_p0;
    private Transform B_p1;
    private Transform B_p2;

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Camera_pos_array;
    public GameObject[] Fruit_pos_array;
    public GameObject[] Box_pos_array;

    void Start()
    {
        Camera_position = Manager_obj_4.instance.Camera_position;
        Main_Camera = Manager_obj_4.instance.Main_Camera;
        //Fruit_position = Manager_obj_4.instance.Fruit_position;
        //Box_position = Manager_obj_4.instance.Box_position;
        //Main_Box = Manager_obj_4.instance.Main_Box;

        Manager_Seq = Manager_obj_4.instance.Get_managerseq();
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();

        Init_Seq_camera();
        //Init_Seq_fruit();

    }

    //[common] Camera
    void Init_Seq_camera()
    {
        Camera_pos_array = new GameObject[Camera_position.transform.childCount];
        Camera_seq = new Sequence[Camera_position.transform.childCount];
        Number_Camera_seq = 0;

        for (int i = 0; i < Camera_position.transform.childCount; i++)
        {
            Camera_pos_array[i] = Camera_position.transform.GetChild(i).gameObject;

            Camera_seq[i] = DOTween.Sequence();

            Transform pos = Camera_position.transform.GetChild(i).transform;
            Camera_seq[i].Append(Main_Camera.transform.DORotate(pos.transform.rotation.eulerAngles, 1f));
            Camera_seq[i].Join(Main_Camera.transform.DOMove(pos.transform.position, 1f).SetEase(Ease.InOutQuad));
            Camera_seq[i].Pause();


            //Debug.Log("length " + Camera_seq.Length);
        }
    }
    public void Move_Seq_camera()
    {
        Camera_seq[Number_Camera_seq].Play();
        Number_Camera_seq++;
        //Debug.Log("C_SEQ = " + Number_Camera_seq);
    }

    //[EDIT] Contents camera sequence
    public void Change_Animation(int Number_seq)
    {
        Content_Seq = Number_seq;
        if (Content_Seq == 11 || Content_Seq == 12 || Content_Seq == 17)
        {
            Move_Seq_camera();
            //Debug.Log("SEQ = " + Content_Seq);
        }
    }

    public void Init_Icon_array()
    {
        Main_Icon_1_array = Manager_obj_4.instance.Main_Icon_1_array;
        Main_Icon_2_array = Manager_obj_4.instance.Main_Icon_2_array;

        //근데 3번은 아마도 재생성할듯
        //Main_Icon_3_array = Manager_obj_4.instance.Main_Icon_3_array;
        Icon_buttion_position = Manager_obj_4.instance.Icon_buttion_position;
    }
    public void Activate_all_emoji1()
    {
        //Icon_1에 있는 이모지 전부 받아와서
        //전부 활성화
        GameObject emoji;
        for (int i = 0; i < Main_Icon_1_array.Length; i++)
        {
            emoji = Main_Icon_1_array[i].transform.GetChild(0).gameObject;
            Animator animator = emoji.GetComponent<Animator>();

            animator.SetBool("ON", true);
        }
    }
    public void Inactivate_all_emoji1()
    {
        //Icon_1에 있는 이모지 전부 받아와서
        //전부 활성화
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
        Manager_Seq = Manager_obj_4.instance.Get_managerseq();
        Animator animator = emoji.GetComponent<Animator>();

        animator.SetBool("ON", true);
    }
    public void Inactivate_emoji(GameObject Emoji)
    {
        GameObject emoji;
        emoji = Emoji.transform.GetChild(0).gameObject;
        Manager_Seq = Manager_obj_4.instance.Get_managerseq();
        Animator animator = emoji.GetComponent<Animator>();

        animator.SetBool("ON", false);
    }
    public void Activate_emojitext_popup(GameObject Emoji)
    {
        GameObject Selected_emoji_text;
        Selected_emoji_text = Emoji.transform.GetChild(1).gameObject;
        //(구현필요) 해당 하는 이모지 나레이션 재생 필요

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
    public void Read_Seq_Emoji()
    {

        //그냥 해당 이모지 활성화
        //해당 이모지 텍스트 팝업, 나레이션 재생
        //다 끝나면 다음버튼 활성화
        round_number = 0;

        StartCoroutine(Temp_Message());
    }

    IEnumerator Temp_Message(float time = 2f)
    {
        if (round_number == 5)
        {
            //5, Active next button
            Manager_obj_4.instance.Btn_Next.SetActive(true);

            Sequence seq = DOTween.Sequence();
            seq.Append(Manager_obj_4.instance.Btn_Next.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic));

            StopCoroutine(Temp_Message(time));
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


            Managers.soundManager.Play(SoundManager.Sound.Narration, Manager_obj_4.instance.Msg_narration[emoji_number], 1f);
            Manager_Seq.Active_emoji_clickable(Selected_emoji);

            round_number += 1;

            StartCoroutine(Temp_Message(time));

            //Manager_Text.Changed_UI_message_c3(round_number + 7, fruit_number, Eng_mode); // �� ���� �������� �ʱ�ȭ

        }
    }


    //public Transform [] Get_Fp2(int round)
    //{
    //    return F_p2[num];
    //}
}
