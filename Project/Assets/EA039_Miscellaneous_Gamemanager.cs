using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA039_Miscellaneous_Gamemanager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default,
        MainIntro,
        SortOut_Miscellanous,
        SortOut_Shoes,
        Sortout_WithColors,
        OnOutro,
        OnFinish
    }

    private enum Objs
    {
        Cars,
        Blocks,
        Balls,
        Shoes,
        ColorShoes,

        AnimalEffect,

        AnimalMovePosA,
        AnimalMovePosB,
        MiscellaneousTargetPos,

        ShoesTargetPos,

        Fx_OnSuccessA,
        Fx_OnSuccessB
    }

    private ParticleSystem _fxOnSuccessA;
    private ParticleSystem _fxOnSuccessB;

    private Vector3[] _miscellaneousTargetPositions;
    private Vector3[] _shoesTargetPositions;


    private enum MiscellaneousType
    {
        Car,
        Block,
        Ball,

        Shoes,
        ColorShoes
    }

    private bool _isClickableForRound;
    private EA039_InGameUIManager _uiManager;

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
                case (int)MainSeq.MainIntro:

                    DOVirtual.DelayedCall(3f, () =>
                    {
                        _uiManager.PopInstructionUIFromScaleZero("친구들과 같이 교실 정리를 해볼까요?");
                        DOVirtual.DelayedCall(Managers.Sound.GetNarrationClipLength()+0.5f, () =>
                        {
                            CurrentMainMainSeq = (int)MainSeq.SortOut_Miscellanous;
                        });
                    });

                 

                    break;


                case (int)MainSeq.SortOut_Miscellanous:
                    animalEffectTriggerCount = Random.Range(4, 8);
                    ResetClickable();


                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA039/FindAndSortOut");
                    DOVirtual.DelayedCall(
                        Managers.Sound.GetNarrationClipLength()+ 0.5f, () =>
                        {
                            _uiManager.PopInstructionUIFromScaleZero("정리해야할 장난감을 찾아 터치해주세요!");

                            DOVirtual.DelayedCall(5f, () =>
                            {
                                _uiManager.ActivateImageAndUpdateCount(0,
                                    _totalCountToClickOnMisellaneous - _currentCountClickedOnMiscellaneous);
                                _uiManager.PlayReadyAndStart(() =>
                                {
                                    _isClickableForRound = true;
                                });
                            });
                        });

                    break;
                case (int)MainSeq.SortOut_Shoes:
                    _uiManager.DeactivateRoundScoreBoard();
                    _uiManager.PopInstructionUIFromScaleZero("이제 신발장으로 가서 정리해볼까요?");
                    GetObject((int)Objs.AnimalEffect).SetActive(false);

                    OnShoesSortOut(_shoesObjPool);
                    ResetClickable();

                    _isClickableForRound = false;
                    animalEffectTriggerCount = Random.Range(4, 8);


                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA039/FindAndSortOut");
                    DOVirtual.DelayedCall(
                        Managers.Sound.GetAudioLength((int)SoundManager.Sound.Narration) + 1.5f, () =>
                        {
                            _uiManager.PopInstructionUIFromScaleZero("정리해야할 신발을 찾아 터치해주세요!");

                            DOVirtual.DelayedCall(5f, () =>
                            {
                                _uiManager.ActivateImageAndUpdateCount(0,
                                    _totalCountToClickOnShoes - _currentCountClickedOnShoes);
                                _uiManager.PlayReadyAndStart(() =>
                                {
                                    _isClickableForRound = true;
                                });
                            });
                        });

                    break;
                case (int)MainSeq.Sortout_WithColors:
                    _uiManager.PopInstructionUIFromScaleZero("이번엔 색깔별로 정리해볼까요?");
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        PlayAnimalEffectAndReset(_animalMovePathB, () =>
                        {
                            OnShoesSortOut(_colorShoesObjPool);
                            StartColorShoesSession();
                        }, false);
                    });


                    break;


                case (int)MainSeq.OnOutro:
                    break;
                case (int)MainSeq.OnFinish:
                    _uiManager.PopInstructionUIFromScaleZero("우리 정리 잘하기로 약속해요!");
                    RestartScene();
                   
                    break;
            }
        }
    }


    private void ResetClickable()
    {
        foreach (int key in _isClickableMapByTfID.Keys.ToList()) _isClickableMapByTfID[key] = true;
    }
#if UNITY_EDITOR
    [SerializeField] private MainSeq START_SEQ;
