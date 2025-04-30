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
            //여기에서 나타나는 각 도형 텍스트는 한 번 나타나고 화면 밖으로 사라지는게 맞는 것 같음
            //클릭 안됨
        }
        else if (Content_Seq == 3)
        {
            Onclick = false;
            //Main icon 1 도형 전부 삭제
            //그리고 2 게임 세팅
        }
        else if (Content_Seq == 4)
        {
            Init_wheel();
        }
        else
        {   
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

        int randomTargetIndex = Set_Wheel(Manager_Obj_14.instance.preSelectedShapes[Game_round]);

        float targetAngle = randomTargetIndex * sliceAngle;
        float extraSpins = 5f * 360f;
        float finalRotation = extraSpins + targetAngle;

        wheel.DORotate(new Vector3(0, 0, finalRotation), 5f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                isSpinning = false;
                Game_round += 1;

                selectedShape = shapeList[randomTargetIndex];
                Debug.Log("선택된 감정: " + selectedShape);

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
        //결과 화면 표출 또는 다시 진행 필요
        Ready_Msg_seq();
        Manager_Anim.Anim_Active(Button_Spin);
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

    //[EDIT] Touching main object, Tag, Click script should be revised
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        _raycastHits = Physics.RaycastAll(GameManager_Ray);

        if (_raycastHits.Length == 0)

            foreach (var hit in _raycastHits)
            {
                Debug.Log(hit.transform.gameObject.tag);
                Debug.Log(hit.transform.gameObject.name);

                if (hit.transform.gameObject.CompareTag("MainObject"))
                {

                    if (Onclick)
                    {
                        //(구현) 해당 부분 구현 필요
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
