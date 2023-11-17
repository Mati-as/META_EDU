using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using 땅속탐험.Utils;
using System.Xml;

public class GroundGameManager : MonoBehaviour
{
    private static GameStart _gameStart;
    private static GameOver _gameover;
    private static StageStart _stageStart;
    private static StageFinished _stageFinished;
    private static NotGameStarted _notGameStarted;

    public New_SoundManager s_soundManager;
    

    private void Init()
    {
        _notGameStarted = new NotGameStarted();
        _gameStart = new GameStart();
        _stageStart = new StageStart();
        _stageFinished = new StageFinished();
        _gameover = new GameOver();
    }

    // GameManager에서만 상태컨트롤 및 상태인스턴스는 StateController만 소유.
    public StateController _stateController { get; private set; }
    public int TOTAL_ANIMAL_COUNT { get; private set; }

  


    #region legacy 예정

    public static bool isGameStarted { get; }
    private bool _initialRoundIsReady; //최초 라운드 시작 이전을 컨트롤 하기 위한 논리연산자 입니다. 
    public static bool isRoundReady { get; }
    public static bool isGameFinished { get; }
    public static bool isRoudnFinished { get; }

    #endregion

    /*
     * step -> footstep
     * chapter -> animal
     * level -> BG
     * */

    private static int Step = 0;
    private static int Chapter = 0;
    private static int level = 0;

    //맨 마지막 예외처리 및 게임 종료 부분 구현
    public ReactiveProperty<float> UIintroDelayTime;
    public ReactiveProperty<bool> isStartButtonClicked; // Tutorial Button 종료 시.. 
    public ReactiveProperty<bool> isStageStartButtonClicked;
    public ReactiveProperty<bool> isGameFinishedRP;


    public static bool isGameFinishedbool = false;
    public static bool isGameStartedbool;

   
    //10/10 초기화 관련 null 문제 해결

    public ReactiveProperty<IState> currentStateRP { get; private set; }


    private void Awake()
    {
        SetResolution(2560, 1440);
        Application.targetFrameRate = 30;
        //---------------------------------------------
        TOTAL_ANIMAL_COUNT = 12;
            
            
        isStartButtonClicked = new ReactiveProperty<bool>(false);
        isStageStartButtonClicked = new ReactiveProperty<bool>(false);
        isGameFinishedRP = new ReactiveProperty<bool>(false);
        _stateController = new StateController();
       

        Init();

        currentStateRP = new ReactiveProperty<IState>();

        currentStateRP.Value = new NotGameStarted();
        currentStateRP.Value.GameState = IState.GameStateList.NotGameStarted;
    }


    private void Start()
    {

        s_soundManager = new New_SoundManager();
        
        s_soundManager.Init();

        // 1. must be without extenstion name
        // 2. must be below resources folder
        s_soundManager.Play(Define.Sound.Bgm,
            "Sound/Underground/Bgm/01. Take It Easy",0.115f);
        
        
        isStartButtonClicked
            .Where(value=>value==true)
            .Subscribe(_ =>
            {
#if UNITY_EDITOR
                Debug.Log("게임시작");
#endif
               
                SetStage(_gameStart);
                isGameStartedbool = true;
                
#if UNITY_EDITOR
                Debug.Log($" 현재 statecontroller의 GameState{_stateController.GameState}");
#endif
               
            });


        isStageStartButtonClicked
            .Where(value=>value==true)
            .Subscribe(_ =>
            {
                SetStage(_stageStart);
#if UNITY_EDITOR
                Debug.Log("스테이지 시작");
                Debug.Log($" 현재 statecontroller의 GameState{_stateController.GameState}");
#endif

            });

        isGameFinishedRP
            .Where(value=>value ==true)
            .Subscribe(_ =>
            {
                
                isGameFinishedbool = true;
                SetStage(_gameover);
               
#if UNITY_EDITOR
                Debug.Log("게임종료");
#endif
                _finishAnimalActivationCoroutine = StartCoroutine(ActivateAllAnimals());
            });
    }

    private Coroutine _finishAnimalActivationCoroutine;
    private void Update()
    {
        _mainElapsedTime += Time.deltaTime;
        _stateController?.Update();
    }

    private void SetStage(BaseState state)
    {
        //Model Area
        _stateController.ChangeState(state);
        _stateController.GameState = state.GameState;
        
        _stateController?.Enter();
        
        //Rx사용을 위한 Presenter Area
        currentStateRP.Value = state;
        currentStateRP.Value.GameState = state.GameState;

       

#if UNITY_EDITOR
        Debug.Log($"현재 게임상태 RP: {currentStateRP.Value.GameState}");
#endif
    }

    private float _mainElapsedTime;


    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
    }

    public List<GameObject> AllAnimals;
    private void SetAnimalIntoDictionaryAndList()
    {
       
        
        // //group에 저장되어있는 순서대로 저장
        // for (var i = 0; i < Animal_group.transform.childCount; i++)
        //     animalGameObjectList.Add(i, Animal_group.transform.GetChild(i).gameObject);
        // //Debug.Log(Animal_group.transform.childCount);
    }

    IEnumerator ActivateAllAnimals()
    {
        yield return new WaitForSeconds(5.5f);
        
        foreach (GameObject obj in AllAnimals)
        {
#if UNITY_EDITOR
            Debug.Log("게임종료 및 동물 표출");
#endif
           
            obj.SetActive(true);
            var scaleFactor = obj.GetComponent<UndergroundAnimalAttachedUIController>().animalMaximizedSize;
            obj.transform.DOScale(scaleFactor, 1.1f);
           
            if (obj.transform.childCount > 0) // 자식 오브젝트가 있는지 확인
            {
                // 랜덤시스템 도입시 여우가 아닌 
                if (obj.name != "여우")
                {
                    Transform lastChild = obj.transform.GetChild(obj.transform.childCount - 1);
                    lastChild.gameObject.SetActive(false);
                }
                
            }
            
        }
    }
}