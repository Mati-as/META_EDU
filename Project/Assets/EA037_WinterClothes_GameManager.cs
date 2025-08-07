using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA037_WinterClothes_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default,
        Main_Into,
        Top,
        Botton,
        Outwear,
        Gloves,
        Hat,
        Main_Outro,
        OnFinish
    }

    private enum Objs
    {
        Avatar_Girl,
        Avatar_Boy,
        Avatars,
        ButtonController,
        Fx_OnAnswer,
        AvatarControllerOnFinish,
        OutroClothes,
        CenterPos
    }

    private enum Clothes
    {
        Top,
        Outwear,
        Pants,
        Gloves,
        Hat,
        Default_Upper,
        Pants_Default
    }

    private enum OutroClothesOrder
    {
        Top,
        Pants,
        Outwear,
        Gloves,
        Hat
    }


    #region 이펙트 관리

    #endregion

    private Dictionary<int, Sprite[]> _clothesSpritesMap = new();
    private readonly Dictionary<int, Dictionary<int, GameObject>> _clothesOnAvatarMap = new();


    private ButtonClickEventController _buttonClickEventController;
    private AvatarAnimationController _avatarAnimationController;
    private AvatarAnimationController _avatarControllerOnFinish;

    private ParticleSystem _fxOnAnswer;
    private readonly float ROUND_DELAY = 6;
    private Transform[] _outroClothes;

    protected override void Init()
    {
        SHADOW_MAX_DISTANCE = 30;
        base.Init();
        BindObject(typeof(Objs));

        _buttonClickEventController = GetObject((int)Objs.ButtonController).GetComponent<ButtonClickEventController>();
        _buttonClickEventController.ClickableDelay = 1f;

        _avatarAnimationController = GetObject((int)Objs.Avatars).GetComponent<AvatarAnimationController>();
        
        _avatarControllerOnFinish =
            GetObject((int)Objs.AvatarControllerOnFinish).GetComponent<AvatarAnimationController>();
        GetObject((int)Objs.AvatarControllerOnFinish).SetActive(false);
        _clothesOnAvatarMap.Add((int)Objs.Avatar_Girl, new Dictionary<int, GameObject>());
        _clothesOnAvatarMap.Add((int)Objs.Avatar_Boy, new Dictionary<int, GameObject>());

        for (int i = 0; i < Enum.GetValues(typeof(Clothes)).Length; i++)
        {
            _clothesOnAvatarMap[(int)Objs.Avatar_Girl]
                .Add(i, GetObject((int)Objs.Avatar_Girl).transform.GetChild(i).gameObject);
            _clothesOnAvatarMap[(int)Objs.Avatar_Boy]
                .Add(i, GetObject((int)Objs.Avatar_Boy).transform.GetChild(i).gameObject);
        }

        InitClothesOnAvatar();
        _fxOnAnswer = GetObject((int)Objs.Fx_OnAnswer).GetComponent<ParticleSystem>();

        GetObject((int)Objs.OutroClothes).SetActive(false);
        _outroClothes = new Transform[GetObject((int)Objs.OutroClothes).transform.childCount];

        for (int i = 0; i < GetObject((int)Objs.OutroClothes).transform.childCount; i++)
            _outroClothes[i] = GetObject((int)Objs.OutroClothes).transform.GetChild(i);

        _buttonClickEventController.OnButtonClicked -= OnBtnClicked;
        _buttonClickEventController.OnButtonClicked += OnBtnClicked;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _buttonClickEventController.OnButtonClicked -= OnBtnClicked;
    }

    public int CurrentMainMainSeq
    {
        get
        {
            return currentMainMainSequence;
        }
        set
        {
            currentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");
            ChangeThemeSeqAnim(value);
            switch (value)
            {
                case (int)MainSeq.Default:
                    break;

                case (int)MainSeq.Main_Into:

                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA037/OnStartBtn");
                    BaseInGameUIManager.PopInstructionUIFromScaleZero("따뜻하게 입어요");
                    DOVirtual.DelayedCall(8f, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA037/TooCold");
                        BaseInGameUIManager.PopInstructionUIFromScaleZero("따뜻하게 옷을 입고 밖에 나가서 놀아볼까요?");
                        DOVirtual.DelayedCall(6f, () =>
                        {
                            BaseInGameUIManager.PopInstructionUIFromScaleZero("친구들에게 따뜻한 옷을 입혀주세요!");
                            Managers.Sound.Play(SoundManager.Sound.Narration, "EA037/PutClothes");
                            DOVirtual.DelayedCall(5f, () =>
                            {
                                CurrentMainMainSeq = (int)MainSeq.Top;
                            });
                        });
                    });
                    break;

                case (int)MainSeq.Top:
                    StartClothingSequence("따뜻한 윗옷을 터치해주세요!", "EA037/Nar/TouchTop", "윗옷!", "EA037/Nar/NameTop");
                    break;

                case (int)MainSeq.Botton:
                    StartClothingSequence("따뜻한 바지를 터치해주세요!", "EA037/Nar/TouchPants", "긴바지!", "EA037/Nar/NamePants");
                    break;

                case (int)MainSeq.Outwear:
                    StartClothingSequence("따뜻한 외투를 터치해주세요!", "EA037/Nar/TouchOuter", "외투!", "EA037/Nar/NameOuter");
                    break;

                case (int)MainSeq.Gloves:
                    StartClothingSequence("따뜻한 장갑을 터치해주세요!", "EA037/Nar/TouchGlove", "장갑!", "EA037/Nar/NameGlove");
                    break;

                case (int)MainSeq.Hat:
                    StartClothingSequence("따뜻한 모자를 터치해주세요!", "EA037/Hat", "모자!", "EA037/Nar/NameHat");
                    break;

                case (int)MainSeq.Main_Outro:
                    foreach (var clothe in _outroClothes) clothe.localScale = Vector3.zero;
                    GetObject((int)Objs.OutroClothes).SetActive(true);

                    var OutroSeq = DOTween.Sequence();
                    OutroSeq.AppendCallback(() =>
                    {
                        BaseInGameUIManager.PopInstructionUIFromScaleZero("친구들! 입은옷을 다시 살펴볼까요?",
                            narrationPath: "EA037/Nar/LookClothesAgain");
                    });
                    OutroSeq.AppendInterval(4f);
                    int currentIndex = 0;
                    foreach (var clothe in _outroClothes)
                    {
                        int indexCache = currentIndex;
                        var originalPos = clothe.position;
                        OutroSeq.Append(clothe.DOScale(Vector3.one * 1.25f, 0.3f).SetEase(Ease.OutBounce));
                        OutroSeq.AppendInterval(0.2f);
                        OutroSeq.AppendCallback(() =>
                        {
                            clothe.gameObject.SetActive(true);
                            switch (indexCache)
                            {
                                case (int)OutroClothesOrder.Top:
                                    BaseInGameUIManager.PopInstructionUIFromScaleZero("윗옷",
                                        narrationPath: "EA037/Nar/NameTop");
                                    break;
                                case (int)OutroClothesOrder.Pants:
                                    BaseInGameUIManager.PopInstructionUIFromScaleZero("긴바지",
                                        narrationPath: "EA037/Nar/NamePants");
                                    break;
                                case (int)OutroClothesOrder.Outwear:
                                    BaseInGameUIManager.PopInstructionUIFromScaleZero("외투",
                                        narrationPath: "EA037/Nar/NameOuter");
                                    break;
                                case (int)OutroClothesOrder.Gloves:
                                    BaseInGameUIManager.PopInstructionUIFromScaleZero("장갑",
                                        narrationPath: "EA037/Nar/NameGlove");
                                    break;
                                case (int)OutroClothesOrder.Hat:
                                    BaseInGameUIManager.PopInstructionUIFromScaleZero("모자",
                                        narrationPath: "EA037/Nar/NameHat");
                                    break;
                            }
                        });
                        var targetPos = GetObject((int)Objs.CenterPos).transform.position;
                        OutroSeq.Join(clothe.DOJump(targetPos, 0.5f, 1, Random.Range(0.2f, 0.25f))
                            .SetEase(Ease.OutQuad));
                        OutroSeq.Join(clothe.DOShakeScale(0.5f, 0.2f)
                            .SetEase(Ease.OutQuad));
                        OutroSeq.AppendInterval(3f);
                        OutroSeq.Append(clothe.DOJump(originalPos, 0.5f, 1, Random.Range(0.2f, 0.25f))
                            .SetEase(Ease.OutQuad));
                        OutroSeq.AppendInterval(2f);
                        currentIndex++;
                    }

                    OutroSeq.AppendInterval(2f);
                    OutroSeq.AppendCallback(() =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.OnFinish;
                    });

                    break;

                case (int)MainSeq.OnFinish:

                    TriggerFinish();
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA037/OnFinish");
                    GetObject((int)Objs.AvatarControllerOnFinish).SetActive(true);
                    _avatarControllerOnFinish.PlayAnimation(0, AvatarAnimationController.AnimClip.SitAndPoint);
                    _avatarControllerOnFinish.PlayAnimation(1, AvatarAnimationController.AnimClip.Wave);
                    BaseInGameUIManager.PopInstructionUIFromScaleZero("겨울은 따뜻하게 옷을입어요!");

                    RestartScene();
                    break;
            }
        }
    }

    private void StartClothingSequence(string instructionText, string instructionNarration, string finalText,
        string finalNarration)
    {
        BaseInGameUIManager.PopInstructionUIFromScaleZero(instructionText, narrationPath: instructionNarration);

        DOVirtual.DelayedCall(ROUND_DELAY, () =>
        {
            BaseInGameUIManager.PlayReadyAndStart(() =>
            {
                _buttonClickEventController.StartBtnOnTimeClickMode();
                SetAnswerClothes();
                _isTimerShowable = false;
                DOVirtual.DelayedCall(0.2f, () =>
                {
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _isTimerShowable = true;
                    });
                    BaseInGameUIManager.PopInstructionUIFromScaleZero(finalText, narrationPath: finalNarration);
                });
            });
        });
    }


