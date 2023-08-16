using UnityEngine;

public class WindowController : MonoBehaviour
{
    [Header("Window")]
    [Space(10f)]
    public Transform _leftWindow;  //y decreasing process to open window forward.
    public Transform _rightWindow; //y increase process to open window forward.

    [Space(30f)]

    [Header("Camera Positions")]
    [Space(10f)]
    private float elapsed;
    public float _waitingTime;
    private float _waitingTimeRemaining;
    public float _windowOpeningSpeed;
    public bool isWindowStartOpening;


    void Start()
    {

    }


  
    

 
    void Update()
    {
        elapsed += Time.deltaTime;
        _waitingTimeRemaining = _waitingTime - elapsed;

        if (elapsed > _waitingTimeRemaining)
        {
            isWindowStartOpening = true; 

            _leftWindow.eulerAngles += Vector3.down * _windowOpeningSpeed * Time.deltaTime;
            _rightWindow.eulerAngles += Vector3.up * _windowOpeningSpeed * Time.deltaTime;
        }

    }
}
