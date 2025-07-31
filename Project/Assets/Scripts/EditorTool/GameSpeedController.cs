using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    
#if UNITY_EDITOR
    [Range(0.1f, 5f)]
    public float gameSpeed = 1f;

    private float _previousSpeed;

    private void Update()
    {
        if (Mathf.Abs(Time.timeScale - gameSpeed) > 0.01f)
        {
            Time.timeScale = gameSpeed;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // FixedUpdate 보정도 함께
            _previousSpeed = gameSpeed;
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
#endif

}