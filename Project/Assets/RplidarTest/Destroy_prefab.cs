using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Destroy_prefab : MonoBehaviour
{
    private float timer = 0f;
    //public GameObject Text;
    // Start is called before the first frame update


    //=====0822
    private Vector3 Temp_position;
    //

    //====1122
    private GameObject UI_Canvas;
    private Camera UI_Camera;
    GraphicRaycaster GR;
    PointerEventData PED;


    void Start()
    {
        ////중요, 이렇게 할 경우 오브젝트의 위치에 마우스가 위치하는 것을 확인함
        //Temp_position = Camera.main.WorldToScreenPoint(this.gameObject.GetComponent<Transform>().position);
        //Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y));
        //Debug.Log(Temp_position);

        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();

        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);
        //Debug.Log("MOUSE CLICKED");
        // - UI 캔버스든 메인 카메라의 캔버스든 동일 위치 동일 좌표이므로
        //   Ball 오브젝트의 위치를 스크린 좌표로 가져온 다음
        //   해당 좌표를 이용해서 메인 카메라의 레이, PED 레이를 각각 만들어줌

        Temp_position = UI_Camera.WorldToScreenPoint(this.transform.position);

        //메인 카메라 레이 캐스트
        Ray ray = Camera.main.ScreenPointToRay(Temp_position);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.name);

        }

        //UI 카메라 레이 캐스트
        PED.position = Temp_position;
        List<RaycastResult> results = new List<RaycastResult>();
        GR.Raycast(PED, results);

        if (results.Count > 0)
        {
            Debug.Log(results[0].gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
            Destroy_obj();
        }
    }

    void Destroy_obj()
    {
        Destroy(this.gameObject);
    }
}
