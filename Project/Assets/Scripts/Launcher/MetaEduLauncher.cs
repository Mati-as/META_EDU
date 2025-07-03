using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MetaEduLauncher : UI_PopUp
{
    public enum Animation
    {
        On,
        Off,
        OnOff
    }

    public enum UIType
    {
        Home,
        Result,
        SelectMode,
        ContentA_PE, // 신체놀이
        ContentB_Art, // 미술놀이
        ContentC_Music, // 음악놀이
        ContentD_Video, // 영상놀이
        Setting,

        //TopMenu_OnLauncher,
        UI_Confirm,
        MainVolume,
        BGMVolume,
        EffectVolume,
        NarrationVolume,
        //센서보정관련
        T0_Sensor_Settings,
        T1_Screen_Setting,
        T2_Sensor_Setting,
        T3_Calibration_Setting,


      
        //Login,
        //Survey
    }

    private Slider[] _volumeSliders = new Slider[(int)SoundManager.Sound.Max];


    private enum UIButtons
    {
        Btn_Home,
        Btn_SelectMode,
        ContentAButton,
        ContentBButton,
        ContentCButton,
        ContentDButton,
        Btn_Setting,
        Btn_Back,
        Btn_Quit,

        SettingCloseButton,
        T1SettingCloseButton,
        T2SettingCloseButton,

        Btn_BackToGameSelect,
        Btn_ConfirmToStart,

        Btn_SensorSettings,
        Btn_SensorScreenSetting,
        Btn_SensorParamSetting,
        Btn_ShowGuideline

        //Btn_Result, //사용시 주석해제
        //LoginButton,
        //SurveyButton
    }
    private DevelopmentUIManager _devUIManager;
    private const int NONE = -1;
    private int _UItab = NONE;

    private readonly string UI_CLICK_SOUND_PATH;
    private GameObject[] _UIs;
    private Animation messageAnim;
    private List<string> _animClips = new();
    private readonly float _clickableIntervalForSensor = 0.3f;
    private readonly float _clickableIntervalForMouse = 0.3f;
    private bool _isClikcable = true;
    private static bool _isLoadFinished;
    public Camera _uiCamera;
    public TextMeshProUGUI tmpConfirm;

    
    
    public static bool isBackButton
    {
        get;
        set;
    } // 뒤로가기의 경우, 씬로드 이후 게임선택화면이 나타나야합니다. 

    private void Start()
    {
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/Bgm/Launcher", 0.05f);
        _gm = GameObject.FindWithTag("GameManager").GetComponent<Base_GameManager>();
        _uiCamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
    }


    private void OnT1T2SettingCloseBtnClicked()
    {
        ShowTab(UIType.T0_Sensor_Settings);
    }

    private void OnBackBtnClicked()
    {
        if (!_isClikcable)
        {
            Logger.Log("클릭 시도가 너무 빠름. 잠시 후 다시 클릭 --------------런쳐 ");
            return;
        }

        Debug.Log("뒤로가기 버튼 클릭");


        if (currentUITab == UIType.SelectMode)
            ShowTab(UIType.Home);
        else if (currentUITab == UIType.ContentA_PE ||
                 currentUITab == UIType.ContentB_Art ||
                 currentUITab == UIType.ContentC_Music ||
                 currentUITab == UIType.ContentD_Video)
            ShowTab(UIType.SelectMode);

        else if (currentUITab == UIType.UI_Confirm) Logger.Log($"not valid click : {currentUITab}");
         else if (currentUITab == UIType.T1_Screen_Setting || currentUITab == UIType.T2_Sensor_Setting)
         {
             ShowTab(UIType.T0_Sensor_Settings);
         }
         else if(currentUITab == UIType.T0_Sensor_Settings) ShowTab(UIType.Home);
    }


    private void Awake()
    {
        _raySynchronizer = GameObject.FindWithTag("RaySynchronizer").GetComponent<RaySynchronizer>();


        UI_LoadInitialScene.onInitialLoadComplete -= OnLoadFinished;
        UI_LoadInitialScene.onInitialLoadComplete += OnLoadFinished;


        FP_Prefab.onPrefabInput -= OnRaySyncByPrefab;
        FP_Prefab.onPrefabInput += OnRaySyncByPrefab;

        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
        RaySynchronizer.OnGetInputFromUser += OnRaySynced;


        BindObject(typeof(UIType));

        tmpConfirm = GetObject((int)UIType.UI_Confirm).GetComponentInChildren<TextMeshProUGUI>();


        BindButton(typeof(UIButtons));

        GetButton((int)UIButtons.Btn_Home).gameObject.BindEvent(() => ShowTab(UIType.Home));

        GetButton((int)UIButtons.Btn_SensorSettings).gameObject.BindEvent(() => ShowTab(UIType.T0_Sensor_Settings));
        GetButton((int)UIButtons.Btn_SensorScreenSetting).gameObject.BindEvent(() => ShowTab(UIType.T1_Screen_Setting));
        GetButton((int)UIButtons.Btn_SensorParamSetting).gameObject.BindEvent(() => ShowTab(UIType.T2_Sensor_Setting));
        GetButton((int)UIButtons.T1SettingCloseButton).gameObject.BindEvent(OnT1T2SettingCloseBtnClicked);
        GetButton((int)UIButtons.T2SettingCloseButton).gameObject.BindEvent(OnT1T2SettingCloseBtnClicked);


        GetButton((int)UIButtons.Btn_SelectMode).gameObject.BindEvent(() => ShowTab(UIType.SelectMode));
        GetButton((int)UIButtons.ContentAButton).gameObject.BindEvent(() => ShowTab(UIType.ContentA_PE));
        GetButton((int)UIButtons.ContentBButton).gameObject.BindEvent(() => ShowTab(UIType.ContentB_Art));
        GetButton((int)UIButtons.ContentCButton).gameObject.BindEvent(() => ShowTab(UIType.ContentC_Music));
        GetButton((int)UIButtons.ContentDButton).gameObject.BindEvent(() => ShowTab(UIType.ContentD_Video));
        GetButton((int)UIButtons.Btn_Setting).gameObject.BindEvent(() => ShowTab(UIType.Setting));
        GetButton((int)UIButtons.Btn_Quit).gameObject.BindEvent(() =>
        {
            Application.Quit();
        });
        
        GetButton((int)UIButtons.Btn_ShowGuideline).gameObject.BindEvent
            (() =>
            {
                if(_devUIManager ==null) _devUIManager = GameObject.FindWithTag("LidarMenu").GetComponent<DevelopmentUIManager>();
                _devUIManager.DisableAllImages();
            });
        


        GetButton((int)UIButtons.Btn_ConfirmToStart).gameObject.BindEvent(() =>
        {
            LoadScene(_gameNameWaitingForConfirmation);
        });
        GetButton((int)UIButtons.Btn_BackToGameSelect).gameObject.BindEvent(OnBackBtnOnConfirmMessageClicked);
        GetButton((int)UIButtons.Btn_Back).gameObject.BindEvent(OnBackBtnClicked);


        GetButton((int)UIButtons.SettingCloseButton).gameObject.BindEvent(() =>
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Common/Launcher_UI_Click", 1f);
            GetObject((int)UIType.Setting).gameObject.SetActive(false);
            ShowTab(currentUITab);
        });

        var sceneObjects = FindObjectsOfType<GameObject>();

        foreach (var obj in sceneObjects)
            // 이름에 "SceneName_"이 포함된 오브젝트 필터링
            if (obj.name.Contains("SceneName_"))
            {
                // 버튼 컴포넌트를 찾음
                var button = obj.GetComponent<Button>();

                // 버튼이 존재하면 이벤트를 할당
                if (button != null)
                    // 버튼에 이벤트 바인딩
                    button.gameObject.BindEvent(() =>
                    {
                        if (!_isClikcable) return;
                        CheckAndSetClickable();

                        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Common/Launcher_UI_Click", 1f);
                      
                        //컨펌화면 게임이름 노출 로직 만들때 활용, 현재 미활용중 10/2/2024
                        _gameNameWaitingForConfirmation = button.gameObject.name;
                        GetObject((int)UIType.UI_Confirm).SetActive(true);
                        if (Define.GameNameMap.ContainsKey(_gameNameWaitingForConfirmation))
                            tmpConfirm.text = $"-{Define.GameNameMap[_gameNameWaitingForConfirmation]}-" +
                                              "\n해당 놀이를 시작 할까요?";
                        else
                            tmpConfirm.text = "\n해당 놀이를 시작할까요?";
                    }); // 원하는 동작 할당

                Logger.Log($"게임 컨텐츠 객체 버튼 할당 :{obj.name}");
            }

        // GetButton((int)UIButtons.LoginButton).gameObject.BindEvent(() => ShowTab(UIType.Login));
        // GetButton((int)UIButtons.SurveyButton).gameObject.BindEvent(() => ShowTab(UIType.Survey));

        _volumeSliders = new Slider[(int)SoundManager.Sound.Max];

        _volumeSliders[(int)SoundManager.Sound.Main] = GetObject((int)UIType.MainVolume).GetComponent<Slider>();
        _volumeSliders[(int)SoundManager.Sound.Main].value =
            Managers.Sound.volumes[(int)SoundManager.Sound.Main];
#if UNITY_EDITOR
        Debug.Log($" 메인 볼륨 {Managers.Sound.volumes[(int)SoundManager.Sound.Main]}");
#endif

        _volumeSliders[(int)SoundManager.Sound.Bgm] = GetObject((int)UIType.BGMVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Effect] = GetObject((int)UIType.EffectVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Narration] =
            GetObject((int)UIType.NarrationVolume).GetComponent<Slider>();

        for (int i = 0; i < (int)SoundManager.Sound.Max; i++)
        {
            _volumeSliders[i].maxValue = Managers.Sound.VOLUME_MAX[i];
            _volumeSliders[i].value = Managers.Sound.volumes[i];
        }


        // default Volume값은 SoundManager에서 관리하며, 초기화 이후, UI Slider가 이를 참조하여 표출하도록 합니다.
        // default Value는 시연 테스트에 결과에 따라 수정가능합니다. 
        _volumeSliders[(int)SoundManager.Sound.Main].onValueChanged.AddListener(_ =>
        {
            Managers.Sound.volumes[(int)SoundManager.Sound.Main] =
                _volumeSliders[(int)SoundManager.Sound.Main].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Main].volume =
                Managers.Sound.volumes[(int)SoundManager.Sound.Main];

            Managers.Sound.volumes[(int)SoundManager.Sound.Bgm] =
                _volumeSliders[(int)SoundManager.Sound.Bgm].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Bgm].value);

            Managers.Sound.volumes[(int)SoundManager.Sound.Effect] =
                _volumeSliders[(int)SoundManager.Sound.Effect].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Effect].value);

            Managers.Sound.volumes[(int)SoundManager.Sound.Narration] =
                _volumeSliders[(int)SoundManager.Sound.Narration].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Narration],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Narration].value);

            //    A_SettingManager.SaveCurrentSetting(SensorManager.height,);
        });
        _volumeSliders[(int)SoundManager.Sound.Bgm].onValueChanged.AddListener(_ =>
        {
            Managers.Sound.volumes[(int)SoundManager.Sound.Bgm] =
                _volumeSliders[(int)SoundManager.Sound.Bgm].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Bgm].value);
        });

        _volumeSliders[(int)SoundManager.Sound.Effect].onValueChanged.AddListener(_ =>
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/TestSound/Test_Effect");

            Managers.Sound.volumes[(int)SoundManager.Sound.Effect] =
                _volumeSliders[(int)SoundManager.Sound.Effect].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Effect].value);
        });

        _volumeSliders[(int)SoundManager.Sound.Narration].onValueChanged.AddListener(_ =>
        {
            if (!Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].isPlaying)
                Managers.Sound.Play(SoundManager.Sound.Narration, "Audio/TestSound/Test_Narration");
            Managers.Sound.volumes[(int)SoundManager.Sound.Narration] =
                _volumeSliders[(int)SoundManager.Sound.Narration].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Narration],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Narration].value);
        });


        //  GetObject((int)UIType.Loading).gameObject.SetActive(false);
        GetObject((int)UIType.Home).gameObject.SetActive(false);
        GetObject((int)UIType.Result).gameObject.SetActive(false);
        GetObject((int)UIType.SelectMode).gameObject.SetActive(false);
        GetObject((int)UIType.ContentA_PE).gameObject.SetActive(false);
        GetObject((int)UIType.ContentB_Art).gameObject.SetActive(false);
        GetObject((int)UIType.ContentC_Music).gameObject.SetActive(false);
        GetObject((int)UIType.ContentD_Video).gameObject.SetActive(false);
        GetObject((int)UIType.Setting).gameObject.SetActive(false);
        GetObject((int)UIType.UI_Confirm).gameObject.SetActive(false);


