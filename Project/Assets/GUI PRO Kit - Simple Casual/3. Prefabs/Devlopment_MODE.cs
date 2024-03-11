using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Devlopment_MODE : MonoBehaviour
{
    public GameObject Button_Dev;
    public GameObject Button_Demo;

    private bool Mode_Development = false;
    
    // Start is called before the first frame update
    void Start()
    {
        //현재 실행 모드 체크하고 해당 변수 활성화   
        Debug.Log("개발자 모드 화면");
    }

    public void Mode()
    {
        if (Mode_Development == false)
        {

            //공이 나오고 터치 O
            //UI활성화
            Debug.Log("개발자 모드 활성화 됨");
            Button_Demo.SetActive(false);
            Button_Dev.SetActive(true);
        }
        else if(Mode_Development == true)
        {

            //공나오지 않고 터치 가능
            //UI 비활성화
            Debug.Log("데모 모드 활성화 됨");
            Button_Dev.SetActive(false);
            Button_Demo.SetActive(true);
        }
        Mode_Development =!Mode_Development;

    }
}
