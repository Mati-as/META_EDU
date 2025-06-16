using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonChange : MonoBehaviour
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

    public void OnClick()
    {
        transform.DOKill();
        transform.localScale = Vector3.one * 1.3f;

        Sequence clickSequence = DOTween.Sequence();
        clickSequence.AppendCallback(() => buttonImage.sprite = yellowSprite);
        clickSequence.Append(transform.DOScale(1.2f, 0.05f));
        clickSequence.Append(transform.DOScale(1.4f, 0.05f));
        clickSequence.Append(transform.DOScale(1.3f, 0.05f));
        clickSequence.AppendInterval(0.25f);
        clickSequence.AppendCallback(() => buttonImage.sprite = blueSprite);

    }

    
}
