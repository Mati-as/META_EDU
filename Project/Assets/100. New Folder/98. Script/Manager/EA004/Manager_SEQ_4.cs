using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;


public class Manager_SEQ_4 : Base_GameManager
{
    private static GameObject s_UIManager;

    private ParticleSystem _finishMakingPs;

    public static bool isGameStart { get; private set; }

    private bool _isClickable = true;

    public bool Eng_MODE = false;

    private Manager_anim_4 Manager_Anim;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;

    public bool toggle = false;
    public bool Onclick = true;


    public enum FruitColor
    {
        Red, Purple, Green, Orange, Yellow
    }

    // Ex) Orange, Carrot -> 1,0
    public GameObject[] fruitPrefabs;

    private GameObject Main_Icon_1;
    private GameObject Main_Icon_2;


    private GameObject[] Main_Box_array;

    public FruitColor mainColor;
    public int selectedFruitCount = 0;

    private int round = 0;
    private int maxRounds = 5;

    private const int MaxFruits = 16;
    private const int MaxFruitsinColor = 4;

    public AudioClip Effect_Success;

    [Header("[ COMPONENT CHECK ]")]

    public int Content_Seq = 0;
    public float Sequence_timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Assert(isInitialized);

        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_anim_4>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        Main_Icon_1 = Manager_obj_4.instance.Main_Icon_1;

        //Main_Box_array = new GameObject[Main_Box.transform.childCount];

        //for (int i = 0; i < Main_Box.transform.childCount; i++)
        //{
        //    Main_Box_array[i] = Main_Box.transform.GetChild(i).gameObject;
        //}

        //
        Managers.soundManager.Play(SoundManager.Sound.Bgm, "EA003/EA003",0.3f);
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
            //icon_1이 화면에 나타나 있는채로 시작
            //일단 Icon_1에 있는 이모지 전부 애니메이션 활성화
            Manager_obj_4.instance.BG.SetActive(true);
            Manager_Anim.Activate_all_emoji1();

