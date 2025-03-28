using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Logger 
{
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
    
    
    
    [Conditional("SensorTest")]
    public static void SensorRelatedLog(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    
    
    [Conditional("ContentTest")]
    public static void ContentTestLog(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
}