#if UNITY_EDITOR
        Debug.Log("Launcher Init Completed");
#endif
        if (!isBackButton)
        {
            isBackButton = false;
            ShowTab(UIType.Home);
        }
        else
            ShowTab(UIType.SelectMode);

        DOVirtual.Float(0, 0, 2.5f, _ =>
            {
            })
            .OnComplete(() =>
            {
                Managers.IsGameStopped = false;
            });
    }

    private void OnDestroy()
    {
        UI_LoadInitialScene.onInitialLoadComplete -= OnLoadFinished;
        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
        FP_Prefab.onPrefabInput -= OnRaySyncByPrefab;

        Managers.Sound.Stop(SoundManager.Sound.Bgm);
    }

    private void OnLoadFinished()
    {
        InitEssentialUI();
        SetUIEssentials();

        // 널방지를 위한 딜레이 입니다.
        DOVirtual.Float(0, 0, 1f, _ =>
        {
        }).OnComplete(() =>
        {
            _isLoadFinished = true;
        });
    }

    private UIType currentUITab = UIType.Home;

    public override bool InitEssentialUI()
    {
        return true;
    }


    public void ShowTab(UIType tab)
    {
        if (!_isClikcable)
        {
            Logger.Log("클릭 시도가 너무 빠름. 잠시 후 다시 클릭 --------------런쳐 ");
            return;
        }

        CheckAndSetClickable();

        if ((UIType)_UItab == tab) return;

        if (tab != UIType.Setting) currentUITab = tab;
        _UItab = (int)tab;


        GetObject((int)UIType.Home).gameObject.SetActive(false);
        GetObject((int)UIType.Result).gameObject.SetActive(false);
        GetObject((int)UIType.SelectMode).gameObject.SetActive(false);
        GetObject((int)UIType.ContentA_PE).gameObject.SetActive(false);
        GetObject((int)UIType.ContentB_Art).gameObject.SetActive(false);
        GetObject((int)UIType.ContentC_Music).gameObject.SetActive(false);
        GetObject((int)UIType.ContentD_Video).gameObject.SetActive(false);
        GetObject((int)UIType.Setting).gameObject.SetActive(false);
         GetObject((int)UIType.T0_Sensor_Settings).gameObject.SetActive(false);
         GetObject((int)UIType.T1_Screen_Setting).gameObject.SetActive(false);
         GetObject((int)UIType.T2_Sensor_Setting).gameObject.SetActive(false);


        if (currentUITab == UIType.Home)
            GetButton((int)UIButtons.Btn_Back).gameObject.SetActive(false);
        else
            GetButton((int)UIButtons.Btn_Back).gameObject.SetActive(true);
        //GetObject((int)UIType.Login).gameObject.SetActive(false);
        //GetObject((int)UIType.Survey).gameObject.SetActive(false);

#if UNITY_EDITOR
        Debug.Log($"Current UI : {currentUITab}");
#endif

        switch (tab)
        {
            case UIType.Home:
                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.Home).gameObject.SetActive(true);
                GetObject((int)UIType.Home).GetComponent<ScrollRect>().ResetVertical();

                break;

            case UIType.SelectMode:

                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.SelectMode).gameObject.SetActive(true);
                GetObject((int)UIType.SelectMode).GetComponent<ScrollRect>().ResetHorizontal();


                break;

            case UIType.ContentA_PE:
                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);


                GetObject((int)UIType.ContentA_PE).gameObject.SetActive(true);
                GetObject((int)UIType.ContentA_PE).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.ContentB_Art:

                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);


                GetObject((int)UIType.ContentB_Art).gameObject.SetActive(true);
                GetObject((int)UIType.ContentB_Art).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.ContentC_Music:

                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);


                GetObject((int)UIType.ContentC_Music).gameObject.SetActive(true);
                GetObject((int)UIType.ContentC_Music).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.ContentD_Video:

                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);

                GetObject((int)UIType.ContentD_Video).gameObject.SetActive(true);
                GetObject((int)UIType.ContentD_Video).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.Setting:
                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.Setting).gameObject.SetActive(true);
                GetObject((int)UIType.Setting).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.Result:
                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.Result).gameObject.SetActive(true);
                GetObject((int)UIType.Result).GetComponent<ScrollRect>().ResetHorizontal();
                break;


            case UIType.T0_Sensor_Settings:
                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.T0_Sensor_Settings).gameObject.SetActive(true);
                break;

            case UIType.T1_Screen_Setting:
                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.T0_Sensor_Settings).gameObject.SetActive(false);
                GetObject((int)UIType.T1_Screen_Setting).gameObject.SetActive(true);
                break;

            case UIType.T2_Sensor_Setting:
                Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.T0_Sensor_Settings).gameObject.SetActive(false);
                GetObject((int)UIType.T2_Sensor_Setting).gameObject.SetActive(true);
                break;

        }

        if (isInitialSoundBlocked)
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Common/Launcher_UI_Click", 1f);
            isInitialSoundBlocked = true;
        }
    }

    private bool isInitialSoundBlocked = false;

    private void OnBackBtnOnConfirmMessageClicked()
    {
        if (!_isClikcable)
        {
            Logger.Log("클릭 시도가 너무 빠름. 잠시 후 다시 클릭  ");
            return;
        }

        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Common/Launcher_UI_Click", 1f);
        CheckAndSetClickable();
        GetObject((int)UIType.UI_Confirm).gameObject.SetActive(false);
    }

    private void CheckAndSetClickable()
    {
        if (_isClikcable) StartCoroutine(CheckAndSetClickableCo());
    }

    private WaitForSeconds _waitForSensorClick;
    private WaitForSeconds _waitForMouseClick;

    private IEnumerator CheckAndSetClickableCo()
    {
        if (!_isClikcable) //!EventSystem.current.IsPointerOverGameObject()
            yield break;
        _isClikcable = false;


        if (_waitForSensorClick == null) _waitForSensorClick = new WaitForSeconds(_clickableIntervalForSensor);
        if (_waitForMouseClick == null) _waitForMouseClick = new WaitForSeconds(_clickableIntervalForMouse);


        if (EventSystem.current.IsPointerOverGameObject())
            yield return _waitForMouseClick;
        else
            yield return _waitForSensorClick;


        _isClikcable = true;
//        Logger.Log("클릭가능---------------------------------------------");
    }

    private GraphicRaycaster _launcherGR;
    private PointerEventData _launcherPED;
    private GameObject _launcherCanvas;

    private void SetUIEssentials()
    {
        _launcherGR = _raySynchronizer.graphicRaycaster;
        _launcherPED = _raySynchronizer.PointerEventData;
    }

    private RaySynchronizer _raySynchronizer;

    private List<RaycastResult> _results;
    private Ray _ray;
    private Vector3 screenPosition;


    public Base_GameManager _gm;

    //Raysychronizer.cs와 동일한 로직사용. 
    public void OnRaySynced()
    {
#if UNITY_EDITOR
        //Debug.Log($"Raysync From Launcher -----------------------");
#endif
        if (!_isLoadFinished) return;

        //마우스 및 포인터 위치를 기반으로 하고싶은경우.
        screenPosition = Mouse.current.position.ReadValue();

        if (_launcherPED != null) _launcherPED.position = screenPosition;

        _results = new List<RaycastResult>();
        if (_launcherGR != null) _launcherGR.Raycast(_launcherPED, _results);

        if (_results.Count <= 0) return;
        {
            ShowTabOrLoadScene(_results);
        }
    }


    private List<RaycastResult> _resultsByPrefab;

    public Vector3 currentPrefabPosition
    {
        get;
        set;
    }

    private Vector3 _screenPositionByPrefab;


    /// <summary>
    ///     하드웨어(빔 프로젝터) 상에서 프리팹으로 클릭하는 로직을 위한 OnRaySync 커스텀 이벤트 함수입니다.
    ///     씬변경 후 일반 게임로직에서는 동작하지 않습닌다
    /// </summary>
    private Button _btn;

    private void OnRaySyncByPrefab()
    {
        //테스트 후 삭제 필요
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //
    //     float radius = 50f;
    //     Gizmos.DrawSphere(currentPrefabPosition, radius);
    // }


    private string _gameNameWaitingForConfirmation;

    public void ShowTabOrLoadScene(List<RaycastResult> results)
    {
        DOVirtual.Float(0, 0, 0.25f, _ =>
            {
            })
            .OnComplete(() =>
            {
                UIType clickedUI = 0;

                foreach (var result in results)
                {
                    // 설정,홈,컨텐츠 **버튼** ---------------------------------------------------------
                    if (Enum.TryParse(SetButtonString(result.gameObject.name), out clickedUI))
                    {
                        ShowTab(clickedUI);
                        return;
                    }

                    if (result.gameObject.name.Contains("SceneName"))
                    {
                        // ** 씬 로드** ---------------------------------------------------------
                        Logger.Log($"{result.gameObject.name}게임 씬 버튼 클릭 됨--------------------------------------");
                        if (!_isClikcable)
                        {
                            Logger.Log("클릭 시도가 너무 빠름. 잠시 후 다시 클릭 --------------런쳐 ");
                            return;
                        }


                        return;
                    }
                }
            });
    }


    public void LoadScene(string sceneName)
    {
        string originalName = sceneName;
        string modifiedName = originalName.Substring("SceneName_".Length);

        gameObject.SetActive(false);
        SceneManager.LoadScene(modifiedName);
    }

    private string SetButtonString(string input)
    {
        if (!input.Contains("Button")) return null;

        string originalName = input;
        string modifiedName = originalName.Substring(0, originalName.Length - 6);
        return modifiedName;
    }


    private IEnumerator Active_false()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
}