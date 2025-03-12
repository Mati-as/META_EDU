using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchZone : MonoBehaviour
{
    private float lifetime = 1.0f; // 1초 후 자동 삭제
    public float timer = 0f;
    public bool isTouched = false; // 터치 감지 여부

    /// <summary>
    /// 터치가 감지되면 타이머 리셋
    /// </summary>
    public void ResetTimer()
    {
        isTouched = true;
        timer = 0f;
    }

    private void Update()
    {
        if (!isTouched)
        {
            timer += Time.deltaTime;

            if (timer > lifetime)
            {
                Debug.Log($"?? 터치 영역 삭제됨: {gameObject.name}");
                Destroy(gameObject);
            }
        }
        else
        {
            // ? 터치가 감지되면 타이머 리셋
            timer = 0f;
            isTouched = false; // ? 다음 프레임에서 다시 감지되도록 설정
        }
    }
}
