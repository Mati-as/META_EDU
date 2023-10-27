using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Positions")] [Space(10f)]
    public Transform _introStartCamPoint;

    public Transform _playStartCamPoint;

    [Space(30f)] [Header("Speed")] [Space(10f)]
    public float _moveSpeed;

    public float movingTimeSec;

    [Space(30f)] [Header("Reference")] [Space(10f)] [SerializeField]
    private WindowController windowController;

    private float _moveSpeedLerp;
    private float elapsedTime;
    public float Xoffset;
    public float rotationDuration = 1.0f;

    private readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new();

    private WaitForSeconds GetWaitForSeconds(float seconds)
    {
        if (!waitForSecondsCache.ContainsKey(seconds)) waitForSecondsCache[seconds] = new WaitForSeconds(seconds);
        return waitForSecondsCache[seconds];
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        SubscribeGameManagerEvents();
        transform.position = _introStartCamPoint.position;
        transform.LookAt(_playStartCamPoint);
        // transform.rotation *= Quaternion.Euler(Xoffset,0,0);
    }

    private void Update()
    {
        if (windowController.isWindowStartOpening) MoveTowardPlayScene();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("camera")) GameManager.isCameraArrivedToPlay = true;
    }

    private void OnDestroy()
    {
        UnsubscribeGamaManagerEvents();
    }


    //코루틴 및 메소드 -------------------------
    private void MoveTowardPlayScene()
    {
        elapsedTime += Time.deltaTime;

        // Lerp의 t값을 계산 (0 ~ 1 사이)
        var t = Mathf.Clamp01(elapsedTime / movingTimeSec);
        t = Lerp2D.EaseInQuad(0, 1, t);


        transform.position = Vector3.Lerp(transform.position, _playStartCamPoint.position, t);
    }

    private void OnGameStart()
    {
        _cameraRotateCoroutine = StartCoroutine(RotateOverTime(Xoffset, rotationDuration));
    }

    private void OnGameFinished()
    {
        _cameraRotateCoroutine = StartCoroutine(RotateOverTime(-Xoffset, rotationDuration));
    }

    private Coroutine _cameraRotateCoroutine;
    public float waitTimeToRotateCamera;

    private IEnumerator RotateOverTime(float angle, float duration)
    {
        yield return GetWaitForSeconds(waitTimeToRotateCamera);
        var startRotation = transform.rotation;
        var endRotation = startRotation * Quaternion.Euler(angle, 0, 0);
        var elapsed = 0f;
        
        while (true)
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
                yield return null;
            }

            transform.rotation = endRotation;
           
            if (elapsed > duration)
            {
                StopCoroutine(_cameraRotateCoroutine);
            }
            yield return null;
        }
    }



    private void SubscribeGameManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        GameManager.onGameStartEvent += OnGameStart;

        // GameManager.onRoundReadyEvent -= OnRoundReady;
        // GameManager.onRoundReadyEvent += OnRoundReady;
        //
        // GameManager.onCorrectedEvent -= OnCorrect;
        // GameManager.onCorrectedEvent += OnCorrect;
        //
        // GameManager.onRoundFinishedEvent -= OnRoundFinished;
        // GameManager.onRoundFinishedEvent += OnRoundFinished;
        //
        // GameManager.onRoundStartedEvent -= OnRoundStarted;
        // GameManager.onRoundStartedEvent += OnRoundStarted;
        //
        GameManager.onGameFinishedEvent -= OnGameFinished;
        GameManager.onGameFinishedEvent += OnGameFinished;
    }

    private void UnsubscribeGamaManagerEvents()
    {
        GameManager.onGameStartEvent -= OnGameStart;
        // GameManager.onRoundReadyEvent -= OnRoundReady;
        // GameManager.onCorrectedEvent -= OnCorrect;
        // GameManager.onRoundFinishedEvent -= OnRoundFinished;
        // GameManager.onRoundStartedEvent -= OnRoundStarted;
        GameManager.onGameFinishedEvent -= OnGameFinished;
    }
}