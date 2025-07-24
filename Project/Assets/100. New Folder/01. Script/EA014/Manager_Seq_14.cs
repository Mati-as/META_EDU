using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;
using TMPro;
public class Manager_Seq_14 : Base_GameManager
{
    private static GameObject s_UIManager;

    private ParticleSystem _finishMakingPs;

    public static bool isGameStart
    {
        get; private set;
    }

    private bool _isClickable = true;
    public bool Eng_MODE = false;

    private Manager_Anim_14 Manager_Anim;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;

    public bool toggle = false;
    public bool Onclick = true;

    public int Order_number = 0;
    public int Game_round = 0;
    public int Number_Maxemoji_game;

    public AudioClip Effect_Success;
    public AudioClip[] Effect_Emotion;

    //[EDIT] SPINNER
    public GameObject Lucky_Spin_common;
    public TextMeshProUGUI Text_Number;

    private Transform wheel; // 원판의 Transform
    private GameObject Button_Spin;
    private List<string> shapeList = new List<string> { "RECTANGLE", "Empty", "TRIANGLE", "Empty", "CIRCLE", "Empty", "STAR", "HEART" };

    private bool isSpinning = false;
    private bool isSpinResult = false;


    private string selectedShape; // 선택된 감정

    private float sliceAngle; // 45도 (8조각 기준)

    //[Common] Seq,timer
    [Header("[ COMPONENT CHECK ]")]
    public int Content_Seq = 0;
    public float Sequence_timer = 0f;
    public int[] Number_Ofeachgameshape = { 0, 0, 0, 0, 0 };

    //[Common] Start, Update
    void Start()
    {
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_14>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        //Managers.soundManager.Play(SoundManager.Sound.Bgm, "EA003/EA003",0.3f);
    }

    // Update is called once per frame
    void Update()
    {
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

        if (Content_Seq == 0)
        {
            Onclick = false;

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 1)
        {
            Onclick = true;
            Manager_Anim.Read_Seq_Shape();
            //(보류) 여기에서 나타나는 각 도형 텍스트는 한 번 나타나고 화면 밖으로 사라지는게 맞는 것 같음
        }
        else if (Content_Seq == 2)
        {
            Onclick = false;

            var randomChar = (char)Random.Range('A', 'F' + 1);
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Sandwich/Click_" + randomChar, 0.3f);

            //아이콘 1 비활성화
            Manager_Anim.Inactive_Seq_Icon_1();
            Lucky_Spin_common.SetActive(true);
            Manager_Anim.Anim_Active_shake(Lucky_Spin_common.transform.GetChild(1).gameObject);
            Init_wheel();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 3)
        {
            Manager_Anim.Anim_Active(Button_Spin);
            //버튼 활성화 하되, 끝나고 난 다음에 게임 시작 시퀀스 돌입하면?
            //어쩌피 세팅 다 하고 바로 준비할텐데
        }
        else if (Content_Seq == 6 || Content_Seq == 9 || Content_Seq == 12 || Content_Seq == 15)
        {
            //이전 게임 도형 비활성화
            Manager_Obj_14.instance.Main_Shapeicon_2[Shape_number].SetActive(false);
            Reset_Wheelresult();

            //앞서 했던 것과 동일한 애니메이션 재생
            Lucky_Spin_common.SetActive(true);
            Manager_Anim.Anim_Active(Lucky_Spin_common.transform.GetChild(1).gameObject);
            Manager_Anim.Anim_Active(Button_Spin);
            
            //StartCoroutine(GameStart_Seq());
        }
        else if (Content_Seq == 4 || Content_Seq == 7 || Content_Seq == 10 || Content_Seq == 13 || Content_Seq == 16)
        {
            Active_status();
        }
        else if (Content_Seq == 5 || Content_Seq == 8 || Content_Seq == 11 || Content_Seq == 14 || Content_Seq == 17)
        {
            //게임 종료 처리 추가 필요 > 성공 효과음, 이펙트
            Manager_Anim.Anim_Inactive(Manager_Obj_14.instance.UI_Status);

            Manager_Anim.Active_Effect(1);
            Manager_Anim.Active_Effect(2);

            Content_Seq += 1;
            toggle = true;

            Timer_set();
        }
        else if (Content_Seq == 18)
        {
            //이전 게임 도형 비활성화
            Manager_Obj_14.instance.Main_Shapeicon_2[Shape_number].SetActive(false);
            Reset_Wheelresult();

            Manager_Anim.Active_Seq_Icon_1();

            Onclick = true;
            Manager_Anim.Read_Seq_Shape();
        }
        else
        {
            Content_Seq += 1;
            toggle = true;

            Timer_set();
        }
    }


