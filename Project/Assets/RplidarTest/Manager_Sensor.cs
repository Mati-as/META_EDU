using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_Sensor : MonoBehaviour
{
    public static Manager_Sensor instance = null;

    public GameObject UI_Canvas;
    public Camera UI_Camera;

    private FP_controller FPC;
    //1212 수정
    public RectTransform Ray_position;

    public float Prev_Ray_position_x;
    public float Prev_Ray_position_y;

    public GameObject BALLPrefab;

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
        UI_Camera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        UI_Canvas = GameObject.FindWithTag("UIManager");

        FPC = this.GetComponent<FP_controller>();
        //1212 수정
        Ray_position = this.gameObject.GetComponent<RectTransform>();
        Prev_Ray_position_x = 0f;
        Prev_Ray_position_y = 0f;

    }
    public GameObject Get_UIcanvas()
    {
        return UI_Canvas;
    }
    public Camera Get_UIcamera()
    {
        return UI_Camera;
    }
    public FP_controller Get_RPC()
    {
        return FPC;
    }
    //1212 수정
    public void Set_RayPosition(RectTransform RayPos)
    {
        Ray_position = RayPos;
    }
    public void Set_PrevRayPosition(float x, float y)
    {
        Prev_Ray_position_x = x;
        Prev_Ray_position_y = y;

    }
}
