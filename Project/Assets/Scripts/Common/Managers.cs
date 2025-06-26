using System.Threading;
using Unity.VisualScripting;
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
    private static PlayInfoManager _sInfoManager = new PlayInfoManager();
    private static CursorImageManager s_cursorImageManager= new CursorImageManager();
    private static SettingManager s_SettingManager = new SettingManager();
    
    public static float PROJECTOR_SCREEN_HEIGHT; 
    public static bool isGameStopped { get; set; }


    public static SettingManager Setting
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
    public static PlayInfoManager UserInfo 
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


    private static Mutex mutex;



    private void Awake()
    {

        CheckSingleInstance();

        Init();
    }



    void OnApplicationQuit()
    {
        RemoveMutex();
    }
    
    
    
    
    /// <summary>
    /// 소프트웨어 중복실행 방지로직입니다. 
    /// </summary>
    private void CheckSingleInstance()
    {
        bool isNewInstance;
        string mutexName = "META_EDU"; // 시스템 전체에서 유일한 이름 사용

        mutex = new Mutex(true, mutexName, out isNewInstance);

        if (!isNewInstance)
        {
            Debug.Log("앱이 이미 실행 중입니다.");
            Application.Quit(); // 실행 중인 인스턴스가 있으면 종료
        }
    }

    private void RemoveMutex()
    {
        if (mutex != null)
        {
            mutex.ReleaseMutex();
            mutex.Close();
            mutex = null;
        }
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
            
            Application.runInBackground = true;
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