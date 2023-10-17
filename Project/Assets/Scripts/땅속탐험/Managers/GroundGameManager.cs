using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using 땅속탐험.Utils;

public class GroundGameManager : MonoBehaviour
{
    private static GameStart _gameStart;
    private static GameOver _gameover;
    private static StageStart _stageStart;
    private static StageFinished _stageFinished;
    private static NotGameStarted _notGameStarted;

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
        SetResolution(1920, 1080);
        Application.targetFrameRate = 30;
        //---------------------------------------------
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
        
        isStartButtonClicked
            .Subscribe(_ =>
            {
                Debug.Log("게임시작");
                SetStage(_gameStart);
                isGameStartedbool = true;
                Debug.Log($" 현재 statecontroller의 GameState{_stateController.GameState}");
            });


        isStageStartButtonClicked
            .Subscribe(_ =>
            {
                Debug.Log("스테이지 시작");
                SetStage(_stageStart);
                Debug.Log($" 현재 statecontroller의 GameState{_stateController.GameState}");
            });

        isGameFinishedRP
            .Where(value=>value ==true)
            .Subscribe(_ =>
            {
                Debug.Log("게임종료");
                isGameFinishedbool = true;
                SetStage(_gameover);
                ActivateAllAnimals();
            });
    }

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


        //Rx사용을 위한 Presenter Area
        currentStateRP.Value = state;
        currentStateRP.Value.GameState = state.GameState;
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

    private void ActivateAllAnimals()
    {
        foreach (GameObject obj in AllAnimals)
        {
            Debug.Log("게임종료 및 동물 표출");
            obj.SetActive(true);
            obj.transform.DOScale(1f, 1f);
           
            if (obj.transform.childCount > 0) // 자식 오브젝트가 있는지 확인
            {
                Transform lastChild = obj.transform.GetChild(obj.transform.childCount - 1);
                lastChild.gameObject.SetActive(false);
            }
            
        }
    }
}