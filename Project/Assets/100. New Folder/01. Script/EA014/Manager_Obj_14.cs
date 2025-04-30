using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Obj_14 : MonoBehaviour
{
    public static Manager_Obj_14 instance = null;

    //[common] Camera, UI
    [Header("[COMMON] REQUIRED")]
    public GameObject Main_Camera;
    public GameObject Camera_position;
    public GameObject UI_Text;
    public GameObject UI_Message;
    public GameObject UI_Panel;
    public GameObject Game_effect;

    public GameObject UI_READY;
    public GameObject UI_Result;

    //Emoji icon
    public GameObject Main_Icon_1;  //이모지 보여주기
    public Transform wheel; // 원판의 Transform
    public GameObject Button_Spin;
    public GameObject Icon_buttion_position;

    public List<string> Seq_shape = new List<string> { "RECTANGLE", "Empty", "TRIANGLE", "Empty", "CIRCLE", "Empty", "STAR", "HEART" };

    //[common, EDIT] Manager
    private Manager_Anim_14 Manager_Anim;
    private Manager_Seq_14 Manager_Seq;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;
    public GameObject Btn_Next;

    private int MaxShape = 5;

    public List<int> shapeOrder = new List<int>();           // 0~4 도형 순서  "RECTANGLE", "TRIANGLE", "CIRCLE", "STAR", "HEART"
    public List<int> preSelectedShapes = new List<int>();    // 실제 사용할 배열 (꽝 포함)

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Shape_prefabs;
    public GameObject[] Shape_array;

    public AudioClip[] Seq_narration;
    public GameObject[] Seq_text;
    public AudioClip[] Result_narration;
    public AudioClip[] READY_narration;
    public AudioClip[] Msg_narration;

    private AudioClip[] seq_narration;
    private GameObject[] seq_text;

    //public AudioClip[] Msg_narration_eng;
    //public Sprite[] Msg_textsprite;
    //public Sprite[] Msg_textsprite_eng;
    public GameObject[] Effect_array;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //Destroy(this.gameObject);
        }
    }
    void Start()
    {
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_14>();
        Manager_Seq = this.gameObject.GetComponent<Manager_Seq_14>();

        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        init_Audio();
        init_Text();
        init_Prefab();
        //Init_Effectarray();
        //Init_Icon_array();


        GenerateShapeOrder();
        GeneratePreSelectedShapes();

        Rearrange_Text();
        Rearrange_Narration();
    }

    void init_Text()
    {

        Seq_text = new GameObject[UI_Text.transform.childCount];
        seq_text = new GameObject[UI_Text.transform.childCount];

        for (int i = 0; i < UI_Text.transform.childCount; i++)
        {
            Seq_text[i] = UI_Text.transform.GetChild(i).gameObject;
            seq_text[i] = UI_Text.transform.GetChild(i).gameObject;
        }

        Manager_Text.Init_UI_text(UI_Text, UI_Message, UI_Panel);
    }
    void init_Audio()
    {
        Msg_narration = Resources.LoadAll<AudioClip>("EA014/audio_message");
        Seq_narration = Resources.LoadAll<AudioClip>("EA014/audio_seq_1");
        seq_narration = Resources.LoadAll<AudioClip>("EA014/audio_seq_1");
        Result_narration = Resources.LoadAll<AudioClip>("EA014/audio_result");
        READY_narration = Resources.LoadAll<AudioClip>("EA014/audio_READY");

        //Animal_effect = Resources.LoadAll<AudioClip>("EA004/audio_effect");

        //나레이션 재설정 이후 전달로 기능 수정
        //Manager_Narr.Set_Audio_seq_narration(Seq_narration);
    }

    //Emoji icon 1,2,3 init
    void Init_Icon_array()
    {
        Shape_array = new GameObject[Main_Icon_1.transform.childCount];

        for (int i = 0; i < Main_Icon_1.transform.childCount; i++)
        {
            Shape_array[i] = Main_Icon_1.transform.GetChild(i).gameObject;
        }

        //GenerateShapeOrder();
        //GeneratePreSelectedShapes();

        //Manager_Anim.Init_Icon_array();
    }

    void init_Prefab()
    {
        Shape_prefabs = Resources.LoadAll<GameObject>("EA005/prefab");
    }

    //#1. 생성되는 리스트 수 조절을 위해 pool에 있는 요소를 수정해야함
    //ex. 5개일 경우 4번까지
    void GenerateShapeOrder()
    {
        shapeOrder.Clear();
        List<int> pool = new List<int> { 0, 1, 2, 3, 4 };

        while (pool.Count > 0)
        {
            int randIndex = Random.Range(0, pool.Count);
            shapeOrder.Add(pool[randIndex]);
            pool.RemoveAt(randIndex);
        }
    }

    //#2. 꽝 개수 조절을 위해 전체 시퀀스 대비 몇개가 필요한지 수정 필요
    //전체 돌리는 횟수가 10번, 실제 나와야할 정답 4개 일 경우 꽝은 "6"개
    int blankCount = 6; 

    //#3. 사전 결정된 돌림판 선택 배열 만들기
    void GeneratePreSelectedShapes()
    {
        preSelectedShapes.Clear();

        List<int> tempList = new List<int>(shapeOrder); // 모양 순서 복사
        List<int> blanksToInsert = new List<int>();
        for (int i = 0; i < blankCount; i++) blanksToInsert.Add(-1);

        int totalLength = tempList.Count + blanksToInsert.Count; // 전체 배열 길이
        List<int> possiblePositions = new List<int>();

        // 마지막 인덱스는 무조건 도형이 배치되어야 하므로 제외
        for (int i = 0; i < totalLength - 1; i++)
            possiblePositions.Add(i);

        // 무작위로 blankCount개의 삽입 위치 선택
        List<int> blankPositions = new List<int>();
        while (blankPositions.Count < blankCount)
        {
            int rand = possiblePositions[Random.Range(0, possiblePositions.Count)];
            if (!blankPositions.Contains(rand))
                blankPositions.Add(rand);
        }

        blankPositions.Sort();

        int shapeIndex = 0;
        for (int i = 0; i < totalLength; i++)
        {
            if (blankPositions.Contains(i))
                preSelectedShapes.Add(-1); // 꽝
            else
            {
                preSelectedShapes.Add(tempList[shapeIndex]);
                shapeIndex++;
            }
        }

        // 마지막이 꽝이면 모양으로 바꾸기
        if (preSelectedShapes[preSelectedShapes.Count - 1] == -1)
        {
            for (int i = preSelectedShapes.Count - 2; i >= 0; i--)
            {
                if (preSelectedShapes[i] != -1)
                {
                    preSelectedShapes[preSelectedShapes.Count - 1] = preSelectedShapes[i];
                    preSelectedShapes[i] = -1;
                    break;
                }
            }
        }
    }
    void Init_Effectarray()
    {
        Effect_array = new GameObject[Game_effect.transform.childCount];

        for (int i = 0; i < Game_effect.transform.childCount; i++)
        {
            Effect_array[i] = Game_effect.transform.GetChild(i).gameObject;
        }
    }

    public Manager_Seq_14 Get_managerseq()
    {
        return Manager_Seq;
    }

    //shape order 5개 생성된것을 가지고 원래 있는 Text 아래의 자식 오브젝트의 순서를 재정렬
    //순서는 0123 은 유지하고 0: 4-6, 1: 7-9, 2: 10-12, 3: 13-15, 4: 16-18 까지 그룹으로 세팅을 변경함
    //각각 저장 배열을 재정렬하는 배열을 새로 만듬
    void Rearrange_Text()
    {
        int Temp_shapenum;

        for(int i = 0; i < 5; i++)
        {
            
                Temp_shapenum = shapeOrder[i];

            Seq_text[3 * i + 5] = seq_text[3 * Temp_shapenum + 5];
            Seq_text[3 * i + 6] = seq_text[3 * Temp_shapenum + 6];
        }
    }

    void Rearrange_Narration()
    {
        int Temp_shapenum;

        for (int i = 0; i < 5; i++)
        {
            Temp_shapenum = shapeOrder[i];

            Seq_narration[3 * i + 5] = seq_narration[3 * Temp_shapenum + 5];
            Seq_narration[3 * i + 6] = seq_narration[3 * Temp_shapenum + 6];
        }

        Manager_Narr.Set_Audio_seq_narration(Seq_narration);
    }
}
