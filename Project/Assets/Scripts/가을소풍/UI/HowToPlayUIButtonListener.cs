using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class HowToPlayUIButtonListener :MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    [Header("Reference")] [Space(10f)]
    [SerializeField] 
    private GroundGameManager gameManager;

    [Space(20f)] [Header("Dot-ween Parameters")]
    public float buttonChangeDuration;
    [Space(20f)]
    [Header("Audio")] 
    private Button _button;
    [SerializeField]
    private AudioSource _audioSource;

 

    public float maximizedSize;
    private RectTransform _buttonRectTransform;
   
    private Vector2 _originalSizeDelta;
    private void Awake()
    {
        _button = GetComponent<Button>();
        _buttonRectTransform = GetComponent<RectTransform>();
      
        var position = _buttonRectTransform.position;

        _originalSizeDelta =
            _buttonRectTransform.sizeDelta;
    }

    // Update is called once per frame
    private void Start()
    {
        _button.onClick.AddListener(ButtonClicked);
    }

    private bool _isUIPlayed;
    void ButtonClicked()
    {
        if (_audioSource != null)
        {
            _audioSource.Play();
        }
        if (!_isUIPlayed)
        {
            _isUIPlayed = true;
        }
        
        gameManager.isStartButtonClicked.Value = true;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OPointerEnter");
        var targetSize = _originalSizeDelta * maximizedSize;
        _buttonRectTransform.DOSizeDelta(targetSize, buttonChangeDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OPointerExit");
        _buttonRectTransform.DOSizeDelta(_originalSizeDelta,buttonChangeDuration);
    }
    
}