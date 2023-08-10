using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    [Header("Camera Positions")]
    [Space(10f)]
    public Transform _introStartCamPoint;
    public Transform _playStartCamPoint;

    [Space(30f)]


    [Header("Speed")]
    [Space(10f)]
    public float _moveSpeed;
    private float _moveSpeedLerp;


    [Space(30f)]
    [Header("Reference")]
    [Space(10f)]

    [SerializeField]
    private WindowController windowController;
    private float lerp;

    void Start()
    {
        transform.position = _introStartCamPoint.position;
    }

    void Update()
    {
        Debug.Log($"window is opend? : {windowController.isWindowStartOpening}");
        if (windowController.isWindowStartOpening)
        {
            lerp = Lerp2D.EaseInQuad(0, 1, _moveSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, _playStartCamPoint.position, lerp);
        }
    }
}
