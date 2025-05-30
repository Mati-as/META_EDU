using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonChange : MonoBehaviour, IPointerClickHandler ,IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite yellowSprite;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        blueSprite = Resources.Load<Sprite>("Construction/Sprite/BaseFrame_Blue");
        yellowSprite = Resources.Load<Sprite>("Construction/Sprite/BaseFrame_Yellow");

        if (buttonImage != null)
        {
            buttonImage.sprite = blueSprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.DOKill();
        transform.localScale = Vector3.one;

        Sequence clickSequence = DOTween.Sequence();
        clickSequence.Append(transform.DOScale(0.9f, 0.05f));
        clickSequence.Append(transform.DOScale(1.1f, 0.05f));
        clickSequence.Append(transform.DOScale(1f, 0.05f));

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = yellowSprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = blueSprite;
        }
    }
}
