using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchZone : MonoBehaviour
{
    private float lifetime = 1.0f; // 1�� �� �ڵ� ����
    public float timer = 0f;
    public bool isTouched = false; // ��ġ ���� ����

    /// <summary>
    /// ��ġ�� �����Ǹ� Ÿ�̸� ����
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
