using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
<<<<<<< HEAD
=======

>>>>>>> e0d85006f2c00aa48779501356461fc3f2f9c0e1
public class Manager_Anim_3 : MonoBehaviour
{
    //Common
    public int Content_Seq = 0;

    private Manager_Text Manager_Text;
    private Manager_Seq_3 Manager_Seq;
    private Manager_obj_3 Manager_Obj;
    private bool Eng_mode;

    //Camera
    private GameObject Main_Camera;
    private GameObject Camera_position;
    private Sequence[] Camera_seq;
    private int Number_Camera_seq;


    //fruit
    private GameObject Fruit_position;

    private GameObject Main_Box;
    private GameObject[] Main_Box_array;
    private GameObject Box_position;


    private GameObject Selected_fruit;
    private int fruit_number;
    private int round_number;
    private int Box_number;

    //p0 �׷� 7��
    //p1 �׷� 5��
    //p2 �׷� 5��

    //���� �ִϸ��̼� ����
    //0~6��
    private Sequence[] Reveal_f_seq;

    //��� transform�� ���� ������ �ְ� �װ� �����ؼ� ����ϴ� �ɷ�
    //���� ��̴� �������� �޶��� �� ����

    private Transform[] B_p0; //�ڽ� �ʱ� ��ġ 5��
    private Transform B_p1; //���� ���� �ڽ� ��ġ
    private Transform B_p2; //���� �б� �ڽ� ��ġ

    public Transform[] F_p0; //���� ���� ���� ��ġ
    private Transform[] F_p1; //�ڽ��� ���� ��ġ
    private Transform[] F_p2; //���� �б� ���� ��ġ
    private Transform[] F_p3; //�б� ���� �ڽ��� ���� ��ġ

    [Header("[ COMPONENT CHECK ]")]
    //��ġ �Է°� Ȯ�ο�
    public GameObject[] Camera_pos_array;
    public GameObject[] Fruit_pos_array;
    public GameObject[] Box_pos_array;

    void Start()
    {
        Camera_position = Manager_obj_3.instance.Camera_position;
        Main_Camera = Manager_obj_3.instance.Main_Camera;
        Fruit_position = Manager_obj_3.instance.Fruit_position;
        Box_position = Manager_obj_3.instance.Box_position;
        Main_Box = Manager_obj_3.instance.Main_Box;

        Manager_Seq = Manager_obj_3.instance.Get_managerseq();
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();

        Init_Seq_camera();
        Init_Seq_fruit();
        Init_Seq_box();

        Eng_mode = Manager_Seq.Eng_MODE;
    }
    //�������� Ȱ���� �κ�
    void Init_M_obj()
    {

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

    //�׷��ٸ� �̰� ũ�� �ǹ̰� ������?
    //�׳� �׶� �׶� �ʱ�ȭ�ؼ� ����� �����ص� ũ�� ������� �� ���⵵�ϰ�
    //�׳� transform���� ���� �� ������ �ִ°ɷ�?
    void Init_Seq_fruit()
    {
        Fruit_pos_array = new GameObject[Fruit_position.transform.childCount];


        F_p0 = new Transform[7]; 
        F_p1 = new Transform[5]; 
        F_p2 = new Transform[5]; 
        F_p3 = new Transform[5];

        for (int i = 0; i < Fruit_position.transform.childCount; i++)
        {
            Fruit_pos_array[i] = Fruit_position.transform.GetChild(i).gameObject;
        }
        //FP
        F_p0[0] = Fruit_position.transform.GetChild(0);
        F_p0[1] = Fruit_position.transform.GetChild(1);
        F_p0[2] = Fruit_position.transform.GetChild(2);
        F_p0[3] = Fruit_position.transform.GetChild(3);
        F_p0[4] = Fruit_position.transform.GetChild(4);
        F_p0[5] = Fruit_position.transform.GetChild(5);
        F_p0[6] = Fruit_position.transform.GetChild(6);
        F_p1[0] = Fruit_position.transform.GetChild(7);
        F_p1[1] = Fruit_position.transform.GetChild(8);
        F_p1[2] = Fruit_position.transform.GetChild(9);
        F_p1[3] = Fruit_position.transform.GetChild(10);
        F_p1[4] = Fruit_position.transform.GetChild(11);
        F_p2[0] = Fruit_position.transform.GetChild(12);
        F_p2[1] = Fruit_position.transform.GetChild(13);
        F_p2[2] = Fruit_position.transform.GetChild(14);
        F_p2[3] = Fruit_position.transform.GetChild(15);
        F_p2[4] = Fruit_position.transform.GetChild(16);
        F_p3[0] = Fruit_position.transform.GetChild(17);
        F_p3[1] = Fruit_position.transform.GetChild(18);
        F_p3[2] = Fruit_position.transform.GetChild(19);
        F_p3[3] = Fruit_position.transform.GetChild(20);
        F_p3[4] = Fruit_position.transform.GetChild(21);


    }
    void Init_Seq_box()
    {

        B_p0 = new Transform[5];  //�ڽ� �ʱ� ��ġ 5��

        Main_Box_array = new GameObject[Main_Box.transform.childCount];
        Box_pos_array = new GameObject[Box_position.transform.childCount];

        for (int i = 0; i < Box_position.transform.childCount; i++)
        {
            Box_pos_array[i] = Box_position.transform.GetChild(i).gameObject;
        }

        for (int i = 0; i < Main_Box.transform.childCount; i++)
        {
            Main_Box_array[i] = Main_Box.transform.GetChild(i).gameObject;
        }
        //BP
        B_p0[0] = Box_position.transform.GetChild(0);
        B_p0[1] = Box_position.transform.GetChild(1);
        B_p0[2] = Box_position.transform.GetChild(2);
        B_p0[3] = Box_position.transform.GetChild(3);
        B_p0[4] = Box_position.transform.GetChild(4);
        B_p1 = Box_position.transform.GetChild(5);
        B_p2 = Box_position.transform.GetChild(6);

    }

    public void Move_Seq_camera()
    {
        Camera_seq[Number_Camera_seq].Play();
        Number_Camera_seq++;
        //Debug.Log("C_SEQ = " + Number_Camera_seq);
    }

    public void Popup_fruit(GameObject fruit)
    {
        Sequence seq = DOTween.Sequence();

        //2�� �� �˾� �ִϸ��̼� �� ���� Ȱ��ȭ
        seq.Append(fruit.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => fruit.SetActive(true))).SetDelay(2f);
        //seq.Append(fruit.transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad));
    }

