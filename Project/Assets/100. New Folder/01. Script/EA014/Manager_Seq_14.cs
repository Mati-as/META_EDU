using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

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

    public int Game_round = 0;
    public int Number_Maxemoji_game;

    public AudioClip Effect_Success;
    public AudioClip[] Effect_Emotion;

    //[EDIT] SPINNER
    public GameObject Lucky_Spin_common;
    public Text Text_Number;

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
    public int [] Number_Shape = { 0, 0, 0, 0, 0 };

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
        else if (Content_Seq == 2)
        {
            Onclick = true;
            Manager_Anim.Read_Seq_Shape();
            //(보류) 여기에서 나타나는 각 도형 텍스트는 한 번 나타나고 화면 밖으로 사라지는게 맞는 것 같음
        }
        else if (Content_Seq == 3)
        {
            Onclick = false;

            var path = "Audio/BasicContents/Sandwich/SandwichFalling0" + Random.Range(1, 6);
            Managers.Sound.Play(SoundManager.Sound.Effect, path, 0.25f);

            //아이콘 1 비활성화
            Manager_Anim.Inactive_Seq_Icon_1();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 4)
        {
            Init_wheel();
        }
        else if (Content_Seq == 5 || Content_Seq == 8 || Content_Seq == 11 || Content_Seq == 14 || Content_Seq == 17)
        {
            Init_EachGame_emoji(Game_round);
        }
        else if (Content_Seq == 6 || Content_Seq == 9 || Content_Seq == 12 || Content_Seq == 15 || Content_Seq == 18)
        {
            //게임 종료 시퀀스
        }
        else
        {   
            Content_Seq += 1;
            toggle = true;

            Timer_set();
        }
    }

    void Init_EachGame_emoji(int round)
    {
        //여기에서 돌림판, 결과 비활성화 필요
        Lucky_Spin_common.SetActive(false);
        Manager_Obj_14.instance.Result_array[Shape_number].SetActive(false);

        //각 게임 회차 상단 이미지,텍스트 숫자 바꾸는 부분
        Number_Shape[round] = Manager_Obj_14.instance.Get_shapenumber(round);
        Text_Number.text = Number_Shape[round].ToString("0.00");

        var path = "Audio/BasicContents/Sandwich/SandwichFalling0" + Random.Range(1, 6);
        Managers.Sound.Play(SoundManager.Sound.Effect, path, 0.25f);

        Manager_Anim.Setting_Seq_Eachgame(round);
        //Number_Maxemoji_game = Manager_obj_4.instance.Number_of_Eachemoji[round];

        Onclick = true;

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

        Button_Spin.GetComponent<Button>().onClick.AddListener(SpinWheel);

        Lucky_Spin_common.SetActive(true);
    }
    //(수정) 일단은 결과 리셋을 여기에다가 넣었으나 나중에 옮겨야함
    public void SpinWheel()
    {
        Reset_Wheelresult();

        if (isSpinning) return;

        Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[4], 1f);
        Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[2], 1f);

        Manager_Anim.Anim_Inactive(Manager_Obj_14.instance.Seq_text[Content_Seq]);
        Manager_Anim.Anim_Inactive(Button_Spin);

        isSpinning = true;

        int randomTargetIndex = Set_Wheel(Manager_Obj_14.instance.preSelectedShapes[Game_round]);

        float targetAngle = randomTargetIndex * sliceAngle;
        float extraSpins = 5f * 360f;
        float finalRotation = extraSpins + targetAngle;


        wheel.DORotate(new Vector3(0, 0, finalRotation), 5f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                Shape_number = Manager_Obj_14.instance.preSelectedShapes[Game_round];
                Result_Wheel(Shape_number);

                isSpinning = false;

                Game_round += 1;

                selectedShape = shapeList[randomTargetIndex];
                Debug.Log("선택된 감정: " + selectedShape);

            });

        wheel.DOScale(1.05f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    int Prev_result=-2;
    void Result_Wheel(int num)
    {
        //0번이 결과 화면의 이펙트
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


            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }

        Prev_result = num;
    }

    void Reset_Wheelresult()
    {
        Manager_Obj_14.instance.Effect_array[0].SetActive(false);

        if (Prev_result >-1)
        {
            Lucky_Spin_common.SetActive(false);
            Manager_Obj_14.instance.Result_array[Prev_result].SetActive(true);

        }
        else if (Prev_result == -1)
        {
            Manager_Obj_14.instance.Result_array[5].SetActive(false);
        }
    }

    //#돌림판의 순서 값을 리턴하는 부분
    //(EDIT) 세팅에 맞춰서 return 값, Rand_numbers값 수정 필요
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

    int Ready_seq;

    //휠 활성화 되고
    //휠에서 정상적으로 다음으로 진행하면 Ready Msg 실행되도록 할 필요 있음
    public void Ready_Msg_seq()
    {
        Ready_seq = 0;
        StartCoroutine(Ready_Msg());
        //클릭 비활성화 필요
        //UI 클릭은?
        //Onclick = false;
    }

    //(구현) 나레이션 정상 작동은 확인하였으며, 다른 기능들 추가 구현이 필요함

    IEnumerator Ready_Msg(float time = 1.5f)
    {
        if (Ready_seq == 5)
        {
            //4, Start game
            //(구현) 3,2,1 하고 게임에서 클릭할 수 있도록 활성화 하는 기능 필요
            StopCoroutine(Ready_Msg(time));
            Ready_seq = 0;
        }
        else
        {
            yield return new WaitForSeconds(time);

            //(구현) 메시지 텍스트 팝업 필요
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.READY_narration[Ready_seq], 1f);
            Ready_seq += 1;

            StartCoroutine(Ready_Msg(time));
        }
    }
    public void Inactive_shape_clickable(GameObject Shape)
    {
        Shape.GetComponent<Clicked_Block_14>().Inactive_Clickable();
    }

    public void Active_shape_clickable(GameObject Shape)
    {
        Shape.GetComponent<Clicked_Block_14>().Active_Clickable();
    }
    int Shape_number;
    public void Click(GameObject Shape, int num_emoji, int num_table)
    {
        var randomChar = (char)Random.Range('A', 'F' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Sandwich/Click_" + randomChar, 0.3f);

        Debug.Log("Shape CLICKED!");

        if (Content_Seq == 2)
        {
            Inactive_shape_clickable(Shape);

            //Activate selected emoji, text animation
            Manager_Anim.Activate_emoji(Shape);
            Manager_Anim.Activate_emojitext_popup(Shape);

            Shape.transform.DOShakeScale(1.0f, 1, 10, 90, true).SetEase(Ease.OutQuad).OnComplete(() => Active_shape_clickable(Shape));
        }
        else if (Content_Seq == 5 || Content_Seq == 8 || Content_Seq == 11 || Content_Seq == 14 || Content_Seq == 17)
        {
            //Clicked this round emoji
            if (Shape_number == num_emoji)
            {
                Debug.Log("RIGHT SHAPE!");
                Shape.transform.DOScale(1f, 1f).From(0).SetEase(Ease.OutElastic);
                //(구현) 나중에 하나하나 클릭할 때 효과음, 이펙트, 애니메이션 추가 필요
                //Manager_obj_4.instance.Effect_array[num_table].SetActive(true);

                Number_Maxemoji_game -= 1;
                Inactive_shape_clickable(Shape);
                Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.Msg_narration[num_emoji], 1f);
                //Shape.GetComponent<Image>().sprite = Manager_Obj_14.instance.White;

                //End
                if (Number_Maxemoji_game == 0)
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, Manager_Obj_14.instance.Audio_effect_array[3], 1f);

                    //끝나면 나타나는 이모지 큰거
                    //Manager_Obj_14.instance.Main_Icon_3_array[Game_round].SetActive(true);
                    //Manager_Obj_14.instance.Main_Icon_3_array[Game_round].transform.DOScale(1f, 1f).From(0).SetEase(Ease.OutElastic);

                    //Managers.Sound.Play(SoundManager.Sound.Effect, Effect_Emotion[Game_round], 1f);

                    Content_Seq += 1;
                    toggle = true;
                    Game_round += 1;
                    Timer_set();

                    Debug.Log("ALL SHAPE FOUND!");

                    Onclick = false;
                }
            }
            else
            {

            }
        }
        //(구현)여기에서 정답 처리하기 시작할 때 상단 결과 텍스트도 같이 수정 필요, 이미지 교체 기능도 필요함
        //Text_Number.text = Number_Shape[round].ToString("0.00");
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
                    Debug.Log("Shape Clicked!");
                    Selected_shape = hit.transform.gameObject;
                    Selected_shape.GetComponent<Clicked_Block_14>().Click();
                }
                return;
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
