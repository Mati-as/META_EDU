using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class Manager_Seq : MonoBehaviour
{


    public int Content_Seq = 0;


    //���� ���� ó�� ��ſ� 0���� ����
    public float Sequence_timer = 0f;

    private bool toggle = true;

    //���� �׷�
    //��� �׷�, �� �׷� ��� ���� �ؾ��� �� ������
    public GameObject Sleigh;
    public GameObject Bear;
    public GameObject Fishs;

    //��� ����
    public GameObject UI_Button;
    private int On_penguin = 0;
    public GameObject UI_Message;

    //���� ����
    private int On_fish = 0;
    private bool Fishing = false;
    private GameObject [] Fish;

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] UI_Button_array2;     
    //�ð�, ���� ����?

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //�ٵ� ������Ʈ�� ��� �װ� ������ ��� ���ư��ٵ�?

        Sequence_timer -= Time.deltaTime;
        if (Sequence_timer < 0)
        {
            if (toggle == true)
            {
                toggle = false;
                Act();
                //Debug.Log("timer done");
            }
        }

        //12 ���� ����
        if (Fishing)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Fish[On_fish].SetActive(false);
                Fish[On_fish+7].SetActive(true);
                On_fish+=1;
                if (On_fish == 8)
                {
                    Content_Seq += 1;
                    toggle = true;
                    Timer_set();
                    Fishing = false;
                }
            }
        }
    }


    //���ʷ� ������ ������ �� ��Ʈ�� �� �������� �̰� ���ư��� �ǰ�
    //�ƴϸ� ��Ʈ�� �����ϰ� �����ϴ� �ɷ� �ϵ�,


    void Act()
    {
        this.GetComponent<Manager_Text>().Change_UI_text(Content_Seq);
        this.GetComponent<Manager_Narr>().Change_Audio_narr(Content_Seq);
        this.GetComponent<Manager_Anim>().Change_Animation(Content_Seq);

        if (Content_Seq == 3)
        {
            Sleigh.SetActive(true);
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if(Content_Seq == 4)
        {
            Init_Game_penquin1();
        }
        else if (Content_Seq == 6)
        {
            Init_Game_penquin2();
        }
        else if (Content_Seq == 9)
        {
            Sleigh.SetActive(false);
            Content_Seq += 1;
            toggle = true;
            Bear.SetActive(true);
            Fishs.SetActive(true);
            Timer_set();
        }
        else if (Content_Seq == 10)
        {
            Init_Game_bear();
        }
        else if (Content_Seq == 12)
        {
            Sleigh.SetActive(true);
            Debug.Log("CONTENT END");
        }
        else
        {
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }

    void Init_Game_penquin1()
    {
        //��� 1
        On_penguin = 0;
        UI_Button.SetActive(true);
        UI_Button_array2 = new GameObject[UI_Button.transform.childCount];

        for (int i = 0; i < UI_Button.transform.childCount; i++)
        {
            UI_Button_array2[i] = UI_Button.transform.GetChild(i).gameObject;
        }
    }
    void Init_Game_penquin2()
    {
        //��� 1
        On_penguin = 0;
        UI_Button.SetActive(true);
    }

    void Init_Game_bear()
    {
        Fish = new GameObject[Fishs.transform.childCount];
        for (int i = 0; i < Fishs.transform.childCount; i++)
        {
            Fish[i] = Fishs.transform.GetChild(i).gameObject;
        }
        Fishing = true;
    }

    void Timer_set()
    {
        Sequence_timer = 5f;
        //���� �� �κ��� ���߿��� ������ ���ִ��� �ƴϸ� �� Ư�� �κи� �ٸ� �����ͷ� �־��ִ��� �ؾ���
    }

    public void penguin_button(int Num_button)
    {
        //penguin game 1
        if (Content_Seq == 4)
        {
            On_penguin++;
            //Ŭ���� �� ž�� �ִϸ��̼� �� ��� �� �ٽ� ���� �ִϸ��̼� ���
            this.GetComponent<Manager_Anim>().Move_Seq_penguin(Num_button);
            if(On_penguin == 3) 
            { 
                UI_Message.SetActive(true);
            }
            
            if (On_penguin == 10)
            {
                //���࿡ ����� ���� ���ƿ��� ������? �ش��ϴ� ��ư Ŭ���� ��� ��Ȱ��ȭ �ؾ��ϴ°� �ƴѰ�?
                this.GetComponent<Manager_Anim>().Move_All_penguin();

                UI_Button.SetActive(false);

                //�ӽ� �޽��� ȸ��
                UI_Message.SetActive(false);
                //ȿ���� ���, ����Ʈ ����
                Content_Seq += 1;
                toggle = true;
                Timer_set();
            }
            //�� 10�� �������� �ϼ��Ǹ� ȿ����, ����Ʈ �����鼭 �������� ����

        }
        //penguin game 2
        else if (Content_Seq == 6)
        {
            On_penguin++;
            UI_Button_array2[Num_button].SetActive(false);
            this.GetComponent<Manager_Anim>().Shake_Seq_sleigh();
            //�ش��ϴ� ��� ��
            if (On_penguin == 7)
            {

                this.GetComponent<Manager_Anim>().Fly_Seq_sleigh();

                Content_Seq += 1;
                toggle = true;
                Timer_set();
            }
        }
    }
}
