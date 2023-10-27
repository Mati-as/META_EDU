using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class IdleLakeStartButtonListener : MonoBehaviour
{

  private Button _button;
  [SerializeField] private CanvasGroup canvasGroup;
  [SerializeField] private RectTransform UIRect;
  public RectTransform awayRectPosition;
  [SerializeField] private GameObject UIGameObj;
  private void Awake()
  {
      _button = GetComponent<Button>();
  }

  private void Start()
  {
      canvasGroup.DOFade(0, 0.01f);
      canvasGroup.DOFade(1, 2f);
      _button.onClick.AddListener(OnClick);
      
  }

  private bool _isButtonClicked;
  private void OnClick()
  {
      if (!_isButtonClicked)
      {
          _isButtonClicked = true;
          UIRect
              .DOAnchorPos(awayRectPosition.anchoredPosition, 1.5f)
              .SetEase(Ease.InBack)
              .OnComplete(() =>
              {
                  UIGameObj.SetActive(false);
              });
      }
  }
}
