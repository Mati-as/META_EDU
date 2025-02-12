using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager_Anim_2 : MonoBehaviour
{

    //Camera, ����� �Ƹ� ����
    private GameObject Main_Camera;
    private GameObject Camera_position;
    private Sequence[] Camera_seq;
    private int Number_Camera_seq;


    //Animal
    private GameObject Animal_position;
    private GameObject[] Main_Animal_array;

    //����, Hide, Reveal �迭�� ����
    //0~6��
    private Sequence[] Hide_a_seq;
    private Sequence[] Reveal_a_seq;
    private Sequence[] Reset_a_seq;

    //���� �ִϸ�����
    private Animator Animalanim;
    private Manager_Seq_2 Manager_Seq;

    [Header("[ COMPONENT CHECK ]")]
    //�Է� �� Ȯ�ο�
    public int Content_Seq = 0;

    public GameObject[] Camera_pos_array;
    public GameObject[] Animal_pos_array;

    void Start()
    {

        //obj ����ȭ
        Camera_position = Manager_obj_2.instance.Camera_position;
        Main_Camera = Manager_obj_2.instance.Main_Camera;

        Init_Seq_camera();
    }
    //�������� Ȱ���� �κ�
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

    void Init_Seq_animal()
    {
        Animal_pos_array = new GameObject[Animal_position.transform.childCount];
        Hide_a_seq = new Sequence[Animal_position.transform.childCount];
        Reveal_a_seq = new Sequence[Animal_position.transform.childCount];
        Reset_a_seq = new Sequence[Animal_position.transform.childCount];

        //hide, reveal ������ ���� �ѹ��� �ʱ�ȭ
        //���� �̵� ���� �Ҵ�
        for (int i = 0; i < Animal_position.transform.childCount; i++)
        {
            Animal_pos_array[i] = Animal_position.transform.GetChild(i).gameObject;

            Hide_a_seq[i] = DOTween.Sequence();
            Reveal_a_seq[i] = DOTween.Sequence();
            Reset_a_seq[i] = DOTween.Sequence();

            Transform p0 = Animal_pos_array[i].transform.GetChild(0).transform;
            Transform p1 = Animal_pos_array[i].transform.GetChild(1).transform;
            Transform p2 = Animal_pos_array[i].transform.GetChild(2).transform;
            Transform p3 = Animal_pos_array[i].transform.GetChild(3).transform;

            //���� ������ �׷��� �ȵǴ� �׳� ���ϰ� ��Ÿ���°ɷ�?
            //Hide_a_seq[i].OnStart(()=>StartRunning(Main_Animal_array[i])); , �̰� ���� �������� ����
            //�ش� �ϴ� �ڸ��� ����Ʈ�� ���ϰ� ����鼭 ������ ������ �ڸ��� �̵��ϵ��� ����
            Hide_a_seq[i].Append(Main_Animal_array[i].transform.DOMove(p1.position, 0f));
            Hide_a_seq[i].Join(Main_Animal_array[i].transform.DORotate(p1.rotation.eulerAngles, 0f));


            //������ �� �ٴ� �ִϸ��̼� ����ϰ�, �����ϸ� attack���� ���¸� �ٲ�

            Reveal_a_seq[i].Append(Main_Animal_array[i].transform.DOMove(p2.position, 1f).SetEase(Ease.InOutQuad));
            Reveal_a_seq[i].Join(Main_Animal_array[i].transform.DORotate(p2.rotation.eulerAngles, 1f));
            Reveal_a_seq[i].Append(Main_Animal_array[i].transform.DOScale(p2.localScale, 1f).SetEase(Ease.InOutQuad));
            Reveal_a_seq[i].Append(Main_Animal_array[i].transform.DOMove(p3.position, 1.5f).SetEase(Ease.InOutQuad).SetDelay(5f));


            Reset_a_seq[i].Append(Main_Animal_array[i].transform.DOMove(p0.position, 0.1f).SetEase(Ease.InOutQuad));
            Reset_a_seq[i].Join(Main_Animal_array[i].transform.DORotate(p0.rotation.eulerAngles, 0.1f));
            Reset_a_seq[i].Join(Main_Animal_array[i].transform.DOScale(p0.localScale, 0.1f));

            Hide_a_seq[i].Pause();
            Reveal_a_seq[i].Pause();
            Reset_a_seq[i].Pause();


            //Debug.Log("length " + Camera_seq.Length);

            //���� ��ġ�� �޶���
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

    public void Hide_Seq_animal(int Num)
    {
        Hide_a_seq[Num].Play();
        //(�ӽ�) �ش� ���� Ŭ�� ��ũ��Ʈ ��Ȱ��ȭ
        //Main_Animal_array[Num].GetComponent<Clicked_animal>().enabled = false;
        //Main_Animal_array[Num].GetComponent<BoxCollider>().enabled = false;

        Manager_Seq = Manager_obj_2.instance.Get_managerseq();
        Manager_Seq.Inactive_animal_clickable(Main_Animal_array[Num]);
    }
    public void Reveal_Seq_animal(int Num)
    {

        Manager_Seq = Manager_obj_2.instance.Get_managerseq();
        Manager_Seq.Inactive_animal_clickable(Main_Animal_array[Num]);
        Reveal_a_seq[Num].Play();

        Manager_Seq = Manager_obj_2.instance.Get_managerseq();
        Manager_Seq.Onclick = false;
        DOVirtual.Float(0, 0, 0f, _ => { }).OnComplete(() => StartAttacking(Main_Animal_array[Num]));
        DOVirtual.Float(0, 0, 5f, _ => { }).OnComplete(() => StartRunning(Main_Animal_array[Num]));

    }
    public void Read_Seq_animal(int Num)
    {
        DOVirtual.Float(0, 0, 0f, _ => { }).OnComplete(() => StartAttacking(Main_Animal_array[Num]));
        DOVirtual.Float(0, 0, 4f, _ => { }).OnComplete(() => ReturnToIdle(Main_Animal_array[Num]));
    }
    public void Final_Click_Seq_animal(int Num)
    {
        Manager_Seq = Manager_obj_2.instance.Get_managerseq();
        Manager_Seq.Inactive_animal_clickable(Main_Animal_array[Num]);
        DOVirtual.Float(0, 0, 0f, _ => { }).OnComplete(() => StartAttacking(Main_Animal_array[Num]));
        DOVirtual.Float(0, 0, 2f, _ => { }).OnComplete(() => ReturnToIdle(Main_Animal_array[Num]));
        DOVirtual.Float(0, 0, 2f, _ => { }).OnComplete(() => Manager_Seq.Active_animal_clickable(Main_Animal_array[Num]));
    }

    public void Reset_Seq_animal(int Num)
    {
        Reveal_a_seq[Num].Pause();
        //�����ϰ� �ִ� ������ ��ž�ϰ� �ٽ� ����ϴ� �ɷ�?
        Reset_a_seq[Num].Play();

        //(�ӽ�) �ش� ���� Ŭ�� ��ũ��Ʈ Ȱ��ȭ
        Main_Animal_array[Num].GetComponent<Clicked_animal>().enabled = true;

        DOVirtual.Float(0, 0, 0f, _ => { }).OnComplete(() => ReturnToIdle(Main_Animal_array[Num]));
    }

    //(�ӽ�) ���� Ŭ�� ��ũ��Ʈ Ȱ��ȭ
    public void Active_click_animal()
    {
        for(int i = 0; i < 7; i++)
        {
            Manager_Seq = Manager_obj_2.instance.Get_managerseq();
            Manager_Seq.Active_animal_clickable(Main_Animal_array[i]);
            Main_Animal_array[i].GetComponent<Clicked_animal>().enabled = true;
        }
    }

    public void Reveal_All_animal()
    {
        Reveal_a_seq[0].Play();
        Reveal_a_seq[1].Play();
        Reveal_a_seq[2].Play();
        Reveal_a_seq[3].Play();
        Reveal_a_seq[4].Play();
        Reveal_a_seq[5].Play();
        Reveal_a_seq[6].Play();
    }


    public void Change_Animation(int Number_seq)
    {
        Content_Seq = Number_seq;
        if (Content_Seq == 1 || Content_Seq == 3 || Content_Seq == 5)
        {
            Move_Seq_camera();
            //Debug.Log("SEQ = " + Content_Seq);
        }
    }

    public void Init_Animalarray()
    {
        Main_Animal_array = Manager_obj_2.instance.Main_Animal_array;
        Animal_position = Manager_obj_2.instance.Animal_position;

        Init_Seq_animal();

    }

    public void StartRunning(GameObject Animal)
    {
        Manager_Seq = Manager_obj_2.instance.Get_managerseq();
        Manager_Seq.Onclick = true;
        Animator animator = Animal.GetComponent<Animator>();

        //�ʱ�ȭ �κ�
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        animator.SetBool("isRunning", true);
        //Debug.Log("Running animation started.");
    }

    public void StartAttacking(GameObject Animal)
    {
        Animator animator = Animal.GetComponent<Animator>();

        //�ʱ�ȭ �κ�
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        animator.SetBool("isAttacking", true);
        //Debug.Log("Attacking animation started.");
    }

    public void StartWalking(GameObject Animal)
    {
        Animator animator = Animal.GetComponent<Animator>();
        //�ʱ�ȭ �κ�
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        animator.SetBool("isWalking", true);
        //Debug.Log("Walking animation started.");
    }
    public void ReturnToIdle(GameObject Animal)
    {
        Animator animator = Animal.GetComponent<Animator>();
        //�ʱ�ȭ �κ�
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        //Debug.Log("Returning to Idle.");
    }
}
