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

    //중복클릭방지
    private bool _isClickable = true;

    // 기존 내용 ------------------------------------------------------------------------

    public static Manager_Seq_3 instance = null;

    private Manager_Anim_3 Manager_Anim;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;

    private GameObject Eventsystem;

    public bool toggle = false;


    //과일 게임
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

    //과일의 경우 프리팹으로 무조건 생성이 되는 걸로하고
    //생성된 프리팹을 무조건 스크립트에 연결해놓음


    //선택된 과일 리스트
    // Ex) Orange, Carrot -> 1,0
    public GameObject[] fruitPrefabs; // 과일 프리팹

    private GameObject Main_Box;


    private GameObject[] Main_Box_array;

    public FruitColor mainColor; // 메인 색깔
    public int selectedFruitCount = 0;

    private int round = 0; // 현재 게임 회차
    private int maxRounds = 5; // 게임의 최대 회차

    private const int MaxFruits = 16;
    private const int MaxFruitsinColor = 4;


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
        //Debug.Assert(isInitialized);

        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_3>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        Eventsystem = Manager_obj_3.instance.Eventsystem;
        Main_Box = Manager_obj_3.instance.Main_Box;

        Main_Box_array = new GameObject[Main_Box.transform.childCount];

        for (int i = 0; i < Main_Box.transform.childCount; i++)
        {
            //현재 있는 과일 전부 삭제
            //모든 바구니 할당 받았고
            //순서대로 R - P -G -O -Y
            Main_Box_array[i] = Main_Box.transform.GetChild(i).gameObject;
        }
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
                Debug.Log("timer done");

            }
        }
    }


    //최초로 콘텐츠 실행할 때 인트로 한 다음부터 이게 돌아가게끔 전부 수정 필요

    void Act()
    {
        Manager_Text.Change_UI_text(Content_Seq);
        Manager_Narr.Change_Audio_narr(Content_Seq);
        Manager_Anim.Change_Animation(Content_Seq);

        //1,3,5,7,9 과일 담기 게임
        //2,4,6,8,10 게임 종료 후 텍스트
        //12 ~ 16 과일 읽어주는 게임
        if (Content_Seq == 0)
        {
            Init_Game_fruit((int)FruitColor.Red);
            Eventsystem.SetActive(false);

            Content_Seq += 1;
            toggle = true;
            Timer_set();
        }
        else if (Content_Seq == 1)
        {
            //여기에서 터치 활성화
            Eventsystem.SetActive(true);
        }
        else if (Content_Seq == 3)
        {
            Init_Game_fruit((int)FruitColor.Orange);
            Eventsystem.SetActive(true);
            //StartCoroutine(ResetAfterTime(7f)); // 7초 후에 재설정
        }
        else if (Content_Seq == 5)
        {
            Init_Game_fruit((int)FruitColor.Yellow);
            Eventsystem.SetActive(true);
            //StartCoroutine(ResetAfterTime(7f)); // 7초 후에 재설정
        }
        else if (Content_Seq == 7)
        {
            Init_Game_fruit((int)FruitColor.Green);
            Eventsystem.SetActive(true);
            //StartCoroutine(ResetAfterTime(7f)); // 7초 후에 재설정
        }
        else if (Content_Seq == 9)
        {
            Init_Game_fruit((int)FruitColor.Purple);
            Eventsystem.SetActive(true);
            //StartCoroutine(ResetAfterTime(7f)); // 7초 후에 재설정
        }
        else if (Content_Seq == 2 || Content_Seq == 4 || Content_Seq == 6 || Content_Seq == 8 || Content_Seq == 10)
        {
            End_Game_fruit();
            Eventsystem.SetActive(false);

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
            Read_fruit(round);
        }
        else if (Content_Seq == 17)
        {
            Manager_Anim.Jump_box_bp0(4);
            //마지막으로 카메라 시점 다시 위로 올리고
            //마지막 상자 원위치
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
        //과일 읽어주기
        round = 0;
        Eventsystem.SetActive(false);
    }
    void Timer_set()
    {
        Sequence_timer = 5f;
        //여기 이 부분을 나중에는 지정을 해주던가 아니면 그 특정 부분만 다른 데이터로 넣어주던가 해야함
    }

    void Init_Game_fruit(int colorIndex)
    {
        //박스도 추적해서 따라가야함
        mainColor = (FruitColor)colorIndex;

        Manager_Anim.Jump_box_bp1(round);
        //위 과정이 끝나야 아래가 진행

        //메인 색깔에서 2개
        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 0);
        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 1);

        for (int i = 0; i < 5; i++)
        {
            //전체 랜덤으로 5개
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
        //전부 바구니로 이동
        for (int i = 0; i < 5; i++)
        {
            GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
            Manager_Anim.Devide_Seq_fruit(fruit, i);
        }

        Content_Seq += 1;
        toggle = true;
        Timer_set();
        round += 1;
    }

    //과일 고르면 제대로 안 없어지거나, 클론이 많거나
    //이쪽을 클릭으로 대체할 필요 있음
    public void Click(GameObject plate_Fruit, int num_fruit, int num_table)
    {
        
        if (num_fruit / 4 == (int)mainColor)
        {
            //현재 테이블에 있는 과일 중에 해당 과일 삭제
            Manager_Anim.Devide_Seq_fruit(plate_Fruit, selectedFruitCount);
            plate_Fruit.transform.SetSiblingIndex(selectedFruitCount);
            //여기에서 해당 과일 오브젝트 인덱스 올림
            Manager_Text.Changed_UI_message_c3(num_table, num_fruit);
            //해당 하는 과일, 채소 텍스트, 나레이션도 나와야함

            //그냥 전체 랜덤으로 하나 다시 추가
            Generate_fruit(UnityEngine.Random.Range(0, MaxFruits), num_table);

            selectedFruitCount++;

            // 5개 선택 시 처리
            if (selectedFruitCount == 5)
            {
                Debug.Log("선택된 과일 5개 완료!");
                //여기에서 다음으로 바로 넘어갔으면 좋겠는데?
                selectedFruitCount = 0; // 초기화

                Content_Seq += 1;
                toggle = true;
            }
        }
        //메인 색깔에서 고르지 않은 경우
        else
        {
            //현재 테이블에 있는 과일 중에 해당 과일 삭제
            Manager_Anim.Inactive_Seq_fruit(plate_Fruit,0f);

            //메인 색깔에서 랜덤
            Generate_fruit((int)mainColor * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), num_table);

        }
    }
    //다시 모으는게 제대로 동작하지 않음
    public void Inactive_All_fruit()
    {
        for (int i =5;i< Main_Box_array[round].transform.childCount; i++)
        {
            GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
            Manager_Anim.Inactive_Seq_fruit(fruit,2f);
        }
    }
    
    void Generate_fruit(int num_fruit, int num_table)
    {
        //과일을 비활성화 인채로 받아오고 팝업 애니메이션에서 활성화함
        Transform pos = Manager_Anim.Get_Fp0(num_table);
        Transform fruit_group = Manager_obj_3.instance.Fruit_position.transform;

        GameObject fruit = Instantiate(Manager_obj_3.instance.Fruit_prefabs[num_fruit]);
        fruit.transform.SetParent(Main_Box_array[round].transform);
        fruit.transform.localPosition = pos.localPosition;

        fruit.GetComponent<Clicked_fruit>().Set_Number_fruit(num_fruit);
        fruit.GetComponent<Clicked_fruit>().Set_Number_table(num_table);

        Manager_Anim.Popup_fruit(fruit);
    }



    IEnumerator ResetAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Init_Game_fruit(UnityEngine.Random.Range(0, 5)); // 새 랜덤 색상으로 초기화
        StartCoroutine(ResetAfterTime(time)); // 계속 반복
    }


    protected override void Init()
    {
        //여기에서 필요한 카메라, 오브젝트, 이런 것들을 사전에 저장을 해주면 좋을 것 같음
        //그리고 난 무조건 find하지 않고 인스펙터창에서 저장하는게 어떨까 싶ㅇ므

        _finishMakingPs = GameObject.Find("CFX_FinishMaking").GetComponent<ParticleSystem>();

        base.Init();
    }


    //기존 스크립트 내용 붙인거

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

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        _raycastHits = Physics.RaycastAll(GameManager_Ray);

        //(임시) 여기에서 일부 내가 활용하지 않는 것 같으니 일단은 비활성화 함
        //if (_isRoundFinished) return;
        //if (!isGameStart) return;
        //if (!_isClickable)
        //{
        //    return;
        //}

        foreach (var hit in _raycastHits)
        {
            //클릭하면 호출 되는 함수는 여기로 대체 필요
            //여긴 어떻게 관리하면 되나?
            //var selectedIndex = FindIndexByName(hit.transform.gameObject.name);

            Debug.LogError(hit.transform.name);

            var randomChar = (char)Random.Range('A', 'F' + 1);

            //여기가 클릭 효과음 재생하는 부분인 것 같고
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar,
                0.3f);

            return;
        }
    }

    protected override void OnStartButtonClicked()
    {
        //업데이트 안에 있는거 실행 시작
        base.OnStartButtonClicked();
    }

    public void ButtonClicked()
    {

        toggle = true;
    }


    // methods ------------------------------------------------------------------------

    //private void SetClickableAfterDelay(float delay)
    //{
    //    DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() => { _isClickable = true; });
    //}


    private void StackCamera()
    {
        var uiCamera = s_UIManager.GetComponentInChildren<Camera>();

        if (Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out var mainCameraData))
            mainCameraData.cameraStack.Add(uiCamera);
        else
            Debug.LogError("Main camera does not have UniversalAdditionalCameraData component.");
    }

}
