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

    //�ߺ�Ŭ������
    private bool _isClickable = true;

    // ���� ���� ------------------------------------------------------------------------

    //�츮�͵� �ߺ� ���� ���� �ؾ��ϴµ�
    //�׸��� �츮�͵� ȿ�� ����Ʈ�� ȿ���� ����ؾ��ϴµ�

    public bool Eng_MODE = false;

    private Manager_Anim_3 Manager_Anim;
    private Manager_Text Manager_Text;
    private Manager_Narr Manager_Narr;

    private GameObject Eventsystem;

    public bool toggle = false;


    //���� ����
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

    //������ ��� ���������� ������ ������ �Ǵ� �ɷ��ϰ�
    //������ �������� ������ ��ũ��Ʈ�� �����س���


    //���õ� ���� ����Ʈ
    // Ex) Orange, Carrot -> 1,0
    public GameObject[] fruitPrefabs; // ���� ������

    private GameObject Main_Box;


    private GameObject[] Main_Box_array;

    public FruitColor mainColor; // ���� ����
    public int selectedFruitCount = 0;

    private int round = 0; // ���� ���� ȸ��
    private int maxRounds = 5; // ������ �ִ� ȸ��

    private const int MaxFruits = 16;
    private const int MaxFruitsinColor = 4;


    [Header("[ COMPONENT CHECK ]")]

    public int Content_Seq = 0;
    //���� ���� ó�� ��ſ� 0���� ����
    public float Sequence_timer = 0f;
    //�ð�, ���� ����?

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
            //���� �ִ� ���� ���� ����
            //��� �ٱ��� �Ҵ� �޾Ұ�
            //������� R - P -G -O -Y
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

        //1,3,5,7,9 ���� ��� ����
        //2,4,6,8,10 ���� ���� �� �ؽ�Ʈ
        //12 ~ 16 ���� �о��ִ� ����
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
            //��ġ Ȱ��ȭ
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
        //���� �о��ֱ�
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

        //���� ���� 2��
        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 0);
        Generate_fruit(colorIndex * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), 1);

        for (int i = 0; i < 5; i++)
        {
            //��ü ���� 5��
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
        //���� �ٱ��Ϸ� �̵�
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

    //Ŭ�� ��ü �κ�
    public void Click(GameObject plate_Fruit, int num_fruit, int num_table)
    {

        if (num_fruit / 4 == (int)mainColor)
        {
            //���� ���̺� �ִ� ���� �߿� �ش� ���� ����
            Manager_Anim.Devide_Seq_fruit(plate_Fruit, selectedFruitCount);
            plate_Fruit.transform.SetSiblingIndex(selectedFruitCount);

            Manager_Text.Changed_UI_message_c3(num_table, num_fruit, Eng_MODE);

            Generate_fruit(UnityEngine.Random.Range(0, MaxFruits), num_table);

            selectedFruitCount++;

            // 5�� ���� �� ó��
            if (selectedFruitCount == maxRounds)
            {
                Debug.Log("���õ� ���� 5�� �Ϸ�!");
                selectedFruitCount = 0; // �ʱ�ȭ

                Content_Seq += 1;
                toggle = true;
            }
        }
        //���� ���򿡼� ���� ���� ���
        else
        {
            Manager_Anim.Inactive_Seq_fruit(plate_Fruit, 0f);

            Generate_fruit((int)mainColor * 4 + UnityEngine.Random.Range(0, MaxFruitsinColor), num_table);

        }
        //�̰� �ѹ��̸� ��� ���µ� ������ �̸� ������ ���� �� ���� �� ����
        //�������� ȿ���� ����Ʈ�� ���� �ۼ��س��°ɷ� �ϰ� �װɷ� ������ ������ ����ȭ �ؼ� �������°ɷ� �ϴ°� ���� �� ����
        //�׷� �ᱹ ����� string���°� �ƴ϶� �������·� ���� ȣ�� �� �� ������
        //�ᱹ Ŭ���ϴ� ȿ������ ���� �� ���������� ���� �� ������ ����Ǵ� �켱�� ��ü ȿ������ �ε��ϰ� �� �߿� �ش� ��������

        //Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/�⺻������/Sandwich/Click_" + randomChar, 0.3f);
    }
    //�ٽ� �����°� ����� �������� ����
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
        //������ ��Ȱ��ȭ ��ä�� �޾ƿ��� �˾� �ִϸ��̼ǿ��� Ȱ��ȭ��
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
        Init_Game_fruit(UnityEngine.Random.Range(0, 5)); // �� ���� �������� �ʱ�ȭ
        StartCoroutine(ResetAfterTime(time)); // ��� �ݺ�
    }

    //�Ʒ����� ���� ��ũ��Ʈ ����
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

        if(_raycastHits.Length ==0 )Logger.Log("클릭된 것이 아무것도 없습니다. ------------------");
        
        foreach (var hit in _raycastHits)
        {
            Debug.Log(hit.transform.gameObject.tag);
            Debug.Log(hit.transform.gameObject.name);

            if (hit.transform.gameObject.CompareTag("MainObject"))
            {
                Debug.Log("Fruit Clicked!");
                //���Ⱑ �������� Ȯ�� �ʿ���
                Selected_fruit = hit.transform.gameObject;
                Selected_fruit.GetComponent<Clicked_fruit>().Click();
                return;
            }
            
            //�Ҹ� ���°� Ȯ���Ͽ���
            var randomChar = (char)Random.Range('A', 'F' + 1);
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/�⺻������/Sandwich/Click_" + randomChar,0.3f);

        
        }
    }

    protected override void OnStartButtonClicked()
    {
        //Update �����
        toggle = true;
        base.OnStartButtonClicked();
    }
    //���� Ŭ���� �ι��� �߻��ϴ� ������ ����?

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
