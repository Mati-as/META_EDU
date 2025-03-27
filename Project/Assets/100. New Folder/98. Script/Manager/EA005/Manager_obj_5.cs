using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_obj_5 : MonoBehaviour
{
    public static Manager_obj_5 instance = null;
    // Start is called before the first frame update

    //[common] Camera
    //private GameObject Main_Camera;
    //private GameObject Camera_position;
    public GameObject UI_Text;
    public GameObject UI_Message;
    public GameObject UI_Panel;
    public GameObject Game_effect;

    //Emoji icon
    public GameObject Main_Icon_1;  //이모지 보여주기
    public GameObject Bigsize_emotion;
    public GameObject Main_Icon_2_1P;  //클릭 게임 그룹, 각 개별적으로 클릭
    public GameObject Main_Icon_2_2P;  //클릭 게임 그룹, 각 개별적으로 클릭
    public GameObject Main_Icon_3;      //다 클릭하고 난 다음 큰 이모지 보여주는 부분
    public GameObject Icon_buttion_position;
    public GameObject BG;

    public List<string> Seq_emoji= new List<string> { "Happy", "Sad", "Empty", "Angry", "Sleep", "Empty", "Good", "Laugh" };


    public Sprite White;
    public Sprite Yellow;

    //[common, EDIT] Manager
    private Manager_anim_5 Manager_Anim;
    public Manager_SEQ_5 Manager_Seq;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;
    public GameObject Btn_Next;

    private int MaxEmoji = 6; //완료하고 난 다음 마찬가지로 5개의 이모지를 전부 볼 수 있도록함

    public List<int> emotionOrder = new List<int>();           // 0~5 감정 순서  "Happy", "Sad", "Angry", "Sleep", "Good", "Laugh"
    public List<int> preSelectedEmotions = new List<int>();    // 실제 사용할 배열 (꽝 포함)

    [Header("[ COMPONENT CHECK ]")]
    public int[] Number_of_Eachemoji;

    public GameObject[] Emoji_prefabs;
    public GameObject[] Main_Icon_1_array;
    public GameObject[] Bigsize_emotion_array;

    public GameObject[] Main_Icon_2_1P_array;
    public GameObject[] Main_Icon_2_2P_array;

    public GameObject[] Main_Icon_3_array;
    public AudioClip[] Seq_narration;
    public AudioClip[] Msg_narration;
    public AudioClip[] Msg_narration_eng;
    public Sprite[] Msg_textsprite;
    public Sprite[] Msg_textsprite_eng;
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
        UI_Text = GameObject.Find("UI_Text");
        UI_Message = GameObject.Find("UI_Message");
        //Main_Camera = GameObject.Find("Main Camera");
        //Camera_position = GameObject.Find("Camera_position");
        UI_Panel = GameObject.Find("UI_Panel");
        Game_effect = GameObject.Find("Game_effect");

        Manager_Anim = this.gameObject.GetComponent<Manager_anim_5>();
        Manager_Seq = this.gameObject.GetComponent<Manager_SEQ_5>();

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
        //Animal_effect = Resources.LoadAll<AudioClip>("EA004/audio_effect");

        Manager_Narr.Set_Audio_seq_narration(Seq_narration);
    }

    //Emoji icon 1,2,3 init
    void Init_Icon_array()
    {
        Main_Icon_1_array = new GameObject[Main_Icon_1.transform.childCount];

        for (int i = 0; i < Main_Icon_1.transform.childCount; i++)
        {
            Main_Icon_1_array[i] = Main_Icon_1.transform.GetChild(i).gameObject;
        }

        //Main_Icon_2_array = new GameObject[Icon_buttion_position.transform.childCount];
        //Number_of_Eachemoji = new int[5] { 0, 0, 0, 0, 0 };
        //for (int i = 0; i < 5; i++)
        //{
        //    //각 숫자를 1번씩 랜덤으로 무조건 넣음
        //    Number_of_Eachemoji[i] += 1;
        //    Generate_emoji(i, i);
        //}

        //for (int i = 5; i < Icon_buttion_position.transform.childCount; i++)
        //{
        //    int Random_number = UnityEngine.Random.Range(0, MaxEmoji);
        //    Number_of_Eachemoji[Random_number] += 1;
        //    Generate_emoji(Random_number, i);
        //}

        Bigsize_emotion_array = new GameObject[Bigsize_emotion.transform.childCount];

        for (int i = 0; i < Bigsize_emotion.transform.childCount; i++)
        {
            Bigsize_emotion_array[i] = Bigsize_emotion.transform.GetChild(i).gameObject;
            Bigsize_emotion_array[i].SetActive(false);
        }
        GenerateEmotionOrder();
        GeneratePreSelectedEmotions();

        Manager_Anim.Init_Icon_array();
    }

    void init_Prefab()
    {
        Emoji_prefabs = Resources.LoadAll<GameObject>("EA005/prefab");
    }

    ////생성하는 부분은 각각 1P, 2P로 구분해서 생성함
    //void Generate_emoji(int num_emoji, int num_table)
    //{
    //    //그냥 0 ~ 마지막 번호까지 for문돌리고
    //    //각 테이블 위치에 랜덤으로 표정 프리팹을 배치시킴
    //    GameObject emoji = Instantiate(Manager_obj_4.instance.Emoji_prefabs[num_emoji]);
    //    RectTransform pos = Icon_buttion_position.transform.GetChild(num_table).GetComponent<RectTransform>();

    //    emoji.transform.SetParent(Main_Icon_2.transform);
    //    emoji.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
    //    emoji.GetComponent<RectTransform>().localPosition = new Vector3(pos.anchoredPosition.x, pos.anchoredPosition.y, 0f);
    //    emoji.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

    //    emoji.GetComponent<Clicked_emoji>().Set_Number_emoji(num_emoji);
    //    emoji.GetComponent<Clicked_emoji>().Set_Number_table(num_table);

    //    Main_Icon_2_array[num_table] = emoji;
    //}

    void GenerateEmotionOrder()
    {
        emotionOrder.Clear();
        List<int> pool = new List<int> { 0, 1, 2, 3, 4, 5 };

        while (pool.Count > 0)
        {
            int randIndex = Random.Range(0, pool.Count);
            emotionOrder.Add(pool[randIndex]);
            pool.RemoveAt(randIndex);
        }
    }

    int blankCount = 6; // (확인) 꽝 갯수 조절 위해 수정 필요

    // #2: 사전 결정된 선택 배열 만들기
    void GeneratePreSelectedEmotions()
    {
        preSelectedEmotions.Clear();

        List<int> tempList = new List<int>(emotionOrder); // 감정 순서 복사
        List<int> blanksToInsert = new List<int>();
        for (int i = 0; i < blankCount; i++) blanksToInsert.Add(-1);

        int totalLength = tempList.Count + blanksToInsert.Count; // 전체 배열 길이
        List<int> possiblePositions = new List<int>();

        // 마지막 인덱스는 감정이 들어가야 하므로 제외
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

        int emotionIndex = 0;
        for (int i = 0; i < totalLength; i++)
        {
            if (blankPositions.Contains(i))
                preSelectedEmotions.Add(-1); // 꽝
            else
            {
                preSelectedEmotions.Add(tempList[emotionIndex]);
                emotionIndex++;
            }
        }

        // 마지막이 꽝이면 감정이 되도록 바꾸기
        if (preSelectedEmotions[preSelectedEmotions.Count - 1] == -1)
        {
            for (int i = preSelectedEmotions.Count - 2; i >= 0; i--)
            {
                if (preSelectedEmotions[i] != -1)
                {
                    preSelectedEmotions[preSelectedEmotions.Count - 1] = preSelectedEmotions[i];
                    preSelectedEmotions[i] = -1;
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

    public Manager_SEQ_5 Get_managerseq()
    {
        return Manager_Seq;
    }
}
