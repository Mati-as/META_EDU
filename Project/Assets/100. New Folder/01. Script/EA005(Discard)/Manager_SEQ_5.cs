using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;


public class Manager_SEQ_5 : Base_GameManager
{
    //[Common] Raysync part
    private static GameObject s_UIManager;
    private ParticleSystem _finishMakingPs;
    private bool _isClickable = true;
    public bool toggle = false;
    public bool Onclick = true;

    //[Common]  Manager
    public bool Eng_MODE = false;
    private Manager_anim_5 Manager_Anim;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;

    //[Common] Seq,timer
    [Header("[ COMPONENT CHECK ]")]
    public int Content_Seq = 0;
    public float Sequence_timer = 0f;
    public int Emoji_game = 0;

    public GameObject Lucky_Spin_common;

    private GameObject Bigsize_emotion;
    private Transform wheel; // 원판의 Transform
    private GameObject Button_Spin;
    private List<string> emotionList = new List<string> { "Happy", "Sad", "Empty", "Angry", "Sleep", "Empty", "Good", "Laugh" };

    private bool isSpinning = false;
    private bool isSpinResult = false;


    private string selectedEmotion; // 선택된 감정

    private float sliceAngle; // 45도 (8조각 기준)

    //[Common] Start, Update
    void Start()
    {
        //Debug.Assert(isInitialized);

        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_anim_5>();
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
            Manager_Anim.Activate_all_emoji1();

            Onclick = false;

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 1)
        {
            Manager_Anim.Inactivate_all_emoji1();
            Read_Emoji();

            Onclick = true;
        }
        else if (Content_Seq == 2)
        {
            Init_Game_emoji();
            Init_wheel();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
            Onclick = false;
        }
        else if (Content_Seq == 3 || Content_Seq == 5 || Content_Seq == 7 || Content_Seq == 9 || Content_Seq == 11)
        {
            //Init_EachGame_emoji(Game_round);
        }
        else if (Content_Seq == 13)
        {
            Manager_obj_4.instance.Main_Icon_2.SetActive(false);
            Manager_obj_4.instance.Main_Icon_3_array[4].transform.DOScale(0, 1f).SetEase(Ease.OutElastic);

            //액티베이트 했을 때 이모지 애니메이션 활성화
            Manager_Anim.Active_Seq_Icon_1();
            Read_Emoji();

            Onclick = true;
        }
        else
        {   //여기에서 중간중간 큰 이모지 활성화 하는 기능 추가
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }

    void Timer_set()
    {
        Sequence_timer = 5f;
    }

    void Read_Emoji()
    {
        Manager_Anim.Read_Seq_Emoji();
    }
    void Init_Game_emoji()
    {
        var path = "Audio/BasicContents/Sandwich/SandwichFalling0" + Random.Range(1, 6);
        Managers.Sound.Play(SoundManager.Sound.Effect, path, 0.25f);


        Manager_Anim.Inactive_Seq_Icon_1();

        //아래 부분은 각각 1P, 2P 별로 블록 그룹 세팅할 수 있도록함
        Manager_Anim.Setting_Seq_Icon_2();
    }

    void Init_wheel()
    {

        Bigsize_emotion = Manager_obj_5.instance.Bigsize_emotion;
        wheel = Manager_obj_5.instance.wheel;
        Button_Spin = Manager_obj_5.instance.Button_Spin;
        sliceAngle = 360f / emotionList.Count; // 8조각 = 45도

        Button_Spin.GetComponent<Button>().onClick.AddListener(SpinWheel);

        Lucky_Spin_common.SetActive(true);
    }


    public void SpinWheel()
    {
        if (isSpinning) return;

        Manager_Anim.Anim_Inactive(Button_Spin);
        isSpinning = true;

        int randomTargetIndex = Manager_obj_5.instance.preSelectedEmotions[Emoji_game];
        float targetAngle = randomTargetIndex * sliceAngle;
        float extraSpins = 5f * 360f;
        float finalRotation = extraSpins + targetAngle;

        wheel.DORotate(new Vector3(0, 0, -finalRotation), 5f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                isSpinning = false;
                Emoji_game += 1;

                selectedEmotion = emotionList[randomTargetIndex];
                Debug.Log("선택된 감정: " + selectedEmotion);

                //결과 처리 함수 호출
                Show_Emotion(randomTargetIndex);
            });

