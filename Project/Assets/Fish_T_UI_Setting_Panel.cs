using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Fish_T_UI_Setting_Panel : MonoBehaviour
{
    private Button _button;
    private Vector3 _defaultPos;
    private float _moveAmount= 370;
    private RectTransform _panel;
    private bool _isPanelOn;


    private void OnGameStart()
    {
        _panel.DOAnchorPos(_defaultPos, 0.33f).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                _isPanelOn = false;
            });
    }

    private void OnModeSelection()
    {
        _panel.DOAnchorPos(_defaultPos + Vector3.left * _moveAmount, 0.33f).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                _isPanelOn = true;
            });;
    }

    private void Awake()
    {
        U_FishOnWater_UIManager.OnModeSelectionUIAppear -= OnModeSelection;
        U_FishOnWater_UIManager.OnModeSelectionUIAppear += OnModeSelection;
        
        U_FishOnWater_UIManager.OnReadyUIAppear -= OnGameStart;
        U_FishOnWater_UIManager.OnReadyUIAppear += OnGameStart;
        
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);

        _panel = transform.parent.Find("TopMenuUI").GetComponent<RectTransform>();
        _defaultPos = _panel.anchoredPosition;
    }

    private void OnDestroy()
    {
        U_FishOnWater_UIManager.OnRestartBtnClicked -= OnModeSelection;
        U_FishOnWater_UIManager.OnReadyUIAppear -= OnGameStart;
    }

    public void OnClick()
    {
        if (!_isPanelOn)
        {
            _panel.DOAnchorPos(_defaultPos + Vector3.left * _moveAmount, 0.33f).SetEase(Ease.InOutSine)
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
