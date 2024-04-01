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
  private float _moveAmount= 1.20f;
  private RectTransform _panel;
  private bool _isPanelOn;
      

  private void Awake()
  {
      _button = GetComponent<Button>();
      _button.onClick.AddListener(OnClick);

      _panel = transform.parent.Find("TopMenuUI").GetComponent<RectTransform>();
      
      _defaultPos = _panel.position;
  }

  private void OnClick()
  {
      if (!_isPanelOn)
      {
          _panel.DOMove(_panel.position + Vector3.left * _moveAmount, 0.33f).SetEase(Ease.InOutSine)
              .OnComplete(() =>
              {
                  _isPanelOn = true;
              });;
      }
    
      else
      {
          _panel.DOMove(_defaultPos, 0.33f).SetEase(Ease.InOutSine)
              .OnComplete(() =>
              {
                  _isPanelOn = false;
              });
      }
  }
}