        wheel.DOScale(1.05f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    //(구현) 점프 버튼 클릭하면 바로 없어짐
    //결과 나오고 재반복이 필요하면 버튼 활성화
    //그렇지 않을 경우 스핀 비활성화 및 다음 시퀀스 진행

    void Show_Emotion(int selectedEmotion)
    {
        //여기는 공통으로 해당하는 이모션 활성화
        //애니메이션, 효과음도 필요
        Manager_obj_5.instance.Bigsize_emotion_array[selectedEmotion].SetActive(true);


        //여기에서 애니메이션 전부 끝나면 
        if (selectedEmotion == 2 || selectedEmotion == 5)
        {
            //isSpinResult = false;
            Manager_Anim.Anim_Active(Button_Spin);
            //다음 진행 불가 스핀 반복
        }
        else
        {
            //다음 진행
        }
    }
    public void Click(GameObject Emoji, int num_emoji, int num_table)
    {
        var randomChar = (char)Random.Range('A', 'F' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Sandwich/Click_" + randomChar, 0.3f);

        Debug.Log("EMOJI CLICKED!");

        //처음 이모지 클릭, 마지막 이모지 클릭 기능
        if (Content_Seq == 1 || Content_Seq == 13)
        {
            Inactive_emoji_clickable(Emoji);

            //Activate selected emoji, text animation
            Manager_Anim.Activate_emoji(Emoji);
            Manager_Anim.Activate_emojitext_popup(Emoji);

            Emoji.transform.DOShakeScale(1.0f, 1, 10, 90, true).SetEase(Ease.OutQuad).OnComplete(() => Active_emoji_clickable(Emoji));

        }
        //else if (Content_Seq == 3 || Content_Seq == 5 || Content_Seq == 7 || Content_Seq == 9 || Content_Seq == 11)
        //{
        //    //Clicked this round emoji
        //    if (Game_round == num_emoji)
        //    {
        //        Debug.Log("RIGHT EMOJI!");
        //        Emoji.transform.DOScale(1f, 1f).From(0).SetEase(Ease.OutElastic);
        //        Manager_obj_4.instance.Effect_array[num_table].SetActive(true);

        //        Number_Maxemoji_game -= 1;
        //        Manager_Anim.Inactivate_emoji(Emoji);
        //        Inactive_emoji_clickable(Emoji);
        //        Managers.Sound.Play(SoundManager.Sound.Narration, Manager_obj_4.instance.Msg_narration[num_emoji], 1f);
        //        Emoji.GetComponent<Image>().sprite = Manager_obj_4.instance.White;

        //        //End
        //        if (Number_Maxemoji_game == 0)
        //        {
        //            Managers.Sound.Play(SoundManager.Sound.Effect, Effect_Success, 1f);

        //            Manager_obj_4.instance.Main_Icon_3_array[Game_round].SetActive(true);
        //            Manager_obj_4.instance.Main_Icon_3_array[Game_round].transform.DOScale(1f, 1f).From(0).SetEase(Ease.OutElastic);

        //            Managers.Sound.Play(SoundManager.Sound.Effect, Effect_Emotion[Game_round], 1f);

        //            Content_Seq += 1;
        //            toggle = true;
        //            Game_round += 1;
        //            Timer_set();

        //            Debug.Log("ALL EMOJI FOUND!");

        //            Onclick = false;
        //        }
        //    }
        //    else
        //    {

        //    }
        //}
    }

    public void Inactive_emoji_clickable(GameObject Emoji)
    {
        Emoji.GetComponent<Clicked_EMOJI>().Inactive_Clickable();
    }

    public void Active_emoji_clickable(GameObject Emoji)
    {
        Emoji.GetComponent<Clicked_EMOJI>().Active_Clickable();
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
    private GameObject Selected_fruit;
    //private Clicked_fruit Selected_fruitCF;

    //[EDIT] Touching main object
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        _raycastHits = Physics.RaycastAll(GameManager_Ray);

        if (_raycastHits.Length == 0)

            foreach (var hit in _raycastHits)
            {
                if (hit.transform.gameObject.CompareTag("MainObject"))
                {
                    if (Onclick)
                    {
                        //Debug.Log("Fruit Clicked!");
                        Selected_fruit = hit.transform.gameObject;
                        Selected_fruit.GetComponent<Clicked_fruit>().Click();
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
        Manager_obj_5.instance.Btn_Next.SetActive(false);
        Sequence seq = DOTween.Sequence();
        seq.Append(Manager_obj_5.instance.Btn_Next.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic));

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
