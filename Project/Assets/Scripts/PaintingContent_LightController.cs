using UnityEngine;

public class PaintingContent_LightController : MonoBehaviour
{
    public float rotationSpeed;

    private void Update()
    {
        // 현재 회전값을 Quaternion으로 가져옵니다.
        var currentRotation = transform.rotation;

        // y축 주위의 회전을 추가합니다.
        var rotationChange = Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);

        // 현재 회전값에 회전 변경값을 곱하여 새로운 회전값을 얻습니다.
        var newRotation = currentRotation * rotationChange;

        // 새로운 회전값을 Transform에 적용합니다.
        transform.rotation = newRotation;
    }
}