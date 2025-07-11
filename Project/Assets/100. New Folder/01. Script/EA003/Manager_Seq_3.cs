using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;


public class Manager_Seq_3 : Base_GameManager
{
    private static GameObject s_UIManager;

    private ParticleSystem _finishMakingPs;

    public static bool isGameStart { get; private set; }

    private bool _isClickable = true;

    public bool Eng_MODE = false;

    private Manager_Anim_3 Manager_Anim;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;

    public bool toggle = false;
    public bool Onclick = true;


    public enum FruitColor
    {
        Red, Purple, Green, Orange, Yellow
    }

    public enum Fruit
    {
        Strawberry, Apple, Tomato, Cherry,
        Grapes, Blueberry, Eggplant, Beetroot,
        Watermelon, Cucumber, Avocado, GreenOnion,
        Carrot, Pumpkin, Orange, Onion,
        Banana, Lemon, Corn, Pineapple
    }

    // Ex) Orange, Carrot -> 1,0
    public GameObject[] fruitPrefabs; 

    private GameObject Main_Box;


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
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_3>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        Main_Box = Manager_obj_3.instance.Main_Box;

        Main_Box_array = new GameObject[Main_Box.transform.childCount];

        for (int i = 0; i < Main_Box.transform.childCount; i++)
        {
            Main_Box_array[i] = Main_Box.transform.GetChild(i).gameObject;
        }

