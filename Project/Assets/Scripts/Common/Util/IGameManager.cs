using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
///     IGameManager는 UI,Resource,GameLogic Manager를 모두 포함합니다.
///     이는 Singleton Managers 방식입니다.
/// </summary>
public abstract class IGameManager : MonoBehaviour
{
    private static readonly ResourceManager s_resourceManager = new();
    public static Managers s_Managers;
    public static Managers Managers => s_Managers;

    public static ResourceManager Resource
    {
        get
        {
            InitManagers();
            return s_resourceManager;
        }
    }


    public bool isStartButtonClicked { get; private set; } // startButton 클릭 이전,이후 동작 구분용입니다. 
    protected bool isInitialized { get; set; }
    protected int TARGET_FRAME { get; } = 60; //

    protected float BGM_VOLUME { get; set; } = 0.105f;

    public static float DEFAULT_SENSITIVITY { get; protected set; } //런타임에서 고정
    protected float SHADOW_MAX_DISTANCE { get; set; } //런타임에서 고정

    public LayerMask layerMask;

    // Ray 및 상호작용 관련 멤버
    public static Ray GameManager_Ray { get; private set; } // 마우스, 발로밟은 위치에 Ray 발사. 

    public RaycastHit[]
        GameManager_Hits { get; set; } // Ray에 따른 객체를 GameManager에서만 컨트롤하며, 다른객체는 이를 참조합니다. 즉 추가적인 레이를 발생하지 않습니다. 

    public static event Action On_GmRay_Synced;

    protected virtual void Awake()
    {
        Init();
    }

    private static void InitManagers()
    {
        var go = GameObject.Find("@Managers");
        if (go == null)
            go = new GameObject { name = "@Managers" };

        s_Managers = Utils.GetOrAddComponent<Managers>(go);
        DontDestroyOnLoad(go);

        s_resourceManager.Init();
        // s_sceneManager.Init();
        // s_adsManager.Init();
        // s_iapManager.Init();
        // s_dataManager.Init();
        // s_soundManager.Init();
    }


    protected virtual void Init()
    {
        if (isInitialized) return;
        isStartButtonClicked = false;
        ManageProjectSettings(SHADOW_MAX_DISTANCE, DEFAULT_SENSITIVITY);
        BindEvent();
        SetResolution(1920, 1080, TARGET_FRAME);

        if (!SceneManager.GetActiveScene().name.Contains("LAUNCHER"))
        {
            PlayNarration();
        }
        
        var uiLayer = LayerMask.NameToLayer("UI");
        LayerMask maskWithoutUI = ~(1 << uiLayer);
        layerMask = maskWithoutUI;
        isInitialized = true;
    }


    protected void OnOriginallyRaySynced()
    {
        GameManager_Ray = RaySynchronizer.initialRay;
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray, Mathf.Infinity, layerMask);

        On_GmRay_Synced?.Invoke();
    }

    protected virtual void ManageProjectSettings(float defaultShadowMaxDistance, float defaultSensitivity)
    {
        DEFAULT_SENSITIVITY = defaultSensitivity;
        // Shadow Settings-------------- 게임마다 IGameMager상속받아 별도 지정
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (urpAsset != null)
        {
            // Max Distance 값을 설정합니다.
            SHADOW_MAX_DISTANCE = defaultShadowMaxDistance;
            urpAsset.shadowDistance = SHADOW_MAX_DISTANCE;
        }
        else
        {
            Debug.LogError("Current Render Pipeline is not Universal Render Pipeline.");
        }
    }

    /// <summary>
    ///   1. Raysynchronizer에서 첫번째 ray 동기화합니다. 
    /// </summary>
    public virtual void OnRaySynced()
    {
        if (!isStartButtonClicked) return;
        if (!isInitialized) return;
    }

    protected virtual void BindEvent()
    {
#if UNITY_EDITOR
        Debug.Log("Ray Sync Subscribed, RayHits being Shared");
#endif
        //1차적으로 하드웨어에서 동기화된 Ray를 GameManger에서 읽어옵니다.
        RaySynchronizer.OnGetInputFromUser -= OnOriginallyRaySynced;
        RaySynchronizer.OnGetInputFromUser += OnOriginallyRaySynced;

        //On_GmRay_Synced에서 나머지 Ray관련 로직 분배 및 처리합니다. 
        On_GmRay_Synced -= OnRaySynced;
        On_GmRay_Synced += OnRaySynced;

        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        UI_Scene_Button.onBtnShut += OnStartButtonClicked;
    }

    private void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnOriginallyRaySynced;
        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        On_GmRay_Synced -= OnRaySynced;
    }


    protected virtual void OnStartButtonClicked()
    {
        // 버튼 클릭시 Ray가 게임로직에 영향미치는 것을 방지하기위한 약간의 Delay 입니다. 
        DOVirtual
            .Float(0, 1, 0.5f, _ => { })
            .OnComplete(() => { isStartButtonClicked = true; });
    }


    protected virtual void PlayNarration()
    {   
        // skip narration if it is the launcher scene
        if (!IsLauncherScene())
        {
            // delay for narration
            DOVirtual.Float(0, 1, 2f, _ => { })
                .OnComplete(() =>
                {

                   var isPlaying=  Managers.Sound.Play(SoundManager.Sound.Narration,
                        "Audio/나레이션/Intro/" + SceneManager.GetActiveScene().name + "_Intro", 0.5f);
                    
#if UNITY_EDITOR
                    Debug.Log(
                        $"Intro Narration Playing........{isPlaying}");
#endif
                });

            Managers.Sound.Play(SoundManager.Sound.Bgm, $"Audio/Bgm/{SceneManager.GetActiveScene().name}",
                BGM_VOLUME);
        }
    }

    private bool IsLauncherScene()
    {
        return SceneManager.GetActiveScene().name.Contains("LAUNCHER");
    }

    private void SetResolution(int width, int height, int targetFrame)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = targetFrame;

#if UNITY_EDITOR
        Debug.Log(
            $"Game Title: {SceneManager.GetActiveScene().name}," +
            $" Frame Rate: {TARGET_FRAME}, vSync: {QualitySettings.vSyncCount}" +
            $"Shadow Max Distance: {SHADOW_MAX_DISTANCE},");
#endif
    }

    private static void PrintGameInfo()
    {
    }
}