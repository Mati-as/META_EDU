using Unity.VisualScripting;
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

    public void OnClicked()
    {
        if (gameManager.isStageStart)
        {

            char randomLetter = (char)('A' + Random.Range(0, 6));
            Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/Click_{randomLetter}");

            clickCount++;
            if (clickCount >= maxClicks)
            {
                gameObject.SetActive(false);
                return;
            }

            // 1) 먼저 랜덤 오프셋 계산
            Vector3 randomOffset = new Vector3(
                Random.Range(-2f, 2f),
                Random.Range(-2f, 2f),
                0f
            );
            Vector3 candidatePos = transform.position + randomOffset;

            // 2) World → Viewport 변환
            Camera cam = Camera.main;
            Vector3 viewportPos = cam.WorldToViewportPoint(candidatePos);

            // 3) 화면의 80%만 사용하기 위한 패딩 계산 (뷰포트 좌표 기준)
            //    - 예: viewportUsage = 0.8f → padding = (1 - 0.8f) / 2 = 0.1f
            float padding = (1f - viewportUsage) * 0.5f;

            // 4) 뷰포트 좌표를 [padding .. 1-padding] 범위로 Clamp
            viewportPos.x = Mathf.Clamp(viewportPos.x, padding, 1f - padding);
            viewportPos.y = Mathf.Clamp(viewportPos.y, padding, 1f - padding);

            // 5) 다시 Viewport → World로 변환하여 최종 위치 결정
            Vector3 clampedWorldPos = cam.ViewportToWorldPoint(viewportPos);
            clampedWorldPos.z = transform.position.z; // Z값(깊이)은 기존 값 유지

            transform.position = clampedWorldPos;
        }
    }
}
