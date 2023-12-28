using System;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    [Header("Window")] [Space(10f)] public Transform leftWindow; //y decreasing process to open window forward.
    public Transform rightWindow; //y increase process to open window forward.

    public float waitingTime;
    public float windowOpeningSpeed;
    public bool isWindowStartOpening;

    [Space(30f)] [Header("Camera Positions")] [Space(10f)]
    private float _elapsed;

    private float _waitingTimeRemaining;
    private float _deactivateOffset = 5f;


    public static event Action WindowOpenEvent;

    private bool _isInvoked;
    private void Update()
    {
        if (UIManager.isHowToPlayUIFinished)
        {
            _elapsed += Time.deltaTime;
            _waitingTimeRemaining = waitingTime - _elapsed;

            if (_elapsed > _waitingTimeRemaining)
            {
                isWindowStartOpening = true;
                if (!_isInvoked)
                {
                    WindowOpenEvent?.Invoke();
                    _isInvoked = true;
                }
               
                
                leftWindow.eulerAngles += Vector3.down * (windowOpeningSpeed * Time.deltaTime);
                rightWindow.eulerAngles += Vector3.up * (windowOpeningSpeed * Time.deltaTime);
            }
            if (_elapsed > _waitingTimeRemaining + _deactivateOffset) Deactivate();
        }
     

    }

    private void Deactivate() => gameObject.SetActive(false);
}