            Onclick = false;

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 1)
        {
            //전체 애니메이션 스탑 O
            //클릭하면 나레이션 읽어주는 기능 활성화 O
            //다음버튼 누르면 다음으로 진행
            Manager_Anim.Inactivate_all_emoji1();
            Read_Emoji();

            Onclick = true;
        }
        else if (Content_Seq == 2)
        {
            //대신 버튼 클릭할 수 없도록 비활성화 필요
            Init_Game_emoji();
            Onclick = true;
        }
        else if (Content_Seq == 3)
        {
            //panel, icon_2 중 good 활성화
            //그리고 클릭할 수 있는 게임 진행
            //화면상에 있는 해당 이모지 애니메이션 활성화
            //

            //Init_Game_fruit((int)FruitColor.Orange);
            Onclick = true;
        }
        else if (Content_Seq == 5)
        {
            //Init_Game_fruit((int)FruitColor.Yellow);
            Onclick = true;
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
        var path = "Audio/기본컨텐츠/Sandwich/SandwichFalling0" + Random.Range(1, 6);
        Managers.soundManager.Play(SoundManager.Sound.Effect, path, 0.25f);

        round = 0;
        //1번 아이콘 비활성화
        Manager_Anim.Inactive_Seq_Icon_1();
        //2번 아이콘 활성화 애니메이션 추가
        //Manager_Anim.Inactive_Seq_Icon_1();
        Main_Icon_2.SetActive(false);
        //여기에서 Icon_1번에 있던거 전부 비활성화 및 화면 세팅
        //버튼들 활성화

        //마찬가지로 해당하는 버튼 클릭하면 읽어주고 해당하는 이모지 애니메이션 끄기
        //
    }

    //void Init_Game_fruit(int colorIndex)
    //{
    //    mainColor = (FruitColor)colorIndex;

    //    Manager_Anim.Jump_box_bp1(round);

    //    Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 0);
    //    Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 1);

    //    for (int i = 0; i < 5; i++)
    //    {
    //        Generate_fruit(UnityEngine.Random.Range(0, MaxFruits), i + 2);
    //    }
    //}

    //void End_Game_fruit()
    //{
    //    Manager_Anim.Jump_box_bp0(round);
    //    Inactive_All_fruit();

    //    round += 1;
    //}
    //void Read_fruit(int round)
    //{
    //    Manager_Anim.Move_box_bp2(round);
    //    Manager_Anim.Read_Seq_fruit(round);
    //}

    //public void Reset_Game_read()
    //{
    //    for (int i = 0; i < maxRounds; i++)
    //    {
    //        GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
    //        Inactive_fruit_clickable(fruit);
    //        Manager_Anim.Return_Seq_fruit(fruit, i);
    //    }
    //}

    public void Click(GameObject Emoji, int num_emoji, int num_table)
    {
        var randomChar = (char)Random.Range('A', 'F' + 1);
        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar, 0.3f);

        if (Content_Seq == 1)
        {
            //일단 비활성화
            Inactive_emoji_clickable(Emoji);

            //Activate selected emoji animation
            Manager_Anim.Activate_emoji(Emoji);

            //Activate selected emoji text animation
            Manager_Anim.Activate_emojitext_popup(Emoji);

            //스케일값이 바뀌고 난 다음에 다시 활성화
            Emoji.transform.DOShakeScale(1.5f, 1, 10, 90, true).SetEase(Ease.OutQuad).OnComplete(() => Active_emoji_clickable(Emoji));
        }
        //else
        //{

        //    Inactive_fruit_clickable(plate_Fruit);
        //    //맞는 과일 눌렀을 때
        //    if (num_fruit / 4 == (int)mainColor)
        //    {
        //        Manager_Anim.Devide_Seq_fruit(plate_Fruit, selectedFruitCount);
        //        plate_Fruit.transform.SetSiblingIndex(selectedFruitCount);

        //        Manager_Text.Changed_UI_message_c3(num_table, num_fruit, Eng_MODE);
        //        Manager_obj_3.instance.Effect_array[num_table].SetActive(true);

        //        Generate_fruit(UnityEngine.Random.Range(0, MaxFruits), num_table);

        //        plate_Fruit.GetComponent<Clicked_fruit>().Number_table = selectedFruitCount;
        //        selectedFruitCount++;

        //        if (selectedFruitCount == maxRounds)
        //        {
        //            Managers.soundManager.Play(SoundManager.Sound.Effect, Effect_Success, 1f);

        //            Debug.Log("5 FRUITS!");
        //            selectedFruitCount = 0;

        //            Content_Seq += 1;
        //            toggle = true;
        //            Onclick = false;
        //        }
        //    }
        //    else
        //    {
        //        //틀린 과일 눌렀을 때
        //        var path = "Audio/기본컨텐츠/Sandwich/SandwichFalling0" + Random.Range(1, 6);
        //        Managers.soundManager.Play(SoundManager.Sound.Effect, path, 0.25f);

        //        Manager_Anim.Inactive_Seq_fruit(plate_Fruit, 0f);

        //        Generate_fruit((int)mainColor * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), num_table);

        //    }
        //}
    }
    public void Inactive_emoji_clickable(GameObject Emoji)
    {
        Emoji.GetComponent<Clicked_emoji>().Inactive_Clickable();
    }

    public void Active_emoji_clickable(GameObject Emoji)
    {
        Emoji.GetComponent<Clicked_emoji>().Active_Clickable();
    }

    //public void Inactive_All_fruit()
    //{
    //    for (int i = 6; i < Main_Box_array[round].transform.childCount; i++)
    //    {
    //        GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
    //        Manager_Anim.Inactive_Seq_fruit(fruit, 2f);
    //    }
    //}

    //void Generate_fruit(int num_fruit, int num_table)
    //{
    //    Transform pos = Manager_Anim.Get_Fp0(num_table);
    //    Transform fruit_group = Manager_obj_3.instance.Fruit_position.transform;

    //    GameObject fruit = Instantiate(Manager_obj_3.instance.Fruit_prefabs[num_fruit]);
    //    fruit.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
    //    fruit.transform.SetParent(Main_Box_array[round].transform);
    //    fruit.transform.localPosition = pos.localPosition;
    //    fruit.transform.localRotation = Quaternion.Euler(0, 0, 0);

    //    fruit.GetComponent<Clicked_fruit>().Set_Number_fruit(num_fruit);
    //    fruit.GetComponent<Clicked_fruit>().Set_Number_table(num_table);

    //    Manager_Anim.Popup_fruit(fruit);
    //}

    //IEnumerator ResetAfterTime(float time)
    //{
    //    yield return new WaitForSeconds(time);
    //    Init_Game_fruit(UnityEngine.Random.Range(0, 5));
    //    StartCoroutine(ResetAfterTime(time));
    //}

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

        if (_raycastHits.Length == 0) Logger.Log("클릭된 것이 아무것도 없습니다. ------------------");

        foreach (var hit in _raycastHits)
        {
            //Debug.Log(hit.transform.gameObject.tag);
            //Debug.Log(hit.transform.gameObject.name);

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
        //버튼 클릭하면 그냥 다음으로 진행
        Manager_obj_4.instance.Btn_Next.SetActive(false);
        Sequence seq = DOTween.Sequence();
        seq.Append(Manager_obj_4.instance.Btn_Next.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic));

        Content_Seq += 1;
        toggle = true;
        Timer_set();
        round += 1;
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

