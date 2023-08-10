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
    private float elapsedTime;
    public float movingTimeSec;

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
      


        
        if (windowController.isWindowStartOpening)
        {

            elapsedTime += Time.deltaTime;

            // Lerp의 t값을 계산 (0 ~ 1 사이)
            float t = Mathf.Clamp01(elapsedTime / movingTimeSec);
            t = Lerp2D.EaseInQuad(0, 1, t);

            lerp = Lerp2D.EaseInQuad(0, 1, t);
            transform.position = Vector3.Lerp(transform.position, _playStartCamPoint.position, t);
        }
    }
}