        //
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
            Init_Game_fruit((int)FruitColor.Red);
            Onclick = false;

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 1)
        {
            Onclick = true;
        }
        else if (Content_Seq == 3)
        {
            Init_Game_fruit((int)FruitColor.Orange);
            Onclick = true;
        }
        else if (Content_Seq == 5)
        {
            Init_Game_fruit((int)FruitColor.Yellow);
            Onclick = true;
        }
        else if (Content_Seq == 7)
        {
            Init_Game_fruit((int)FruitColor.Green);
            Onclick = true;
        }
        else if (Content_Seq == 9)
        {
            Init_Game_fruit((int)FruitColor.Purple);
            Onclick = true;
        }
        else if (Content_Seq == 2 || Content_Seq == 4 || Content_Seq == 6 || Content_Seq == 8 || Content_Seq == 10)
        {
            End_Game_fruit();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 11)
        {
            Init_Game_read();

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 12 || Content_Seq == 13 || Content_Seq == 14 || Content_Seq == 15 || Content_Seq == 16)
        {
            Onclick = true;
            Read_fruit(round);
            //기다리는 기능을 추가할게 아니라 펼쳐진채로 다음버튼을 클릭해야 다음으로 진행되게끔 수정 필요

        }
        else if (Content_Seq == 17)
        {
            Manager_Anim.Jump_box_bp0(4);
        }
        else
        {
            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
    }
    void Init_Game_read()
    {
        round = 0;
    }
    void Timer_set()
    {
        Sequence_timer = 5f;
    }

    void Init_Game_fruit(int colorIndex)
    {
        mainColor = (FruitColor)colorIndex;

        Manager_Anim.Jump_box_bp1(round);

        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 0);
        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 1);

        for (int i = 0; i < 5; i++)
        {
            Generate_fruit(UnityEngine.Random.Range(0, MaxFruits), i + 2);
        }
    }

    void End_Game_fruit()
    {
        Manager_Anim.Jump_box_bp0(round);
        Inactive_All_fruit();

        round += 1;
    }
    void Read_fruit(int round)
    {
        Manager_Anim.Move_box_bp2(round);
        Manager_Anim.Read_Seq_fruit(round);
    }

    public void Reset_Game_read()
    {
        for (int i = 0; i < maxRounds; i++)
        {
            GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
            Inactive_fruit_clickable(fruit);
            Manager_Anim.Return_Seq_fruit(fruit, i);
        }
    }

    public void Click(GameObject plate_Fruit, int num_fruit, int num_table)
    {
        var randomChar = (char)Random.Range('A', 'F' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/Sandwich/Click_" + randomChar, 0.3f);

        if (Content_Seq >= 12)
        {
            //일단 비활성화
            Inactive_fruit_clickable(plate_Fruit);
            Manager_Text.Changed_UI_message_c3(num_table + 7, num_fruit, Eng_MODE); // 새 랜덤 색상으로 초기화

            //스케일값이 바뀌고 난 다음에 다시 활성화
            plate_Fruit.transform.DOShakeScale(1.5f, 1, 10, 90, true).SetEase(Ease.OutQuad).OnComplete(()=> Active_fruit_clickable(plate_Fruit));
        }
        else{
            
            Inactive_fruit_clickable(plate_Fruit);
            //맞는 과일 눌렀을 때
            if (num_fruit / 4 == (int)mainColor)
            {
                Manager_Anim.Devide_Seq_fruit(plate_Fruit, selectedFruitCount);
                plate_Fruit.transform.SetSiblingIndex(selectedFruitCount);

                Manager_Text.Changed_UI_message_c3(num_table, num_fruit, Eng_MODE);
                Manager_obj_3.instance.Effect_array[num_table].SetActive(true);

                Generate_fruit(UnityEngine.Random.Range(0, MaxFruits), num_table);

                plate_Fruit.GetComponent<Clicked_fruit>().Number_table = selectedFruitCount;
                selectedFruitCount++;

                if (selectedFruitCount == maxRounds)
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, Effect_Success, 1f);

                    Debug.Log("5 FRUITS!");
                    selectedFruitCount = 0;

                    Content_Seq += 1;
                    toggle = true;
                    Onclick = false;
                }
            }
            else
            {
                //틀린 과일 눌렀을 때
                var path = "Audio/BasicContents/Sandwich/SandwichFalling0" + Random.Range(1, 6);
                Managers.Sound.Play(SoundManager.Sound.Effect, path, 0.25f);

                Manager_Anim.Inactive_Seq_fruit(plate_Fruit, 0f);

                Generate_fruit((int)mainColor * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), num_table);

            }
        }
    }
    public void Inactive_fruit_clickable(GameObject plate_Fruit)
    {
        plate_Fruit.GetComponent<Clicked_fruit>().Inactive_Clickable();
    }

    public void Active_fruit_clickable(GameObject plate_Fruit)
    {
        plate_Fruit.GetComponent<Clicked_fruit>().Active_Clickable();
    }

    public void Inactive_All_fruit()
    {
        for (int i = 6; i < Main_Box_array[round].transform.childCount; i++)
        {
            GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
            Manager_Anim.Inactive_Seq_fruit(fruit, 2f);
        }
    }

    void Generate_fruit(int num_fruit, int num_table)
    {
        Transform pos = Manager_Anim.Get_Fp0(num_table);
        Transform fruit_group = Manager_obj_3.instance.Fruit_position.transform;

        GameObject fruit = Instantiate(Manager_obj_3.instance.Fruit_prefabs[num_fruit]);
        fruit.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        fruit.transform.SetParent(Main_Box_array[round].transform);
        fruit.transform.localPosition = pos.localPosition;
        fruit.transform.localRotation = Quaternion.Euler(0,0,0);

        fruit.GetComponent<Clicked_fruit>().Set_Number_fruit(num_fruit);
        fruit.GetComponent<Clicked_fruit>().Set_Number_table(num_table);

        Manager_Anim.Popup_fruit(fruit);
    }

    IEnumerator ResetAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Init_Game_fruit(UnityEngine.Random.Range(0, 5)); 
        StartCoroutine(ResetAfterTime(time)); 
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
    private GameObject Selected_fruit;
    //private Clicked_fruit Selected_fruitCF;

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        _raycastHits = Physics.RaycastAll(GameManager_Ray);

        if(_raycastHits.Length ==0 ) Logger.Log("클릭된 것이 아무것도 없습니다. ------------------");
        
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

    protected override void OnGameStartButtonClicked()
    {
        //to start update
        toggle = true;
        base.OnGameStartButtonClicked();
    }

    public void ButtonClicked()
    {
        //버튼 클릭하면 원위치 시작
        Reset_Game_read();
        Manager_obj_3.instance.Btn_Next.SetActive(false);
        Sequence seq = DOTween.Sequence();
        seq.Append(Manager_obj_3.instance.Btn_Next.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic));


        //여기서 클릭 받으면 현재 박스 들어가기전에 해당하는 과일 전부 다시 비활성화 필요

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
