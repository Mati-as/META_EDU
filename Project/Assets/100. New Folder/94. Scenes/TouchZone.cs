using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchZone : MonoBehaviour
{
    private float lifetime = 1.0f; // 1�� �� �ڵ� ����
    public float timer = 0f;
    public bool isTouched = false; // ��ġ ���� ����
    private Image _image;

    /// <summary>
    /// ��ġ�� �����Ǹ� Ÿ�̸� ����
    /// </summary>
    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void ResetTimer()
    {
        isTouched = true;
        timer = 0f;
      
    }
    
    protected void OnEnable()
    {
        
        _image.enabled = SensorManager.BallActive;
    }


    private void Update()
    {
        if (!isTouched)
        {
            timer += Time.deltaTime;

            if (timer > lifetime)
            {
                Debug.Log($"?? ��ġ ���� ������: {gameObject.name}");
                Destroy(gameObject);
            }
        }
        else
        {
            // ? ��ġ�� �����Ǹ� Ÿ�̸� ����
            timer = 0f;
            isTouched = false; // ? ���� �����ӿ��� �ٽ� �����ǵ��� ����
        }
    }
}
