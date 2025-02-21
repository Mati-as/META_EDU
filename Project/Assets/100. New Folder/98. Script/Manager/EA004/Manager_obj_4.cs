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


    //Animal
    public GameObject Main_Animal;
    public GameObject Animal_position;
    public GameObject Game_effect;


    //[common, EDIT] Manager
    private Manager_anim_4 Manager_Anim;
    public Manager_SEQ_4 Manager_Seq;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;


    //[EDIT]
    public Sprite[] Animal_text;
    public Sprite[] Animal_textsprite;
    public AudioClip[] Animal_effect;

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Main_Animal_array;
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
        Init_Animalarray();
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

    //¾Æ¸¶ Ç¥Á¤À¸·Î ¹Ù²î¸é °Â³×¸¦ ÇØÁà¾ßÇÒµí
    void Init_Animalarray()
    {
        Main_Animal_array = new GameObject[Main_Animal.transform.childCount];

        for (int i = 0; i < Animal_position.transform.childCount; i++)
        {
            Main_Animal_array[i] = Main_Animal.transform.GetChild(i).gameObject;
        }
        //¾Æ¸¶ Ç¥Á¤À¸·Î ¹Ù²î¸é °Â³×¸¦ ÇØÁà¾ßÇÒµí
        //Manager_Anim.Init_Animalarray();
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
