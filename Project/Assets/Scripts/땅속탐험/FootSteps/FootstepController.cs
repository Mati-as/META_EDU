
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
    

    public float maximizedSize;
    private Transform _buttonRectTransform;
    private Vector2 _originalSizeDelta;

    public static event Action OnButtionClicked;
    private void Awake()
    {
        
  
    }


    private void Start()
    {
       
    }

    private bool _isUIPlayed;
    private void ButtonClicked()
    {

        FootstepManager.currentFootstepGroupOrder = footstepGroupOrder;
        OnButtionClicked?.Invoke();
        
        // if (_audioSource != null)
        // {
        //     _audioSource.Play();
        // }
        // if (!_isUIPlayed)
        // {
        //     _isUIPlayed = true;
        // }
        
        Debug.Log("startButtonClicked");
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