    //�ش��ϴ� ������ ��� ���������� �̵�����
    public void Jump_fruit(GameObject fruit, Transform pos, float timer)
    {
        Sequence seq = DOTween.Sequence();

        if (timer != 0f)
        {
            seq.Append(fruit.transform.DOJump(pos.position, 1f, 1, 1f)).SetDelay(timer);
            seq.Append(fruit.transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad));
        }
        else
        {
            seq.Append(fruit.transform.DOJump(pos.position, 1f, 1, 1f));
            seq.Append(fruit.transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad));
        }
    }

    public void Jump_box_bp1(int round)
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(Main_Box_array[round].transform.DOJump(B_p1.position, 1f, 1, 1f));
        seq.Append(Main_Box_array[round].transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad));
    }
    public void Jump_box_bp0(int round)
    {
        Sequence seq = DOTween.Sequence();

        //���� �ð� ���� ������ �����ϴ� �ִϸ��̼�
        seq.Append(Main_Box_array[round].transform.DOJump(B_p0[round].position, 1f, 1, 1f)).SetDelay(3f);
        seq.Join(Main_Box_array[round].transform.DOShakeScale(0.5f, 1, 10, 90, true).SetEase(Ease.OutQuad));
        seq.Append(Main_Box_array[round].transform.DOScale(B_p0[round].localScale, 1f));


        //seq.Append(Main_Box_array[round].transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad));

    }
    public void Move_box_bp2(int round)
    {
        Sequence seq = DOTween.Sequence();

        if (round != 0)
        {
            //���� �ٱ��ϸ� ����־��ְ� �� ������ ���� ���� �ٱ��ϸ� ����
            seq.Append(Main_Box_array[round - 1].transform.DOMove(B_p0[round - 1].position, 1f).SetEase(Ease.InOutQuad));
            seq.Join(Main_Box_array[round - 1].transform.DOScale(B_p0[round - 1].localScale, 1f));
            seq.Append(Main_Box_array[round].transform.DOMove(B_p2.position, 1f).SetEase(Ease.InOutQuad));
        }
        else
        {
            seq.Append(Main_Box_array[round].transform.DOMove(B_p2.position, 1f).SetEase(Ease.InOutQuad));
        }

        seq.Join(Main_Box_array[round].transform.DOScale(B_p2.localScale, 1f));
    }
    public void Inactive_Seq_fruit(GameObject fruit, float timer)
    {
        //���� ������ ������°� ���� ���� �� ���� ���
        //(�̽�) ������ �������� �ѹ��� ������� �Ŷ� ������ ���� ���� ���� ������� �ϴ°Ŷ� ���ϰ� �浹 �߻��ϴ� �� ����
        if (timer != 0f)
        {
            fruit.transform.DOScale(0, 0.5f).SetEase(Ease.InOutQuint).OnComplete(() => Destroy(fruit)).SetDelay(timer);
        }
        else
        {
            fruit.transform.DOScale(0, 0.5f).SetEase(Ease.InOutQuint).OnComplete(() => Destroy(fruit));
        }
    }
    public void Devide_Seq_fruit(GameObject plate_Fruit, int number)
    {
        //�������� �ű�� ���ô� ��Ȱ��ȭ
        GameObject plate = plate_Fruit.transform.GetChild(0).gameObject;
        plate.transform.DOScale(0, 1f).SetEase(Ease.OutElastic);
        plate.SetActive(false);

        Jump_fruit(plate_Fruit, F_p1[number], 0f);
    }
    public void Return_Seq_fruit(GameObject plate_Fruit, int number)
    {
        //�������� �ű�� ���ô� ��Ȱ��ȭ
        GameObject plate = plate_Fruit.transform.GetChild(0).gameObject;
        plate.transform.DOScale(0, 1f).SetEase(Ease.OutElastic);
        plate.SetActive(false);

        Jump_fruit(plate_Fruit, F_p3[number], 0f);
    }

    public void Read_Seq_fruit(int round)
    {
        //(�ӽ�) �Ŵ��� seq 3 ȣ����
        //Sequence seq = DOTween.Sequence();
        //this.transform.DOScale(1, 11f).OnComplete(() => Manager_Seq_3.instance.Reset_Game_read());

        round_number = 0;
        Box_number = round;
        StartCoroutine(Temp_Message());
    }

    IEnumerator Temp_Message(float time=2f)
    {
        if (round_number == 5)
        {
            //5���� ���� ���� ���� �ൿ
<<<<<<< HEAD
            //���⿡�� ��� �����ϴ� ��� �߰� �ʿ�
=======
>>>>>>> e0d85006f2c00aa48779501356461fc3f2f9c0e1

            Manager_obj_3.instance.Btn_Next.SetActive(true);

            Sequence seq = DOTween.Sequence();
            seq.Append(Manager_obj_3.instance.Btn_Next.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic));

            StopCoroutine(Temp_Message(time));
        }
        else {

            yield return new WaitForSeconds(time);

<<<<<<< HEAD
            //�ٱ��Ͽ��� ���� ������ �κ�
=======
            //�ٱ��Ͽ��� ���� ������ �κ�, �ִϸ��̼� �� �ؽ�Ʈ���� ������
>>>>>>> e0d85006f2c00aa48779501356461fc3f2f9c0e1
            GameObject Selected_fruit;
            int fruit_number;

            Sequence seq_read = DOTween.Sequence();

            Selected_fruit = Main_Box_array[Box_number].transform.GetChild(round_number).gameObject;
            fruit_number = Selected_fruit.GetComponent<Clicked_fruit>().Number_fruit;

            //������� �����
            seq_read.Append(Selected_fruit.transform.DOJump(F_p2[round_number].position, 1f, 1, 1f));
            seq_read.Append(Selected_fruit.transform.DOShakeScale(1, 1, 10, 90, true).SetEase(Ease.OutQuad));
<<<<<<< HEAD
            //�̵��ϰ� �� ������ Ŭ�� �ٽ� Ȱ��ȭ
            Manager_Seq.Active_fruit_collider(Selected_fruit);
=======

            //�̵��ϰ� �� ������ Ŭ�� �ٽ� Ȱ��ȭ
            Manager_Seq.Active_fruit_clickable(Selected_fruit);
>>>>>>> e0d85006f2c00aa48779501356461fc3f2f9c0e1


            //���� ������� �ƴ��� üũ�� �ʿ�
            Manager_Text.Changed_UI_message_c3(round_number + 7, fruit_number, Eng_mode); // �� ���� �������� �ʱ�ȭ
            round_number += 1;

            StartCoroutine(Temp_Message(time)); // ��� �ݺ�
        }
    }

    public Transform Get_Fp0(int num)
    {
        return F_p0[num];
    }

    //public Transform [] Get_Fp2(int round)
    //{
    //    return F_p2[num];
    //}

    public void Change_Animation(int Number_seq)
    {
        Content_Seq = Number_seq;
        if (Content_Seq == 11 || Content_Seq == 12 || Content_Seq == 17)
        {
            Move_Seq_camera();
            //Debug.Log("SEQ = " + Content_Seq);
        }
    }
}
