using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SettingPanel : MonoBehaviour
{
    private Vector3 _defaultPos;
    private float _moveAmount = 265;
    private RectTransform _panel;
    private bool _isPanelOn;
    private Button _button;

    private void Awake()
    {
        // 버튼과 패널 초기화
        _button = GetComponent<Button>();
        _panel = GameObject.FindWithTag("InGame_SideMenu").GetComponent<RectTransform>();
        _defaultPos = _panel.anchoredPosition;
    }

    private void Update()
    {
        // 마우스 클릭 시 버튼 위에 있는지 확인
        if (Input.GetMouseButtonDown(0) && IsPointerOverButton())
        {
            HandlePanelToggle();
        }
    }

    private bool IsPointerOverButton()
    {
        // 현재 마우스 포인터가 UI 버튼 위에 있는지 확인
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition // 현재 마우스 위치
        };

        List<RaycastResult> results = new List<RaycastResult>();

        if (EventSystem.current == null) return false;
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == _button.gameObject)
            {
                return true; // 버튼 위에 있다면 true 반환
            }
        }

        return false; // 버튼 위가 아니면 false 반환
    }

    private void HandlePanelToggle()
    {
        if (!_isPanelOn)
        {
            _panel.DOAnchorPos(_defaultPos + Vector3.left * _moveAmount, 0.66f).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    _isPanelOn = true;
                });
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