#if UNITY_EDITOR
    [SerializeField] private MainSeq SEQ_ON_START_BTN;
#else
    MainSeq SEQ_ON_START_BTN = MainSeq.Main_Into;
#endif

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();

        CurrentMainMainSeq = (int)SEQ_ON_START_BTN;
    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();
    }


    protected override void OnBtnClickEvent(int btnId)
    {
        base.OnBtnClickEvent(btnId);

        switch (currentMainMainSequence)
        {
            case (int)MainSeq.Default:
                break;
            case (int)MainSeq.Main_Into:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("창밖에 눈이 내리고 있어요~ 날씨가 너무 추워요~");
                BaseInGameUIManager.PopInstructionUIFromScaleZero("친구들에게 따뜻한 옷을 입혀주세요!");
                break;
            case (int)MainSeq.Top:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("따뜻한 상의를 터치해주세요!");
                break;
            case (int)MainSeq.Botton:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("따뜻한 바지를 터치해주세요!");
                break;
            case (int)MainSeq.Outwear:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("따뜻한 외투를 터치해주세요!");
                break;
            case (int)MainSeq.Gloves:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("장갑을 터치해주세요!");
                break;
            case (int)MainSeq.Main_Outro:

                // CurrentMainMainSeq = (int)MainSeq.OnFinish;
                break;
            case (int)MainSeq.OnFinish:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("겨울은 따뜻하게 옷을입어요!");
                break;
        }
    }


    #region 아바타 옷입히기 관리

    private void InitClothesOnAvatar()
    {
        foreach (int key in _clothesOnAvatarMap[(int)Objs.Avatar_Girl].Keys.ToArray())
            _clothesOnAvatarMap[(int)Objs.Avatar_Girl][key].SetActive(false);

        foreach (int key in _clothesOnAvatarMap[(int)Objs.Avatar_Boy].Keys.ToArray())
            _clothesOnAvatarMap[(int)Objs.Avatar_Boy][key].SetActive(false);

        _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Default_Upper].SetActive(true);
        _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Default_Upper].SetActive(true);
        _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Pants_Default].SetActive(true);
        _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Pants_Default].SetActive(true);
    }

    private void ChangeClothes(int clothIndex)
    {
    }

    #endregion

    private void OnBtnClicked(int index)
    {
        if (index == _currentAnswerIndex)
            OnAnswerClick(true);
        else
            OnWrongAnswerClick();
    }

    private bool _isTimerShowable;
    private Sequence _isTimerShowableSeq;

    private void OnWrongAnswerClick()
    {
        _isTimerShowable = false;


        switch (CurrentMainMainSeq)
        {
            case (int)MainSeq.Top:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("틀렸어요! 따뜻한 긴팔을 골라야해요!");
                break;
            case (int)MainSeq.Botton:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("틀렸어요! 따뜻한 긴바지를 골라야해요!");
                break;
            case (int)MainSeq.Outwear:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("틀렸어요! 따뜻한 외투를 골라야해요!");
                break;
            case (int)MainSeq.Gloves:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("틀렸어요! 장갑을 골라야해요!");
                break;
            case (int)MainSeq.Hat:
                BaseInGameUIManager.PopInstructionUIFromScaleZero("틀렸어요! 모자를 골라야해요!");
                break;
        }

        _isTimerShowableSeq?.Kill();
        _isTimerShowableSeq = DOTween.Sequence();

        _isTimerShowableSeq.AppendInterval(2f);
        _isTimerShowableSeq.AppendCallback(() =>
        {
            _isTimerShowable = true;
        });
        _isTimerShowableSeq.OnKill(() =>
        {
            //_isTimerShowable = false;
        });
    }

    #region 이미지 스프라이트 관리

    private int _currentAnswerIndex = -1;
    private Sequence _answerTimerSeq;
