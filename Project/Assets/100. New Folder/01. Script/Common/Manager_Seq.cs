using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class Manager_Seq : MonoBehaviour
{


    public int Content_Seq = 0;


    //최초 예외 처리 대신에 0으로 설정
    public float Sequence_timer = 0f;

    private bool toggle = true;

    //동물 그룹
    //펭귄 그룹, 곰 그룹 잠시 조작 해야할 거 같은데
    public GameObject Sleigh;
    public GameObject Bear;
    public GameObject Fishs;

    //펭귄 게임
    public GameObject UI_Button;
    private int On_penguin = 0;
    public GameObject UI_Message;

    //낚시 게임
    private int On_fish = 0;
    private bool Fishing = false;
    private GameObject [] Fish;

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] UI_Button_array2;     
    //시간, 게임 유무?

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //근데 업데이트에 계속 그게 있으면 계속 돌아갈텐데?

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

        //12 낚시 게임
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


    //최초로 콘텐츠 실행할 때 인트로 한 다음부터 이게 돌아가도 되고
    //아니면 인트로 포함하고 실행하는 걸로 하되,


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
        //펭귄 1
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
        //펭귄 1
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
        //여기 이 부분을 나중에는 지정을 해주던가 아니면 그 특정 부분만 다른 데이터로 넣어주던가 해야함
    }

    public void penguin_button(int Num_button)
    {
        //penguin game 1
        if (Content_Seq == 4)
        {
            On_penguin++;
            //클릭한 뒤 탑승 애니메이션 및 잠시 후 다시 하차 애니메이션 재생
            this.GetComponent<Manager_Anim>().Move_Seq_penguin(Num_button);
            if(On_penguin == 3) 
            { 
                UI_Message.SetActive(true);
            }
            
            if (On_penguin == 10)
            {
                //만약에 펭귄이 아직 돌아오지 않으면? 해당하는 버튼 클릭은 잠시 비활성화 해야하는거 아닌가?
                this.GetComponent<Manager_Anim>().Move_All_penguin();

                UI_Button.SetActive(false);

                //임시 메시지 회수
                UI_Message.SetActive(false);
                //효과음 재생, 이펙트 출현
                Content_Seq += 1;
                toggle = true;
                Timer_set();
            }
            //총 10개 보내놓고 완성되면 효과음, 이펙트 나오면서 다음으로 진행

        }
        //penguin game 2
        else if (Content_Seq == 6)
        {
            On_penguin++;
            UI_Button_array2[Num_button].SetActive(false);
            this.GetComponent<Manager_Anim>().Shake_Seq_sleigh();
            //해당하는 펭귄 들어감
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
