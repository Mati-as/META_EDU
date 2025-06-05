using Unity.VisualScripting;
using UnityEngine;

public class ClickableMover : MonoBehaviour
{
    [SerializeField] private int maxClicks = 3;

    private int clickCount;

    public void OnClicked()
    {
        Debug.Log("ON CLICKED");
        clickCount++;
        Debug.Log(clickCount);
        if (clickCount >= maxClicks)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("클릭됨");
            Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);
            Vector3 randomPos = transform.position + randomOffset;

            transform.position = randomPos;
        }
    }
}