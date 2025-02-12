using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Manager_obj_3 : MonoBehaviour
{
    //���⿡�� �������� ����� �޾Ƽ� �ϰ� ������ ���ٵ�
    //�ᱹ �߿��� text�� �ʿ��� ������Ʈ�� ������ ������ ���ְ�
    //seq�� ���������� �ʿ��� ������Ʈ�� ������ ��������

    //������� �����ϴ� �͵� ��� �ϸ� ������
    //������ ����� �ϳ��ϳ� �����ؼ� �������°ɷ� �ߴµ�

    public static Manager_obj_3 instance = null;
    // Start is called before the first frame update

    //Camera
    public GameObject Main_Camera;
    public GameObject Camera_position;


    //UI Text,Msg
    public GameObject UI_Text;
    public GameObject UI_Message;
    public GameObject Panel;

    //Fruit
    public GameObject Fruit_position;

    //public GameObject Main_Box;
    public GameObject Main_Box;
    public GameObject Box_position;
    public GameObject Game_effect;

    //Eventsystem
    public GameObject Eventsystem;


    private Manager_Anim_3 Manager_Anim;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;

    public Manager_Seq_3 manager_seq;
    public GameObject Btn_Next;

    [Header("[ COMPONENT CHECK ]")]
    public GameObject[] Fruit_prefabs;
    public Sprite[] Fruit_textsprite;

    //��� ������ ������ �����ϰ� ����� �� ����
    //CHECK â���� ���� �� ������ Ȯ�� ����
    public AudioClip[] Seq_narration;
    public AudioClip[] Msg_narration;
    public AudioClip[] Msg_narration_eng;
    public Sprite[] Msg_textsprite;
    public Sprite[] Msg_textsprite_eng;
    public GameObject[] Effect_array;

    //��� �׽�Ʈ ����
    //�ٱ��� �� ���� ���������� ���� �������� �׽�Ʈ
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
        //�̺�Ʈ �ý����� �׳� �ϳ� �Ҵ� ���ִ� �ɷ�

        Manager_Text = this.gameObject.GetComponent<Manager_Text>();
        Manager_Anim = this.gameObject.GetComponent<Manager_Anim_3>();
        Manager_Narr = this.gameObject.GetComponent<Manager_Narr>();
        manager_seq = this.gameObject.GetComponent<Manager_Seq_3>();

        init_Audio();
        init_Text();
        init_Prefab();
        Init_Effectarray();
    }

    //������� ������ ��ȣ�� ���缭 �־��ָ� ����
    void init_Text()
    {
        Fruit_textsprite = Resources.LoadAll<Sprite>("EA003/sprite_message");
        Msg_textsprite_eng = Resources.LoadAll<Sprite>("EA003/sprite_message_eng");

        if (Fruit_textsprite.Length > 0)
        {
            Debug.Log($"�� {Fruit_textsprite.Length}���� ����� Ŭ���� �ε�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("����� Ŭ���� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        }

        Msg_textsprite = new Sprite[20];
        for (int i = 0; i < 20; i++)
        {
            Msg_textsprite[i] = Fruit_textsprite[i + 14];
        }

        //��ü �Ҵ� �޾ƿ��� �������� �ش��ϴ� ��ũ��Ʈ�� ������
        Manager_Text.Init_UI_text(UI_Text, UI_Message, Panel);
    }
    void init_Audio()
    {
        //
        AudioClip[] Temp_Seq_narration;
        AudioClip[] Temp_Msg_narration;

        Temp_Msg_narration = Resources.LoadAll<AudioClip>("EA003/audio_message");
        Msg_narration = new AudioClip [Temp_Msg_narration.Length];

        Msg_narration_eng = Resources.LoadAll<AudioClip>("EA003/audio_message_eng");

        //1. ���谡 �� �Ǿ��� -> �׷��� �׳� ó������ �� �ְ� ���� �ȿ��� �ٽ� ���ִ� �� ���� �׳� �� ���°ɷ� �ϰ�
        //2. ���谡 �߰��� �ٲ���� -> �׷��� �׳� �ϳ��� ���� �־��ִ� �ɷ�
        Msg_narration[0] = Temp_Msg_narration[0];
        Msg_narration[1] = Temp_Msg_narration[1];
        Msg_narration[2] = Temp_Msg_narration[2];
        Msg_narration[3] = Temp_Msg_narration[3];
        Msg_narration[4] = Temp_Msg_narration[16];
        Msg_narration[5] = Temp_Msg_narration[17];
        Msg_narration[6] = Temp_Msg_narration[18];
        Msg_narration[7] = Temp_Msg_narration[19];
        Msg_narration[8] = Temp_Msg_narration[12];
        Msg_narration[9] = Temp_Msg_narration[13];
        Msg_narration[10] = Temp_Msg_narration[14];
        Msg_narration[11] = Temp_Msg_narration[15];
        Msg_narration[12] = Temp_Msg_narration[4];
        Msg_narration[13] = Temp_Msg_narration[5];
        Msg_narration[14] = Temp_Msg_narration[6];
        Msg_narration[15] = Temp_Msg_narration[7];
        Msg_narration[16] = Temp_Msg_narration[8];
        Msg_narration[17] = Temp_Msg_narration[9];
        Msg_narration[18] = Temp_Msg_narration[10];
        Msg_narration[19] = Temp_Msg_narration[11];

        //�̹� ������ ������ �ȿ��� ���� ������ �ٽ� ���������ʱ� ����
        Temp_Seq_narration = Resources.LoadAll<AudioClip>("EA003/audio_seq");
        Seq_narration = new AudioClip[18];

        //�ش� �ϴ� ������� �Է�
        Seq_narration[0] = Temp_Seq_narration[0];
        Seq_narration[1] = Temp_Seq_narration[3];
        Seq_narration[2] = Temp_Seq_narration[1];
        Seq_narration[3] = Temp_Seq_narration[5];
        Seq_narration[4] = Temp_Seq_narration[1];
        Seq_narration[5] = Temp_Seq_narration[4];
        Seq_narration[6] = Temp_Seq_narration[1];
        Seq_narration[7] = Temp_Seq_narration[6];
        Seq_narration[8] = Temp_Seq_narration[1];
        Seq_narration[9] = Temp_Seq_narration[7];
        Seq_narration[10] = Temp_Seq_narration[1];
        Seq_narration[11] = Temp_Seq_narration[2];
        Seq_narration[12] = Temp_Seq_narration[8];
        Seq_narration[13] = Temp_Seq_narration[10];
        Seq_narration[14] = Temp_Seq_narration[9];
        Seq_narration[15] = Temp_Seq_narration[11];
        Seq_narration[16] = Temp_Seq_narration[12];
        Seq_narration[17] = Temp_Seq_narration[13];

        Manager_Narr.Set_Audio_seq_narration(Seq_narration);

        //��ü �Ҵ� �޾ƿ��� �������� �ش��ϴ� ��ũ��Ʈ�� ������
        Manager_Text.Init_UI_text(UI_Text, UI_Message, Panel);

    }
    void init_Prefab()
    {

        Fruit_prefabs = Resources.LoadAll<GameObject>("EA003/prefab");

    }

    void Init_Effectarray()
    {
        Effect_array = new GameObject[Game_effect.transform.childCount];

        for (int i = 0; i < Game_effect.transform.childCount; i++)
        {
            Effect_array[i] = Game_effect.transform.GetChild(i).gameObject;
        }
    }

    public Manager_Seq_3 Get_managerseq()
    {
        return manager_seq;
    }
}
