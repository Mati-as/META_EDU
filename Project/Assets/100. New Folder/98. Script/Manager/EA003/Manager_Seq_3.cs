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

    //우리것도 중복 실행 방지 해야하는데
    //그리고 우리것도 효과 이펙트랑 효과음 재생해야하는데

    public bool Eng_MODE = false;

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
                //Debug.Log("timer done");
            }
        }
    }

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
            //터치 활성화
            Eventsystem.SetActive(true);
        }
        else if (Content_Seq == 3)
        {
            Init_Game_fruit((int)FruitColor.Orange);
            Eventsystem.SetActive(true);
        }
        else if (Content_Seq == 5)
        {
            Init_Game_fruit((int)FruitColor.Yellow);
            Eventsystem.SetActive(true);
        }
        else if (Content_Seq == 7)
        {
            Init_Game_fruit((int)FruitColor.Green);
            Eventsystem.SetActive(true);
        }
        else if (Content_Seq == 9)
        {
            Init_Game_fruit((int)FruitColor.Purple);
            Eventsystem.SetActive(true);
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
    }

    void Init_Game_fruit(int colorIndex)
    {
        mainColor = (FruitColor)colorIndex;

        Manager_Anim.Jump_box_bp1(round);

        //메인 색깔 2개
        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 0);
        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 1);

        for (int i = 0; i < 5; i++)
        {
            //전체 랜덤 5개
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
        for (int i = 0; i < maxRounds; i++)
        {
            GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
            Manager_Anim.Devide_Seq_fruit(fruit, i);
        }

        Content_Seq += 1;
        toggle = true;
        Timer_set();
        round += 1;
    }

    //클릭 대체 부분
    public void Click(GameObject plate_Fruit, int num_fruit, int num_table)
    {

        if (num_fruit / 4 == (int)mainColor)
        {
            //현재 테이블에 있는 과일 중에 해당 과일 삭제
            Manager_Anim.Devide_Seq_fruit(plate_Fruit, selectedFruitCount);
            plate_Fruit.transform.SetSiblingIndex(selectedFruitCount);

            Manager_Text.Changed_UI_message_c3(num_table, num_fruit, Eng_MODE);

            Generate_fruit(UnityEngine.Random.Range(0, MaxFruits), num_table);

            selectedFruitCount++;

            // 5개 선택 시 처리
            if (selectedFruitCount == maxRounds)
            {
                Debug.Log("선택된 과일 5개 완료!");
                selectedFruitCount = 0; // 초기화

                Content_Seq += 1;
                toggle = true;
            }
        }
        //메인 색깔에서 고르지 않은 경우
        else
        {
            Manager_Anim.Inactive_Seq_fruit(plate_Fruit, 0f);

            Generate_fruit((int)mainColor * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), num_table);

        }
        //이게 한번이면 상관 없는데 여러개 이면 문제가 생길 수 있을 것 같음
        //콘텐츠별 효과음 리스트를 따로 작성해놓는걸로 하고 그걸로 각각의 변수를 세분화 해서 가져가는걸로 하는게 좋을 것 같음
        //그럼 결국 여기는 string형태가 아니라 변수형태로 딱딱 호출 할 수 있으니
        //결국 클릭하는 효과음은 거의 매 콘텐츠마다 동일 할 것으로 예상되니 우선은 전체 효과음을 로드하고 그 중에 해당 콘텐츠의

        //Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar, 0.3f);
    }
    //다시 모으는게 제대로 동작하지 않음
    public void Inactive_All_fruit()
    {
        for (int i = 5; i < Main_Box_array[round].transform.childCount; i++)
        {
            GameObject fruit = Main_Box_array[round].transform.GetChild(i).gameObject;
            Manager_Anim.Inactive_Seq_fruit(fruit, 2f);
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

    //아래부터 기존 스크립트 내용
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

        foreach (var hit in _raycastHits)
        {
            Debug.Log(hit.transform.gameObject.tag);
            Debug.Log(hit.transform.gameObject.name);

            if (hit.transform.gameObject.CompareTag("MainObject"))
            {
                Debug.Log("Fruit Clicked!");
                //여기가 나오는지 확인 필요함
                Selected_fruit = hit.transform.gameObject;
                Selected_fruit.GetComponent<Clicked_fruit>().Click();
            }
            //소리 나는것 확인하였음
            var randomChar = (char)Random.Range('A', 'F' + 1);
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar,0.3f);

            return;
        }
    }

    protected override void OnStartButtonClicked()
    {
        //Update 실행용
        toggle = true;
        base.OnStartButtonClicked();
    }
    //뭔가 클릭이 두번씩 발생하는 이유가 뭐지?

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
