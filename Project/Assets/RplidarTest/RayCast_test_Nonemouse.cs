using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RayCast_test_Nonemouse : MonoBehaviour
{
    public Canvas Test_UI_Canvas;
    GraphicRaycaster GR;
    PointerEventData PED;
    private Camera UI_Camera;
    private Vector3 Temp_position;

    public float moveSpeed = 5f; // 이미지의 이동 속도

    // Start is called before the first frame update
    void Start()
    {
        GR = Test_UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);

        UI_Camera= GameObject.FindWithTag("UICamera").GetComponent<Camera>();
    }
    // Update is called once per frame

    //1. 마우스 클릭 할 때 레이캐스트 생성 하는거 확인
    //2. 레이 캐스트랑 UI 이미지랑 반응하는 것 확인
    //3. 마우스가 아니라 내가 제어하는 이미지로 그렇게 되는지 확인 필요
    //3. 최종 연동 작업 >> 3D 오브젝트랑 2D UI를 각각 구분해서 작동하도록 방법이 필요할 것으로 생각됨

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;

        // 이미지 이동
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("MOUSE CLICKED");
            // - UI 캔버스든 메인 카메라의 캔버스든 동일 위치 동일 좌표이므로
            //   Ball 오브젝트의 위치를 스크린 좌표로 가져온 다음
            //   해당 좌표를 이용해서 메인 카메라의 레이, PED 레이를 각각 만들어줌

            Temp_position = UI_Camera.WorldToScreenPoint(this.transform.position);
            //Debug.Log("Image Screen: " + Temp_position);

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

            //예외처리하면 될 듯?
            if (results.Count > 0)
            {
                Debug.Log(results[0].gameObject.name);
            }

        }

    }

}

