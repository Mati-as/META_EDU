using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

using System;

[StructLayout(LayoutKind.Sequential,CharSet = CharSet.Ansi)]
public struct LidarData
{
    public byte syncBit;
    public float theta;
    public float distant;
    public uint quality;
};


public class RplidarBinding
{
#if UNITY_EDITOR_64
    private const string _dllFileName = "RplidarCppEditorOnly.dll";
#else 
  private const string _dllFileName = "RplidarCpp.dll";
#endif
    static RplidarBinding()
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
#if UNITY_EDITOR_64
        currentPath += Path.PathSeparator + Application.dataPath + "/Plugins_EditorOnly/x86_64/";
        Debug.Log("Rplida :64");
    
#else 
          currentPath += Path.PathSeparator + Application.dataPath+ "/Plugins/x86_64/";
        Debug.Log("Rplida :Editor");
#endif
        Environment.SetEnvironmentVariable("PATH", currentPath);
    }


    [DllImport(_dllFileName)]
    public static extern int OnConnect(string port);
    [DllImport(_dllFileName)]
    public static extern bool OnDisconnect();

    [DllImport(_dllFileName)]
    public static extern bool StartMotor();
    [DllImport(_dllFileName)]
    public static extern bool EndMotor();

    [DllImport(_dllFileName)]
    public static extern bool StartScan();
    [DllImport(_dllFileName)]
    public static extern bool EndScan();

    [DllImport(_dllFileName)]
    public static extern bool ReleaseDrive();

    [DllImport(_dllFileName)]
    public static extern int GetLDataSize();

    [DllImport(_dllFileName)]
    private static extern void GetLDataSampleArray(IntPtr ptr);

    [DllImport(_dllFileName)]
    private static extern int GrabData(IntPtr ptr);

    public static LidarData[] GetSampleData()
    {
        var d = new LidarData[2];
        var handler = GCHandle.Alloc(d, GCHandleType.Pinned);
        GetLDataSampleArray(handler.AddrOfPinnedObject());
        handler.Free();
        return d;
    }

    public static int GetData(ref LidarData[] data)
    {
        var handler = GCHandle.Alloc(data, GCHandleType.Pinned);
        int count = GrabData(handler.AddrOfPinnedObject());
        handler.Free();

        return count;
    }
}
