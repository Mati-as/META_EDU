using System;
using System.Collections;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopMenuUI : UI_PopUp
{
    private enum UI_Type
    {
        Btn_Setting,
        Btn_Home,
        Btn_SensorRefresh,
        Btn_Quit
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
        BindButton(typeof(UI_Type));
        GetButton((int)UI_Type.Btn_Home).gameObject.BindEvent(OnSceneQuitAndToHomeScreen);
        GetButton((int)UI_Type.Btn_Quit).gameObject.BindEvent(OnQuit);
        GetButton((int)UI_Type.Btn_SensorRefresh).gameObject.BindEvent(RefreshSensor);
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
}