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
    // 구형버전, 빨강센서
    // private const string DLL_FILENAME = "RplidarCppLegacy.dll";
    // private const string DLL_PATH = "/Plugins_Legacy/x86_64";

    // // 신형버전, 보라색센서 (모델명: A2M12)
    private const string DLL_FILENAME = "RplidarCppA2M12.dll";
    private const string DLL_PATH = "/Plugins_A2M12/x86_64/";


    static RplidarBinding()
    {
        var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
        currentPath += Path.PathSeparator + Application.dataPath + DLL_PATH;
        Debug.Log($"Filename: {DLL_FILENAME}, current Path Of Lida: {currentPath}");
        Environment.SetEnvironmentVariable("PATH", currentPath);
    }


    [DllImport(DLL_FILENAME)]
    public static extern int OnConnect(string port);
    [DllImport(DLL_FILENAME)]
    public static extern bool OnDisconnect();

    [DllImport(DLL_FILENAME)]
    public static extern bool StartMotor();
    [DllImport(DLL_FILENAME)]
    public static extern bool EndMotor();

    [DllImport(DLL_FILENAME)]
    public static extern bool StartScan();
    [DllImport(DLL_FILENAME)]
    public static extern bool EndScan();

    [DllImport(DLL_FILENAME)] 
    public static extern bool ReleaseDrive();

    [DllImport(DLL_FILENAME)]
    public static extern int GetLDataSize();

    [DllImport(DLL_FILENAME)]
    private static extern void GetLDataSampleArray(IntPtr ptr);

    [DllImport(DLL_FILENAME)]
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
