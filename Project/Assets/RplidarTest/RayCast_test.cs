using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RayCast_test : MonoBehaviour
{
    public Canvas Test_UI_Canvas;
    GraphicRaycaster GR;
    PointerEventData PED;

    public float moveSpeed = 5f; // �̹����� �̵� �ӵ�

    // Start is called before the first frame update
    void Start()
    {
        GR = Test_UI_Canvas.GetComponent<GraphicRaycaster>();
        PED = new PointerEventData(null);

        Debug.Log("RAYCAST");
    }
    // Update is called once per frame

    //1. ���콺 Ŭ�� �� �� ����ĳ��Ʈ ���� �ϴ°� Ȯ��
    //2. ���� ĳ��Ʈ�� UI �̹����� �����ϴ� �� Ȯ��
    //3. ���콺�� �ƴ϶� ���� �����ϴ� �̹����� �׷��� �Ǵ��� Ȯ�� �ʿ�
    //3. ���� ���� �۾� >> 3D ������Ʈ�� 2D UI�� ���� �����ؼ� �۵��ϵ��� ����� �ʿ��� ������ ������

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
//            Debug.Log("MOUSE CLICKED");
            

            //���� ī�޶� ���� ĳ��Ʈ
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.transform.name);

            }

            //UI ī�޶� ���� ĳ��Ʈ
            PED.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            GR.Raycast(PED, results);

            //����ó���ϸ� �� ��?
            if (results.Count>0)
            {
                Debug.Log(results[0].gameObject.name);
            }

        }


    }

}
