using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MOUSE_prefab : MonoBehaviour
{
    public GameObject CAMERA;
    private Camera camera;
    private Vector3 Temp_position;
    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        Destroy_obj();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Destroy_obj()
    {
        Temp_position = camera.WorldToScreenPoint(this.transform.position);
        //Debug.Log("BEFORE 1. temp_x : " + this.transform.position.x + "  2. temp_y : " + (1080 - this.transform.position.y));
        //Debug.Log("AFTER 1. temp_x : " + Temp_position.x + "  2. temp_y : " + (1080 - Temp_position.y));
        Mouse.current.WarpCursorPosition(new Vector2(Temp_position.x, Temp_position.y ));

        Destroy(this.gameObject);
    }
}