    void Init_EachGame_emoji()
    {
        //각 게임 회차 상단 이미지,텍스트 숫자 바꾸는 부분
        Number_Ofeachgameshape[Game_round] = Manager_Obj_14.instance.Get_numbermaxshape(Shape_number);

        var path = "Audio/BasicContents/Sandwich/SandwichFalling0" + Random.Range(1, 6);
        Managers.Sound.Play(SoundManager.Sound.Effect, path, 0.25f);

        //순차적으로 도형 번호에 맞춰서 게임 세팅
        Manager_Anim.Setting_Seq_Eachgame(Shape_number);
        Number_Maxemoji_game = Number_Ofeachgameshape[Game_round];

        Onclick = true;

        //준비 시작 하면서 게임 자체는 세팅이 되는게 맞음
        //지연이 되어야하는 것은 클릭, 텍스트, 나레이션, active status임
    }

    public void Flip()
    {
        Sequence flipSequence = DOTween.Sequence();

        // 1. 튀는 느낌 - Y축 살짝 위로 이동
        flipSequence.Append(transform.DOJump(
            transform.position,     // 목표 위치는 그대로
            0.5f,                   // 튀는 높이
            1,                      // 한 번만 점프
            0.3f                    // 점프 지속 시간
        ));

        // 2. 튀는 동안 Y축 회전
        flipSequence.Join(transform.DORotate(
            new Vector3(0, 180, 0), // Y축 기준 180도 회전
            0.3f,
            RotateMode.WorldAxisAdd // 현재 회전에 더함
        ).SetEase(Ease.OutBack)); // 살짝 통통 튀는 느낌

        // (선택) 이후 동작 추가 가능
        // flipSequence.Append(...);
    }

    void Init_wheel()
    {
        wheel = Manager_Obj_14.instance.wheel;
        Button_Spin = Manager_Obj_14.instance.Button_Spin;
        sliceAngle = 360f / shapeList.Count; // 8조각 = 45도
        Button_Spin.SetActive(false);

        Button_Spin.GetComponent<Button>().onClick.AddListener(SpinWheel);
    }

