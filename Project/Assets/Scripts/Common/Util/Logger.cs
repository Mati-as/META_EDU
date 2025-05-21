using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Logger 
{
    
    /// <summary>
    /// 일반로그 
    /// </summary>
    /// <param name="msg"></param>
    [Conditional("DevOnly")]
    public static void Log(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    [Conditional("DevOnly")]
    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarningFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    
    //[Conditional("Dev")] 에러는 개발용 상관없이 무조건 에러가 남게 해주어야 합니다. 
    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogErrorFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    
    
    /// <summary>
    /// 센서관련을 사용하는 로그 입니다.
    /// </summary>
    /// <param name="msg"></param>
    [Conditional("SensorTest")]
    public static void SensorRelatedLog(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    
    /// <summary>
    /// 개별 컨텐츠별 사용하는 로그 입니다.
    /// </summary>
    /// <param name="msg"></param>
    
    [Conditional("ContentTest")]
    public static void ContentTestLog(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    
    /// <summary>
    /// 공통모듈, 코어 클래스에서만 사용되는 로그입니다.
    /// </summary>
    /// <param name="msg"></param>
    
    [Conditional("CoreClass")]
    public static void CoreClassLog(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
}
