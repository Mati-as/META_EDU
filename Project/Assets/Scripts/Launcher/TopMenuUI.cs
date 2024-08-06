using System;
using System.Collections;
using System.Collections.Generic;
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
    private bool _isSensorRefreshable =true;

    private const int REFRESH_INTERIM_MIN = 10;
    private WaitForSeconds _wait = new (REFRESH_INTERIM_MIN);
    private Button[] _btns;
    // scene-related part -----------------------------------
 
    // Start is called before the first frame update
    void Start()
    {
        BindButton(typeof(UI_Type));
        GetButton((int)UI_Type.Btn_Home).gameObject.BindEvent(OnQuit);
        GetButton((int)UI_Type.Btn_SensorRefresh).gameObject.BindEvent(RefreshSensor);
        GetButton((int)UI_Type.Btn_Quit).gameObject.BindEvent(OnQuit);
    }
    private void RefreshSensor()
    {
        if (_isSensorRefreshable)
        {
            StartCoroutine(ResetSensorRefreshable());
            OnRefreshEvent?.Invoke();
        }
    }

    IEnumerator ResetSensorRefreshable()
    {
        _isSensorRefreshable = false;
        yield return _wait;
        _isSensorRefreshable = true;
    }
    
    private void OnQuit()
    {
        StartCoroutine(QuitApplicationCo());
    }

    private IEnumerator QuitApplicationCo()
    {
        OnSceneQuit?.Invoke(SceneManager.GetActiveScene().name,DateTime.Now);
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }
}
