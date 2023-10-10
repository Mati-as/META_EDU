using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using KoreanTyper;
using UniRx;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;


public class UndergroundUIManager : MonoBehaviour
{
    enum UI
    {
        HowToPlayA,
        HowToPlayB,
        StoryA,
        StoryB,
        Finish
    }
    
    
    [Header("References")]
    public GroundGameManager gameManager;

    [Header("Tutorial UI Parts")]
    [FormerlySerializedAs("tutorialUICVSGroup")]
    [FormerlySerializedAs("tutorialUICVGroup")]
    [FormerlySerializedAs("tutorialUI")]
  
    [Space(10f)] 
    [Header("Images")]
    public CanvasGroup tutorialUICvsGroup;
    
    [Space(20f)][Header("StoryUIController")]
    
    [SerializeField]
    private UIAudioController _uiAudioController;

    [SerializeField]
    private TMP_Text _storyUITmp;

    [SerializeField] 
    private Transform playerIcon;
    [SerializeField] 
    private Transform playerIconDefault;
    [SerializeField] 
    private Transform playerIconMovePosition;
    
    [Header("Message Settings")]  [Space(10f)]
    private readonly string _firstUIMessage = "가을을 맞아 동물 친구들이 소풍을 왔어요! 함께 친구들을 만나 보러 가볼까요?"; 
    private readonly string  _secondUIMessage = "이제 밤이되어 모든 동물 친구들이 숲속으로 사라졌어요! 친구들을 찾아볼까요?"; 
    private readonly string  _lastUIMessage = "우와! 동물친구들을 모두 찾았어요!"; 
    
    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();
    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }
    
    // public GameObject Gameboard;
    // public GameObject Message_Intro_Howto;
    // public GameObject Message_Intro_Story;
    // public GameObject Message_Endchapter;
    // public GameObject Message_Ready;
    // public GameObject Message_Endgame;


    // private RectTransform GB_transform;
    // private List<Vector3> GB_positionList = new();
    //
    // //TEST
    // private int GB_Listindex = 0;
    // private Vector3 GB_targetVector3;
    // private bool flag_num_1 = false;

    
    private void Awake()
    {
        DOTween.Init();
        tutorialUICvsGroup.alpha = 0;
        tutorialUICvsGroup.DOFade(1, 1);
    }

    private void Start()
    {
        //unirx.subscribe(기존 구독)
        gameManager.UIintroDelayTime.
            Where(_=> (int)gameManager.UIintroDelayTime.Value == 3)
            .Subscribe(_ => SetUIIntroUsingUniRx())
            .AddTo(this); 
        
        // GB_transform = Gameboard.GetComponent<RectTransform>();
        // Init_GBPosition();
        // Set_GBPosition(1);
        //Move_BGPosition(2);
    }

    void Update()
    {
        // if (flag_num_1 == true)
        // {
        //     GB_transform.localPosition = GB_transform.localPosition
        //         = Vector3.Lerp(GB_transform.localPosition, GB_targetVector3, 0.5f * Time.deltaTime);
        //
        //     float distance = Vector3.Distance(GB_transform.localPosition, GB_targetVector3);
        //     if (distance < 0.01f)
        //     {
        //         GB_transform.localPosition = GB_targetVector3;
        //         flag_num_1 = false;
        //     }
        // }

    }

    [SerializeField] private float cameraMoveTime;
    private float cameraMoveElapsed;
    private IEnumerator CameraMoveCoroutine()
    {
        cameraMoveElapsed = 0f;

        while (true)
        {
            yield return null;
           // Lerp2D.EaseInQuart()
        }
       
       
    }

    private void Init_GBPosition()
    {
        // GB_positionList.Add(new Vector3(0, 0, 0));
        // GB_positionList.Add(new Vector3(0, 1080, 0));
        // GB_positionList.Add(new Vector3(-1920, 1080, 0));
        // GB_positionList.Add(new Vector3(-3840, 1080, 0));
        // GB_positionList.Add(new Vector3(-5760, 1080, 0));
        //
        // //Debug.Log(BG_positionList.Count);
    }
    private void Set_GBPosition(int number)
    {
        // if (number < GB_positionList.Count)
        // {
        //     GB_targetVector3 = GB_positionList[number];
        // }
        // else
        // {
        //   
        //     GB_targetVector3 = GB_positionList[1];
        // }
    }

  
    public void Move_GBPosition()
    {
     
    }


    public static float INTRO_UI_DELAY;
    public static float INTRO_UI_DELAY_SECOND;
    public GameObject howToPlayUI;
    private void SetUIIntroUsingUniRx()
    {
        Observable.Timer(TimeSpan.FromSeconds(INTRO_UI_DELAY))
            .Do(_ =>
            {
                howToPlayUI.SetActive(true);
                Debug.Log("Second introduction message.");
                // 첫 번째 메시지를 비활성화하고 두 번째 메시지를 활성화
                // Message_Intro_Howto.SetActive(false);
                // Message_Intro_Story.SetActive(true);
            });
        
    }

    public void PlayIntroMessage()
    {

    }

    public void PlayEndChapterMessage()
    {
        Debug.Log("END CHAPTER CHECK");

    }

    public void PlayFinishMessage()
    {
        Debug.Log("FINISH CHECK");
     
    }

    
}