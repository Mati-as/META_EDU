using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public class Mousecontroller : MonoBehaviour
{

    //1012 마우스 클릭 기능 확인


    public bool MouseClickOn = false;
    private GameObject CANVAS;


    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
    //Mouse actions
    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
    private const int MOUSEEVENTF_RIGHTUP = 0x10;



    private float Cursor_x;
    private float Cursor_y;
    private float pre_Cursor_x;
    private float pre_Cursor_y;

    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        CANVAS = GameObject.Find("UIManager");
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    MouseClickOn = !MouseClickOn;
        //}

        timer += Time.deltaTime;

        //if (MouseClickOn)
        //{
        //    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        //}

        //데모용, 클릭 이벤트 발생
        if (MouseClickOn)
        {
            Cursor_x = Input.mousePosition.x;
            Cursor_y = Input.mousePosition.y;

            //정해놓은 사각형 밖일 경우 클릭 발생
            if (pre_Cursor_x - Cursor_x < -50 || pre_Cursor_x - Cursor_x > 50)
            {
                if (pre_Cursor_y - Cursor_y < -50 || pre_Cursor_y - Cursor_y > 50)
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                }
            }
            //사각형 밖을 벗어나진 않았지만, 이동이 발생했고 시간이 1초 이상 경과 했을 경우 클릭 발생
            else
            {
                if (pre_Cursor_x != Cursor_x)
                {
                    //2D 콘텐츠의 경우 0.1 그외에 나머지 0.5
                    if (timer > 0.1f)
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        timer = 0;
                    }
                }
            }
            pre_Cursor_x = Cursor_x;
            pre_Cursor_y = Cursor_y;
        }

        //if (Input.GetMouseButton(0))
        //{
        //    Debug.Log("CLICKED");
        //}
    }
}
