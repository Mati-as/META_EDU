using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class Check_resolution : MonoBehaviour
{
    private string Resolution;
    private GameObject CANVAS;
    public GameObject Guideline;

    public GameObject Res_text;
    public GameObject CANVAS_text;
    public GameObject Guideline_text;
    public GameObject Mousepos_text;
    // Start is called before the first frame update
    void Start()
    {
        CANVAS = GameObject.Find("UIManager");
        //CANVAS = GameObject.Find("UI 캔버스 (UI Manager)");

        Resolution = Screen.width.ToString() + "x" + Screen.height.ToString();
        Res_text.GetComponent<Text>().text = Resolution;

        Resolution = CANVAS.GetComponent<RectTransform>().rect.width.ToString() + "x" + CANVAS.GetComponent<RectTransform>().rect.height.ToString();
        CANVAS_text.GetComponent<Text>().text = Resolution;

        Resolution = Guideline.GetComponent<RectTransform>().rect.width.ToString() + "x" + Guideline.GetComponent<RectTransform>().rect.height.ToString();
        Guideline_text.GetComponent<Text>().text = Resolution;
    }

    // Update is called once per frame
    void Update()
    {

        Resolution = Input.mousePosition.x.ToString() + "x" + Input.mousePosition.y.ToString();
        Mousepos_text.GetComponent<Text>().text = Resolution;
    }
}
