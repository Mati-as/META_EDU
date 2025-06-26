using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ButtonClickEventController : Ex_MonoBehaviour
{

    public enum ButtonClickMode
    {
        Sequential,     // 클릭 순서 대로, 오름차순
        AnyOrder        // 순서 없이 자유 클릭
    }

    private enum Objs
    {
        ButtonA,
        ButtonB,
        ButtonC,
        ButtonD,
        ButtonE,
        ButtonF,
        ButtonG,
    }

    private ButtonClickMode _currentClickMode = ButtonClickMode.Sequential;
    private int _currentOrder = 0;
    private const int maxClickCount = 7; // 총 버튼 개수
    
    private bool isClickable = true;

    public event Action<int> OnButtonClicked;
    public event Action OnAllBtnClicked;
    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));
        
        for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++)
        {
          
            var indexCache = i;
            GetObject(indexCache).BindEvent(() =>
            {
                if (!isClickable) return;
                
                if(_currentClickMode == ButtonClickMode.Sequential && indexCache == _currentOrder)
                {
                    OnButtonClicked?.Invoke(indexCache);
                    _currentOrder++;
                    OnButtonClickedOnSequentialMode(indexCache);
                }
                else if (_currentClickMode == ButtonClickMode.AnyOrder)
                {
                    OnButtonClicked?.Invoke(indexCache);
                    OnButtonClickedOnAnyOrderMode(indexCache);
                }
             
            });

            GetObject(i).transform.localScale = Vector3.zero;

        }
    }
    
    public void StartBtnClickSequential()
    {
        transform.localScale = Vector3.one;
        isClickable = true;
        
        _currentClickMode = ButtonClickMode.Sequential;
        AnimateButton((int)Objs.ButtonA);
     
    }
    
    public void StartBtnClickAnyOrder()
    {  transform.localScale = Vector3.one;
        isClickable = true;
        
        _currentClickMode = ButtonClickMode.AnyOrder;
        AnimateButtonsAll();
    }
    private void AnimateButton(int buttonIndex)
    {
        GetObject(buttonIndex).SetActive(true);
        AnimateButtonLoop((Objs)buttonIndex);
    }
    private void AnimateButtonsAll()
    {
        for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++)
        {
            Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(Objs)i}");
            GetObject(i).SetActive(true);
            AnimateButtonLoop((Objs)i);
        }
    }

    private void OnButtonClickedOnSequentialMode(int clickedIndex)
    {
        Transform buttonTransform = GetObject(clickedIndex).transform;
        
        _sequenceMap[(int)clickedIndex]?.Kill();
        _sequenceMap[(int)clickedIndex] = DOTween.Sequence();
        
        _sequenceMap[(int)clickedIndex]
            .Append(buttonTransform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.InOutBounce));

        if (_currentOrder < maxClickCount)
        {
            AnimateButton(_currentOrder);
        }
        else
        {
            _currentOrder = 0; // Reset for next round
            OnAllBtnClicked?.Invoke();
            isClickable = false;
        }
    }
    
    private void OnButtonClickedOnAnyOrderMode(int clickedIndex,bool isToDisappear = false)
    {
        
        Transform buttonTransform = GetObject(clickedIndex).transform;

        _sequenceMap[(int)clickedIndex]?.Kill();
        _sequenceMap[(int)clickedIndex] = DOTween.Sequence();

        if (isToDisappear)
        {
            _sequenceMap[(int)clickedIndex]
                .Append(buttonTransform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.InOutBounce));
        }
        if (_currentOrder >= maxClickCount)
        {
            _currentOrder = 0; // Reset for next round
            OnAllBtnClicked?.Invoke();
            isClickable = false;
        }
 

    }
    
    private void DeactivateAllButtons()
    {
        for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++) _sequenceMap[i]?.Kill();

        TweenCallback _scaleCallback = () =>
        {
            for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++)
            {
                var SeatTransform = GetObject(i).transform;
                _sequenceMap[i] = DOTween.Sequence();
                _sequenceMap[i].Append(SeatTransform.DOScale(Vector3.zero, 0.75f));
            }
        };

        DOVirtual.DelayedCall(1f, _scaleCallback);
    }
    
    private void AnimateButtonLoop(Objs button)
    {
        Transform buttonTransform = GetObject((int)button).transform;

        _sequenceMap[(int)button]?.Kill();
        _sequenceMap[(int)button] = DOTween.Sequence();
        _sequenceMap[(int)button]
            .Append(buttonTransform.DOScale(_defaultSizeMap[(int)button] * 1.1f, 0.25f))
            .Append(buttonTransform.DOScale(_defaultSizeMap[(int)button] * 0.9f, 0.35f))
            .SetLoops(100, LoopType.Yoyo)
            .OnKill(() =>
            {
              if(_currentClickMode==ButtonClickMode.AnyOrder) buttonTransform.DOScale(_defaultSizeMap[(int)button], 1);
            });

        _sequenceMap[(int)button].Play();
    }
}
