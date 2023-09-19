using System;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Positions")] [Space(10f)]
    public Transform _introStartCamPoint;

    public Transform _playStartCamPoint;

    [Space(30f)] [Header("Speed")] [Space(10f)]
    public float _moveSpeed;

    public float movingTimeSec;

    [Space(30f)] [Header("Reference")] [Space(10f)] 
    [SerializeField]
    private WindowController windowController;

    private float _moveSpeedLerp;
    private float elapsedTime;
    public float Xoffset;



    private void Deactivate() => gameObject.SetActive(false);

    private void Start()
    {
        transform.position = _introStartCamPoint.position;
        transform.LookAt(_playStartCamPoint);
        transform.rotation *= Quaternion.Euler(Xoffset,0,0);
    }

    private void Update()
    {
        if (windowController.isWindowStartOpening)
        {
            MoveTowardPlayScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("camera"))
        {
            GameManager.isCameraArrivedToPlay = true;
        }
    }

    private void MoveTowardPlayScene()
    {
        elapsedTime += Time.deltaTime;

        // Lerp의 t값을 계산 (0 ~ 1 사이)
        var t = Mathf.Clamp01(elapsedTime / movingTimeSec);
        t = Lerp2D.EaseInQuad(0, 1, t);


        transform.position = Vector3.Lerp(transform.position, _playStartCamPoint.position, t);
    }

 
}