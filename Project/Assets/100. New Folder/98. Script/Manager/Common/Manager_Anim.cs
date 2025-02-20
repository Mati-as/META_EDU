using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager_Anim : MonoBehaviour
{
    //Common
    public int Content_Seq = 0;

    //Camera
    public GameObject Main_Camera;
    public GameObject Camera_position;
    private Sequence[] Camera_seq;
    private int Number_Camera_seq;


    //Animal
    public GameObject Main_Penguin;
    private GameObject[] Main_Penguin_array;
    public GameObject Penguin_position;

    //Sleigh
    public GameObject Sleigh;
    public GameObject Sleigh_position;
    //���, in sequence �迭, out sequence �迭�� ����
    //0~6��
    private Sequence[] In_p_seq;
    private Sequence[] Out_p_seq;

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Camera_pos_array;
    public GameObject[] Penguin_pos_array;

    void Start()
    {
        Init_Seq_camera();
        Init_Seq_penguin();
        Shake_Seq_sleigh();
    }
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

    void Init_Seq_penguin()
    {
        Penguin_pos_array = new GameObject[Penguin_position.transform.childCount];
        Main_Penguin_array = new GameObject[Main_Penguin.transform.childCount];
        In_p_seq = new Sequence[Penguin_position.transform.childCount];
        Out_p_seq = new Sequence[Penguin_position.transform.childCount];

        //in.out ������ ���� �ѹ��� �ʱ�ȭ
        //���� �̵� ��� �Ҵ�
        for (int i = 0; i < Penguin_position.transform.childCount; i++)
        {
            Penguin_pos_array[i] = Penguin_position.transform.GetChild(i).gameObject;
            Main_Penguin_array[i] = Main_Penguin.transform.GetChild(i).gameObject;

            In_p_seq[i] = DOTween.Sequence().SetAutoKill(false);
            Out_p_seq[i] = DOTween.Sequence().SetAutoKill(false);

            Transform p1 = Penguin_pos_array[i].transform.GetChild(0).transform;
            Transform p2 = Penguin_pos_array[i].transform.GetChild(1).transform;
            Transform p3 = Penguin_pos_array[i].transform.GetChild(2).transform;
            Transform p4 = Penguin_pos_array[i].transform.GetChild(3).transform;


            //�ִϸ��̼��� ���� �� �ڿ������� ���� �ʿ�� ����
            In_p_seq[i].Append(Main_Penguin_array[i].transform.DOMove(p2.transform.position, 1f).SetEase(Ease.InOutQuad));
            In_p_seq[i].Join(Main_Penguin_array[i].transform.DORotate(p2.transform.rotation.eulerAngles, 1f));
            In_p_seq[i].Append(Main_Penguin_array[i].transform.DOJump(p3.transform.position, 1f, 1, 1f));
            In_p_seq[i].Join(Main_Penguin_array[i].transform.DORotate(p3.transform.rotation.eulerAngles, 1f));


            Out_p_seq[i].Append(Main_Penguin_array[i].transform.DOJump(p4.transform.position, 1f, 1, 1f));
            Out_p_seq[i].Join(Main_Penguin_array[i].transform.DORotate(p4.transform.rotation.eulerAngles, 1f));
            Out_p_seq[i].Append(Main_Penguin_array[i].transform.DOMove(p1.transform.position, 1f).SetEase(Ease.InOutQuad));
            Out_p_seq[i].Join(Main_Penguin_array[i].transform.DORotate(p1.transform.rotation.eulerAngles, 1f));

            In_p_seq[i].Pause();
            Out_p_seq[i].Pause();


            //Debug.Log("length " + Camera_seq.Length);
        }
    }

    
    void Move_Seq_camera()
    {
        //�Լ��� ���� �����ϴ°� ����
        //Ŭ���� �Ǹ� �ش��ϴ� ����� ������ �ſ����ϱ�
        Camera_seq[Number_Camera_seq].Play();
        Number_Camera_seq++;
        //Debug.Log("C_SEQ = " + Number_Camera_seq);
    }

    public void Shake_Seq_sleigh()
    {
        Sequence Shake = DOTween.Sequence();
        Shake.Append(Sleigh.transform.DOShakeScale(1,1,10,90,true).SetEase(Ease.OutQuad));
        //Shake.Append(Sleigh.transform.DOShakePosition(1,1,10,1,false,true).SetEase(Ease.InOutQuad));
        //��鸮�� �ִϸ��̼�
        //���ư��� �ִϸ��̼�
    }

    public void Fly_Seq_sleigh()
    {
        Sequence Fly = DOTween.Sequence();

        Transform p1 = Sleigh_position.transform.GetChild(0).transform;
        Transform p2 = Sleigh_position.transform.GetChild(1).transform;
        Transform p3 = Sleigh_position.transform.GetChild(2).transform;


        //��� ���� ���� �ִϸ��̼�
        Fly.Append(Sleigh.transform.DOScale(new Vector3(0.7f, 0.7f, 0.7f), 1f).SetEase(Ease.InOutQuad));
        Fly.Append(Sleigh.transform.DOMove(p1.transform.position, 1f).SetEase(Ease.InOutQuad));
        Fly.Join(Sleigh.transform.DORotate(p1.transform.rotation.eulerAngles, 1f));
        Fly.Append(Sleigh.transform.DOJump(p2.transform.position, 1f, 1, 1f));
        Fly.Join(Sleigh.transform.DORotate(p2.transform.rotation.eulerAngles, 1f));
        Fly.Append(Sleigh.transform.DOJump(p3.transform.position, 1f, 1, 1f));
        Fly.Join(Sleigh.transform.DORotate(p3.transform.rotation.eulerAngles, 1f));
    }
    public void Move_Seq_penguin(int Num)
    {
        In_p_seq[Num].Play();
        Out_p_seq[Num].Play().SetDelay(3);

        //�ð� ������ �ٽ� ���ƿ��� �ִϸ��̼�?
    }
    public void Move_All_penguin()
    {
        In_p_seq[0].Restart();
        In_p_seq[1].Restart();
        In_p_seq[2].Restart();
        In_p_seq[3].Restart();
        In_p_seq[4].Restart();
        In_p_seq[5].Restart();
        In_p_seq[6].Restart();
    }

    //��� �ִϸ��̼��� ��� �� 10�� �ϴ°ɷ� �ϰ�, ���� �ѹ��� ���ƿ� ������ �������� 3�� �� �ϰ� ����ϴ� �ɷ�
    //��� �ִϸ��̼��� ��� ��� �����ִ°ɷ� �ϰ�, ���� ���� �� ���Ŀ� �ѹ��� ��
    //�׸��� ī�޶� �̵��ϰ� ���� �ؽ�Ʈ ������
    //����� �ִϸ��̼��� �ϳ� �� �ձ���
    //�� �ִϸ��̼��� ��� �ϳ��� ���� ����⸸ Ƣ����� ������ ������ �ϴ°ɷ�

    //�ش��ϴ� seq ��ȣ�� ȣ�� �ʿ�
    public void Change_Animation(int Number_seq)
    {
        Content_Seq = Number_seq;
        if (Content_Seq == 3 || Content_Seq == 7 || Content_Seq == 9 || Content_Seq == 12)
        {
            Move_Seq_camera();
            //Debug.Log("SEQ = " + Content_Seq);
        }
    }
}
