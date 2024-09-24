using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
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
        NarrationVolume
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
        Btn_BackToGameSelect,
        Btn_ConfirmToStart
        
        //Btn_Result, //사용시 주석해제
        //LoginButton,
        //SurveyButton
    }

    private const int NONE = -1;
    private int _UItab = NONE;

    private readonly string UI_CLICK_SOUND_PATH;
    private GameObject[] _UIs;
    private Animation messageAnim;
    private List<string> _animClips = new();
    private readonly float _clickableInterval = 0.48f;
    private bool _isClikcable = true;
    private static bool _isLoadFinished;


    public static bool isBackButton { get; set; } // 뒤로가기의 경우, 씬로드 이후 게임선택화면이 나타나야합니다. 

    private void Start()
    {
        Managers.soundManager.Play(SoundManager.Sound.Bgm, "Audio/Bgm/Launcher", 0.05f);
    }


    private void OnBackBtnClicked()
    {
        Debug.Log("뒤로가기 버튼 클릭");
        
            
        if (currentUITab == UIType.SelectMode)
        {
            ShowTab(UIType.Home);
        }
        else if (currentUITab == UIType.ContentA_PE ||
                 currentUITab == UIType.ContentB_Art ||
                 currentUITab == UIType.ContentC_Music||
                 currentUITab == UIType.ContentD_Video)
        {
            ShowTab(UIType.SelectMode);
        }
        else if(currentUITab == UIType.UI_Confirm)
        {
            Logger.Log($"not valid click : {currentUITab}");
        }
    }

    private void Awake()
    {
        _raySynchronizer = GameObject.FindWithTag("RaySynchronizer").GetComponent<RaySynchronizer>();
        
        
        LoadInitialScene.onInitialLoadComplete -= OnLoadFinished;
        LoadInitialScene.onInitialLoadComplete += OnLoadFinished;

        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
        RaySynchronizer.OnGetInputFromUser += OnRaySynced;

        Destroy_prefab.onPrefabInput -= OnRaySyncByPrefab;
        Destroy_prefab.onPrefabInput += OnRaySyncByPrefab;

       

        BindObject(typeof(UIType));

        BindButton(typeof(UIButtons));

        GetButton((int)UIButtons.Btn_Home).gameObject.BindEvent(() => ShowTab(UIType.Home));
        GetButton((int)UIButtons.Btn_SelectMode).gameObject.BindEvent(() => ShowTab(UIType.SelectMode));
        GetButton((int)UIButtons.ContentAButton).gameObject.BindEvent(() => ShowTab(UIType.ContentA_PE));
        GetButton((int)UIButtons.ContentBButton).gameObject.BindEvent(() => ShowTab(UIType.ContentB_Art));
        GetButton((int)UIButtons.ContentCButton).gameObject.BindEvent(() => ShowTab(UIType.ContentC_Music));
        GetButton((int)UIButtons.ContentDButton).gameObject.BindEvent(() => ShowTab(UIType.ContentD_Video));
        GetButton((int)UIButtons.Btn_Setting).gameObject.BindEvent(() => ShowTab(UIType.Setting));
        // GetButton((int)UIButtons.Btn_Result).gameObject.BindEvent(() => ShowTab(UIType.Result));
        GetButton((int)UIButtons.Btn_Quit).gameObject.BindEvent(() => { Application.Quit(); });


        GetButton((int)UIButtons.Btn_ConfirmToStart).gameObject.BindEvent(()=>LoadScene(_gameNameWaitingForConfirmation));
        GetButton((int)UIButtons.Btn_BackToGameSelect).gameObject.BindEvent(OnBackBtnOnConfirmMessageClicked);
        GetButton((int)UIButtons.Btn_Back).gameObject.BindEvent(OnBackBtnClicked);
       
        
        
        
        GetButton((int)UIButtons.SettingCloseButton).gameObject.BindEvent(() =>
        {
            GetObject((int)UIType.Setting).gameObject.SetActive(false);
            ShowTab(currentUITab);
        });
        // GetButton((int)UIButtons.LoginButton).gameObject.BindEvent(() => ShowTab(UIType.Login));
        // GetButton((int)UIButtons.SurveyButton).gameObject.BindEvent(() => ShowTab(UIType.Survey));

        _volumeSliders = new Slider[(int)SoundManager.Sound.Max];

        _volumeSliders[(int)SoundManager.Sound.Main] = GetObject((int)UIType.MainVolume).GetComponent<Slider>();
        _volumeSliders[(int)SoundManager.Sound.Main].value =
            Managers.soundManager.volumes[(int)SoundManager.Sound.Main];
#if UNITY_EDITOR
        Debug.Log($" 메인 볼륨 {Managers.soundManager.volumes[(int)SoundManager.Sound.Main]}");
#endif

        _volumeSliders[(int)SoundManager.Sound.Bgm] = GetObject((int)UIType.BGMVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Effect] = GetObject((int)UIType.EffectVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Narration] =
            GetObject((int)UIType.NarrationVolume).GetComponent<Slider>();

        for (var i = 0; i < (int)SoundManager.Sound.Max; i++)
        {
            _volumeSliders[i].maxValue = Managers.soundManager.VOLUME_MAX[i];
            _volumeSliders[i].value = Managers.soundManager.volumes[i];
        }


        // default Volume값은 SoundManager에서 관리하며, 초기화 이후, UI Slider가 이를 참조하여 표출하도록 합니다.
        // default Value는 시연 테스트에 결과에 따라 수정가능합니다. 
        _volumeSliders[(int)SoundManager.Sound.Main].onValueChanged.AddListener(_ =>
        {
            Managers.soundManager.volumes[(int)SoundManager.Sound.Main] =
                _volumeSliders[(int)SoundManager.Sound.Main].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Main].volume =
                Managers.soundManager.volumes[(int)SoundManager.Sound.Main];

            Managers.soundManager.volumes[(int)SoundManager.Sound.Bgm] =
                _volumeSliders[(int)SoundManager.Sound.Bgm].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Bgm].value);

            Managers.soundManager.volumes[(int)SoundManager.Sound.Effect] =
                _volumeSliders[(int)SoundManager.Sound.Effect].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Effect].value);

            Managers.soundManager.volumes[(int)SoundManager.Sound.Narration] =
                _volumeSliders[(int)SoundManager.Sound.Narration].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Narration].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Narration],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Narration].value);

        //    A_SettingManager.SaveCurrentSetting(SensorManager.height,);


        });
        _volumeSliders[(int)SoundManager.Sound.Bgm].onValueChanged.AddListener(_ =>
        {
            Managers.soundManager.volumes[(int)SoundManager.Sound.Bgm] =
                _volumeSliders[(int)SoundManager.Sound.Bgm].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Bgm].value);
        });

        _volumeSliders[(int)SoundManager.Sound.Effect].onValueChanged.AddListener(_ =>
        {
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/TestSound/Test_Effect");

            Managers.soundManager.volumes[(int)SoundManager.Sound.Effect] =
                _volumeSliders[(int)SoundManager.Sound.Effect].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Effect].value);
        });

        _volumeSliders[(int)SoundManager.Sound.Narration].onValueChanged.AddListener(_ =>
        {
            if (!Managers.soundManager.audioSources[(int)SoundManager.Sound.Narration].isPlaying)
                Managers.soundManager.Play(SoundManager.Sound.Narration, "Audio/TestSound/Test_Narration");
            Managers.soundManager.volumes[(int)SoundManager.Sound.Narration] =
                _volumeSliders[(int)SoundManager.Sound.Narration].value;
            Managers.soundManager.audioSources[(int)SoundManager.Sound.Narration].volume =
                Mathf.Lerp(0, Managers.soundManager.VOLUME_MAX[(int)SoundManager.Sound.Narration],
                    Managers.soundManager.volumes[(int)SoundManager.Sound.Main] *
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
        {
            ShowTab(UIType.SelectMode);
        }

        DOVirtual.Float(0, 0, 2.5f, _ => { })
            .OnComplete(() =>
        {
            Managers.isGameStopped = false;
        });
        

    }

    private void OnDestroy()
    {
        LoadInitialScene.onInitialLoadComplete -= OnLoadFinished;
        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
        Destroy_prefab.onPrefabInput -= OnRaySyncByPrefab;

        Managers.soundManager.Stop(SoundManager.Sound.Bgm);
    }

    private void OnLoadFinished()
    {
        Init();
        SetUIEssentials();
        
        // 널방지를 위한 딜레이 입니다.
        DOVirtual.Float(0, 0, 1f, _ => { }).OnComplete(() =>
        {
         _isLoadFinished = true;
        });
     
    }

    private UIType currentUITab = UIType.Home;

    public override bool Init()
    {
        return true;
    }


    public void ShowTab(UIType tab)
    {
        if (!_isClikcable) return;
        _isClikcable = false;
        DOVirtual.Float(0, 0, _clickableInterval, _ => { })
            .OnComplete(() => { _isClikcable = true; });

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

        if (currentUITab == UIType.Home)
        {
            GetButton((int)UIButtons.Btn_Back).gameObject.SetActive(false);
        }
        else
        {
        GetButton((int)UIButtons.Btn_Back).gameObject.SetActive(true);
            
        }
        //GetObject((int)UIType.Login).gameObject.SetActive(false);
        //GetObject((int)UIType.Survey).gameObject.SetActive(false);

#if UNITY_EDITOR
        Debug.Log($"Current UI : {currentUITab}");
#endif

        switch (tab)
        {
            case UIType.Home:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.Home).gameObject.SetActive(true);
                GetObject((int)UIType.Home).GetComponent<ScrollRect>().ResetVertical();
                // GetButton((int)Buttons.AbilityButton).image.sprite = Managers.Resource.Load<Sprite>("Sprites/Main/Common/btn_18");
                // GetImage((int)Images.AbilityBox).sprite = Managers.Resource.Load<Sprite>("Sprites/Main/Common/btn_12");

                break;

            case UIType.SelectMode:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.SelectMode).gameObject.SetActive(true);
                GetObject((int)UIType.SelectMode).GetComponent<ScrollRect>().ResetHorizontal();


                break;

            case UIType.ContentA_PE:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.ContentA_PE).gameObject.SetActive(true);
                GetObject((int)UIType.ContentA_PE).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.ContentB_Art:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.ContentB_Art).gameObject.SetActive(true);
                GetObject((int)UIType.ContentB_Art).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.ContentC_Music:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.ContentC_Music).gameObject.SetActive(true);
                GetObject((int)UIType.ContentC_Music).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.ContentD_Video:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.ContentD_Video).gameObject.SetActive(true);
                GetObject((int)UIType.ContentD_Video).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.Setting:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.Setting).gameObject.SetActive(true);
                GetObject((int)UIType.Setting).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            case UIType.Result:
                Managers.soundManager.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
                GetObject((int)UIType.Result).gameObject.SetActive(true);
                GetObject((int)UIType.Result).GetComponent<ScrollRect>().ResetHorizontal();
                break;

            // case UIType.Login:
            // 	Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
            // 	GetObject((int)UIType.Login).gameObject.SetActive(true);
            // 	GetObject((int)UIType.Login).GetComponent<ScrollRect>().ResetHorizontal();
            // 	break;

            // case UIType.Survey:
            // 	Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
            // 	GetObject((int)UIType.Survey).gameObject.SetActive(true);
            // 	GetObject((int)UIType.Survey).GetComponent<ScrollRect>().ResetHorizontal();
            // 	break;
        }

        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/Launcher_UI_Click",volume:1f);
    }

    private void OnBackBtnOnConfirmMessageClicked()
    {
        GetObject((int)UIType.UI_Confirm).gameObject.SetActive(false);
    }


    private GraphicRaycaster _launcherGR;
    private PointerEventData _launcherPED;
    private GameObject _launcherCanvas;

    private void SetUIEssentials()
    {
        _launcherGR = _raySynchronizer.GR;
        _launcherPED = _raySynchronizer.PED;
    }

    private RaySynchronizer _raySynchronizer;
    private List<RaycastResult> _results;
    private Ray _ray;
    private Vector3 screenPosition;


    //Raysychronizer.cs와 동일한 로직사용. 
    public void OnRaySynced()
    {
#if UNITY_EDITOR
        //Debug.Log($"Raysync From Launcher -----------------------");
#endif
        if (!_isLoadFinished) return;

        //마우스 및 포인터 위치를 기반으로 하고싶은경우.
        screenPosition = Mouse.current.position.ReadValue();

        if(_launcherPED!=null) _launcherPED.position = screenPosition;

        _results = new List<RaycastResult>();
        if(_launcherGR!=null)_launcherGR.Raycast(_launcherPED, _results);

        if (_results.Count <= 0) return;
        ShowTabOrLoadScene(_results);
    }

    private List<RaycastResult> _resultsByPrefab;
    public Vector3 currentPrefabPosition { private get; set; }
    private Vector3 _screenPositionByPrefab;


    /// <summary>
    ///     하드웨어(빔 프로젝터) 상에서 프리팹으로 클릭하는 로직을 위한 OnRaySync 커스텀 이벤트 함수입니다.
    ///     씬변경 후 일반 게임로직에서는 동작하지 않습닌다
    /// </summary>
    private void OnRaySyncByPrefab()
    {
        //_screenPositionByPrefab = _uiCamera.WorldToScreenPoint(currentPrefabPosition);
        // _ray = _uiCamera.ScreenPointToRay(_screenPositionByPrefab);

        _launcherPED.position = currentPrefabPosition;
        _resultsByPrefab = new List<RaycastResult>();
        _launcherGR.Raycast(_launcherPED, _resultsByPrefab);

#if UNITY_EDITOR
        Debug.Log($"프리팹 시 PED Position 변환 전: {currentPrefabPosition}");
#endif

#if UNITY_EDITOR
        Debug.Log($"프리팹 시 PED Position 변환 후 : {_launcherPED.position}");
#endif

        if (_resultsByPrefab != null)
        {
            ShowTabOrLoadScene(_resultsByPrefab);
        }


        else
        {
#if UNITY_EDITOR
            Debug.LogError("result is null");
#endif
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var radius = 10f;
        Gizmos.DrawSphere(currentPrefabPosition, radius);
    }


    private string _gameNameWaitingForConfirmation;
    public void ShowTabOrLoadScene(List<RaycastResult> results)
    {
        if (!_isClikcable) return;
        DOVirtual.Float(0, 0, 0.2f, _ => { })
            .OnComplete(() =>
            {
                UIType clickedUI = 0;
#if UNITY_EDITOR
                //			Debug.Log("LAUNCHER RAY");
#endif
                foreach (var result in results)
                {
#if UNITY_EDITOR
//					Debug.Log($" result Name:{result.gameObject.name}");
#endif
                    // 설정,홈,컨텐츠 **버튼** ---------------------------------------------------------
                    if (Enum.TryParse(SetButtonString(result.gameObject.name), out clickedUI)) ShowTab(clickedUI);

                    // ** 씬 로드** ---------------------------------------------------------
                    if (result.gameObject.name.Contains("SceneName_"))
                    {
                        GetObject((int)UIType.UI_Confirm).SetActive(true);
                        _gameNameWaitingForConfirmation = result.gameObject.name;
                        
                    }
                }
            });
    }


    public void LoadScene(string sceneName)
    {
        var originalName = sceneName;
        var modifiedName = originalName.Substring("SceneName_".Length);

        gameObject.SetActive(false);
        SceneManager.LoadScene(modifiedName);
    }

    private string SetButtonString(string input)
    {
        if (!input.Contains("Button")) return null;

        var originalName = input;
        var modifiedName = originalName.Substring(0, originalName.Length - 6);
        return modifiedName;
    }


    private IEnumerator Active_false()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }



}