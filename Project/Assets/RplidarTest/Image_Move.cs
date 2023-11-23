using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Image_Move : MonoBehaviour
{
    public float moveSpeed = 5f; // 이미지의 이동 속도

    public Image imageA; // A 이미지

    private GameObject UI_Canvas;
    private Camera UI_Camera;

    private GraphicRaycaster GR;
    private PointerEventData PED;
    private Vector3 Temp_position;

    void Start()
    {
        UI_Canvas = Manager_Sensor.instance.Get_UIcanvas();
        UI_Camera = Manager_Sensor.instance.Get_UIcamera();

        GR = UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);

    }
    void Update()
    {
        // 수평 및 수직 입력을 받아 이동 방향을 계산
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;

        // 이미지 이동
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootRay();
        }
    }

    void ShootRay()
    {
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
}
