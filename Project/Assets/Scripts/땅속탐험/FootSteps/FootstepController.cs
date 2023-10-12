
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;


public class FootstepController : MonoBehaviour
{
    [Header("button info")] [Space(10f)]
    public int footstepGroupOrder;

    [Header("Reference")]
    [Space(10f)]
    [SerializeField]
    private FootstepManager footstepManager;
   

    [Space(20f)] [Header("Tween Parameters")]
    public float buttonChangeDuration;
    [Space(20f)]
   
    private Button _button;
    [SerializeField]
    
    [Header("Audio")] 
    private AudioSource _audioSource;
    
    private SpriteRenderer _spriteRenderer;

    public float maximizedSize;
    private Transform _buttonRectTransform;
    private Vector2 _originalSizeDelta;

    private void Awake()
    {
        FootstepManager.OnFootstepClicked -= OnButtonClicked;
        FootstepManager.OnFootstepClicked += OnButtonClicked;
    }
  

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    private void OnDestroy()
    {
        FootstepManager.OnFootstepClicked -= OnButtonClicked;
    }

    private bool _isUIPlayed;
    private void OnButtonClicked()
    {
        FadeOutSprite();
        
        if (_audioSource != null)
        {
            _audioSource.Play();
        }
        if (!_isUIPlayed)
        {
            _isUIPlayed = true;
        }
        
        Debug.Log("footstep Cliceked");
    }
    private void FadeOutSprite()
    {
       
        _spriteRenderer.DOFade(0, 1f).OnComplete(() => 
        {
          
            this.gameObject.SetActive(false);
            
            // Destroy(this.gameObject);
        });
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var targetSize = _originalSizeDelta * maximizedSize;
        //_buttonRectTransform.DOSizeDelta(targetSize, buttonChangeDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
     
      //  _buttonRectTransform.DOSizeDelta(_originalSizeDelta,buttonChangeDuration);
    }
}
