using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
    private Dictionary<int,Image> _buttonImageMap = new();
    
    private bool isClickable = true;

    public event Action<int> OnButtonClicked;
    public event Action OnAllBtnClicked;
    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));
        
        for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++)
        {
          
            GetObject(i).transform.localScale = Vector3.zero;
            _buttonImageMap.Add(i, GetObject(i).GetComponentInChildren<Image>());
            EmptyBtnImage();
        }
    }
    
    protected override void OnRaySyncedByGameManager()
    {
        foreach (var hit in GameManager.GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();
            if (!_tfIdToEnumMap.ContainsKey(id)) return; 
            
            
            int clickedEnum = _tfIdToEnumMap[id];
            
            
            if (!isClickable) return;
            Char randomChar = (char)UnityEngine.Random.Range('A','D'+1);

            Managers.Sound.Play(SoundManager.Sound.Effect,"Audio/Common/Click/Click"+randomChar.ToString());
            if(_currentClickMode == ButtonClickMode.Sequential && clickedEnum == _currentOrder)
            {
                OnButtonClicked?.Invoke(clickedEnum);
                _currentOrder++;
                OnButtonClickedOnSequentialMode(clickedEnum);
            }
            else if (_currentClickMode == ButtonClickMode.AnyOrder)
            {
     
                OnButtonClicked?.Invoke(clickedEnum);
                OnButtonClickedOnAnyOrderMode(clickedEnum);
            }
         
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
        
        _sequenceMap[(int)clickedIndex].Append(buttonTransform
            .DOShakeScale(0.25f, 0.05f, 5, 70, true)
            .SetEase(Ease.InOutBack));

        _sequenceMap[(int)clickedIndex].OnKill(() =>
        {
          buttonTransform.localScale =    _defaultSizeMap[(int)clickedIndex] ;
        });
        
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
    
    
    public void DeactivateAllButtons()
    {
        isClickable = false;
       
        Logger.Log("DeactivateAllButtons called");

        foreach (var key in _sequenceMap.Keys.ToArray())
        {
           Logger.Log($"Killing sequence {key}");
            _sequenceMap[key]?.Kill();
            _sequenceMap[key] = DOTween.Sequence();
        }

        DOVirtual.DelayedCall(0.5f, () =>
        {
           Logger.Log("Running delayed hide");
            for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++)
            {
                var btnTransform = GetObject(i).transform;
                _sequenceMap[i]?.Kill();
                _sequenceMap[i] = DOTween.Sequence();
                _sequenceMap[i].Append(btnTransform.DOScale(Vector3.zero, 0.55f));
            }
        });

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
              if(_currentClickMode==ButtonClickMode.AnyOrder) buttonTransform.DOScale(_defaultSizeMap[(int)button], 0.15f);
            });

        _sequenceMap[(int)button].Play();
    }

    public void ChangeBtnImage(string imagePath)
    {
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite == null)
        {
            Debug.LogError($"Sprite not found at path: Resources/{imagePath}");
            return;
        }

        foreach (var key in _buttonImageMap.Keys.ToArray())
        {
            _buttonImageMap[key].sprite = sprite;
        }
    }
    
    public void EmptyBtnImage()
    {
        foreach (var key in _buttonImageMap.Keys.ToArray())
        {
            _buttonImageMap[key].sprite = null;
        }
    }
    public void ChangeBtnImage(Sprite sprite)
    {
        foreach (var key in _buttonImageMap.Keys.ToArray())
        {
            _buttonImageMap[key].sprite = sprite;
        }
    }
}
