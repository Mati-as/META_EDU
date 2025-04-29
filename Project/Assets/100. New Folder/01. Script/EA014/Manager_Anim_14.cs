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
    void Start()
    {
        //Camera_position = Manager_obj_4.instance.Camera_position;
        //Main_Camera = Manager_obj_4.instance.Main_Camera;

        Manager_Seq = Manager_Obj_14.instance.Get_managerseq();
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
}
