using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

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
        Shoes,
        ColorShoes,

        AnimalEffect,

        AnimalMovePos,
        MiscellaneousTargetPos
    }

    private Vector3[] _miscellaneousTargetPositions;

    private enum MiscellaneousType
    {
        Car,
        Block,
        Shoes,
        ColorShoes
    }

    private bool _isClickableForRound;

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
                    baseUIManager.PopInstructionUIFromScaleZero("친구들과 같이 교실 정리를 해볼까요?");

                    DOVirtual.DelayedCall(3f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.SortOut_Miscellanous;
                    });

                    break;


                case (int)MainSeq.SortOut_Miscellanous:
                    animalEffectTriggerCount = Random.Range(4, 10);
                    ResetClickable();


                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA039/FindAndSortOut");
                    DOVirtual.DelayedCall(
                        Managers.Sound.GetAudioLength((int)SoundManager.Sound.Narration) + 0.5f, () =>
                        {
                            baseUIManager.PopInstructionUIFromScaleZero("정리해야할 장난감을 찾아 터치해주세요!");

                            DOVirtual.DelayedCall(5f, () =>
                            {
                                baseUIManager.PlayReadyAndStart(() =>
                                {
                                    _isClickableForRound = true;
                                });
                            });
                        });

                    break;
                case (int)MainSeq.SortOut_Shoes:
                    baseUIManager.PopInstructionUIFromScaleZero("이제 신발장으로 가서 정리해볼까요?");
                    GetObject((int)Objs.AnimalEffect).SetActive(false);

                    ResetClickable();

                    break;
                case (int)MainSeq.Sortout_WithColors:
                    ResetClickable();


                    break;


                case (int)MainSeq.OnOutro:
                    break;
                case (int)MainSeq.OnFinish:
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
    private MainSeq START_SEQ = START_SEQ;
#endif
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        CurrentMainMainSeq = (int)START_SEQ;
    }

    // 행동양식 다른부분 별도 클래스 선언없이 Pool형태로 관리
    private readonly Dictionary<int, MiscellaneousType> _carPool = new();
    private readonly Dictionary<int, MiscellaneousType> _blockPool = new();
    private readonly Dictionary<int, MiscellaneousType> _shoesPool = new();
    private readonly Dictionary<int, MiscellaneousType> _colorShoesPool = new();


    // 초기화 및 오브젝트 설정용
    private readonly Dictionary<int, Transform> _carObjPool = new();
    private readonly Dictionary<int, Transform> _blockObjPool = new();
    private readonly Dictionary<int, Transform> _shoesObjPool = new();
    private readonly Dictionary<int, Transform> _colorShoesObjPool = new();

    private readonly Dictionary<int, Vector3> _originalPositionMap = new();
    private readonly Dictionary<int, Quaternion> _originalRotationMap = new();

    private readonly Vector3[] animalMovePath = new Vector3[2]; //start,end

    private int _totalCountToClickOnMisellaneous;
    private int _currentCountClickedOnMiscellaneous;

    protected override void Init()
    {
        psResourcePath = "EA039/OnArrive";
        base.Init();
        BindObject(typeof(Objs));

        InitializePool(GetObject((int)Objs.Cars), _carPool, MiscellaneousType.Car);
        InitializePool(GetObject((int)Objs.Blocks), _blockPool, MiscellaneousType.Block);
        InitializePool(GetObject((int)Objs.Shoes), _shoesPool, MiscellaneousType.Shoes);
        InitializePool(GetObject((int)Objs.ColorShoes), _colorShoesPool, MiscellaneousType.ColorShoes);

        _miscellaneousTargetPositions = new Vector3[GetObject((int)Objs.MiscellaneousTargetPos).transform.childCount];
        for (int i = 0; i < _miscellaneousTargetPositions.Length; i++)
            _miscellaneousTargetPositions[i] =
                GetObject((int)Objs.MiscellaneousTargetPos).transform.GetChild(i).position;
        OnMiscellaneousSortOut();


        GetObject((int)Objs.AnimalEffect).SetActive(false);

        animalMovePath[0] = GetObject((int)Objs.AnimalMovePos).transform.GetChild(0).position;
        animalMovePath[1] = GetObject((int)Objs.AnimalMovePos).transform.GetChild(1).position;


        _totalCountToClickOnMisellaneous = _carPool.Count + _blockPool.Count;
    }

    private Sequence _animalMoveSeq;
    private bool _isAnimalEffectAnimated;

    private void PlayAnimalEffectAndReset()
    {
        GetObject((int)Objs.AnimalEffect).SetActive(true);

        _animalMoveSeq?.Kill();
        _animalMoveSeq = DOTween.Sequence();

        float duration = 3.0f;

        _animalMoveSeq.AppendCallback(() =>
        {
            GetObject((int)Objs.AnimalEffect).transform.position = animalMovePath[0];
        });

        _animalMoveSeq.Append(GetObject((int)Objs.AnimalEffect).transform.DOPath(animalMovePath, duration)
                .SetEase(Ease.OutQuad)).OnStart(() =>
            {
                DOVirtual.DelayedCall(duration / 3f, () =>
                {
                    _currentCountClickedOnMiscellaneous = 0;
                    OnMiscellaneousSortOut();
                });
            })
            .AppendInterval(1f)
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
                    if (!_isClickableMapByTfID[clickedID]) continue; // 이미 클릭된 오브젝트는 무시
                    _isClickableMapByTfID[clickedID] = false;
                    _currentCountClickedOnMiscellaneous++;

                    if (_currentCountClickedOnMiscellaneous == animalEffectTriggerCount && !_isAnimalEffectAnimated)
                    {
                        _isAnimalEffectAnimated = true;
                        DOVirtual.DelayedCall(Random.Range(1f, 2f), () =>
                        {
                            PlayAnimalEffectAndReset();
                        });
                    }


                    if (_carPool.ContainsKey(clickedID))
                        OnCarClicked();
                    else if (_blockPool.ContainsKey(clickedID)) OnBlockOrBallClicked();
                }

                if (_currentCountClickedOnMiscellaneous >= _totalCountToClickOnMisellaneous)
                {
                    baseUIManager.PopInstructionUIFromScaleZero("친구들이 정리를 잘 해서 교실이 깨끗해졌어요!");

                    DOVirtual.DelayedCall(5f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.SortOut_Shoes;
                    });
                }


                break;
            case (int)MainSeq.SortOut_Shoes:
                foreach (var hit in GameManager_Hits)
                {
                    int clickedID = hit.transform.GetInstanceID();

                    if (_shoesPool.ContainsKey(clickedID)) OnShoesClicked();
                }

                break;
            case (int)MainSeq.Sortout_WithColors:
                foreach (var hit in GameManager_Hits)
                {
                    int clickedID = hit.transform.GetInstanceID();

                    if (_shoesPool.ContainsKey(clickedID)) OnShoesClickedOnColorSession();
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
            if (!_blockObjPool.TryGetValue(id, out var block)) continue;

            ReturnToOriginWithJump(block, id);
        }
    }

    private void OnShoesClickedOnColorSession()
    {
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();
            if (!_blockObjPool.TryGetValue(id, out var block)) continue;

            ReturnToOriginWithJump(block, id);
        }
    }


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
            });
    
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
            new Vector3(0, 5f, 0),
            10,
            90,
            true
        ));

        shakeSeq.AppendInterval(Random.Range(1f, 2f));
        shakeSeq.SetLoops(100, LoopType.Restart);

        _sequencePerEnumMap[id] = shakeSeq;
    }
}