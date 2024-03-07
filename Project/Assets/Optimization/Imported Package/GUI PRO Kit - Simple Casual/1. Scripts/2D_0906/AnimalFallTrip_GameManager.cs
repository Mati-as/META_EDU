using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class AnimalFallTrip_GameManager : IGameManager
{
    private static Dictionary<int, GameObject> animalGameObjectList = new();
    private static Dictionary<int, GameObject> footstepGameObjectList = new();

    public GameObject Animal_group;
    public GameObject Footstep_group;

    public static bool isCameraArrivedToPlay { get; set; }
    public static bool isGameStarted { get; private set; }
    private bool _initialRoundIsReady; //���� ���� ���� ������ ��Ʈ�� �ϱ� ���� �������� �Դϴ�. 
    public static bool isRoundReady { get; private set; }
    public static bool isRoundStarted { get; private set; }
    public static bool isCorrected { get; private set; }
    public static bool isGameFinished { get; private set; }
    public static bool isRoudnFinished { get; private set; }

    /*
     * step -> footstep
     * chapter -> animal
     * level -> BG
     * */

    private static int Step = 0;
    private static int Chapter = 0;
    private static int level = 0;

    //�� ������ ����ó�� �� ���� ���� �κ� ����

    // UI ����� ���� Event ó��
    [Header("UI Events")]
    [Space(10f)]

    [SerializeField]
    private UnityEvent _IntroMessageEvent;

    [SerializeField]
    private UnityEvent _EndofLevelMessageEvent;

    [SerializeField]
    private UnityEvent _finishedMessageEvent;
    //[SerializeField]
    //private UnityEvent _messageInitializeEvent;


    

    // Start is called before the first frame update
    void Start()
    {
        
        SetAnimalIntoDictionaryAndList();
        SetFootstepIntoDictionaryAndList();
        
     

        isGameStarted = true;
        isGameFinished = false;
        isRoundReady = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameStarted && isGameFinished == false)
        {
            if (isRoundReady)
            {
                Debug.Log("���� ����");
                _IntroMessageEvent.Invoke();
                isRoundReady=false;
            }

            if (isRoudnFinished)
            {
                Debug.Log("���� ����");
                _EndofLevelMessageEvent.Invoke();
                isRoudnFinished = false;
            }
        }
        if (isGameFinished)
        {
            Debug.Log("���� ����");
            isGameFinished = false;
            isGameStarted = false;
            _finishedMessageEvent.Invoke();
        }
    }


    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
    }

    private void SetAnimalIntoDictionaryAndList()
    {
        //group�� ����Ǿ��ִ� ������� ����
        for (int i = 0; i < Animal_group.transform.childCount; i++)
        {
            animalGameObjectList.Add(i, Animal_group.transform.GetChild(i).gameObject);
        }
        //Debug.Log(Animal_group.transform.childCount);
    }
    private void SetFootstepIntoDictionaryAndList()
    {
        //group parent�� ����Ǿ��ִ� ������� ����
        for (int i = 0; i < Footstep_group.transform.childCount; i++)
        {
            footstepGameObjectList.Add(i, Footstep_group.transform.GetChild(i).gameObject);
        }
    }
    public static GameObject GetAnimal(int num_chapter)
    {
        return animalGameObjectList[num_chapter];
    }
    public static GameObject GetFootstep(int num_step)
    {
        return footstepGameObjectList[num_step];
    }
    public static int GetStep()
    {
        return Step;
    }
    public static int GetChapter()
    {
        return Chapter;
    }
    public static int GetLevel()
    {
        return level;
    }

    public static void AddStep()
    {
        Step += 1;
    }
    public static void AddChapter()
    {
        Chapter += 1;
    }
    public static void AddLevel()
    {
        level += 1;
    }
    public static void SetisRoudnFinished()
    {
        isRoudnFinished = true;
    }
    public static void SetisGameFinished()
    {
        isGameFinished = true;
    }

    //���� ù ���� ������ ��� ó������ ����ó�� �ʿ�

    //���� �Ŵ����� ������ ���� �뵵�θ� ����Ѵ�
    //UI ȭ�鿡 �����ִ� �뵵�� Ȱ���ϴ°ɷ�

    /*
     * 
     *  ** ���� �� ������ ó������ �غ�� ���¿��� ����
     *  
     * 1. ��������
     *  - ���������� �θ��� ����
     *    
     * 2. ��������
     *  - ���������� �θ��� ����
     *  
     * 3. é�� �� ����(�� ������ ���� �� ��) ����
     *  - �����̼� ���, é�� ����, �޽��� ����� ����
     * 
     * ���ͷ��� ���
     * ** ����
     * 
     * Ŭ���� ���, 
     * ** ���� �̵�, ���� ���� Ȱ��ȭ, UI �޽���
     *
     */

}
