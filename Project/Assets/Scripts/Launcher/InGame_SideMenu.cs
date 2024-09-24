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
        Btn_SensorRefresh,
        Btn_Quit,
        SettingCloseButton
     
    }
    
    private enum UIType
    {
        Setting,
        MainVolume,
        BGMVolume,
        EffectVolume,
        NarrationVolume
    }

    //sensor-related part.-----------------------------------
    public static event Action OnRefreshEvent;
    public static event Action<string, DateTime> OnSceneQuit;
    public static event Action<string, DateTime> OnAppQuit;
    private bool _isSensorRefreshable = true;
    private bool _isXMLSavable = true;

    private const int REFRESH_INTERIM_MIN = 10;
    private readonly WaitForSeconds _wait = new(REFRESH_INTERIM_MIN);
    private Button[] _btns;
    // scene-related part -----------------------------------

    // Start is called before the first frame update
    private void Start()
    {
        BindButton(typeof(Btn_Type));
        BindObject(typeof(UIType));
        
        GetButton((int)Btn_Type.Btn_Home).gameObject.BindEvent(OnSceneQuitAndToHomeScreen);
        GetButton((int)Btn_Type.Btn_Home).gameObject.BindEvent(OnHomeBtnClicked);
        
        GetButton((int)Btn_Type.Btn_Quit).gameObject.BindEvent(OnQuit);
        GetButton((int)Btn_Type.Btn_SensorRefresh).gameObject.BindEvent(RefreshSensor);
        
        GetButton((int)Btn_Type.Btn_Setting).gameObject.BindEvent(OnSettingBtnClicked,Define.UIEvent.PointerUp);
        GetButton((int)Btn_Type.SettingCloseButton).gameObject.BindEvent(() =>
        {
            GetObject((int)UIType.Setting).gameObject.SetActive(false);
        });
        GetObject((int)UIType.Setting).gameObject.SetActive(false);

        SetSlider();
    }

    private void RefreshSensor()
    {
        if (!_isSensorRefreshable) return;

        StartCoroutine(ResetSensorRefreshable());
        OnRefreshEvent?.Invoke();
    }

    private IEnumerator ResetSensorRefreshable()
    {
        _isSensorRefreshable = false;
        yield return _wait;
        _isSensorRefreshable = true;
    }

    private IEnumerator XMLSaveCo()
    {
        OnSceneQuit?.Invoke(SceneManager.GetActiveScene().name, DateTime.Now);
        yield return _wait;
        _isXMLSavable = true;
    }

    private void OnSceneQuitAndToHomeScreen()
    {
#if UNITY_EDITOR
        Debug.Log("Scene Quit ");
# endif
        if (!_isXMLSavable) return;
        _isXMLSavable = false;
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
        _isXMLSavable = true;
    }
    
    private Base_GameManager _gm;




    private bool isSettingActive = false;
    public void OnSettingBtnClicked()
    {
        isSettingActive = !isSettingActive;
        GetObject((int)UIType.Setting).gameObject.SetActive(!isSettingActive);
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

        Managers.isGameStopped = true;
       
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
    private void SetSlider()
    {
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

    }
}