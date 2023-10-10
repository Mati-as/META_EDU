using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UniRx;
using UnityEngine.UI;
using 땅속탐험.Utils;
using Unit = UniRx.Unit;

public class GroundGameManager : MonoBehaviour
{

    private GameStart _gameStart;
    private GameOver _gameover;
    private StageStart _stageStart;
    private StageFinished _stageFinished = new StageFinished();
    private NotGameStarted _notGameStarted = new NotGameStarted();
    
    // GameManager에서만 상태컨트롤 및 상태인스턴스는 StateController만 소유.
    private static StateController _stateController;
    
    
    private static Dictionary<int, GameObject> animalGameObjectList = new();
    private static Dictionary<int, GameObject> footstepGameObjectList = new();
    
    public GameObject Animal_group;
    public GameObject Footstep_group;
    
    
    
    #region legacy 예정
    public static bool isGameStarted { get; private set; }
    private bool _initialRoundIsReady; //최초 라운드 시작 이전을 컨트롤 하기 위한 논리연산자 입니다. 
    public static bool isRoundReady { get; private set; }
    public static bool isGameFinished { get; private set; }
    public static bool isRoudnFinished { get; private set; }
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
   
    //10/10 초기화 관련 null 문제 해결

    public ReactiveProperty<IState> currentStateRP { get; private set; }
    
    
    private void Awake()
    {
        SetResolution(1920, 1080);
        Application.targetFrameRate = 30;
        //---------------------------------------------
        
        
        
        isStartButtonClicked
        = new ReactiveProperty<bool>(false);
     
        isStageStartButtonClicked
            = new ReactiveProperty<bool>(false);

        currentStateRP = new ReactiveProperty<IState>();
        
        _stateController = new StateController();
        _stateController.currentState = _notGameStarted;
        _stateController.GameState = IState.GameStateList.NotGameStarted;
        
        currentStateRP.Value = _stateController.currentState;
        currentStateRP.Value.GameState = _stateController.GameState;
    }

  
    void Start()
    {
        
        isStartButtonClicked
            .Where(_ => isStartButtonClicked.Value)
            .Subscribe(_ =>
            {
                Debug.Log("게임시작");
                
                if (_gameStart == null)
                {
                    _gameStart = new GameStart();
                }
                SetStage(IState.GameStateList.GameStart,_gameStart);
            });


            isStageStartButtonClicked.Where(_ => isStageStartButtonClicked.Value)
            .Subscribe(_ =>
            {
                Debug.Log("스테이지 시작");
                
                if (_stageStart == null)
                {
                    _stageStart = new StageStart();
                }
                SetStage(IState.GameStateList.StageStart,_stageStart);
            }); 
        
        
        
        //_introSubject.OnNext(Unit.Default);
         
        // SetAnimalIntoDictionaryAndList();
        // SetFootstepIntoDictionaryAndList();
        
        isGameStarted = true;
        isGameFinished = false;
        isRoundReady = true;
    }

    private void SetStage(IState.GameStateList stage, BaseState state)
    {
        //Model Area
        _stateController.ChangeState(state);
        _stateController.GameState = state.GameState;
        
        
        //Rx사용을 위한 Presenter Area
        currentStateRP.Value = _stateController.currentState;
        Debug.Log($"현재 상태: {currentStateRP.Value} 그리고 현재 상태의 GameState값: {currentStateRP.Value.GameState}");
        currentStateRP.Value.GameState = stage;
      
    }

    private float _mainElapsedTime;
    void Update()
    {
        _mainElapsedTime += Time.deltaTime;
        _stateController.Update();
        
        
        if (isGameStarted && isGameFinished == false)
        {
            if (isRoundReady)
            {
                Debug.Log("게임 시작");
                //_IntroMessageEvent.Invoke();
                isRoundReady=false;
            }

            if (isRoudnFinished)
            {
                Debug.Log("다음 레벨");
                //_EndofLevelMessageEvent.Invoke();
                isRoudnFinished = false;
            }
        }
        if (isGameFinished)
        {
            Debug.Log("게임 종료");
            isGameFinished = false;
            isGameStarted = false;
           //  _finishedMessageEvent.Invoke();
        }
    }


    private void SetResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
    }

    private void SetAnimalIntoDictionaryAndList()
    {
        //group에 저장되어있는 순서대로 저장
        for (int i = 0; i < Animal_group.transform.childCount; i++)
        {
            animalGameObjectList.Add(i, Animal_group.transform.GetChild(i).gameObject);
        }
        //Debug.Log(Animal_group.transform.childCount);
    }
    private void SetFootstepIntoDictionaryAndList()
    {
        //group parent에 저장되어있는 순서대로 저장
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

    //가장 첫 번재 동물은 어떻게 처리할지 예외처리 필요

    //게임 매니저는 데이터 저장 용도로만 사용한다
    //UI 화면에 보여주는 용도로 활용하는걸로

    /*
     * 
     *  ** 동물 및 발판은 처음부터 준비된 상태에서 시작
     *  
     * 1. 동물저장
     *  - 순차적으로 부르기 위해
     *    
     * 2. 발판저장
     *  - 순차적으로 부르기 위해
     *  
     * 3. 챕터 및 스탭(각 동물별 시작 및 끝) 저장
     *  - 나레이션 재생, 챕터 변경, 메시지 재생을 위해
     * 
     * 인터랙션 요소
     * ** 보드
     * 
     * 클릭할 경우, 
     * ** 동물 이동, 다음 발판 활성화, UI 메시지
     *
     */

}