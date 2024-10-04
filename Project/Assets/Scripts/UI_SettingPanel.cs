using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_SettingPanel : MonoBehaviour
{
  private Button _button;
  private Vector3 _defaultPos;
  private float _moveAmount= 220f;
  private RectTransform _panel;
  private bool _isPanelOn;
      

  private void Awake()
  {
      _button = GetComponent<Button>();
      _button.onClick.AddListener(OnClick);

      _panel = GameObject.FindWithTag("InGame_SideMenu").GetComponent<RectTransform>();
      
      _defaultPos = _panel.anchoredPosition;
  }

  public void OnClick()
  {
      if (!_isPanelOn)
      {
          _panel.DOAnchorPos(_defaultPos + Vector3.left * _moveAmount, 0.66f).SetEase(Ease.InOutSine)
              .OnComplete(() =>
              {
                  _isPanelOn = true;
              });;
      }
    
      else
      {
          _panel.DOAnchorPos(_defaultPos, 0.33f).SetEase(Ease.InOutSine)
              .OnComplete(() =>
              {
                  _isPanelOn = false;
              });
      }
  }
}
