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

    //[Common] Start, Update
    void Start()
    {
        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_14>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        Init_wheel();
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
        //Manager_Anim.Change_Animation(Content_Seq);

        if (Content_Seq == 0)
        {
            Onclick = false;

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else
        {   //여기에서 중간중간 큰 이모지 활성화 하는 기능 추가
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }

    void Init_wheel()
    {
        wheel = Manager_Obj_14.instance.wheel;
        Button_Spin = Manager_Obj_14.instance.Button_Spin;
        sliceAngle = 360f / shapeList.Count; // 8조각 = 45도

        Button_Spin.GetComponent<Button>().onClick.AddListener(SpinWheel);

        Lucky_Spin_common.SetActive(true);
    }


    public void SpinWheel()
    {
        if (isSpinning) return;

        Manager_Anim.Anim_Inactive(Button_Spin);
        isSpinning = true;

        //(구현) 여기에서 해당하는 번호가 나오면 그 번호에 맞춰서 회전하고, 그럼 순서대로 나와야하는거 아닌가?
        //
        int randomTargetIndex = Manager_Obj_14.instance.preSelectedShapes[Game_round];
        float targetAngle = randomTargetIndex * sliceAngle;
        float extraSpins = 5f * 360f;
        float finalRotation = extraSpins + targetAngle;

        //(확인) 돌린 돌림판의 결과 값이 같은가?

        wheel.DORotate(new Vector3(0, 0, -finalRotation), 5f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                isSpinning = false;
                Game_round += 1;

                //여기에서 오차가 있군
                //여기에서 선택된 모양을 찾아가지고 결과값을 리턴해야하는데 엉뚱한게 나옴
                selectedShape = shapeList[randomTargetIndex];
                Debug.Log("선택된 감정: " + selectedShape);

                //결과 처리 함수 호출
                Result_Wheel(randomTargetIndex);
            });

        wheel.DOScale(1.05f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }
    //(구현) 점프 버튼 클릭하면 바로 없어짐
    //결과 나오고 재반복이 필요하면 버튼 활성화
    //그렇지 않을 경우 스핀 비활성화 및 다음 시퀀스 진행

    void Result_Wheel(int selectedEmotion)
    {
        //애니메이션, 성공 효과음도 필요
        //Manager_Obj_14.instance.Bigsize_emotion_array[selectedEmotion].SetActive(true);
        //결과 화면 표출 또는 다시 진행 필요
        Manager_Anim.Anim_Active(Button_Spin);
    }

    int Ready_seq;

    //휠 활성화 되고
    //휠에서 정상적으로 다음으로 진행하면 Ready Msg 실행되도록 할 필요 있음
    public void Ready_Msg_seq()
    {
        Ready_seq = 0;
        StartCoroutine(Temp_Message());
    }

    IEnumerator Temp_Message(float time = 2f)
    {
        if (Ready_seq == 4)
        {
            //4, Start game
            //(구현) 3,2,1 하고 게임에서 클릭할 수 있도록 활성화 하는 기능 필요
            StopCoroutine(Temp_Message(time));
            Ready_seq = 0;
        }
        else
        {
            yield return new WaitForSeconds(time);

            //(구현) 메시지 텍스트 팝업 필요
            Managers.Sound.Play(SoundManager.Sound.Narration, Manager_Obj_14.instance.Msg_narration[Ready_seq], 1.5f);
            Ready_seq += 1;

            StartCoroutine(Temp_Message(time));
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
                        //(구현) 해당 부분 구현 필요
                        //Debug.Log("Fruit Clicked!");
                        Selected_fruit = hit.transform.gameObject;
                        Selected_fruit.GetComponent<Clicked_fruit>().Click();
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
        //Manager_obj_4.instance.Btn_Next.SetActive(false);
        //Sequence seq = DOTween.Sequence();
        //seq.Append(Manager_obj_4.instance.Btn_Next.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic));

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
