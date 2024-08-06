using System;
using System.Collections;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.UI;

public class LidarSensorRefresher : UI_PopUp
{
    //
    // private enum SensorFunc
    // {
    //     Btn_SensorRefresh,
    //     Max
    // }
    //
    // public static event Action OnRefreshEvent;
    // private bool _isSensorRefreshable =true;
    // private WaitForSeconds _wait = new WaitForSeconds(7f);
    // private Button[] _btns;
    //
    // public override bool Init()
    // {
    //     BindButton(typeof(SensorFunc));
    //     GetButton((int)SensorFunc.Btn_SensorRefresh).gameObject.BindEvent(RefreshSensor);
    //      
    //     return true;
    // }
    //
    // private void RefreshSensor()
    // {
    //     if (_isSensorRefreshable)
    //     {
    //         StartCoroutine(ResetSensorRefreshable());
    //         OnRefreshEvent?.Invoke();
    //     }
    //
    // }
    //
    // IEnumerator ResetSensorRefreshable()
    // {
    //     _isSensorRefreshable = false;
    //     yield return _wait;
    //     _isSensorRefreshable = true;
    // }
}
