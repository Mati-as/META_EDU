
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorImageController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Sequence _buttonScaleSeq;

    public Vector3 DefaultScale
    {
        get;
        set;
    }
    public void Awake()
    {
        DefaultScale = transform.localScale;
        Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _buttonScaleSeq?.Kill();
        _buttonScaleSeq = DOTween.Sequence();
        _buttonScaleSeq.Append(transform.DOScale(DefaultScale* 0.9f, 0.2f).SetEase(Ease.OutBack));
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _buttonScaleSeq?.Kill();
        _buttonScaleSeq = DOTween.Sequence();
        
        _buttonScaleSeq.Append(transform.DOScale(DefaultScale* 1.1f, 0.2f).SetEase(Ease.OutBack));
        Cursor.SetCursor(Managers.CursorImage.Get_hand_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _buttonScaleSeq?.Kill();
        _buttonScaleSeq = DOTween.Sequence();
        _buttonScaleSeq.Append(transform.DOScale(DefaultScale, 0.2f).SetEase(Ease.OutBack));
        
        Cursor.SetCursor(Managers.CursorImage.Get_arrow_image(), Vector2.zero, CursorMode.ForceSoftware);
    }

}
