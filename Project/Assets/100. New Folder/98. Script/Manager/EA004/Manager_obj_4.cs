using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_obj_4 : MonoBehaviour
{
    public static Manager_obj_4 instance = null;
    // Start is called before the first frame update

    //[common] Camera
    public GameObject Main_Camera;
    public GameObject Camera_position;

    //[common] UI Text, Msg
    public GameObject UI_Text;
    public GameObject UI_Message;
    public GameObject Panel;


    //Emoji icon
    public GameObject Main_Icon_1;
    public GameObject Main_Icon_2;
    public GameObject Main_Icon_3;
    public GameObject Icon_buttion_position;
    public GameObject BG;
    public GameObject Game_effect;


    //icon_1의 경우 클릭해서 텍스트 나레이션, 효과음 재생
    //icon_2의 경우 클릭 X
    //icon_3의 경우 클릭 필요

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

    [Header("[ COMPONENT CHECK ]")]
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

        Manager_Anim = this.gameObject.GetComponent<Manager_anim_4>();
        Manager_Seq = this.gameObject.GetComponent<Manager_SEQ_4>();

        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        init_Audio();
        init_Text();
        Init_Icon_array();
        Init_Effectarray();

    }

    void init_Text()
    {
        Manager_Text.Init_UI_text(UI_Text, UI_Message, Panel);
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

    //표정 아이콘 1,2 초기화
    void Init_Icon_array()
    {
        Main_Icon_1_array = new GameObject[Main_Icon_1.transform.childCount];

        for (int i = 0; i < Main_Icon_1.transform.childCount; i++)
        {
            Main_Icon_1_array[i] = Main_Icon_1.transform.GetChild(i).gameObject;
        }

        Main_Icon_2_array = new GameObject[Main_Icon_2.transform.childCount];

        for (int i = 0; i < Main_Icon_2.transform.childCount; i++)
        {
            Main_Icon_2_array[i] = Main_Icon_2.transform.GetChild(i).gameObject;
        }

        Manager_Anim.Init_Icon_array();
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
