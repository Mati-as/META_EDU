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

public class UndergroundUIManager : MonoBehaviour
{
    
  
    [Header("References")]
    public GroundGameManager gameManager;

    [Space(10f)] 
    [Header("Images")]
    public CanvasGroup tutorialUI;
    
    
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
        tutorialUI.DOFade(1, 5);
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
        // flag_num_1 = true;
        // GB_Listindex++; 
        // Set_GBPosition(GB_Listindex);
    }

    // IEnumerator Set_UIIntro()
    // {
    //
    //     Message_Intro_Howto.SetActive(true);
    //
    //     yield return new WaitForSeconds(3f);
    //
    //     Message_Intro_Howto.SetActive(false);
    //     Message_Intro_Story.SetActive(true);
    //
    //     yield return new WaitForSeconds(3f);
    //
    //     Message_Intro_Story.SetActive(false);
    //
    //     Move_GBPosition();
    //     yield break;
    // }

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



        // // 다시 3초 대기
        // .SelectMany(_ => Observable.Timer(TimeSpan.FromSeconds(INTRO_UI_DELAY_SECOND)))
        //
        // .Subscribe(_ => 
        // {
        //     // 두 번째 메시지를 비활성화
        //     Message_Intro_Story.SetActive(false);
        //     Move_GBPosition();
        // })

    }

    // IEnumerator Set_UINextlevel()
    // {
        // Move_GBPosition();
        //
        // yield return new WaitForSeconds(1f);
        //
        // Message_Endchapter.SetActive(true);
        //
        // yield return new WaitForSeconds(3f);
        //
        // Message_Endchapter.SetActive(false);
        // Message_Ready.SetActive(true);
        //
        // yield return new WaitForSeconds(3f);
        //
        // Message_Ready.SetActive(false);
        //
        // yield break;
    // }
    // IEnumerator Set_UIEndgame()
    // {
        // Message_Endgame.SetActive(true);
        //
        // yield return new WaitForSeconds(3f);
        //
        // Message_Endgame.SetActive(false);
        //
        // Move_GBPosition();
        //
        // yield break;
    // }
    public void PlayIntroMessage()
    {
        // Debug.Log("INTRO CHECK");
        // StartCoroutine(Set_UIIntro());
    }

    public void PlayEndChapterMessage()
    {
        Debug.Log("END CHAPTER CHECK");
        // StartCoroutine(Set_UINextlevel());
    }

    public void PlayFinishMessage()
    {
        Debug.Log("FINISH CHECK");
        // StartCoroutine(Set_UIEndgame());
    }

    // public IEnumerator TypeIn(string str, float offset)
    // {
    //     Debug.Log("제시문 하단 종료 코루틴 ....");
    //     instructionTMP.text = ""; // 초기화
    //     yield return new WaitForSeconds(offset); // 1초 대기
    //
    //     var strTypingLength = str.GetTypingLength(); // 최대 타이핑 수 구함
    //     for (var i = 0; i <= strTypingLength; i++)
    //     {
    //         // 반복문
    //         instructionTMP.text = str.Typing(i); // 타이핑
    //         yield return new WaitForSeconds(textPrintingSpeed);
    //     } // 0.03초 대기
    //
    //
    //     yield return new WaitForNextFrameUnit();
    // }
}