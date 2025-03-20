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
        var path = "Audio/�⺻������/Sandwich/SandwichFalling0" + Random.Range(1, 6);
        Managers.Sound.Play(SoundManager.Sound.Effect, path, 0.25f);

        //1�� ������ ��Ȱ��ȭ
        Manager_Anim.Inactive_Seq_Icon_1();

        //2�� ������ Ȱ��ȭ
        Manager_Anim.Setting_Seq_Icon_2();
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

    protected override void OnGameStartStartButtonClicked()
    {
        //to start update
        toggle = true;
        base.OnGameStartStartButtonClicked();
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
