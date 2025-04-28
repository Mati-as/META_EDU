using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Obj_14 : MonoBehaviour
{
    public static Manager_Obj_14 instance = null;

    //[common] Camera
    [Header("[COMMON] REQUIRED")]
    public GameObject Main_Camera;
    public GameObject Camera_position;
    public GameObject UI_Text;
    public GameObject UI_Message;
    public GameObject UI_Panel;
    public GameObject Game_effect;

    //Emoji icon
    public GameObject Main_Icon_1;  //이모지 보여주기
    public GameObject Bigsize_emotion;
    public Transform wheel; // 원판의 Transform
    public GameObject Button_Spin;
    public GameObject Main_Icon_2_1P;  //클릭 게임 그룹, 각 개별적으로 클릭
    public GameObject Main_Icon_2_2P;  //클릭 게임 그룹, 각 개별적으로 클릭
    public GameObject Main_Icon_3;      //다 클릭하고 난 다음 큰 이모지 보여주는 부분
    public GameObject Icon_buttion_position;
    public GameObject BG;

    public List<string> Seq_emoji = new List<string> { "Happy", "Sad", "Empty", "Angry", "Sleep", "Empty", "Good", "Laugh" };


    public Sprite White;
    public Sprite Yellow;

    //[common, EDIT] Manager
    private Manager_Anim_14 Manager_Anim;
    private Manager_Seq_14 Manager_Seq;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;
    public GameObject Btn_Next;

    private int MaxShape = 5;

    public List<int> shapeOrder = new List<int>();           // 0~4 도형 순서  "RECTANGLE", "CIRCLE", "TRIANGLE", "STAR", "HEART"
    public List<int> preSelectedShapes = new List<int>();    // 실제 사용할 배열 (꽝 포함)

    [Header("[ COMPONENT CHECK ]")]
    public int[] Number_of_Eachshape;

    public GameObject[] Shape_prefabs;
    public GameObject[] Shape_array;

    public AudioClip[] Seq_narration;
    public AudioClip[] Msg_narration;
    public AudioClip[] Msg_narration_eng;
    public Sprite[] Msg_textsprite;
    public Sprite[] Msg_textsprite_eng;
    public GameObject[] Effect_array;

    
    public AudioClip[] Result_narration;
    public AudioClip[] READY_narration;

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
        Init_Effectarray();
        Init_Icon_array();
    }

    void init_Text()
    {
        Manager_Text.Init_UI_text(UI_Text, UI_Message, UI_Panel);
    }
    void init_Audio()
    {
        AudioClip[] Temp_Seq_narration;
        AudioClip[] Temp_Msg_narration;

        Msg_narration = Resources.LoadAll<AudioClip>("EA005/audio_message");
        Seq_narration = Resources.LoadAll<AudioClip>("EA005/audio_seq");
        Result_narration = Resources.LoadAll<AudioClip>("EA005/audio_null");
        READY_narration = Resources.LoadAll<AudioClip>("EA005/audio_emoji");
        //Animal_effect = Resources.LoadAll<AudioClip>("EA004/audio_effect");

        Manager_Narr.Set_Audio_seq_narration(Seq_narration);
    }

    //Emoji icon 1,2,3 init
    void Init_Icon_array()
    {
        Shape_array = new GameObject[Main_Icon_1.transform.childCount];

        for (int i = 0; i < Main_Icon_1.transform.childCount; i++)
        {
            Shape_array[i] = Main_Icon_1.transform.GetChild(i).gameObject;
        }
        GenerateShapeOrder();
        GeneratePreSelectedShapes();

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
}
