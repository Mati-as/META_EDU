
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;


public class FootstepController : MonoBehaviour
{
    [Header("button info")] [Space(10f)]
    public int footstepGroupOrder;

   
    [Space(10f)]
    [SerializeField]
    private FootstepManager footstepManager;
    public float buttonChangeDuration;
    [Space(20f)]
    
    
  
   
    private Button _button;
    [SerializeField]
    
    [Header("Audio")] 
    private AudioSource _audioSource;
    
    private SpriteRenderer _spriteRenderer;
    [Space(20f)] [Header("Tween Parameters")]
    public float maximizedSize;
    private Transform _buttonRectTransform;
    private Vector2 _originalSizeDelta;
    
    [Space(20f)] [Header("Reference : 마지막 버튼에만 할당할 것 (non-nullable)")] 
    [SerializeField]
    public GameObject animalByLastFootstep;
    [SerializeField] public string animalNameToCall;
   

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

        if (animalByLastFootstep != null && animalNameToCall != string.Empty)
        {
            if (FootstepManager.currentlyClickedObjectName == animalByLastFootstep.name)
            {
                animalByLastFootstep.SetActive(true);
                //tween 추가하세요
            }
        }
     
     
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
