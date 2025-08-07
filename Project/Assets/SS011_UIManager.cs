using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SS011_UIManager : Base_InGameUIManager
{
    public enum UI
    {
        Btn_TeamTwo,
        Btn_TeamThree,
        Btn_TeamFour,
        SpinParent,
        Wheel,
        Btn_StartSpin,
        MaxCount
    }

    public enum ColorType
    {
        Pink,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Navy,
        Purple,
        Max
    }

    private readonly List<ColorType> _colorList = new() {
        ColorType.Pink,
        ColorType.Red,
        ColorType.Orange,
        ColorType.Yellow,
        ColorType.Green,
        ColorType.Blue,
        ColorType.Navy,
        ColorType.Purple
    };

    private ColorType selectedColorType = ColorType.Red;
    private readonly Dictionary<int, Vector3> _uiDefaultSizeMap = new();
    private Sequence spinSequence;

    public override void ExplicitInitInGame()
    {
        base.ExplicitInitInGame();
        BindObject(typeof(UI));

        for (int i = 0; i < (int)UI.MaxCount; i++)
        {
            _uiDefaultSizeMap[i] = GetObject(i).transform.localScale;
            GetObject(i).SetActive(false);
        }

        GetObject((int)UI.Btn_StartSpin).BindEvent(() =>
        {
            SpinWheel();
        });
        InitWheel();
    }

    private float sliceAngle; // 45도 (8조각 기준)
    private bool isSpinning;

    private void InitWheel()
    {
        sliceAngle = 360f / (int)ColorType.Max; // 8조각 기준으로 360도를 나눔
    }

    private void ResetSpin()
    {
        isSpinning = false;
    }

    public void TurnOffWheel()
    {
        spinSequence?.Kill();
        spinSequence = DOTween.Sequence();

        spinSequence.Append(GetObject((int)UI.SpinParent).transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutBack));
        spinSequence.OnComplete(() =>
        {
            ResetSpin();
            GetObject((int)UI.SpinParent).SetActive(false);
        });
    }


    public void TurnOnWheel()
    {
        GetObject((int)UI.SpinParent).SetActive(true);
        GetObject((int)UI.Btn_StartSpin).SetActive(true);
        GetObject((int)UI.Wheel).SetActive(true);

        GetObject((int)UI.SpinParent).transform.localScale = Vector3.zero;
        spinSequence.Kill();
        spinSequence = DOTween.Sequence();
        spinSequence.Append(GetObject((int)UI.SpinParent).transform.DOScale(_uiDefaultSizeMap[(int)UI.SpinParent], 0.5f)
            .SetEase(Ease.InBack));
    }

    public void SpinWheel()
    {
        if (isSpinning) return;

        isSpinning = true;

        Managers.Sound.Play(SoundManager.Sound.Effect, "");
        Managers.Sound.Play(SoundManager.Sound.Effect, "");


        int randomTargetIndex = (int)_colorList[Random.Range(0, _colorList.Count)];
        selectedColorType = (ColorType)randomTargetIndex;

        float targetAngle = randomTargetIndex * sliceAngle;
        float extraSpins = 5f * 360f;
        float finalRotation = extraSpins + targetAngle;


        GetObject((int)UI.Wheel).transform.DORotate(new Vector3(0, 0, finalRotation), 5f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                isSpinning = false;
                //   Manager_Obj_14.instance.Effect_array[0].SetActive(true);
                PopInstructionUIFromScaleZero("색깔이 " + selectedColorType + "로 결정되었습니다!");
            });

        GetObject((int)UI.Wheel).transform.DOScale(1.05f, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    public void ShowTeamSelectionBtn()
    {
        GetObject((int)UI.Btn_TeamTwo).BindEvent(() =>
        {
        });
        GetObject((int)UI.Btn_TeamThree).BindEvent(() =>
        {
        });
        GetObject((int)UI.Btn_TeamTwo).SetActive(true);
        GetObject((int)UI.Btn_TeamThree).SetActive(true);

        GetObject((int)UI.Btn_TeamTwo).transform.localScale = Vector3.zero;
        GetObject((int)UI.Btn_TeamThree).transform.localScale = Vector3.zero;
        //    GetObject((int)UI.Btn_TeamFour).transform.localScale = Vector3.zero;

        GetObject((int)UI.Btn_TeamTwo).transform.DOScale(_uiDefaultSizeMap[(int)UI.Btn_TeamTwo], 0.5f)
            .SetEase(Ease.OutBack);
        GetObject((int)UI.Btn_TeamThree).transform.DOScale(_uiDefaultSizeMap[(int)UI.Btn_TeamThree], 0.5f)
            .SetEase(Ease.OutBack);
    }
}