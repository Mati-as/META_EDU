using DG.Tweening;
using UnityEngine;

public class ClickableMover : MonoBehaviour
{
    [SerializeField] private int maxClicks = 3;

    // 뷰포트의 몇 %만 사용할 것인지 (여기서는 0.8 = 80%)
    [SerializeField, Range(0.01f, 1f)]
    private float viewportUsage = 0.8f;

    private int clickCount;

    [SerializeField]
    private VariousShape_GameManager gameManager;

    Vector3 TargetScale;

    public bool twiceClickIssue = false;

    public enum ShapeType
    {
        Square, Flower, Star, Circle, Triangle
    }

    public ShapeType shapeType;

    Vector3 TargetSquareScale = Vector3.one * 0.2f;
    Vector3 TargetFlowerScale = Vector3.one * 0.55f;
    Vector3 TargetStarScale = Vector3.one * 0.25f;
    Vector3 TargetCircleScale = Vector3.one * 0.25f;
    Vector3 TargetTriangleScale = Vector3.one * 0.25f;

    public void OnClicked(ShapeType shapeType)
    {
        if (!gameManager.isStageStart || twiceClickIssue) return;

        transform.DOKill();
        twiceClickIssue = true;
        char randomLetter = (char)('A' + Random.Range(0, 6));
        Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/Click_{randomLetter}");

        clickCount++;
        if (clickCount >= maxClicks)
        {
            gameObject.transform.DOScale(0.001f,0.5f).SetEase(Ease.Linear);
            DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
            return;
        }

        switch (shapeType)
        {
            case ShapeType.Square:
                TargetScale = TargetSquareScale;
                break;
            case ShapeType.Flower:
                TargetScale = TargetFlowerScale;
                break;
            case ShapeType.Star:
                TargetScale = TargetStarScale;
                break;
            case ShapeType.Circle:
                TargetScale = TargetCircleScale;
                break;
            case ShapeType.Triangle:
                TargetScale = TargetTriangleScale;
                break;

        }

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, vibrato: 1, elasticity: 0.5f))
            .Append(Move())
            .OnComplete(() =>
            {
                transform
                    .DOScale(TargetScale * 1.1f, 0.5f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);

            });
        DOVirtual.DelayedCall(1f, () => twiceClickIssue = false);
    }


    private Tween Move()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f),
            0f
        );
        Vector3 candidatePos = transform.position + randomOffset;

        Camera cam = Camera.main;
        Vector3 viewportPos = cam.WorldToViewportPoint(candidatePos);

        // 화면의 80%만 사용하기 위한 패딩 계산 (뷰포트 좌표 기준)
        // 예: viewportUsage = 0.8f → padding = (1 - 0.8f) / 2 = 0.1f
        float padding = (1f - viewportUsage) * 0.5f;

        viewportPos.x = Mathf.Clamp(viewportPos.x, padding, 1f - padding);
        viewportPos.y = Mathf.Clamp(viewportPos.y, padding, 1f - padding);

        Vector3 clampedWorldPos = cam.ViewportToWorldPoint(viewportPos);
        clampedWorldPos.z = transform.position.z;

        //transform.position = clampedWorldPos;
        return transform
        .DOMove(clampedWorldPos, 0.5f)
        .SetEase(Ease.OutSine);
    }

}
