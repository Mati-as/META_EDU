using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
///     각 씬별 GameManager는 IGameManager를 상속받아 구현됩니다.
/// </summary>
public abstract class Base_GameManager : MonoBehaviour
{
    //20250225 플레이 활동성 체크용 활용변수 
    public int DEV_sensorClick
    {
        get;
        set;
    } // 센서로 클릭이 발생한 빈도 수

    public int DEV_validClick
    {
        get;
        set;
    } //물체가 한개이상 클릭되어 게임로직이 실행된 경우의 빈도수

    private void InitValidClickCount()
    {
        DEV_sensorClick = 0;
        DEV_validClick = 0;
    }

    protected void DEV_OnValidClick()
    {
        DEV_validClick++;
    }

    protected void DEV_OnSensorClick()
    {
        DEV_validClick++;
    }

    private readonly string METAEDU_LAUNCHER = "METAEDU_LAUNCHER";

    protected WaitForSeconds _waitForClickable;

    protected bool _isClikableInGameRay = true;

    public bool isClikableInGameRay
    {
        get
        {
            return _isClikableInGameRay;
        }
        protected set
        {
            _isClikableInGameRay = value;
        }
    }

    /// <summary>
    ///     08/13/2024
    ///     1.Click빈도수를 유니티상에서 제어할때 사용합니다.
    ///     2.물체가 많은경우 정확도 이슈로 센서자체를 필터링 하는것은 권장되지 않다고 판단하고 있습니다.
    /// </summary>
    public bool isStartButtonClicked
    {
        get;
        set;
    } // startButton 클릭 이전,이후 동작 구분용입니다. 

    protected bool isInitialized
    {
        get;
        private set;
    }

    protected int TARGET_FRAME
    {
        get;
    } = 60; //

    protected float BGM_VOLUME
    {
        get;
        set;
    } = 0.105f;

    protected const float DEFAULT_CLICKABLE_IN_GAME_DELAY = 0.08f;
    protected const float CLICKABLE_IN_GAME_DELAY_MIN = 0.035f;
    protected float waitForClickableInGameRayRay = DEFAULT_CLICKABLE_IN_GAME_DELAY;

    public void LoadUIManager()
    {
        var UIManagerCheck = GameObject.FindWithTag("UIManager");
        //  if (UIManagerCheck == null) Managers.UI.ShowPopupUI("UIManager_"+SceneManager.GetActiveScene().name);
    }


    protected virtual void Start()
    {
    }

    protected float waitForClickableInGameRay
    {
        get
        {
            return waitForClickableInGameRayRay;
        }

        set
        {
            if (value < CLICKABLE_IN_GAME_DELAY_MIN)
                waitForClickableInGameRay = CLICKABLE_IN_GAME_DELAY_MIN;

            else
            {
                if (Math.Abs(value - waitForClickableInGameRay) < 0.005f) return;

                waitForClickableInGameRayRay = value;
                _waitForClickable = new WaitForSeconds(waitForClickableInGameRayRay);
            }
        }
    }


    private float _seonsorSensitivity = SensorManager.SENSOR_DEFAULT_SENSITIVITY;

    public float SensorSensitivity
    {
        get
        {
            return _seonsorSensitivity;
        }

        protected set
        {
            if (value < SensorManager.SENSOR_DEFAULT_SENSITIVITY)
            {
                _seonsorSensitivity = SensorManager.SENSOR_DEFAULT_SENSITIVITY;
                SensorManager.sensorSensitivity = SensorManager.SENSOR_DEFAULT_SENSITIVITY;
            }

            else
            {
                _seonsorSensitivity = value;
                SensorManager.sensorSensitivity = SensorManager.SENSOR_DEFAULT_SENSITIVITY;
            }
        }
    }

    protected void SetClickableWithDelay(float waitTime = 0)
    {
        if (waitTime <= 0.001f) waitTime = waitForClickableInGameRay;


        if (!isClikableInGameRay) return;
        StartCoroutine(SetClickableWithDelayCo(waitTime));
    }

    private IEnumerator SetClickableWithDelayCo(float waitTime)
    {
        if (_waitForClickable == null) _waitForClickable = new WaitForSeconds(waitTime);

        isClikableInGameRay = false;
        yield return _waitForClickable;
        isClikableInGameRay = true;
    }

    //런타임에서 고정
    protected float SHADOW_MAX_DISTANCE
    {
        get;
        set;
    } //런타임에서 고정

    public LayerMask layerMask;

    // Ray 및 상호작용 관련 멤버
    public static Ray GameManager_Ray
    {
        get;
        private set;
    } // 마우스, 발로밟은 위치에 Ray 발사. 

    public RaycastHit[]
        GameManager_Hits
    {
        get;
        set;
    } // Ray에 따른 객체를 GameManager에서만 컨트롤하며, 다른객체는 이를 참조합니다. 즉 추가적인 레이를 발생하지 않습니다. 

    public static event Action On_GmRay_Synced;
    public static event Action<string, DateTime> OnSceneLoad;

    protected virtual void Awake()
    {
        Init();

        Debug.Assert(PrecheckOnInit());
    }


    /// <summary>
    ///     카메라 Rect Xml설정값대로 조정
    /// </summary>
    private Camera UICamera;

    private void InitCameraRect()
    {
        if (Camera.main != null)
        {
            Logger.CoreClassLog($"camera_rect :{Camera.main.rect.width} : {Camera.main.rect.height}");
            Camera.main.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );
        }
        else
            Logger.LogError("Main camera not found.");

        UICamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        if (UICamera != null)
        {
            UICamera.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );

            Logger.CoreClassLog($"camera_rect :{UICamera.rect.width} : {UICamera.rect.height}");
        }
        else
            Logger.LogError("UICamera not found.");
    }


    protected virtual void Init()
    {
        if (isInitialized)
        {
            Logger.LogWarning("Scene is already initialized.");


            return;
        }

        //초기값설정 후 이후에 상속받은 게임매니저에서 민감도 별도 설정
        waitForClickableInGameRay = DEFAULT_CLICKABLE_IN_GAME_DELAY;

        ManageProjectSettings(SHADOW_MAX_DISTANCE, SensorSensitivity);
        BindEvent();
        SetResolution(1920, 1080, TARGET_FRAME);

        if (!SceneManager.GetActiveScene().name.Contains("LAUNCHER")) PlayNarration();

        SetLayerMask();

        Logger.Log("scene is initialzied");
        OnSceneLoad?.Invoke(SceneManager.GetActiveScene().name, DateTime.Now);
        LoadUIManager();

        InitValidClickCount();
        isInitialized = true;

        InitCameraRect();
    }


    protected virtual void SetLayerMask()
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        LayerMask maskWithoutUI = ~(1 << uiLayer);
        layerMask = maskWithoutUI;
    }

    protected void OnOriginallyRaySynced()
    {
        GameManager_Ray = RaySynchronizer.initialRay;
        GameManager_Hits = Physics.RaycastAll(GameManager_Ray, Mathf.Infinity, layerMask);


        On_GmRay_Synced?.Invoke();
    }

    protected virtual void ManageProjectSettings(float defaultShadowMaxDistance, float defaultSensorSensitivity,
        float objSensitivity = 0)
    {
        SensorManager.sensorSensitivity = defaultSensorSensitivity;

        if (waitForClickableInGameRay < 0.05f)
        {
        }
        else
            waitForClickableInGameRay = objSensitivity;


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
#if UNITY_EDITOR
            Debug.LogError("Current Render Pipeline is not Universal Render Pipeline.");
#endif
        }
    }

    /// <summary>
    ///     1. Raysynchronizer에서 첫번째 ray 동기화합니다.
    /// </summary>
    public virtual void OnRaySynced()
    {
    }


    /// <summary>
    ///     GameManager 중복실행 체크 관련 디버그용
    ///     1. GameManager는 한 씬에 두개이상 있을 수 없음
    ///     2. 두개이상 있는경우 씬전환 로직에 문제가 있는것일 수 있다고 판단가능
    /// </summary>
    /// <returns></returns>
    private bool PrecheckOnInit()
    {
        if (SceneManager.GetActiveScene().name == nameof(METAEDU_LAUNCHER))
        {
            var gmObjects = GameObject.FindGameObjectsWithTag("GameManager");
            if (gmObjects.Length > 1) return false;
        }

        return true;
    }

    /// <summary>
    ///     1. 게임종료, 스타트버튼 미클릭등 다양한 로직에서, 더이상 Ray발사(클릭)을 허용하지 않아야합니다.
    ///     2. 이는 씬이동간 에러방지, 게임내에서 로직충돌등을 방지하기 위해 사용합니다.
    ///     3. 이를 일괄적으로 검사 및 클릭가능여부를 반환합니다.
    /// </summary>
    public virtual bool PreCheckOnRaySync()
    {
        if (!isStartButtonClicked)
        {
            Logger.Log("StartBtn Should be Clicked");

            return false;
        }

        if (Managers.isGameStopped)
        {
            Logger.Log("gameStopped, Can't be clicked");

            return false;
        }


        Logger.SensorRelatedLog($"게임 내 클릭 민감도 : {waitForClickableInGameRay}초");
        SetClickableWithDelay(waitForClickableInGameRay);


        DEV_sensorClick++;
        return true;
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

        UI_Scene_StartBtn.onGameStartBtnShut -= OnGameStartStartButtonClicked;
        UI_Scene_StartBtn.onGameStartBtnShut += OnGameStartStartButtonClicked;
    }

    protected virtual void OnDestroy()
    {
        Managers.Sound.Clear();


        RaySynchronizer.OnGetInputFromUser -= OnOriginallyRaySynced;
        UI_Scene_StartBtn.onGameStartBtnShut -= OnGameStartStartButtonClicked;
        On_GmRay_Synced -= OnRaySynced;
        DOTween.KillAll();
        Destroy(gameObject);
    }


    protected virtual void OnGameStartStartButtonClicked()
    {
        // 버튼 클릭시 Ray가 게임로직에 영향미치는 것을 방지하기위한 약간의 Delay 입니다. 
        DOVirtual
            .Float(0, 1, 0.5f, _ =>
            {
            })
            .OnComplete(() =>
            {
                isStartButtonClicked = true;
            });
    }


    protected virtual void PlayNarration()
    {
        // skip narration if it is the launcher scene
        if (!IsLauncherScene())
        {
            // delay for narration
            DOVirtual.Float(0, 1, 1.5f, _ =>
                {
                })
                .OnComplete(() =>
                {
                    bool isPlaying = Managers.Sound.Play(SoundManager.Sound.Narration,
                        "Audio/나레이션/Intro/" + SceneManager.GetActiveScene().name + "_Intro", 0.8f);

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

    private void OnApplicationQuit()
    {
    }
}