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
    public GameObject UI_Shapeicon;
    public GameObject UI_Status;

    //Shape Icon
    public GameObject Main_Shapeicon_1;  //전반부 게임 도형
    public GameObject [] Main_Shapeicon_2;  //중반부 각 회차 도형 어레이의 부모 오브젝트
    public GameObject Main_Shapeicon_3;  //결과 도형
    public GameObject Main_Shapeicon_position; //중반부 게임 도형 사전 위치

    public Transform wheel; // 원판의 Transform
    public GameObject Button_Spin;

    public List<string> Seq_shape = new List<string> { "RECTANGLE", "Empty", "TRIANGLE", "Empty", "CIRCLE", "Empty", "STAR", "HEART" };

    //[common, EDIT] Manager
    private Manager_Anim_14 Manager_Anim;
    private Manager_Seq_14 Manager_Seq;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;
    public GameObject Btn_Next;

    private int MaxShape = 5;

    private List<int> shapeOrder = new List<int>();           // 0~4 도형 순서  "RECTANGLE", "TRIANGLE", "CIRCLE", "STAR", "HEART"
    public List<int> preSelectedShapes = new List<int>();    // 실제 사용할 배열 (꽝 포함)

    [Header("[ COMPONENT CHECK ]")]
    private int[] Number_of_Eachemoji;
    public int[] Number_Gameshape = { 0,0,0,0,0};
    public GameObject[] Shape_prefabs;
    public GameObject[] Shape_array;
    public GameObject[] Message_array;
    public GameObject[] Result_array;

    public AudioClip[] Seq_narration;
    public GameObject[] Seq_text;
    public AudioClip[] Result_narration;
    public AudioClip[] READY_narration;
    public AudioClip[] Msg_narration;
    public AudioClip[] Audio_effect_array;

    private AudioClip[] seq_narration;
    private GameObject[] seq_text;

    //public AudioClip[] Msg_narration_eng;
    //public Sprite[] Msg_textsprite;
    //public Sprite[] Msg_textsprite_eng;
    public GameObject[] Effect_array;

    //[EDIT] Object array
    public GameObject[] Main_Shapeicon_1_array; //전반부 읽기 게임
    public GameObject[] Main_Shapeicon_3_array; //중반부 게임 결과
    public GameObject[] UI_Shapeicon_array;     //중반부 아이콘 갯수
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
        Init_Shapeicon_array();

        GenerateShapeOrder();
        GeneratePreSelectedShapes();

        Rearrange_Text();
        Rearrange_Narration();
    }
    void init_Prefab()
    {
        Shape_prefabs = Resources.LoadAll<GameObject>("EA014/prefab");
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

        Message_array = new GameObject[UI_Message.transform.childCount];

        for (int i = 0; i < UI_Message.transform.childCount; i++)
        {
            Message_array[i] = UI_Message.transform.GetChild(i).gameObject;
        }

        Result_array = new GameObject[UI_Result.transform.childCount];

        for (int i = 0; i < UI_Result.transform.childCount; i++)
        {
            Result_array[i] = UI_Result.transform.GetChild(i).gameObject;
        }

        
        UI_Shapeicon_array = new GameObject[UI_Shapeicon.transform.childCount];

        for (int i = 0; i < UI_Shapeicon.transform.childCount; i++)
        {
            UI_Shapeicon_array[i] = UI_Shapeicon.transform.GetChild(i).gameObject;
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
        Audio_effect_array = Resources.LoadAll<AudioClip>("EA014/audio_Effect");
        //Animal_effect = Resources.LoadAll<AudioClip>("EA004/audio_effect");

        //나레이션 재설정 이후 전달로 기능 수정
        //Manager_Narr.Set_Audio_seq_narration(Seq_narration);
    }

    //Shapeicon 1,2,3 init
    //Shape icon 1 전반부, 후반부 도형 게임 어레이
    //Shape icon 2 중반부 게임 위치 어레이
    void Init_Shapeicon_array()
    {
        Main_Shapeicon_1_array = new GameObject[Main_Shapeicon_1.transform.childCount];

        for (int i = 0; i < Main_Shapeicon_1.transform.childCount; i++)
        {
            Main_Shapeicon_1_array[i] = Main_Shapeicon_1.transform.GetChild(i).gameObject;
        }

        Main_Shapeicon_3_array = new GameObject[Main_Shapeicon_3.transform.childCount];

        for (int i = 0; i < Main_Shapeicon_3.transform.childCount; i++)
        {
            Main_Shapeicon_3_array[i] = Main_Shapeicon_3.transform.GetChild(i).gameObject;
            Main_Shapeicon_3_array[i].SetActive(false);
        }

        for(int j = 0; j < 5; j++)
        {
            int index = j;
            int num = 10 + j * 2;
            //각 회차 도형 배열 사전 설정 및 프리팹 생성을 위한 순서 배열 생성
            Number_of_Eachemoji = GenerateSimpleRandomArray(5, 30, index, num);
            List<int> number = GetRandomizedIndices(Number_of_Eachemoji);
            for (int i = 0; i < Main_Shapeicon_position.transform.childCount; i++)
            {
                Generate_shape(number[i], i, index);
            }
            //각 게임 타겟 도형 갯수 저장해주는 부분
            Number_Gameshape[index] = Number_of_Eachemoji[index];

            //(구현) 근데 마지막으로 확인해야할게 하나 더 있어, 순서를 랜덤으로 했는데 그에 맞춰서 인덱스 번호도 맞춰줘야하는데 그건 어떻게?
            //네모 부터 순서대로 되는데 마지막으로 활성화하는 순서를 바꾸면 될듯?
        }

        Manager_Anim.Init_Icon_array();
    }
    //순서대로 배열 사이즈, 각 인덱스 총합, 고정할 인덱스 번호, 고정할 인덱스 수
    int[] GenerateSimpleRandomArray(int size, int total, int fixedIndex, int fixedValue)
    {
        int[] array = new int[size];
        array[fixedIndex] = fixedValue;

        int remaining = total - fixedValue;
        int slots = size - 1;

        // 1. 슬록 구간별 랜덤 분할을 위한 포인트 리스트
        List<int> points = new List<int> { 0, remaining };
        for (int i = 0; i < slots - 1; i++)
        {
            points.Add(Random.Range(0, remaining + 1));
        }

        // 2. 정렬해서 간격으로 분할
        points.Sort();

        List<int> parts = new List<int>();
        for (int i = 1; i < points.Count; i++)
        {
            parts.Add(points[i] - points[i - 1]);
        }

        // 3. parts 값을 array에 삽입 (고정 인덱스 제외)
        for (int i = 0, j = 0; i < size; i++)
        {
            if (i == fixedIndex) continue;
            array[i] = parts[j++];
        }

        return array;
    }
    public List<int> GetRandomizedIndices(int[] counts)
    {
        List<int> indexPool = new List<int>();

        // 각 인덱스를 개수만큼 추가
        for (int i = 0; i < counts.Length; i++)
        {
            for (int j = 0; j < counts[i]; j++)
            {
                indexPool.Add(i);
            }
        }

        // 리스트 섞기 (Fisher-Yates)
        for (int i = 0; i < indexPool.Count; i++)
        {
            int rnd = Random.Range(i, indexPool.Count);
            int temp = indexPool[i];
            indexPool[i] = indexPool[rnd];
            indexPool[rnd] = temp;
        }

        return indexPool;
    }

    //각 회차별 게임 도형 배열 세팅
    //도형 번호, 위치, 배열 오브젝트(부모)
    void Generate_shape(int num_emoji, int num_table, int num_Target)
    {
        //그냥 0 ~ 마지막 번호까지 for문돌리고
        //각 테이블 위치에 랜덤으로 표정 프리팹을 배치시킴
        GameObject emoji = Instantiate(Manager_Obj_14.instance.Shape_prefabs[num_emoji]);
        Transform pos = Main_Shapeicon_position.transform.GetChild(num_table);

        emoji.transform.SetParent(Main_Shapeicon_2[num_Target].transform);
        emoji.transform.localScale = Vector3.zero;
        emoji.transform.localPosition = pos.localPosition;
        emoji.transform.localRotation = Quaternion.Euler(0f, 90f, -90f);

        emoji.GetComponent<Clicked_Block_14>().Set_Number_emoji(num_emoji);
        emoji.GetComponent<Clicked_Block_14>().Set_Number_table(num_table);
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

    public int Get_numbermaxshape(int num)
    {
        return Number_Gameshape[num];
    }

    public GameObject[] Get_GameShapearray(int num)
    {

        GameObject[] Array = new GameObject[Main_Shapeicon_2[num].transform.childCount];

        for (int i = 0; i < Main_Shapeicon_2[num].transform.childCount; i++)
        {
            Array[i] = Main_Shapeicon_2[num].transform.GetChild(i).gameObject;
        }

        return Array;
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

        Manager_Text.Init_UI_text_array(Seq_text);
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
