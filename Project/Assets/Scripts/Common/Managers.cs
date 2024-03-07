using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
 public static Managers s_instance = null;
    public static Managers Instance { get { return s_instance; } }
    private static UIManager s_uiManager = new UIManager();
    private static SoundManager s_soundManager = new SoundManager();

    public static SoundManager Sound {  get { Init(); return s_soundManager; } }
    public static UIManager UI { get { Init(); return s_uiManager; } }
   
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
            
            s_soundManager.Init();
            
          
            

        }
    }
}
