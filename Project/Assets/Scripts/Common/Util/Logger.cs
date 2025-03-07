using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class Logger 
{
    [Conditional("Dev")]
    public static void Log(string msg)
    {
        UnityEngine.Debug.LogFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    [Conditional("Dev")]
    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarningFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    
    //[Conditional("Dev")] 에러는 개발용 상관없이 무조건 에러가 남게 해주어야 합니다. 
    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogErrorFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
    
    [Conditional("RaycastDebug")]
    public static void CliclLog(string msg)
    {
        UnityEngine.Debug.LogErrorFormat("[{0}] {1}",System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),msg);
    }
}