    public void SpinWheel()
    {
        Reset_Wheelresult();

        if (isSpinning) return;

        Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[4], 1f);
        Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[2], 1f);

        Manager_Anim.Anim_Inactive(Manager_Obj_14.instance.Seq_text[Content_Seq]);
        Manager_Anim.Anim_Inactive(Button_Spin);

        isSpinning = true;

        int randomTargetIndex = Set_Wheel(Manager_Obj_14.instance.preSelectedShapes[Order_number]);

        float targetAngle = randomTargetIndex * sliceAngle;
        float extraSpins = 5f * 360f;
        float finalRotation = extraSpins + targetAngle;


        wheel.DORotate(new Vector3(0, 0, finalRotation), 5f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                Shape_number = Manager_Obj_14.instance.preSelectedShapes[Order_number];
                Result_Wheel(Shape_number);

                isSpinning = false;

                Order_number += 1;

                selectedShape = shapeList[randomTargetIndex];
                //Debug.Log("선택된 감정: " + selectedShape);

            });

        wheel.DOScale(1.05f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    int Prev_result = -2;
    void Result_Wheel(int num)
    {
        //0번 결과 화면 이펙트
        Manager_Obj_14.instance.Effect_array[0].SetActive(true);

        //Empty exception
        if (num == -1)
        {
            Manager_Anim.Anim_Active(Button_Spin);
            Manager_Obj_14.instance.Result_array[5].SetActive(true);
            Manager_Anim.Activate_emoji(Manager_Obj_14.instance.Result_array[5]);
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.Result_narration[5], 1f);
            Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[0], 1f);
        }
        else
        {
            Manager_Obj_14.instance.Result_array[num].SetActive(true);
            Manager_Anim.Activate_emoji(Manager_Obj_14.instance.Result_array[num]);
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.Result_narration[num], 1f);
            Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[1], 1f);

            StartCoroutine(GameReady_Seq());
            Timer_set();
        }

        Prev_result = num;
    }

    void Reset_Wheelresult()
    {
        //Manager_Obj_14.instance.Effect_array[0].SetActive(false);

        if (Prev_result > -1)
        {
            //Lucky_Spin_common.SetActive(false);
            Manager_Obj_14.instance.Result_array[Prev_result].SetActive(false);
        }
        else if (Prev_result == -1)
        {
            Manager_Obj_14.instance.Result_array[5].SetActive(false);
        }
    }

    //#돌림판의 순서 값을 리턴하는 부분
    //(EDIT) 세팅에 맞춰서 return 값, Rand_numbers값(꽝 부분) 수정 필요
    int[] Rand_numbers = { 1, 3, 5 };
    int Set_Wheel(int num)
    {
        int Shape_num = num;

        if (Shape_num == -1)
        {
            int randomIndex = Random.Range(0, Rand_numbers.Length);
            return Rand_numbers[randomIndex];
        }
        else if (Shape_num == 0)
        {
            return 0;
        }
        else if (Shape_num == 1)
        {
            return 2;
        }
        else if (Shape_num == 2)
        {
            return 4;
        }
        else if (Shape_num == 3)
        {
            return 6;
        }
        else if (Shape_num == 4)
        {
            return 7;
        }
        else
        {
            return -1;
        }
    }
    IEnumerator GameReady_Seq()
    {
        Onclick = false;
        Manager_Obj_14.instance.UI_READY.SetActive(true);
        GameObject Text_Ready = Manager_Obj_14.instance.UI_READY.transform.GetChild(1).gameObject;
        GameObject Text_Start = Manager_Obj_14.instance.UI_READY.transform.GetChild(2).gameObject;

        yield return new WaitForSeconds(3f);

        //0. 도형 활성화 시퀀스, 스핀 비활성화
        Lucky_Spin_common.SetActive(false);
        Manager_Obj_14.instance.Result_array[Shape_number].SetActive(false);

        Init_EachGame_emoji();
        yield return new WaitForSeconds(1f);
        
        // 1. ready 활성화
        Manager_Anim.Anim_Active(Text_Ready);
        Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.READY_narration[0], 1f);

        yield return new WaitForSeconds(3f);

        // 2. ready 비활성화, start 활성화
        Manager_Anim.Anim_Inactive(Text_Ready);
        Manager_Anim.Anim_Active(Text_Start); 
        Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.READY_narration[4], 1f);

        yield return new WaitForSeconds(1f);

        // 3. start 비활성화 및 다음 함수 호출
        Manager_Anim.Anim_Inactive(Text_Start);
        Manager_Obj_14.instance.UI_READY.SetActive(false);

        //(확인) 다음 시퀀스로 넘기고, 다음 시퀀스에 돌입하면 바로 게임 시작하는 것으로 구현
        Content_Seq += 1;
        toggle = true;

        //Timer_set();
    }

    void Active_status()
    {
        Manager_Text.Inactive_UI_Text(3f);
        //Manager_Text.Inactive_UI_Text(3f);
        Text_Number.text = Number_Maxemoji_game.ToString("0");

        for (int i = 0; i < 5; i++)
        {
            Manager_Obj_14.instance.UI_Shapeicon_array[i].SetActive(false);
        }
        Manager_Obj_14.instance.UI_Shapeicon_array[Shape_number].SetActive(true);

        Manager_Anim.Anim_Activestatus(3f);
    }

    void Active_particle(int num)
    {
        ParticleSystem particle = Manager_Obj_14.instance.InGame_effect_array[num].GetComponent<ParticleSystem>();
        particle.Play();
    }

    public void Inactive_shape_clickable(GameObject Shape)
    {
        Shape.GetComponent<BoxCollider>().enabled = false;
        Shape.GetComponent<Clicked_Block_14>().Inactive_Clickable();
    }

    public void Active_shape_clickable(GameObject Shape)
    {
        Shape.GetComponent<BoxCollider>().enabled = true;
        Shape.GetComponent<Clicked_Block_14>().Active_Clickable();
    }
    int Shape_number;
    public void Click(GameObject Shape, int num_emoji, int num_table)
    {
        var randomChar = (char)Random.Range('A', 'F' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Sandwich/Click_" + randomChar, 0.3f);

        //Debug.Log("Shape CLICKED!");

        if (Content_Seq == 1 || Content_Seq == 18)
        {
            Inactive_shape_clickable(Shape);

            //Activate selected emoji, text animation
            Manager_Anim.Activate_emoji(Shape);
            Manager_Anim.Activate_emojitext_popup(Shape);

            Shape.transform.DOShakeScale(1.0f, 1, 10, 90, true).SetEase(Ease.OutQuad).OnComplete(() => Active_shape_clickable(Shape));
        }
        else if (Content_Seq == 4 || Content_Seq == 7 || Content_Seq == 10 || Content_Seq == 13 || Content_Seq == 16)
        {
            //Clicked this round emoji
            if (Shape_number == num_emoji)
            {
                //Debug.Log("RIGHT SHAPE!");
                Shape.transform.DOScale(1f, 1f).From(0).SetEase(Ease.OutElastic);
                Active_particle(num_table);

                Number_Maxemoji_game -= 1;
                Inactive_shape_clickable(Shape);
                Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.Msg_narration[num_emoji], 1f);
                Manager_Anim.Anim_Inactive(Shape);

                Text_Number.text = Number_Maxemoji_game.ToString("0");

                //End
                if (Number_Maxemoji_game == 0)
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[3], 1f);

                    Content_Seq += 1;
                    toggle = true;
                    Game_round += 1;

                    //Debug.Log("ALL SHAPE FOUND!");

                    Onclick = false;
                }
            }
            else
            {

            }
        }
    }


    //====== [COMMON]
    void Timer_set()
    {
        Sequence_timer = 5f;
    }
    //[common] rplidar scanning
    protected override void Init()
    {
        base.Init();
    }

    protected override void SubscribeRayRelatedEvents()
    {
        base.SubscribeRayRelatedEvents();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    private bool _isRoundFinished;
    private readonly int NO_VALID_OBJECT = -1;
    private RaycastHit[] _raycastHits;
    private GameObject Selected_shape;

    //[EDIT] Touching main object, Object Tag, Click script need to be modified
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        //if (_raycastHits.Length == 0) Logger.Log("클릭된 것이 아무것도 없습니다. ------------------");


        foreach (var hit in GameManager_Hits)
        {
            if (hit.transform.gameObject.CompareTag("MainObject"))
            {

                if (Onclick)
                {
                    //Debug.Log("Shape Clicked!");
                    Selected_shape = hit.transform.gameObject;
                    Selected_shape.GetComponent<Clicked_Block_14>().Click();
                }
                return;
            }
        }
    }

    protected override void OnGameStartButtonClicked()
    {
        //to start update
        toggle = true;
        base.OnGameStartButtonClicked();
    }

    public void ButtonClicked()
    {
        Manager_Obj_14.instance.Btn_Next.SetActive(false);
        Sequence seq = DOTween.Sequence();
        seq.Append(Manager_Obj_14.instance.Btn_Next.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic));

        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);

        Content_Seq += 1;
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
