using System;
using System.Collections;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

public class InGame_SideMenu : UI_PopUp
{
    private enum Btn_Type
    {
        Btn_Setting,
        Btn_Home,
        //Btn_SensorRefresh,
        Btn_Quit,
        SettingCloseButton,
        Btn_SensorShowGuide,
        Btn_SideMenu,
    }
    
    private enum UI
    {
        Setting,
        MainVolume,
        BGMVolume,
        EffectVolume,
        NarrationVolume,
        SideMenu_ScreenDim
      
    }

    //sensor-related part.-----------------------------------
    
    // 센서 활성화(초기화) 시작버튼으로 이전
    // public static event Action OnRefreshEvent;
    // private bool _isSensorRefreshable = true;
    public static event Action<string, DateTime> OnSceneQuit;
    public static event Action<string, DateTime> OnAppQuit;
    
   // private bool _isXMLSavable = true;

    DevelopmentUIManager _devUIManager;
    
    private const int REFRESH_INTERIM_MIN = 10;
    private readonly WaitForSeconds _wait = new(REFRESH_INTERIM_MIN);
    private Button[] _btns;
    // scene-related part -----------------------------------

    private bool _isUIClickable = true;
    private bool _isSideMenuOn = false;

    private Image _screenDim;
    private Animator _animator;
    // Start is called before the first frame update
    private void Start()
    {
        BindButton(typeof(Btn_Type));
        BindObject(typeof(UI));
        
        GetButton((int)Btn_Type.Btn_Home).gameObject.BindEvent(OnSceneQuitAndToHomeScreen);
        GetButton((int)Btn_Type.Btn_Home).gameObject.BindEvent(OnHomeBtnClicked);
        _screenDim = GetObject((int)UI.SideMenu_ScreenDim).GetComponent<Image>();
        GetButton((int)Btn_Type.Btn_Quit).gameObject.BindEvent(OnQuit);
       // GetButton((int)Btn_Type.Btn_SensorRefresh).gameObject.BindEvent(RefreshSensor);
        
        GetButton((int)Btn_Type.Btn_Setting).gameObject.BindEvent(OnSettingBtnClicked,Define.UIEvent.PointerUp);
        GetButton((int)Btn_Type.SettingCloseButton).gameObject.BindEvent(() =>
        {
            isSettingActive = false;
            GetObject((int)UI.Setting).gameObject.SetActive(isSettingActive);
        });

        _animator = GetComponent<Animator>();
        GetButton((int)Btn_Type.Btn_SideMenu).onClick.AddListener(() =>
        {
            if (!_isUIClickable) return;
            _isUIClickable = false;
            
            _isSideMenuOn = !_isSideMenuOn;
            _animator.SetBool(IsOn, _isSideMenuOn);
           // _screenDim.raycastTarget = _isSideMenuOn;

            DOVirtual.DelayedCall(0.5f, () =>
            {
                _isUIClickable = true;
            });
        });
        
        
        _devUIManager = GameObject.FindWithTag("LidarMenu").GetComponent<DevelopmentUIManager>();
        Debug.Assert(_devUIManager!=null);
        GetButton((int)Btn_Type.Btn_SensorShowGuide).gameObject.BindEvent(() =>
        {
            _devUIManager.DisableAllImages();
        });
        
        
        GetObject((int)UI.Setting).gameObject.SetActive(false);
        
        SetSlider();
    }

    // private void RefreshSensor()
    // {
    //     if (!_isSensorRefreshable) return;
    //
    //     StartCoroutine(ResetSensorRefreshable());
    //     OnRefreshEvent?.Invoke();
    // }
    //
    // private IEnumerator ResetSensorRefreshable()
    // {
    //     _isSensorRefreshable = false;
    //     yield return _wait;
    //     _isSensorRefreshable = true;
    // }

    private IEnumerator XMLSaveCo()
    {
        OnSceneQuit?.Invoke(SceneManager.GetActiveScene().name, DateTime.Now);
        yield return _wait;
        //_isXMLSavable = true;
    }

    private void OnSceneQuitAndToHomeScreen()
    {
#if UNITY_EDITOR
        Debug.Log("Scene Quit ");
# endif
       // if (!_isXMLSavable) return;
       // _isXMLSavable = false;
        StartCoroutine(XMLSaveCo());
    }

    private void OnQuit()
    {
        StartCoroutine(QuitApplicationCo());
    }

    private IEnumerator QuitApplicationCo()
    {
#if UNITY_EDITOR
        Debug.Log("App Quit ");
# endif

        OnAppQuit?.Invoke(SceneManager.GetActiveScene().name, DateTime.Now);
        yield return new WaitForSeconds(1f);
        Application.Quit();
      //  _isXMLSavable = true;
    }
    
    private Base_GameManager _gm;




    private bool isSettingActive = false;
    public void OnSettingBtnClicked()
    {
        isSettingActive = !isSettingActive;
        GetObject((int)UI.Setting).gameObject.SetActive(isSettingActive);
    }
    
    
    public void OnHomeBtnClicked()
    {
        //Debug.Log("HomeBtnClicked");
        MetaEduLauncher.isBackButton =
            gameObject.name.Contains("Back") ? true : false;

        StartCoroutine(ChangeScene());

    }

    private WaitForSeconds _waitForSceneChange =new WaitForSeconds(1.0f); 
    private IEnumerator ChangeScene()
    {

        Managers.IsGameStopped = true;
       
        yield return _waitForSceneChange;

        TerminateProcess();
            
        
        if(GameObject.FindWithTag("GameManager") == null) Logger.Log("Successfully Destroyed");
        
        SceneManager.LoadScene("METAEDU_LAUNCHER");
   

      
    }

    /// <summary>
    /// 씬이동 초기화 수행 전, 다양한 초기화를 진행합니다.
    /// </summary>
    private void TerminateProcess()
    {
        
        DOTween.KillAll();
    }
    private Slider[] _volumeSliders = new Slider[(int)SoundManager.Sound.Max];
    private static readonly int IsOn = Animator.StringToHash("isOn");

    private void SetSlider()
    {
         _volumeSliders = new Slider[(int)SoundManager.Sound.Max];

        _volumeSliders[(int)SoundManager.Sound.Main] = GetObject((int)UI.MainVolume).GetComponent<Slider>();
        _volumeSliders[(int)SoundManager.Sound.Main].value =
            Managers.Sound.volumes[(int)SoundManager.Sound.Main];
#if UNITY_EDITOR
        Debug.Log($" 메인 볼륨 {Managers.Sound.volumes[(int)SoundManager.Sound.Main]}");
#endif

        _volumeSliders[(int)SoundManager.Sound.Bgm] = GetObject((int)UI.BGMVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Effect] = GetObject((int)UI.EffectVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Narration] =
            GetObject((int)UI.NarrationVolume).GetComponent<Slider>();

        for (var i = 0; i < (int)SoundManager.Sound.Max; i++)
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

    }
    
    
    

}