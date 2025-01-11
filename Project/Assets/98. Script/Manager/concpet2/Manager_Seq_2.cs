using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class Manager_Seq_2 : MonoBehaviour
{

    public static Manager_Seq_2 instance = null;

    private Manager_Text Manager_Text;
    private Manager_Anim_2  Manager_Anim;
    private Manager_Narr Manager_Narr;

    public GameObject Eventsystem;

    private bool toggle = true;

    //동물 그룹
    private int On_game;

    [Header("[ COMPONENT CHECK ]")]

    public int Content_Seq = 0;
    //최초 예외 처리 대신에 0으로 설정
    public float Sequence_timer = 0f;
    //시간, 게임 유무?

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_2>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        Eventsystem.SetActive(false);
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
    }


    //최초로 콘텐츠 실행할 때 인트로 한 다음부터 이게 돌아가도 되고
    //아니면 인트로 포함하고 실행하는 걸로 하되,


    void Act()
    {
        Manager_Text.Change_UI_text(Content_Seq);
        Manager_Narr.Change_Audio_narr(Content_Seq);
        Manager_Anim.Change_Animation(Content_Seq);

        //여기는 이미 2번인데 텍스트, 나레이션은 아직 1번이어서 오는 괴리가미 있음
        if (Content_Seq == 2)
        {
            Init_Game_hide();
        }
        else if (Content_Seq == 3)
        {
            Manager_Text.Inactiveall_UI_message();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 4)
        {
            Init_Game_reveal();
        }
        else if (Content_Seq == 5)
        {
            Manager_Text.Inactiveall_UI_message();
            Init_Game_read();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if ( 6 <= Content_Seq && 12 >= Content_Seq)
        {
            Game_read();
        }
        else if (Content_Seq == 13)
        {
            //End, 클릭 활성화
            Eventsystem.SetActive(true);
        }
        else
        {
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }

    void Init_Game_hide()
    {
        //메시지의 경우, T - L - E - R - P - Z - F 순서, hide, reveal

        Eventsystem.SetActive(true);
        On_game = 0;
        //Manager_Anim.Hide_All_animal();
    }
    void Init_Game_reveal()
    {
        Eventsystem.SetActive(true);
        Manager_Anim.Active_click_animal();
        //다시 동물 스크립트 활성화
        On_game = 0;
    }
    void Init_Game_read()
    {
        //동물들 위치 원상 복구
        On_game = 0;
        Eventsystem.SetActive(false);
        //왼쪽부터 순서대로 진행되도록 순서 조정 필요
        Manager_Anim.Reset_Seq_animal(0);
        Manager_Anim.Reset_Seq_animal(1);
        Manager_Anim.Reset_Seq_animal(2);
        Manager_Anim.Reset_Seq_animal(3);
        Manager_Anim.Reset_Seq_animal(4);
        Manager_Anim.Reset_Seq_animal(5);
        Manager_Anim.Reset_Seq_animal(6);
    }
    void Game_read()
    {
        On_game += 1;
        toggle = true;
        Content_Seq += 1;
        Timer_set();
        //해당하는 동물 애니메이션 재생
    }
    void Timer_set()
    {
        Sequence_timer = 5f;
        //여기 이 부분을 나중에는 지정을 해주던가 아니면 그 특정 부분만 다른 데이터로 넣어주던가 해야함
    }

    public void animal_button(int Num_button)
    {
        if (Content_Seq == 2)
        {
            Manager_Text.Active_UI_message(Num_button);
            Manager_Anim.Hide_Seq_animal(Num_button);

            On_game += 1;
        }
        else if (Content_Seq == 4)
        {
            Manager_Text.Active_UI_message(Num_button+7);
            Manager_Anim.Reveal_Seq_animal(Num_button);

            //각 동물의 애니메이션이 전부 종료 될 때 까지 지연시간을 두고 다음 것을 클릭할 수 있도록 함
            Manager_Text.Active_UI_Panel();
            On_game += 1;
        }
        else if (Content_Seq == 13)
        {
            Manager_Text.Active_UI_message(Num_button + 7);
            Manager_Anim.Reveal_Seq_animal(Num_button);

        }

        if (On_game == 7)
        {

            //효과음 재생, 이펙트 출현
            Eventsystem.SetActive(false);
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }
}
