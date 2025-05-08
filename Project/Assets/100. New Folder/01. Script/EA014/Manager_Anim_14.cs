using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class Manager_Anim_14 : MonoBehaviour
{
    //[common, EDIT] Manager
    public int Content_Seq = 0;
    private Manager_Text Manager_Text;
    private Manager_Seq_14 Manager_Seq;
    private Manager_Obj_14 Manager_Obj;
    private bool Eng_mode;

    //[common] Camera
    private GameObject Main_Camera;
    private GameObject Camera_position;
    private Sequence[] Camera_seq;
    private int Number_Camera_seq;

    private int round_number;

    private GameObject[] Main_Icon_1_array;
    private GameObject[] Main_Icon_2_array;


    //(확인) 위 메인 카메라와 통합해도 되는지 확인 필요, UI 카메라도 obj에서 할당 필요
    public Camera mainCamera;
    public Camera UICamera;

    //[common] Camera
    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Camera_pos_array;

    void Start()
    {
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );
        }

        if (UICamera != null)
        {
            UICamera.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );
        }

        Camera_position = Manager_Obj_14.instance.Camera_position;
        Main_Camera = Manager_Obj_14.instance.Main_Camera;

        Manager_Seq = Manager_Obj_14.instance.Get_managerseq();
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();

        Init_Seq_camera();
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


    public void Init_Icon_array()
    {
        Main_Icon_1_array = Manager_Obj_14.instance.Main_Shapeicon_1_array;
        //Main_Icon_2_array = Manager_Obj_14.instance.Main_Icon_2_array;
    }

    //[common]
    public void Anim_Active(GameObject obj)
    {
        obj.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => obj.SetActive(true));
    }

    public void Anim_Inactive(GameObject obj)
    {
        obj.transform.DOScale(0, 0.5f).SetEase(Ease.OutElastic).OnComplete(() => obj.SetActive(false));
    }
    public void Anim_Active_shake(GameObject obj)
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(obj.transform.DOScale(0.9f, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => obj.SetActive(true)));
        //seq.Append(obj.transform.DOShakeScale(1f, 1, 10, 90, true).SetEase(Ease.OutQuad)).SetDelay(0.2f);
    }

    public void Active_Effect(int num)
    {
        GameObject obj = Manager_Obj_14.instance.Effect_array[num];

        obj.SetActive(true);
        this.transform.DOShakeScale(4f, 1, 10, 90, true).SetEase(Ease.OutQuad).OnComplete(() => obj.SetActive(false));
    }

    public void Anim_Activestatus(float timer = 2f)
    {
        GameObject obj = Manager_Obj_14.instance.UI_Status;

        obj.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => obj.SetActive(true)).SetDelay(timer);
    }

    public void Activate_emoji(GameObject Shape)
    {
        //자연스럽게 커졌다가 작아지는 부분
        Shape.transform.DOScale(1.5f, 1).SetEase(Ease.OutQuad).OnComplete(() =>
         Shape.transform.DOScale(1f, 0.5f).SetEase(Ease.OutQuad)
         );
    }
    public void Activate_emojitext_popup(GameObject Shape)
    {
        GameObject Selected_shape_text;
        int shape_number;

        //해당하는 번호의 UI 메시지 팝업
        shape_number = Shape.GetComponent<Clicked_Block_14>().Number_shape;
        Selected_shape_text = Manager_Obj_14.instance.Message_array[shape_number];

        Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.Msg_narration[shape_number], 1f);

        Selected_shape_text.SetActive(true);
        Selected_shape_text.transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad);
    }
    public void Inactive_shape_clickable(GameObject Emoji)
    {
        Emoji.GetComponent<Clicked_Block_14>().Inactive_Clickable();
    }

    public void Active_shape_clickable(GameObject Emoji)
    {
        Emoji.GetComponent<Clicked_Block_14>().Active_Clickable();
    }

    //[Animation]
    public void Read_Seq_Shape()
    {
        round_number = 0;
        StartCoroutine(Temp_Message());
    }

    IEnumerator Temp_Message(float time = 2f)
    {
        if (round_number == 5)
        {
            //5, Active next button
            if (Content_Seq == 2)
                Manager_Obj_14.instance.Btn_Next.SetActive(true);

            Sequence seq = DOTween.Sequence();
            seq.Append(Manager_Obj_14.instance.Btn_Next.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic));

            StopCoroutine(Temp_Message(time));
            round_number = 0;

            Anim_Inactive(Manager_Obj_14.instance.Seq_text[Content_Seq]);
        }
        else
        {
            yield return new WaitForSeconds(time);

            GameObject Selected_shape;
            int shape_number;

            Selected_shape = Main_Icon_1_array[round_number].transform.gameObject;
            shape_number = Selected_shape.GetComponent<Clicked_Block_14>().Number_shape;

            //Shake dotween, Active emoji animation
            Activate_emoji(Selected_shape);
            Selected_shape.transform.DOShakeScale(1f, 1, 10, 90, true).SetEase(Ease.OutQuad);
            Activate_emojitext_popup(Selected_shape);
            //this.transform.DOShakeScale(2f, 1, 10, 90, true).SetEase(Ease.OutQuad);

            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.Msg_narration[shape_number], 1f);
            Manager_Seq.Active_shape_clickable(Selected_shape);

            round_number += 1;

            StartCoroutine(Temp_Message(time));
        }
    }
    public void Inactive_Seq_Icon_1()
    {
        GameObject Emoji;

        for (int i = 0; i < 5; i++)
        {
            Emoji = Main_Icon_1_array[i];
            Anim_Inactive(Emoji);
            Manager_Obj_14.instance.Message_array[i].SetActive(false);
        }
    }
    public void Active_Seq_Icon_1()
    {
        GameObject Emoji;

        for (int i = 0; i < 5; i++)
        {
            Emoji = Main_Icon_1_array[i];
            Anim_Active(Emoji);
        }
    }

    int Max_shape = 30;
    int Round_number_shape;
    IEnumerator Setting_icon_2(float time = 0.1f)
    {
        if (Round_number_shape == Max_shape)
        {
            StopCoroutine(Setting_icon_2(time));
        }
        else
        {
            yield return new WaitForSeconds(time);

            GameObject Selected_emoji;

            Selected_emoji = Main_Icon_2_array[Round_number_shape];

            Selected_emoji.SetActive(true);
            Selected_emoji.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic);

            //나중에 효과음 추가 위치
            //Managers.soundManager.Play(SoundManager.Sound.Narration, Manager_obj_4.instance.Msg_narration[emoji_number], 1f);

            Round_number_shape += 1;

            StartCoroutine(Setting_icon_2(time));
        }
    }
    public void Setting_Seq_Eachgame(int round)
    {
        Main_Icon_2_array = Manager_Obj_14.instance.Get_GameShapearray(round);

        Manager_Obj_14.instance.Main_Shapeicon_2[round].SetActive(true);

        Round_number_shape = 0;

        //도형 순차 애니메이션
        StartCoroutine(Setting_icon_2());

        for (int i = 0; i < Main_Icon_2_array.Length; i++)
        {
            int num = Main_Icon_2_array[i].GetComponent<Clicked_Block_14>().Number_shape;

            if (round == num)
            {
               // Main_Icon_2_array[i].transform.DOScale(1.2f, 1f).From(0).SetEase(Ease.OutElastic);
                Manager_Seq.Active_shape_clickable(Main_Icon_2_array[i]);
            }
        }

    }
    //[EDIT] Contents camera sequence
    public void Change_Animation(int Number_seq)
    {
        Content_Seq = Number_seq;
        if (Content_Seq == 2)
        {
            Move_Seq_camera();
            //Debug.Log("SEQ = " + Content_Seq);
        }
    }
}
