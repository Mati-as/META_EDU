using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_obj_2 : MonoBehaviour
{
    public static Manager_obj_2 instance = null;
    // Start is called before the first frame update

    //Camera, 여기는 아마 공통
    public GameObject Main_Camera;
    public GameObject Camera_position;

    //Eventsystem, 여기까지 아마 공통
    public GameObject Eventsystem;
    // Start is called before the first frame update

    //UI Text,Msg, 여기도 공통
    public GameObject UI_Text;
    public GameObject UI_Message;
    public GameObject Panel;


    //Animal
    public GameObject Main_Animal;
    public GameObject Animal_position;
    public GameObject Game_effect;


    //씬마다 변경되어야하는 부분
    private Manager_Anim_2 Manager_Anim;
    public Manager_Seq_2 manager_seq;

    //여기도 공통
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;


    //텍스트 이미지, 나레이션은 위치가 정해져있으므로 따로 정의하지 않음
    //효과음만 따로 저장
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

        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_2>();
        manager_seq = this.gameObject.GetComponent<Manager_Seq_2>();

        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();

        init_Audio();
        init_Text();
        Init_Animalarray();
        Init_Effectarray();
    }

    void init_Text()
    {
        //여기의 경우 그걸 넣어줄 필요가 있었나?

        //전체 할당 받아오고 마지막에 해당하는 스크립트에 던져줌
        Manager_Text.Init_UI_text(UI_Text, UI_Message, Panel);
    }
    void init_Audio()
    {
        //
        AudioClip[] Temp_Seq_narration;
        AudioClip[] Temp_Msg_narration;


        //이미 순서가 맞춰져서 굳이 추가 수정하지 않음
        Msg_narration = Resources.LoadAll<AudioClip>("EA002/audio_message");
        Seq_narration = Resources.LoadAll<AudioClip>("EA002/audio_seq");
        Animal_effect = Resources.LoadAll<AudioClip>("EA002/audio_effect");

        //전체 할당 받아오고 마지막에 해당하는 스크립트에 던져줌
        Manager_Narr.Set_Audio_seq_narration(Seq_narration);

    }

    void Init_Animalarray()
    {
        Main_Animal_array = new GameObject[Main_Animal.transform.childCount];

        for (int i = 0; i < Animal_position.transform.childCount; i++)
        {
            Main_Animal_array[i] = Main_Animal.transform.GetChild(i).gameObject;
        }

        //전체 할당 받아오고 마지막에 해당하는 스크립트에 던져줌
        Manager_Anim.Init_Animalarray();
    }
    void Init_Effectarray()
    {
        Effect_array = new GameObject[Game_effect.transform.childCount];

        for (int i = 0; i < Game_effect.transform.childCount; i++)
        {
            Effect_array[i] = Game_effect.transform.GetChild(i).gameObject;
        }

        //전체 할당 받아오고 마지막에 해당하는 스크립트에 던져줌
        Manager_Anim.Init_Animalarray();
    }

    public Manager_Seq_2 Get_managerseq()
    {
        return manager_seq;
    }
}
