using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class Managers : MonoBehaviour
{
    public static Managers s_instance = null;
    public static Managers Instance 
    { 
        get 
        {
            if (s_instance == null)
                Init();
            return s_instance; 
        } 
    }

    private UIManager s_uiManager = new UIManager();
    private ResourceManager s_resourceManager = new ResourceManager();
    private SoundManager s_soundManager = new SoundManager();
    private SensorManager s_sensorManager = new SensorManager();

    public static SoundManager soundManager 
    {  
        get 
        { 
            Init(); 
            return Instance.s_soundManager; 
        } 
    }

    public static UIManager UI 
    { 
        get 
        { 
            Init(); 
            return Instance.s_uiManager; 
        } 
    }

    public static ResourceManager Resource 
    { 
        get 
        { 
            Init(); 
            return Instance.s_resourceManager; 
        } 
    }

    public static SensorManager sensorManager
    {
        get 
        { 
            Init(); 
            return Instance.s_sensorManager; 
        }
    }

    private void Start()
    {
        Init();
    }

    private static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
                go = new GameObject { name = "@Managers" };

            s_instance = Utils.GetOrAddComponent<Managers>(go);
            DontDestroyOnLoad(go);

            s_instance.s_soundManager.Init();
            s_instance.s_sensorManager.Init();
        }
    }
}