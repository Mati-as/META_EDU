using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_Sensor : MonoBehaviour
{
    public static Manager_Sensor instance = null;

    public GameObject UI_Canvas;
    public Camera UI_Camera;


    // Start is called before the first frame update
    private void Awake()
    {
        Init();
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
                Destroy(this.gameObject);
        }
    }
    

    private void Init()
    {
        UI_Camera = GameObject.Find("UICamera").GetComponent<Camera>();
        UI_Canvas = GameObject.Find("UIManager");

    }
    public GameObject Get_UIcanvas()
    {
        return UI_Canvas;
    }
    public Camera Get_UIcamera()
    {
        return UI_Camera;
    }
}
