using Unity.VisualScripting;
using UnityEngine;

public class ClickableMover : MonoBehaviour
{
    [SerializeField] private int maxClicks = 3;

    private int clickCount;

    public void OnClicked()
    {
        clickCount++;
        if (clickCount >= maxClicks)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 randomOffset = new Vector3(
            Random.Range(-2f, 2f),
            Random.Range(-2f, 2f),
            0f
        );
        Vector3 candidatePos = transform.position + randomOffset;

        Camera cam = Camera.main;
        Vector3 viewportPos = cam.WorldToViewportPoint(candidatePos);

        viewportPos.x = Mathf.Clamp01(viewportPos.x);
        viewportPos.y = Mathf.Clamp01(viewportPos.y);

        Vector3 clampedWorldPos = cam.ViewportToWorldPoint(viewportPos);
        clampedWorldPos.z = transform.position.z;
        
        transform.position = clampedWorldPos;
    }
}