#else
    private MainSeq START_SEQ = MainSeq.MainIntro;
#endif
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        CurrentMainMainSeq = (int)START_SEQ;
    }

    // 행동양식 다른부분 별도 클래스 선언없이 Pool형태로 관리
    private readonly Dictionary<int, MiscellaneousType> _carPool = new();
    private readonly Dictionary<int, MiscellaneousType> _blockPool = new();
    private readonly Dictionary<int, MiscellaneousType> _ballPool = new();
    private readonly Dictionary<int, MiscellaneousType> _shoesPool = new();
    private readonly Dictionary<int, MiscellaneousType> _colorShoesPool = new();


    // 초기화 및 오브젝트 설정용
    private readonly Dictionary<int, Transform> _carObjPool = new();
    private readonly Dictionary<int, Transform> _blockObjPool = new();
    private readonly Dictionary<int, Transform> _shoesObjPool = new();
    private readonly Dictionary<int, Transform> _colorShoesObjPool = new();

    private readonly Dictionary<int, Vector3> _originalPositionMap = new();
    private readonly Dictionary<int, Quaternion> _originalRotationMap = new();

    private readonly Vector3[] _animalMovePathA = new Vector3[2]; //start,end
    private readonly Vector3[] _animalMovePathB = new Vector3[2]; //start,end

    private int _totalCountToClickOnMisellaneous;
    private int _currentCountClickedOnMiscellaneous;


    private int _totalCountToClickOnShoes;
    private int _currentCountClickedOnShoes;

    private int _totalCountToClickOnColorShoes;
    private int _currentCountClickedOnColorShoes;

    protected override void Init()
    {
        psResourcePath = "EA039/OnArrive";
        base.Init();
        BindObject(typeof(Objs));
        _uiManager = UIManagerObj.GetComponent<EA039_InGameUIManager>();
        InitializePool(GetObject((int)Objs.Cars), _carPool, MiscellaneousType.Car);
        InitializePool(GetObject((int)Objs.Blocks), _blockPool, MiscellaneousType.Block);
        //  InitializePool(GetObject((int)Objs.Balls), _shoesPool, MiscellaneousType.Ball);
        InitializePool(GetObject((int)Objs.Shoes), _shoesPool, MiscellaneousType.Shoes);
        InitializePool(GetObject((int)Objs.ColorShoes), _colorShoesPool, MiscellaneousType.ColorShoes);

        _miscellaneousTargetPositions = new Vector3[GetObject((int)Objs.MiscellaneousTargetPos).transform.childCount];
        for (int i = 0; i < _miscellaneousTargetPositions.Length; i++)
            _miscellaneousTargetPositions[i] =
                GetObject((int)Objs.MiscellaneousTargetPos).transform.GetChild(i).position;
        OnMiscellaneousSortOut();

        _shoesTargetPositions = new Vector3[GetObject((int)Objs.ShoesTargetPos).transform.childCount];

        for (int i = 0; i < _shoesTargetPositions.Length; i++)
            _shoesTargetPositions[i] =
                GetObject((int)Objs.ShoesTargetPos).transform.GetChild(i).position;

        GetObject((int)Objs.AnimalEffect).SetActive(false);

        _animalMovePathA[0] = GetObject((int)Objs.AnimalMovePosA).transform.GetChild(0).position;
        _animalMovePathA[1] = GetObject((int)Objs.AnimalMovePosA).transform.GetChild(1).position;

        _animalMovePathB[0] = GetObject((int)Objs.AnimalMovePosB).transform.GetChild(0).position;
        _animalMovePathB[1] = GetObject((int)Objs.AnimalMovePosB).transform.GetChild(1).position;

        _totalCountToClickOnMisellaneous = _carPool.Count + _blockPool.Count;
        _totalCountToClickOnShoes = _shoesPool.Count;
        _totalCountToClickOnColorShoes = _colorShoesPool.Count;

        _fxOnSuccessA = GetObject((int)Objs.Fx_OnSuccessA).GetComponent<ParticleSystem>();
        _fxOnSuccessB = GetObject((int)Objs.Fx_OnSuccessB).GetComponent<ParticleSystem>();
    }

    private Sequence _animalMoveSeq;
    private bool _isAnimalEffectAnimated;

    private void PlayAnimalEffectAndReset(Vector3[] path, Action onComplete, bool isShowRoundUIAgain = true)
    {
        _isClickableForRound = false;
        GetObject((int)Objs.AnimalEffect).SetActive(true);

        _animalMoveSeq?.Kill();
        _animalMoveSeq = DOTween.Sequence();

        float duration = 7f;

        _animalMoveSeq.AppendCallback(() =>
        {
            GetObject((int)Objs.AnimalEffect).transform.position = path[0];
        });

        _animalMoveSeq.Append(GetObject((int)Objs.AnimalEffect).transform.DOPath(path, duration)
                .SetEase(Ease.OutQuad))
            .AppendInterval(1f)
            .OnStart(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("동물친구들이 어지럽히고있어!", 1.3f);
                _uiManager.DeactivateRoundScoreBoard();
                DOVirtual.DelayedCall(duration / 3f, () =>
                {
                    Managers.Sound.PlayRandomEffect("EA039/Elephant", 'B');
                    _currentCountClickedOnMiscellaneous = 0;
                    _currentCountClickedOnShoes = 0;
                    onComplete?.Invoke();
                });
                DOVirtual.DelayedCall(2.5f, () =>
                {
                    if (isShowRoundUIAgain)
                    {
                        _uiManager.PopInstructionUIFromScaleZero("다시 정리 해보자!", 1.3f);
                        DOVirtual.DelayedCall(1.5f, () =>
                        {
                            if (CurrentMainMainSeq == (int)MainSeq.SortOut_Miscellanous)
                                _uiManager.ActivateImageAndUpdateCount(0,
                                    _totalCountToClickOnMisellaneous - _currentCountClickedOnMiscellaneous);
                            else
                                _uiManager.ActivateImageAndUpdateCount(1,
                                    _totalCountToClickOnShoes - _currentCountClickedOnShoes);

                            _isClickableForRound = true;
                        });
                    }
                });
            })
            .OnComplete(() =>
            {
                GetObject((int)Objs.AnimalEffect).SetActive(false);
                _animalMoveSeq = null;
            });
    }

    private void InitializePool(GameObject parent, Dictionary<int, MiscellaneousType> pool, MiscellaneousType type)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            var item = parent.transform.GetChild(i);
            int id = item.GetInstanceID();

            pool.Add(id, type);

            if (type == MiscellaneousType.Car)
                _carObjPool.Add(id, item);
            else if (type == MiscellaneousType.Block)
                _blockObjPool.Add(id, item);
            else if (type == MiscellaneousType.Shoes)
                _shoesObjPool.Add(id, item);
            else if (type == MiscellaneousType.ColorShoes) _colorShoesObjPool.Add(id, item);

            _defaultSizeMap[id] = item.localScale;
            _originalPositionMap.Add(id, item.position);
            _originalRotationMap.Add(id, item.rotation);
            //  item.gameObject.SetActive(false); // 초기에는 비활성화
            _isClickableMapByTfID.Add(id, true); // 클릭 가능 상태 초기화
        }
    }

    private int animalEffectTriggerCount;

    public override void OnRaySynced()
    {
        base.OnRaySynced();
        if (!_isClickableForRound) return;

        switch (CurrentMainMainSeq)
        {
            // case (int)MainSeq.Default:
            //     break;
            // case (int)MainSeq.MainIntro:
            //     break;


            case (int)MainSeq.SortOut_Miscellanous:
                foreach (var hit in GameManager_Hits)
                {
                    int clickedID = hit.transform.GetInstanceID();

                    _isClickableMapByTfID.TryAdd(clickedID, true); // 클릭 가능 상태 초기화
                    if (!_isClickableMapByTfID[clickedID]) continue; // 이미 클릭된 오브젝트는 무시
                    _isClickableMapByTfID[clickedID] = false;


                    Managers.Sound.PlayRandomEffect("Common/Click/Click", 'D');
                    
                    
                    _currentCountClickedOnMiscellaneous++;

                    if (_currentCountClickedOnMiscellaneous == animalEffectTriggerCount && !_isAnimalEffectAnimated)
                    {
                        _isAnimalEffectAnimated = true;
                        DOVirtual.DelayedCall(Random.Range(1f, 2f), () =>
                        {
                            PlayAnimalEffectAndReset(_animalMovePathA, OnMiscellaneousSortOut);
                        });
                    }


                    if (_carPool.ContainsKey(clickedID))
                        OnCarClicked();
                    else if (_blockPool.ContainsKey(clickedID)) OnBlockOrBallClicked();


                    //수량판단, 시퀀스 이동 관련 기준점
                    if (_currentCountClickedOnMiscellaneous >= _totalCountToClickOnMisellaneous &&
                        _isAnimalEffectAnimated)
                    {
                        _isClickableForRound = false;
                        _uiManager.PopInstructionUIFromScaleZero("정리를 잘 해서 교실이 깨끗해졌어요!");
                        _fxOnSuccessA.Play();
                        DOVirtual.DelayedCall(5f, () =>
                        {
                            CurrentMainMainSeq = (int)MainSeq.SortOut_Shoes;
                        });
                    }
                    else
                        _uiManager.ActivateImageAndUpdateCount((int)EA039_InGameUIManager.UI.Image_Miscellaneous,
                            _totalCountToClickOnMisellaneous - _currentCountClickedOnMiscellaneous);
                }


                break;
            case (int)MainSeq.SortOut_Shoes:

                foreach (var hit in GameManager_Hits)
                {
                    int clickedID = hit.transform.GetInstanceID();

                    if (_shoesPool.ContainsKey(clickedID))
                    {
                        _isClickableMapByTfID.TryAdd(clickedID, true); // 클릭 가능 상태 초기화
                        if (!_isClickableMapByTfID[clickedID]) continue; // 이미 클릭된 오브젝트는 무시
                        _isClickableMapByTfID[clickedID] = false;
                        Managers.Sound.PlayRandomEffect("Common/Click/Click", 'D');


                        _currentCountClickedOnShoes++;

                        if (_currentCountClickedOnShoes == animalEffectTriggerCount && !_isAnimalEffectAnimated)
                        {
                            _isAnimalEffectAnimated = true;
                            DOVirtual.DelayedCall(Random.Range(1f, 2f), () =>
                            {
                                PlayAnimalEffectAndReset(_animalMovePathB, () => OnShoesSortOut(_shoesObjPool));
                            });
                        }


                        if (_shoesPool.ContainsKey(clickedID)) OnShoesClicked();


                        //수량판단, 시퀀스 이동 관련 기준점
                        if (_currentCountClickedOnShoes >= _totalCountToClickOnShoes &&
                            _isAnimalEffectAnimated)
                        {
                            _currentCountClickedOnShoes = 0;
                            _isClickableForRound = false;
                            _uiManager.PopInstructionUIFromScaleZero("정리를 잘 해서 교실이 깨끗해졌어요!");
                            _fxOnSuccessA.Play();
                            DOVirtual.DelayedCall(5f, () =>
                            {
                                CurrentMainMainSeq = (int)MainSeq.Sortout_WithColors;
                            });
                        }
                        else
                            _uiManager.ActivateImageAndUpdateCount((int)EA039_InGameUIManager.UI.Image_Shoes,
                                _totalCountToClickOnShoes - _currentCountClickedOnShoes);
                    }
                 
                }

                break;
            case (int)MainSeq.Sortout_WithColors:
                foreach (var hit in GameManager_Hits)
                {
                    int clickedID = hit.transform.GetInstanceID();

                    if (_colorShoesPool.ContainsKey(clickedID))
                    {
                        Managers.Sound.PlayRandomEffect("Common/Click/Click", 'D');
                        OnColorShoesClicked();
                    }
                }

                break;


            // case (int)MainSeq.OnOutro:
            //     break;
            // case (int)MainSeq.OnFinish:
            //     break;
        }
    }


    private void OnMiscellaneousSortOut()
    {
        ResetClickable();
        foreach (int key in _sequencePerEnumMap.Keys.ToArray()) _sequencePerEnumMap[key].Kill();

        var shuffledTargets = _miscellaneousTargetPositions
            .OrderBy(_ => Random.value)
            .ToList();

        int index = 0;

        void AnimateAndRotate(Dictionary<int, Transform> objPool)
        {
            foreach (var kvp in objPool)
            {
                var obj = kvp.Value;
                int id = obj.GetInstanceID();

                // Y축 랜덤 회전
                float randomY = Random.Range(0f, 360f);
                obj.rotation = Quaternion.Euler(0, randomY, 0);

                // 기존 애니메이션(떨림) 제거
                if (_sequencePerEnumMap.TryGetValue(id, out var seq))
                {
                    seq.Kill();
                    _sequencePerEnumMap.Remove(id);
                }

                // 위치 애니메이션 적용
                if (index < shuffledTargets.Count)
                {
                    var target = shuffledTargets[index++];

                    obj.DOJump(target, 1.5f, 1, 0.6f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => AnimateClickInducingShake(obj));
                }
            }
        }

        AnimateAndRotate(_carObjPool);
        AnimateAndRotate(_blockObjPool);
    }

    private void OnShoesSortOut(Dictionary<int, Transform> pool)
    {
        ResetClickable();
        foreach (int key in _sequencePerEnumMap.Keys.ToArray()) _sequencePerEnumMap[key].Kill();

        var shuffledTargets = _shoesTargetPositions
            .OrderBy(_ => Random.value)
            .ToList();

        int index = 0;

        void AnimateAndRotate(Dictionary<int, Transform> objPool)
        {
            foreach (var kvp in objPool)
            {
                var obj = kvp.Value;
                int id = obj.GetInstanceID();

             
                obj.DOScale( _defaultSizeMap[id] * 1.7f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => obj.DOScale( _defaultSizeMap[id] * 1.7f, 0.3f).SetEase(Ease.InBack));
                // Y축 랜덤 회전
                float randomY = Random.Range(0f, 360f);
                obj.rotation = Quaternion.Euler(0, randomY, 0);

                // 기존 애니메이션(떨림) 제거
                if (_sequencePerEnumMap.TryGetValue(id, out var seq))
                {
                    seq.Kill();
                    _sequencePerEnumMap.Remove(id);
                }

                // 위치 애니메이션 적용
                if (index < shuffledTargets.Count)
                {
                    var target = shuffledTargets[index++];

                    obj.DOJump(target, 1.5f, 1, 0.6f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => AnimateClickInducingShake(obj));
                }
            }
        }

        AnimateAndRotate(pool);
    }

    private void OnCarClicked()
    {
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();

            _sequencePerEnumMap[id].Kill();


            if (!_carObjPool.TryGetValue(id, out var car)) continue;

            // 제자리에서 점프 + 회전
            var seq = DOTween.Sequence();
            seq.Append(car.DOJump(car.position, 1f, 1, 0.4f))
                .Join(car.DORotate(new Vector3(0, 360, 0), 0.4f, RotateMode.FastBeyond360))
                .AppendInterval(0.1f)
                .Append(car.DOJump(_originalPositionMap[id], 1.5f, 1, 0.6f))
                .Join(car.DORotateQuaternion(_originalRotationMap[id], 0.3f));
            seq.AppendCallback(() =>
            {
                PlayParticleEffect(car.position);
            });
        }
    }

    private void OnBlockOrBallClicked()
    {
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();

            _sequencePerEnumMap[id].Kill();


            if (!_blockObjPool.TryGetValue(id, out var block)) continue;
            ReturnToOriginWithJump(block, id);
        }
    }

    private void OnShoesClicked()
    {
 
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();
            if (!_sequencePerEnumMap.ContainsKey(id)) continue;
            
            _sequencePerEnumMap[id]?.Kill();
          //  _sequencePerEnumMap.Remove(id);
                        
            if (!_shoesObjPool.TryGetValue(id, out var block)) continue;

            ReturnToOriginWithJump(block, id);
        }
    }

