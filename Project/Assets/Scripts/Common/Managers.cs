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

    //private static MetaEduLauncher s_launcher = new MetaEduLauncher();
    private static UIManager s_uiManager = new UIManager();
    private static ResourceManager s_resourceManager = new ResourceManager();
    private static SoundManager s_soundManager = new SoundManager();
    private static SensorManager s_sensorManager = new SensorManager();
    private static PlayerInfoManager _sInfoManager = new PlayerInfoManager();
    private static CursorImageManager s_cursorImageManager= new CursorImageManager();
    private static A_SettingManager s_SettingManager = new A_SettingManager();
    
    public static float PROJECTOR_SCREEN_HEIGHT; 
    public static bool isGameStopped { get; set; }

    // public static MetaEduLauncher launcher 
    // {  
    //     get 
    //     { 
    //         Init(); 
    //         return s_launcher; 
    //     } 
    // }
    
    public static A_SettingManager Setting
    {  
        get 
        { 
            Init(); 
            return s_SettingManager; 
        } 
    }
    public static CursorImageManager CursorImage
    {  
        get 
        { 
            Init(); 
            return s_cursorImageManager; 
        } 
    }
    public static PlayerInfoManager UserInfo 
    {  
        get 
        { 
            Init(); 
            return _sInfoManager; 
        } 
    }
    public static SoundManager Sound 
    {  
        get 
        { 
            Init(); 
            return s_soundManager; 
        } 
    }

    public static UIManager UI 
    { 
        get 
        { 
            Init(); 
            return s_uiManager; 
        } 
    }

    public static ResourceManager Resource 
    { 
        get 
        { 
            Init(); 
            return s_resourceManager; 
        } 
    }

    public static SensorManager Sensor
    {
        get 
        { 
            Init(); 
            return s_sensorManager; 
        }
    }




    private void Awake()
    {
        Init();
    }

    
    /// <summary>
    /// Manager별 순서 바뀌지않도록 주의합니다.
    /// 예를들어 SoundManager의 멤버변수 및 초기화는 SettingManager가
    /// 읽어온 멤버변수를 필요로 합니다. 
    /// </summary>
    private static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
                go = new GameObject { name = "@Managers" };
            
            GameObject launcher = GameObject.Find("@LauncherRoot");
            s_instance = Utils.GetOrAddComponent<Managers>(go);
            s_SettingManager.Init();
            PROJECTOR_SCREEN_HEIGHT = Setting.SCREEN_PROJECTOER_HEIGHT_FROM_XML;
            // s_launcher.Init(); 
            // s_launcher = Utils.GetOrAddComponent<MetaEduLauncher>(launcher);
            
            s_soundManager.Init();
            _sInfoManager.Init();
            s_cursorImageManager.Init();
            DontDestroyOnLoad(go);
//            Debug.Log("Managers Set--------");
        }
    }
    
    
}