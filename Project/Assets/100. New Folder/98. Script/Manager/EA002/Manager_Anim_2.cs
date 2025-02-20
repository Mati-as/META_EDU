using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Manager_Anim_2 : MonoBehaviour
{

    //Camera, 여기는 아마 공통
    private GameObject Main_Camera;
    private GameObject Camera_position;
    private Sequence[] Camera_seq;
    private int Number_Camera_seq;


    //Animal
    private GameObject Animal_position;
    private GameObject[] Main_Animal_array;

    //동물, Hide, Reveal 배열로 관리
    //0~6번
    private Sequence[] Hide_a_seq;
    private Sequence[] Reveal_a_seq;
    private Sequence[] Reset_a_seq;

    //동물 애니메이터
    private Animator Animalanim;
    private Manager_Seq_2 Manager_Seq;

    [Header("[ COMPONENT CHECK ]")]
    //입력 값 확인용
    public int Content_Seq = 0;

    public GameObject[] Camera_pos_array;
    public GameObject[] Animal_pos_array;

    void Start()
    {

        //obj 동기화
        Camera_position = Manager_obj_2.instance.Camera_position;
        Main_Camera = Manager_obj_2.instance.Main_Camera;

        Init_Seq_camera();
    }
    //공통으로 활용할 부분
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

        //hide, reveal 시퀀스 각각 한번씩 초기화
        //메인 이동 동물 할당
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

            //들어가면 좋은데 그렇게 안되니 그냥 뿅하고 나타나는걸로?
            //Hide_a_seq[i].OnStart(()=>StartRunning(Main_Animal_array[i])); , 이거 전부 동작하지 않음
            //해당 하는 자리에 이펙트가 펑하고 생기면서 동물이 빠르게 자리로 이동하도록 수정
            Hide_a_seq[i].Append(Main_Animal_array[i].transform.DOMove(p1.position, 0f));
            Hide_a_seq[i].Join(Main_Animal_array[i].transform.DORotate(p1.rotation.eulerAngles, 0f));


            //시작할 때 뛰는 애니메이션 재생하고, 도착하면 attack으로 상태를 바꿈

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

            //숨는 위치가 달라짐
        }
    }


    void Move_Seq_camera()
    {
        //함수로 전부 관리하는게 맞음
        //클릭이 되면 해당하는 펭귄을 움직일 거였으니깐
        Camera_seq[Number_Camera_seq].Play();
        Number_Camera_seq++;
        //Debug.Log("C_SEQ = " + Number_Camera_seq);
    }

    public void Hide_Seq_animal(int Num)
    {
        Hide_a_seq[Num].Play();
        //(임시) 해당 동물 클릭 스크립트 비활성화
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
        //현재하고 있는 시퀀스 스탑하고 다시 재생하는 걸로?
        Reset_a_seq[Num].Play();

        //(임시) 해당 동물 클릭 스크립트 활성화
        Main_Animal_array[Num].GetComponent<Clicked_animal>().enabled = true;

        DOVirtual.Float(0, 0, 0f, _ => { }).OnComplete(() => ReturnToIdle(Main_Animal_array[Num]));
    }

    //(임시) 동물 클릭 스크립트 활성화
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

        //초기화 부분
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        animator.SetBool("isRunning", true);
        //Debug.Log("Running animation started.");
    }

    public void StartAttacking(GameObject Animal)
    {
        Animator animator = Animal.GetComponent<Animator>();

        //초기화 부분
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        animator.SetBool("isAttacking", true);
        //Debug.Log("Attacking animation started.");
    }

    public void StartWalking(GameObject Animal)
    {
        Animator animator = Animal.GetComponent<Animator>();
        //초기화 부분
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        animator.SetBool("isWalking", true);
        //Debug.Log("Walking animation started.");
    }
    public void ReturnToIdle(GameObject Animal)
    {
        Animator animator = Animal.GetComponent<Animator>();
        //초기화 부분
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);

        //Debug.Log("Returning to Idle.");
    }
}
