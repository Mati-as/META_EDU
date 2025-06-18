using UnityEngine;
using DG.Tweening;

public class MoveFunction : MonoBehaviour
{
    [SerializeField] float speed;            // 초당 이동 속도
    [SerializeField] float showDistance;     // 보여줄 이동 거리

    public void BeginMove()
    {
        float duration = showDistance / speed;
        Vector3 targetPos = transform.position + transform.forward * showDistance;

        transform
            .DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => gameObject.SetActive(false));
    }
}