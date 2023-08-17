using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoonAndSunController : MonoBehaviour
{
    public float waitTime;
    public float movingTimeSec;
    private float elapsedTime;
    


    public Transform _inPlayPosition;
    public Transform _defaultPosition;



    void Awake()
    {
        transform.position = _defaultPosition.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.IsGameStarted)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime > waitTime)
            {
             

                // Lerp의 t값을 계산 (0 ~ 1 사이)
                float t = Mathf.Clamp01(elapsedTime / movingTimeSec);
                t = Lerp2D.EaseInQuad(0, 1, t);

                
                transform.position = Vector3.Lerp(transform.position, _inPlayPosition.position, t);
            }
        }
    }
}
