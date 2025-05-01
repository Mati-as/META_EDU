using UnityEngine;
using DG.Tweening;
public class DropAndBounceEffect : MonoBehaviour
{
    public RectTransform imageTransform; // 떨어뜨릴 UI 이미지
    public float dropDistance = 800f;    // 시작 위치 (위에서 떨어질 거리)
    public float dropDuration = 0.3f;     // 떨어지는 시간
    public float bounceStrength = 40f;   // 흔들리는 강도
    public float bounceDuration = 0.6f;  // 흔들리는 시간

    void Start()
    {
        PlayDropEffect();
    }

    public void PlayDropEffect()
    {
        // 시작 위치를 위로 설정
        Vector2 originalPosition = imageTransform.anchoredPosition;
        imageTransform.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + dropDistance);

        // 떨어지고 나서 흔들림 추가
        imageTransform.DOAnchorPosY(originalPosition.y, dropDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                imageTransform.DOShakeAnchorPos(
                    duration: bounceDuration,
                    strength: new Vector2(0, bounceStrength),
                    vibrato: 3,
                    randomness: 0,
                    snapping: false,
                    fadeOut: true
                );
            });
    }
}

