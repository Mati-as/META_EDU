using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Random = UnityEngine.Random;
using UnityEngine.Rendering.Universal;


public class Manager_Seq_2 : Base_GameManager
{
    private static GameObject s_UIManager;

    private ParticleSystem _finishMakingPs;

    public static bool isGameStart { get; private set; }

    private bool _isClickable = true;

    //  previous system ------------------------------------------------------------------------

    private Manager_Text Manager_Text;
    private Manager_Anim_2  Manager_Anim;
    private Manager_Narr Manager_Narr;

    public bool toggle = false;
    public bool Onclick = true;

    //동물 그룹
    public int On_game;

    public AudioClip Effect_Success;
    public AudioClip BGM;

    [Header("[ COMPONENT CHECK ]")]

    public int Content_Seq = 0;
    //최초 예외 처리 대신에 0으로 설정
    public float Sequence_timer = 0f;
    //시간, 게임 유무?


    // Start is called before the first frame update
    void Start()
    {
        toggle = false;
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_2>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();


        waitForClickableInGameRay = 0.6f;
        Onclick = false;

        Managers.Sound.Play(SoundManager.Sound.Bgm, BGM, 0.3f);
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
            //이전 사운드 종료 용
            Managers.Sound.Stop(SoundManager.Sound.Main);
            Manager_Text.Inactiveall_UI_message();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 4)
        {
            Init_Game_reveal();
            //여기에서 전부 다시 활성화
        }
        else if (Content_Seq == 5)
        {
            Managers.Sound.Stop(SoundManager.Sound.Main);
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
            Manager_Anim.Active_click_animal();
            On_game = 0;

            Onclick = true;
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

        Onclick = true;
        On_game = 0;
        //Manager_Anim.Hide_All_animal();
    }
    void Init_Game_reveal()
    {
        Onclick = true;
        //다시 동물 스크립트 활성화
        Manager_Anim.Active_click_animal();
        On_game = 0;
    }
    void Init_Game_read()
    {
        //동물들 위치 원상 복구
        On_game = 0;
        Onclick = false;
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
        Manager_Anim.Read_Seq_animal(On_game);
        Managers.Sound.Play(SoundManager.Sound.Main, Manager_obj_2.instance.Animal_effect[On_game], 1f);
        //동물 울음소리도 마찬가지로 재생 필요
        On_game += 1;
        toggle = true;
        Content_Seq += 1;
        Timer_set();
    }
    void Timer_set()
    {
        Sequence_timer = 5f;
        //여기 이 부분을 나중에는 지정을 해주던가 아니면 그 특정 부분만 다른 데이터로 넣어주던가 해야함
    }

    private int Number_animal;

    //숨기기에서 한번 클릭하고 비활성화
    //숨기기 전부 끝나고 난 다음에 찾기 하기 전에 다시 활성화
    //찾기에서 3번 클릭하고 난 다음 비활성화
    //찾기 전부 끝나고 난 다음에 읽기 다 끝난 다음에 다시 활성화

    public void animal_click(int Num_button)
    {
        var randomChar = (char)Random.Range('A', 'F' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Sandwich/Click_" + randomChar, 0.3f);

        if (Content_Seq == 2)
        {
            Manager_Text.Active_UI_message(Num_button);
            Manager_Anim.Hide_Seq_animal(Num_button);
            Manager_obj_2.instance.Effect_array[Num_button].SetActive(true);

            //해당 동물 효과음 재생, play로 그때그때 대체될 수 있도록 구현함
            Managers.Sound.Play(SoundManager.Sound.Main, Manager_obj_2.instance.Animal_effect[Num_button], 1f);
            //BGM 소리 줄이는 기능 필요

            //메시지 나레이션, 정상 작동 확인
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_2.instance.Msg_narration[Num_button], 1f);

            On_game += 1;
        }
        else if (Content_Seq == 4)
        {
            Number_animal = Selected_animal.GetComponent<Clicked_animal>().Get_Clickednumber();
            Manager_obj_2.instance.Effect_array[Num_button + 7].SetActive(true);

            if (Number_animal < 3)
            {
                Selected_animal.GetComponent<Clicked_animal>().Set_Clickednumber();
                Managers.Sound.Play(SoundManager.Sound.Main, Manager_obj_2.instance.Animal_effect[Num_button], 1f);
                //BGM 소리 줄이는 기능 필요
            }
            else if(Number_animal == 3)
            {

                Managers.Sound.Play(SoundManager.Sound.Effect, Effect_Success, 1f);
                Managers.Sound.Play(SoundManager.Sound.Main, Manager_obj_2.instance.Animal_effect[Num_button], 1f);
                Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_2.instance.Msg_narration[Num_button], 1f);
                //BGM 소리 줄이는 기능 필요

                //애니메이션 재생하는 동안 잠시 다른 동물 클릭할 수 없도록 해야함

                Manager_Text.Active_UI_message(Num_button + 7);
                Manager_Anim.Reveal_Seq_animal(Num_button);

                //Manager_Text.Active_UI_Panel();
                On_game += 1;
            }
        }
        else if (Content_Seq == 13)
        {
            Manager_Text.Active_UI_message(Num_button + 7);
            Manager_Anim.Final_Click_Seq_animal(Num_button);
            Manager_obj_2.instance.Effect_array[Num_button].SetActive(true);
            Managers.Sound.Play(SoundManager.Sound.Main, Manager_obj_2.instance.Animal_effect[Num_button], 1f);
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_2.instance.Msg_narration[Num_button], 1f);
            //BGM 소리 줄이는 기능 필요
        }

        if (On_game == 7)
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, Effect_Success, 1f);
            //여기에서 정상적으로 작동하지 않음
            Debug.Log("동물 7마리 채움");
            Onclick = false;
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }

    public void Inactive_animal_clickable(GameObject animal)
    {
        animal.GetComponent<Clicked_animal>().Inactive_Clickable();
    }

    public void Active_animal_clickable(GameObject animal)
    {
        animal.GetComponent<Clicked_animal>().Active_Clickable();
    }


    protected override void Init()
    {
        base.Init();
    }

    protected override void BindEvent()
    {
        base.BindEvent();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private bool _isRoundFinished;
    private readonly int NO_VALID_OBJECT = -1;
    private RaycastHit[] _raycastHits;
    private GameObject Selected_animal;
    //private Clicked_fruit Selected_fruitCF;

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;
        
        _raycastHits = Physics.RaycastAll(GameManager_Ray);

        if (_raycastHits.Length == 0) Logger.Log("클릭된 것이 아무것도 없습니다. ------------------");

        foreach (var hit in _raycastHits)
        {
            //Debug.Log(hit.transform.gameObject.tag);
            //Debug.Log(hit.transform.gameObject.name);
            if (Onclick)
            {
                if (hit.transform.gameObject.CompareTag("MainObject"))
                {
                    Selected_animal = hit.transform.gameObject;
                    Selected_animal.GetComponent<Clicked_animal>().Click();
                    return;
                }
            }


        }
    }
    protected override void OnGameStartStartButtonClicked()
    {
        //to start update
        toggle = true;
        base.OnGameStartStartButtonClicked();
    }

    public void ButtonClicked()
    {
        toggle = true;
    }

    private void StackCamera()
    {
        var uiCamera = s_UIManager.GetComponentInChildren<Camera>();

        if (Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out var mainCameraData))
            mainCameraData.cameraStack.Add(uiCamera);
        else
            Debug.LogError("Main camera does not have UniversalAdditionalCameraData component.");
    }
}
