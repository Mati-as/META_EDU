using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThreeDtoTwoD : MonoBehaviour
{
    // Start is called before the first frame update


    private Vector3 Temp_position;

    public GameObject Origin_obj;
    public GameObject Moving_obj;

    void Start()
    {
        //기준이 되는 오브젝트로 부터 거리값만가지고 이동 시킨 다음에
        //포지션 추출하고
        //마우스 이동시키는 방법으로 진행하기

        Debug.Log(this.gameObject.transform.position);
        Temp_position = Camera.main.WorldToScreenPoint(this.gameObject.GetComponent<Transform>().position);
        Debug.Log(Temp_position);

        //Mouse.current.WarpCursorPosition(new Vector2(400, 400));
        Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y));
        //Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.z));
        Debug.Log(Input.mousePosition);
        

        //Instantiate(spherePrefab, transform.forward * data[i].distant * 0.01f, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        //Moving_obj.transform.position = Origin_obj.transform.forward * 10f;
        Temp_position = Camera.main.WorldToScreenPoint(this.gameObject.GetComponent<Transform>().position);
        Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y));
        //중요, 이렇게 할 경우 원하는 것처럼 회전 하는것에 따라서 forward로 생성이 되는 것을 확인함
    }
}
