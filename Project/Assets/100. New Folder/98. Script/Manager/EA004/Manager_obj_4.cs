using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_obj_4 : MonoBehaviour
{
    public static Manager_obj_4 instance = null;
    // Start is called before the first frame update

    //[common] Camera
    //private GameObject Main_Camera;
    //private GameObject Camera_position;
    public GameObject UI_Text;
    public GameObject UI_Message;
    public GameObject UI_Panel;
    public GameObject Game_effect;

    //Emoji icon
    public GameObject Main_Icon_1;
    public GameObject Main_Icon_2;
    public GameObject Main_Icon_3;
    public GameObject Icon_buttion_position;
    public GameObject BG;
    public Sprite White;
    public Sprite Yellow;


    //icon_1�� ��� Ŭ���ؼ� �ؽ�Ʈ �����̼�, ȿ���� ���
    //icon_2�� ��� Ŭ�� X
    //icon_3�� ��� Ŭ�� �ʿ�

    //[common, EDIT] Manager
    private Manager_anim_4 Manager_Anim;
    public Manager_SEQ_4 Manager_Seq;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;
    public GameObject Btn_Next;

    //[EDIT]
    public Sprite[] Animal_text;
    public Sprite[] Animal_textsprite;
    public AudioClip[] Animal_effect;

    private int MaxEmoji = 5;

    [Header("[ COMPONENT CHECK ]")]
    public int[] Number_of_Eachemoji;

    public GameObject[] Emoji_prefabs;
    public GameObject[] Main_Icon_1_array;
    public GameObject[] Main_Icon_2_array;
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

        Manager_Anim = this.gameObject.GetComponent<Manager_anim_4>();
        Manager_Seq = this.gameObject.GetComponent<Manager_SEQ_4>();

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

        Msg_narration = Resources.LoadAll<AudioClip>("EA004/audio_message");
        Seq_narration = Resources.LoadAll<AudioClip>("EA004/audio_seq");
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

        Main_Icon_2_array = new GameObject[Icon_buttion_position.transform.childCount];
        Number_of_Eachemoji = new int[5] { 0, 0, 0, 0, 0 };
        for (int i = 0; i < 5; i++)
        {
            //�� ���ڸ� 1���� �������� ������ ����
            Number_of_Eachemoji[i] += 1;
            Generate_emoji(i, i);
        }

        for (int i = 5; i < Icon_buttion_position.transform.childCount; i++)
        {
            int Random_number = UnityEngine.Random.Range(0, MaxEmoji);
            Number_of_Eachemoji[Random_number] += 1;
            Generate_emoji(Random_number, i);
        }

        Main_Icon_3_array = new GameObject[Main_Icon_3.transform.childCount];

        for (int i = 0; i < Main_Icon_3.transform.childCount; i++)
        {
            Main_Icon_3_array[i] = Main_Icon_3.transform.GetChild(i).gameObject;
            Main_Icon_3_array[i].SetActive(false);
        }

        Manager_Anim.Init_Icon_array();
    }

    void init_Prefab()
    {
        Emoji_prefabs = Resources.LoadAll<GameObject>("EA004/prefab");
    }
    void Generate_emoji(int num_emoji, int num_table)
    {
        //�׳� 0 ~ ������ ��ȣ���� for��������
        //�� ���̺� ��ġ�� �������� ǥ�� �������� ��ġ��Ŵ
        GameObject emoji = Instantiate(Manager_obj_4.instance.Emoji_prefabs[num_emoji]);
        RectTransform pos = Icon_buttion_position.transform.GetChild(num_table).GetComponent<RectTransform>();

        emoji.transform.SetParent(Main_Icon_2.transform);
        emoji.GetComponent<RectTransform>().localScale = new Vector3(0f, 0f, 0f);
        emoji.GetComponent<RectTransform>().localPosition = new Vector3(pos.anchoredPosition.x, pos.anchoredPosition.y, 0f);
        emoji.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

        emoji.GetComponent<Clicked_emoji>().Set_Number_emoji(num_emoji);
        emoji.GetComponent<Clicked_emoji>().Set_Number_table(num_table);

        Main_Icon_2_array[num_table] = emoji;
    }
    void Init_Effectarray()
    {
        Effect_array = new GameObject[Game_effect.transform.childCount];

        for (int i = 0; i < Game_effect.transform.childCount; i++)
        {
            Effect_array[i] = Game_effect.transform.GetChild(i).gameObject;
        }
    }

    public Manager_SEQ_4 Get_managerseq()
    {
        return Manager_Seq;
    }
}