// EA039_Miscellaneous_Gamemanager.cs - 추가 로직 (ColorShoes 세션)

    private enum ShoeColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple
    }

    private List<ShoeColor> _availableColors = new();
    private ShoeColor _currentColor;
    private readonly Dictionary<ShoeColor, List<int>> _colorToShoeIDs = new();
    private readonly int _colorShoesTargetCount = 3;
    private readonly Dictionary<ShoeColor, string> _colorNameKoreanMap = new()
    {
        { ShoeColor.Red, "빨간" },
        { ShoeColor.Orange, "주황" },
        { ShoeColor.Yellow, "노란" },
        { ShoeColor.Green, "초록" },
        { ShoeColor.Blue, "파란" },
        { ShoeColor.Purple, "보라" }
    };

// MainSeq.Sortout_WithColors 진입 시 변경

    private void StartColorShoesSession()
    {
        ResetClickable();
        _isAnimalEffectAnimated = false;
        _currentCountClickedOnColorShoes = 0;

        if (_availableColors.Count == 0)
            _availableColors = Enum.GetValues(typeof(ShoeColor)).Cast<ShoeColor>().OrderBy(_ => Random.value).ToList();

        _currentColor = _availableColors[0];
        _availableColors.RemoveAt(0);

        // 신발 ID를 색상 기준으로 매핑
        if (_colorToShoeIDs.Count == 0)
            foreach (var kvp in _colorShoesObjPool)
            {
                // var color = (ShoeColor)(kvp.Key % 6); // 예시 분배
                // if (!_colorToShoeIDs.ContainsKey(color))
                //     _colorToShoeIDs[color] = new List<int>();
                // _colorToShoeIDs[color].Add(kvp.Key);
            }

        // UI 설정 및 시작

        DOVirtual.DelayedCall(2f, () =>
        {
            _uiManager.ActivateImageAndUpdateCount(
                (int)Enum.Parse(typeof(EA039_InGameUIManager.UI), $"Image_{_currentColor}"), _colorShoesTargetCount);

            _uiManager.PlayReadyAndStart(() =>
            {
                _isClickableForRound = true;
            });
        });
    }

    private void OnColorShoesClicked()
    {
        foreach (var hit in GameManager_Hits)
        {
            string objName = hit.transform.name;
            string currentColorStr = _currentColor.ToString();

            int id = hit.transform.GetInstanceID();
            if (!_colorShoesObjPool.TryGetValue(id, out var shoe)) continue;
            if (!_isClickableMapByTfID.TryGetValue(id, out bool canClick) || !canClick) continue;
            //if (!_colorToShoeIDs[_currentColor].Contains(id)) continue; // 현재 라운드 색상과 맞지 않으면 무시
            if (!objName.Contains(currentColorStr))
            {
                Logger.ContentTestLog($"잘못된 색상의 신발을 클릭했습니다! clicked: {objName} - {currentColorStr}");

                // 제자리 점프 애니메이션
                var jumpSeq = DOTween.Sequence();
                jumpSeq.Join(shoe.DOJump(shoe.position, 0.3f, 1, 0.2f) // 점프 파워 0.5, 0.4초
                    .SetEase(Ease.OutQuad));
                jumpSeq.Play();

                return;
            }
            _isClickableMapByTfID[id] = false;


            _sequencePerEnumMap[id].Kill();
            ReturnToOriginWithJump(shoe, id);
            shoe.DOScale( _defaultSizeMap[id], 0.3f).SetEase(Ease.InBack);

            _currentCountClickedOnColorShoes++;

            _uiManager.ActivateImageAndUpdateCount(
                (int)Enum.Parse(typeof(EA039_InGameUIManager.UI), $"Image_{_currentColor}"),
                _colorShoesTargetCount - _currentCountClickedOnColorShoes);

            if (_currentCountClickedOnColorShoes >= _colorShoesTargetCount)
            {
                _isClickableForRound = false;
                _fxOnSuccessB.Play();
                _uiManager.DeactivateRoundScoreBoard();
                string colorKo = _colorNameKoreanMap[_currentColor];
                _uiManager.PopInstructionUIFromScaleZero($"{colorKo} 신발을 잘 정리했어요!",2f);
                
                
                DOVirtual.DelayedCall(2f, () =>
                {
                    if (_availableColors.Count == 0)
                        CurrentMainMainSeq = (int)MainSeq.OnFinish;
                    else
                    {
                        
                        StartColorShoesSession();
                        
                    }
                });
            }
        }
    }