#if UNITY_EDITOR
    [SerializeField] [Range(0, 60)] private int TIME_LIMIT = 10;
#else
    private const int TIME_LIMIT = 10;
#endif


    private void SetAnswerClothes()
    {
        //init 

        isAnswerAlreadyClicked = false;

        _answerTimerSeq?.Kill();
        _answerTimerSeq = DOTween.Sequence();

        int remainTime = TIME_LIMIT;

        _answerTimerSeq.AppendCallback(() =>
        {
            if (_isTimerShowable) BaseInGameUIManager.PopInstructionUIFromScaleZero($"{remainTime}초");
            remainTime--;
        });

        _answerTimerSeq.AppendInterval(1f)
            .SetLoops(TIME_LIMIT, LoopType.Restart)
            .OnComplete(() =>
            {
                // 타이머 종료 시 처리
                BaseInGameUIManager.PopInstructionUIFromScaleZero("시간 종료!");
                OnAnswerClick(false);
            });


        int buttonCount = 7;


        // 정답 위치를 무작위로 설정 (0~6)
        _currentAnswerIndex = Random.Range(0, buttonCount);


        switch (CurrentMainMainSeq)
        {
            case (int)MainSeq.Top:
                SetClothingImages("EA037/Clothes/Sprites/AnswerTop", "EA037/Clothes/Sprites/Top ({0})", 16);
                break;

            case (int)MainSeq.Botton:
                SetClothingImages("EA037/Clothes/Sprites/AnswerBottom", "EA037/Clothes/Sprites/Pants ({0})", 9);
                break;

            case (int)MainSeq.Outwear:
                SetClothingImages("EA037/Clothes/Sprites/AnswerOutwear", "EA037/Clothes/Sprites/Outer ({0})", 8);
                break;

            case (int)MainSeq.Gloves:
                SetClothingImages("EA037/Clothes/Sprites/AnswerGloves", "EA037/Clothes/Sprites/Etc ({0})", 11);
                break;

            case (int)MainSeq.Hat:
                SetClothingImages("EA037/Clothes/Sprites/AnswerHat", "EA037/Clothes/Sprites/Etc ({0})", 11);
                break;
        }
    }

    private void SetClothingImages(string answerPath, string randomPathFormat, int totalCount)
    {
        _buttonClickEventController.ChangeBtnImage(answerPath, _currentAnswerIndex);
        int buttonCount = 7;

        var randomClothes = Enumerable.Range(1, totalCount)
            .OrderBy(x => Random.value)
            .Take(buttonCount)
            .Select(i => string.Format(randomPathFormat, i))
            .ToList();

        for (int i = 0; i < buttonCount; i++)
            if (i != _currentAnswerIndex)
                _buttonClickEventController.ChangeBtnImage(randomClothes[i], i);
    }

    private bool isAnswerAlreadyClicked;

    private void OnAnswerClick(bool isCorrect)
    {
        if (isAnswerAlreadyClicked) return;
        isAnswerAlreadyClicked = true;


        Managers.Sound.Play(SoundManager.Sound.Effect, "Common/Effect/OnSuccess");
        _answerTimerSeq?.Kill();
        _answerTimerSeq = DOTween.Sequence();


        int ranndomPoss = Random.Range(0, 2);

        _avatarAnimationController.SetExpression(0, ranndomPoss == 0
            ? (int)AvatarAnimationController.ExpressionAnimClip.Happy
            : (int)AvatarAnimationController.ExpressionAnimClip.Happy_Heart);


        _avatarAnimationController.SetExpression(1, ranndomPoss == 0
            ? (int)AvatarAnimationController.ExpressionAnimClip.Happy
            : (int)AvatarAnimationController.ExpressionAnimClip.Happy_Heart);

        DOVirtual.DelayedCall(3f, () =>
        {
            _avatarAnimationController.InitExpression(0);
            _avatarAnimationController.InitExpression(1);
        });
        
        if (isCorrect)
        {
            _fxOnAnswer.Play();

            switch (CurrentMainMainSeq)
            {
                case (int)MainSeq.Top:
                    BaseInGameUIManager.PopInstructionUIFromScaleZero("긴팔 입으니 따뜻해요!", narrationPath: "EA037/Nar/OnTop");

                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Top].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Top].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Default_Upper].SetActive(false);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Default_Upper].SetActive(false);

                    break;
                case (int)MainSeq.Botton:
                    BaseInGameUIManager.PopInstructionUIFromScaleZero("긴바지를 입으니 따뜻해요!", narrationPath: "EA037/Nar/OnPants");

                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Pants].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Pants].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Pants_Default].SetActive(false);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Pants_Default].SetActive(false);

                    break;
                case (int)MainSeq.Outwear:
                    BaseInGameUIManager.PopInstructionUIFromScaleZero("외투를 입으니 따뜻해요!", narrationPath: "EA037/Nar/OnOuter");
                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Outwear].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Outwear].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Top].SetActive(false);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Top].SetActive(false);

                    break;
                case (int)MainSeq.Gloves:
                    BaseInGameUIManager.PopInstructionUIFromScaleZero("장갑을 끼우니 손이 시렵지 않아요!",
                        narrationPath: "EA037/Nar/OnGlove");
                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Gloves].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Gloves].SetActive(true);

                    break;

                case (int)MainSeq.Hat:
                    BaseInGameUIManager.PopInstructionUIFromScaleZero("모자를 쓰니 더 따뜻해요!", narrationPath: "EA037/Nar/OnHat");
                    _clothesOnAvatarMap[(int)Objs.Avatar_Girl][(int)Clothes.Hat].SetActive(true);
                    _clothesOnAvatarMap[(int)Objs.Avatar_Boy][(int)Clothes.Hat].SetActive(true);

                    break;
            }
        }


        DOVirtual.DelayedCall(3f, () =>
        {
            switch (CurrentMainMainSeq)
            {
                case (int)MainSeq.Top:
                    CurrentMainMainSeq = (int)MainSeq.Botton;
                    break;
                case (int)MainSeq.Botton:
                    CurrentMainMainSeq = (int)MainSeq.Outwear;
                    break;
                case (int)MainSeq.Outwear:
                    CurrentMainMainSeq = (int)MainSeq.Gloves;
                    break;
                case (int)MainSeq.Gloves:
                    CurrentMainMainSeq = (int)MainSeq.Hat;
                    break;
                case (int)MainSeq.Hat:
                    CurrentMainMainSeq = (int)MainSeq.Main_Outro;
                    break;
            }
        });
        _buttonClickEventController.DeactivateAllButtons();
    }

    public string addressKey = "EA037/Clothes/Sprites/Outer (1)"; // 원하는 Addressable 이름

    #endregion
}