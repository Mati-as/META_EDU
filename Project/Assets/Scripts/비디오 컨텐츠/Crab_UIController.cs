using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class Crab_UIController : MonoBehaviour
{
    [SerializeField] private RectTransform _upRect;
    [FormerlySerializedAs("_GreatRect")] [SerializeField] private RectTransform _greatRect;

    public RectTransform defaultRect;
    public RectTransform arrivalRect;
    public RectTransform awayRect;
    
    private Image _up;
    private Image _great;
    private readonly string _crabPrefabPath = "게임별분류/비디오컨텐츠/Crab/";


    private void Start()
    {
        Init();
    }

    private void OnDestroy()
    {
        CrabVideoGameManager.onReplay -= OnReplay;

        VideoContents_UIManager.OnFadeOutFinish -= OnFadeOutComplete;
    }

    private float _defaultSize;
    private void Init()
    {
        _up = Resources.Load<Image>(_crabPrefabPath + "up");
        _great = Resources.Load<Image>(_crabPrefabPath + "great");

        var image = _upRect.transform.GetComponent<Image>();
        image = _up;
        image = _greatRect.transform.GetComponent<Image>();
        image = _great;
        _defaultSize = _upRect.localScale.x;
        
        VideoContents_UIManager.OnFadeOutFinish -= OnFadeOutComplete;
        VideoContents_UIManager.OnFadeOutFinish += OnFadeOutComplete;
        
        CrabVideoGameManager.onReplay -= OnReplay;
        CrabVideoGameManager.onReplay += OnReplay;

        _upRect.anchoredPosition = defaultRect.anchoredPosition;
        _greatRect.anchoredPosition =defaultRect.anchoredPosition;
    }


    private void OnReplay()
    {
        _greatRect.DOAnchorPos(arrivalRect.anchoredPosition, 4.5f)
            .SetEase(Ease.OutQuint)
            .OnComplete(() =>
            {
                _greatRect.DOAnchorPos(awayRect.anchoredPosition, 4.5f)
                    .SetEase(Ease.OutQuint)
                    .SetDelay(2f);
            });
    }
    private void OnFadeOutComplete()
    {
        _upRect.localScale = Vector3.zero;
        _upRect.anchoredPosition = arrivalRect.anchoredPosition;
        
        _upRect.DOScale(_defaultSize, 2f)
            .SetEase(Ease.OutQuint)
            .OnComplete(() =>
        {
            _upRect.DOAnchorPos(awayRect.anchoredPosition, 4.5f)
                .SetEase(Ease.OutQuint)
                .SetDelay(1.8f);;
        });
        
        
    }
}