// 5. UI 매니저 내부에 SetColorIcons(), UpdateColorCount() 메서드 구현 필요

    private void ReturnToOriginWithJump(Transform obj, int id, float jumpPower = 1.5f, float duration = 0.6f)
    {
        var originPos = _originalPositionMap[id];
        var originRot = _originalRotationMap[id];

        obj.DOJump(originPos, jumpPower, 1, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                PlayParticleEffect(obj.position);

                obj.DORotateQuaternion(originRot, 0.3f);
            }).Join(obj.DOScale(_defaultSizeMap[id], 0.3f).SetEase(Ease.InBack));;
        
    }

    private void AnimateClickInducingShake(Transform obj)
    {
        int id = obj.GetInstanceID();

        // 기존 시퀀스 제거
        if (_sequencePerEnumMap.TryGetValue(id, out var existingSequence)) existingSequence.Kill();

        // 새 시퀀스 생성
        var shakeSeq = DOTween.Sequence();

        shakeSeq.Append(obj.DOShakeRotation(
            Random.Range(0.25f, 0.4f),
            new Vector3(0, 5f, 0)
        ));

        shakeSeq.AppendInterval(Random.Range(1f, 2f));
        shakeSeq.SetLoops(100, LoopType.Restart);

        _sequencePerEnumMap[id] = shakeSeq;
    }
}