using UnityEngine;
using DG.Tweening;

public class MoveFunction : MonoBehaviour
{
    [SerializeField] private float speed;            // 초당 이동 속도
    [SerializeField] private float showDistance;     // 보여줄 이동 거리
    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
    }

    public void BeginMove()
    {
        float duration = showDistance / speed;
        var targetPos = transform.position + transform.forward * showDistance;

          
        
        transform
            .DOMove(targetPos, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                transform.position = _startPosition;
            });
            
    }
}