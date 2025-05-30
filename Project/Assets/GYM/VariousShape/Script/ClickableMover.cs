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
        }
        else
        {
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);
            Vector3 randomPos = transform.position + randomOffset;

            transform.position = randomPos;
        }
    }
}