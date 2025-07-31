using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ButtonClickEventController : Ex_MonoBehaviour
{
    public enum ButtonClickMode
    {
        Sequential, // 클릭 순서 대로, 오름차순
        AnyOrder, // 순서 없이 자유 클릭
        OneClickMode // 한 번 클릭 모드
    }

    private enum Objs
    {
        ButtonA,
        ButtonB,
        ButtonC,
        ButtonD,
        ButtonE,
        ButtonF,
        ButtonG
    }

    private ButtonClickMode _currentClickMode = ButtonClickMode.Sequential;
    private int _currentOrder;
    private const int maxClickCount = 7; // 총 버튼 개수
    private readonly Dictionary<int, Image> _buttonImageMap = new();
    private readonly Dictionary<int, SpriteRenderer> _buttonBgImageMap = new();
    private bool _btnDisappearModeInSequentialMode; // 순서대로 버튼 클릭하는 경우, 클릭 후 버튼이 사라지는지 여부
    private bool _isClickable = true;

    private float _clickableDelay=0.05f;
    public float ClickableDelay
    {
        get => _clickableDelay;
        set
        {
            var processedVal = Mathf.Clamp(value, 0.1f, 5f);
            _clickableDelay = processedVal;
        }
    }

    public event Action<int> OnButtonClicked;
    public event Action OnAllBtnClicked;

    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));

        _defaultSizeMap = new Dictionary<int, Vector3>
        {
            {(int)Objs.ButtonA, GetObject((int)Objs.ButtonA).transform.localScale},
            {(int)Objs.ButtonB, GetObject((int)Objs.ButtonB).transform.localScale},
            {(int)Objs.ButtonC, GetObject((int)Objs.ButtonC).transform.localScale},
            {(int)Objs.ButtonD, GetObject((int)Objs.ButtonD).transform.localScale},
            {(int)Objs.ButtonE, GetObject((int)Objs.ButtonE).transform.localScale},
            {(int)Objs.ButtonF, GetObject((int)Objs.ButtonF).transform.localScale},
            {(int)Objs.ButtonG, GetObject((int)Objs.ButtonG).transform.localScale}
        };
        for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++)
        {
            GetObject(i).transform.localScale = Vector3.zero;
            _buttonBgImageMap.Add(i, GetObject(i).GetComponent<SpriteRenderer>());
            _buttonImageMap.Add(i, GetObject(i).GetComponentInChildren<Image>());
            EmptyBtnImage();
        }
    }

    private Sequence _clickableSeq;

    private void SetClickable()
    {
        _isClickable = false;
        
        _clickableSeq?.Kill();
        _clickableSeq = DOTween.Sequence();
        _clickableSeq.AppendInterval(ClickableDelay)
            .OnComplete(() => _isClickable = true);
    }
    protected override void OnRaySyncedByGameManager()
    {
        if (_isClickable)
        {
           
            SetClickable();
            
            foreach (var hit in GameManager.GameManager_Hits)
            {
                int id = hit.transform.GetInstanceID();
                if (!_tfIdToEnumMap.ContainsKey(id)) continue;


                int clickedEnum = _tfIdToEnumMap[id];


                
                char randomChar = (char)Random.Range('A', 'D' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Common/Click/Click" + randomChar);


                if (_currentClickMode == ButtonClickMode.Sequential && clickedEnum == _currentOrder)
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

                else if (_currentClickMode == ButtonClickMode.OneClickMode)
                {
                    OnButtonClicked?.Invoke(clickedEnum);
                    OnBtnClickedOnOneTimeClickMode(clickedEnum);
                }
            }
        }
      

       
    }

    public void StartBtnClickSequential(bool isBtnDisappearMode = false)
    {
        CommoninitOnStart();

        _btnDisappearModeInSequentialMode = isBtnDisappearMode;
        _currentClickMode = ButtonClickMode.Sequential;
        AnimateButton((int)Objs.ButtonA);
    }

    public void StartBtnClickAnyOrder()
    {
        CommoninitOnStart();
        _currentClickMode = ButtonClickMode.AnyOrder;
        AnimateButtonsAll();
    }

    private void CommoninitOnStart()
    {
        transform.localScale = Vector3.one;
        _isClickable = true;

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

    #region 무작위 버튼 클릭하는(순서없음) 경우의 모드

    private void OnButtonClickedOnSequentialMode(int clickedIndex)
    {
        var buttonTransform = GetObject(clickedIndex).transform;

        _sequencePerEnumMap[clickedIndex]?.Kill();
        _sequencePerEnumMap[clickedIndex] = DOTween.Sequence();

        _sequencePerEnumMap[clickedIndex]
            .Append(buttonTransform
                .DOScale(_btnDisappearModeInSequentialMode ? Vector3.zero : _defaultSizeMap[clickedIndex], 0.7f)
                .SetEase(Ease.InOutBounce));

        if (_currentOrder < maxClickCount)
            AnimateButton(_currentOrder);
        else
        {
            _currentOrder = 0; // Reset for next round
            OnAllBtnClicked?.Invoke();
            _isClickable = false;
        }
    }

    #endregion

    #region 무작위 버튼 클릭하는(순서없음) 경우의 모드

    private void OnButtonClickedOnAnyOrderMode(int clickedIndex, bool isToDisappear = false)
    {
        var buttonTransform = GetObject(clickedIndex).transform;

        _sequencePerEnumMap[clickedIndex]?.Kill();
        _sequencePerEnumMap[clickedIndex] = DOTween.Sequence();

        _sequencePerEnumMap[clickedIndex].Append(buttonTransform
            .DOShakeScale(0.25f, 0.05f, 5, 70)
            .SetEase(Ease.InOutBack));

        _sequencePerEnumMap[clickedIndex].OnKill(() =>
        {
            buttonTransform.localScale = _defaultSizeMap[clickedIndex];
        });

        if (isToDisappear)
            _sequencePerEnumMap[clickedIndex]
                .Append(buttonTransform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.InOutBounce));
        if (_currentOrder >= maxClickCount)
        {
            _currentOrder = 0; // Reset for next round
            OnAllBtnClicked?.Invoke();
            _isClickable = false;
        }
    }

    #endregion

    public void DeactivateAllButtons()
    {
        _isClickable = false;

        Logger.Log("DeactivateAllButtons called");

        foreach (int key in _sequencePerEnumMap.Keys.ToArray())
        {
//           Logger.Log($"Killing sequence {key}");
            _sequencePerEnumMap[key]?.Kill();
            _sequencePerEnumMap[key] = DOTween.Sequence();
        }

        DOVirtual.DelayedCall(0.5f, () =>
        {
            //Logger.Log("Running delayed hide");
            for (int i = (int)Objs.ButtonA; i <= (int)Objs.ButtonG; i++)
            {
                var btnTransform = GetObject(i).transform;
                _sequencePerEnumMap[i]?.Kill();
                _sequencePerEnumMap[i] = DOTween.Sequence();
                _sequencePerEnumMap[i].Append(btnTransform.DOScale(Vector3.zero, 0.55f));
            }
        });
    }

    private void AnimateButtonLoop(Objs button)
    {
        Logger.ContentTestLog($"button default size :  {_defaultSizeMap[(int)button]}");
        var buttonTransform = GetObject((int)button).transform;

        buttonTransform.DOScale(_defaultSizeMap[(int)button], 0.55f).SetEase(Ease.OutBounce);

        DOVirtual.DelayedCall(0.75f, () =>
        {
            _sequencePerEnumMap[(int)button]?.Kill();
            _sequencePerEnumMap[(int)button] = DOTween.Sequence();
            _sequencePerEnumMap[(int)button]
                .Append(buttonTransform.DOScale(_defaultSizeMap[(int)button] * 1.1f, 0.25f))
                .Append(buttonTransform.DOScale(_defaultSizeMap[(int)button] * 0.9f, 0.35f))
                .SetLoops(100, LoopType.Yoyo)
                .OnKill(() =>
                {
                    if (_currentClickMode == ButtonClickMode.AnyOrder)
                        buttonTransform.DOScale(_defaultSizeMap[(int)button], 0.15f);
                });

            _sequencePerEnumMap[(int)button].Play();
        });


    }

    public void ChangeBtnImage(string imagePath)
    {
        var sprite = Resources.Load<Sprite>(imagePath);
        if (sprite == null)
        {
            Debug.LogError($"Sprite not found at path: Resources/{imagePath}");
            return;
        }

        foreach (int key in _buttonImageMap.Keys.ToArray()) _buttonImageMap[key].sprite = sprite;
    }

    public void ChangeBtnImage(string imagePath, int btnIndex = -1, bool turnOffBg = false)
    {
        var sprite = Resources.Load<Sprite>(imagePath);
        if (sprite == null)
        {
            Debug.LogError($"Sprite not found at path: Resources/{imagePath}");
            return;
        }

        _buttonImageMap[btnIndex].sprite = sprite;

        if (turnOffBg) SetBgImageStatus(false);
    }

    public void EmptyBtnImage()
    {
        foreach (int key in _buttonImageMap.Keys.ToArray()) _buttonImageMap[key].sprite = null;
    }

    public void ChangeBtnImage(Sprite sprite)
    {
        foreach (int key in _buttonImageMap.Keys.ToArray()) _buttonImageMap[key].sprite = sprite;
    }

    public void SetBgImageStatus(bool isEnabled = true)
    {
        foreach (int key in _buttonBgImageMap.Keys.ToArray()) _buttonBgImageMap[key].enabled = isEnabled;
    }


    #region 단발성 버튼 클릭하는 경우의 모드

    public void StartBtnOnTimeClickMode()
    {
        CommoninitOnStart();

        _currentClickMode = ButtonClickMode.OneClickMode;
        AnimateButtonsAll();
    }

    public void ShowButton(int index)
    {
        _sequencePerEnumMap[index]?.Kill();
        _sequencePerEnumMap[index] = DOTween.Sequence();
    }

    public void PopOffButton(int index)
    {
        _sequencePerEnumMap[index]?.Kill();
        _sequencePerEnumMap[index] = DOTween.Sequence();


        var buttonTransform = GetObject(index).transform;

        // _sequencePerEnumMap[index].Append(buttonTransform
        //     .DOShakeScale(0.25f, 0.05f, 5, 70)
        //     .SetEase(Ease.InOutBack));

        _sequencePerEnumMap[index]
            .Append(buttonTransform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.InOutBounce));
    }

    private void OnBtnClickedOnOneTimeClickMode(int clickedIndex)
    {
        PopOffButton(clickedIndex);
    }

    #endregion
}